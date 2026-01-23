using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Folding;
using LSR.XmlHelper.Wpf.Infrastructure.Outline;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Threading;

namespace LSR.XmlHelper.Wpf.Infrastructure.Behaviors
{
    public sealed class AvalonEditXmlFoldingService : IDisposable
    {
        private readonly TextEditor _editor;
        private readonly FoldingManager _foldingManager;
        private readonly XmlFoldingStrategy _foldingStrategy;
        private readonly DispatcherTimer _timer;
        private readonly Dictionary<string, HashSet<string>> _collapsedByFileKey = new(StringComparer.OrdinalIgnoreCase);
        private bool _isDisposed;
        private bool _pendingUpdate;
        private string? _currentFileKey;
        private string? _pendingRestoreFileKey;
        private List<FoldingNode> _breadcrumbNodes = new();
        private IReadOnlyList<RawXmlOutlineEntry> _outlineRoots = Array.Empty<RawXmlOutlineEntry>();

        public event EventHandler? OutlineChanged;

        public IReadOnlyList<RawXmlOutlineEntry> GetOutlineRoots()
        {
            return _outlineRoots;
        }
        public AvalonEditXmlFoldingService(TextEditor editor)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));

            _foldingManager = FoldingManager.Install(_editor.TextArea);
            _foldingStrategy = new XmlFoldingStrategy();

            _timer = new DispatcherTimer(DispatcherPriority.Background)
            {
                Interval = TimeSpan.FromMilliseconds(350),
            };

            _timer.Tick += TimerOnTick;
            _editor.TextChanged += EditorOnTextChanged;

            _pendingUpdate = true;
            _timer.Start();
        }

        public void SetDocumentKey(string? fileKey)
        {
            if (_isDisposed)
                return;

            var normalized = string.IsNullOrWhiteSpace(fileKey) ? null : fileKey.Trim();

            if (string.Equals(_currentFileKey, normalized, StringComparison.OrdinalIgnoreCase))
                return;

            SaveCollapsedStateForCurrentKey();

            _currentFileKey = normalized;
            _pendingRestoreFileKey = _currentFileKey;
            _pendingUpdate = true;
        }

        public void Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;

            SaveCollapsedStateForCurrentKey();

            _timer.Stop();
            _timer.Tick -= TimerOnTick;

            _editor.TextChanged -= EditorOnTextChanged;

            FoldingManager.Uninstall(_foldingManager);
        }

        private void EditorOnTextChanged(object? sender, EventArgs e)
        {
            _pendingUpdate = true;
        }

        private void TimerOnTick(object? sender, EventArgs e)
        {
            if (_isDisposed)
                return;

            if (!_pendingUpdate)
                return;

            _pendingUpdate = false;

            try
            {
                _foldingStrategy.UpdateFoldings(_foldingManager, _editor.Document);
                RebuildBreadcrumbNodes();
                RebuildOutlineRoots();
                OutlineChanged?.Invoke(this, EventArgs.Empty);

                if (_pendingRestoreFileKey is not null)
                {
                    RestoreCollapsedStateForKey(_pendingRestoreFileKey);
                    _pendingRestoreFileKey = null;
                }
            }
            catch
            {
                _pendingUpdate = true;
            }
        }

        private void SaveCollapsedStateForCurrentKey()
        {
            if (_currentFileKey is null)
                return;

            var doc = _editor.Document;
            if (doc is null)
                return;

            var foldedKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var folding in _foldingManager.AllFoldings)
            {
                if (folding is null)
                    continue;

                if (!folding.IsFolded)
                    continue;

                var key = BuildFoldingKey(doc, folding.StartOffset);
                if (key is null)
                    continue;

                foldedKeys.Add(key);
            }

            _collapsedByFileKey[_currentFileKey] = foldedKeys;
        }

        private void RestoreCollapsedStateForKey(string fileKey)
        {
            if (!_collapsedByFileKey.TryGetValue(fileKey, out var foldedKeys))
                return;

            var doc = _editor.Document;
            if (doc is null)
                return;

            foreach (var folding in _foldingManager.AllFoldings)
            {
                if (folding is null)
                    continue;

                var key = BuildFoldingKey(doc, folding.StartOffset);
                if (key is null)
                    continue;

                folding.IsFolded = foldedKeys.Contains(key);
            }
        }

        private static string? BuildFoldingKey(TextDocument doc, int startOffset)
        {
            if (startOffset < 0 || startOffset >= doc.TextLength)
                return null;

            var loc = doc.GetLocation(startOffset);
            var line = doc.GetLineByNumber(loc.Line);
            if (line is null)
                return null;

            var sliceLen = Math.Min(200, doc.TextLength - line.Offset);
            var slice = doc.GetText(line.Offset, sliceLen);

            var name = TryReadStartTagName(slice);
            if (string.IsNullOrWhiteSpace(name))
                return null;

            return name + "|" + loc.Line.ToString();
        }

        private static string? TryReadStartTagName(string text)
        {
            var lt = text.IndexOf('<');
            if (lt < 0)
                return null;

            var i = lt + 1;
            while (i < text.Length && char.IsWhiteSpace(text[i]))
                i++;

            if (i >= text.Length)
                return null;

            if (text[i] == '/' || text[i] == '!' || text[i] == '?')
                return null;

            var sb = new StringBuilder();

            while (i < text.Length)
            {
                var c = text[i];

                if (char.IsWhiteSpace(c) || c == '>' || c == '/')
                    break;

                sb.Append(c);
                i++;
            }

            return sb.Length == 0 ? null : sb.ToString();
        }

        public IReadOnlyList<(string Title, int Offset)> GetBreadcrumbSegments(int caretOffset)
        {
            if (_breadcrumbNodes.Count == 0)
                return Array.Empty<(string Title, int Offset)>();

            var idx = FindRightmostStartIndex(caretOffset);
            if (idx < 0)
                return Array.Empty<(string Title, int Offset)>();

            var deepestIndex = -1;

            for (var i = idx; i >= 0; i--)
            {
                var n = _breadcrumbNodes[i];
                if (caretOffset < n.StartOffset)
                    continue;

                if (caretOffset <= n.EndOffset)
                {
                    deepestIndex = i;
                    break;
                }
            }

            if (deepestIndex < 0)
                return Array.Empty<(string Title, int Offset)>();

            var list = new List<(string Title, int Offset)>();

            var current = deepestIndex;
            while (current >= 0)
            {
                var node = _breadcrumbNodes[current];
                list.Add((node.Name, node.StartOffset));
                current = node.ParentIndex;
            }

            list.Reverse();
            return list;
        }
        public void CollapseAll()
        {
            foreach (var folding in _foldingManager.AllFoldings)
            {
                if (folding is null)
                    continue;

                folding.IsFolded = true;
            }
        }

        public void ExpandAll()
        {
            foreach (var folding in _foldingManager.AllFoldings)
            {
                if (folding is null)
                    continue;

                folding.IsFolded = false;
            }
        }

        public bool CollapseContainingElement(int caretOffset)
        {
            return SetContainingElementFold(caretOffset, true);
        }

        public bool ExpandContainingElement(int caretOffset)
        {
            return SetContainingElementFold(caretOffset, false);
        }
        public bool TryGetEntrySpan(int caretOffset, out int startOffset, out int endOffset)
        {
            startOffset = 0;
            endOffset = 0;

            if (_breadcrumbNodes.Count == 0)
                return false;

            var deepestIndex = FindDeepestNodeIndex(caretOffset);
            if (deepestIndex < 0)
                return false;

            var entryIndex = FindEntryNodeIndex(deepestIndex);
            if (entryIndex < 0 || entryIndex >= _breadcrumbNodes.Count)
                return false;

            var node = _breadcrumbNodes[entryIndex];
            startOffset = node.StartOffset;
            endOffset = node.EndOffset;
            return true;
        }

        private int FindDeepestNodeIndex(int caretOffset)
        {
            var idx = FindRightmostStartIndex(caretOffset);
            if (idx < 0)
                return -1;

            for (var i = idx; i >= 0; i--)
            {
                var n = _breadcrumbNodes[i];
                if (caretOffset < n.StartOffset)
                    continue;

                if (caretOffset <= n.EndOffset)
                    return i;
            }

            return -1;
        }

        private int FindEntryNodeIndex(int deepestIndex)
        {
            var current = deepestIndex;

            while (current >= 0)
            {
                var node = _breadcrumbNodes[current];
                var parentIndex = node.ParentIndex;

                if (parentIndex >= 0)
                {
                    var siblingCount = 0;

                    for (var i = 0; i < _breadcrumbNodes.Count; i++)
                    {
                        var n = _breadcrumbNodes[i];
                        if (n.ParentIndex == parentIndex && string.Equals(n.Name, node.Name, StringComparison.Ordinal))
                            siblingCount++;
                    }

                    if (siblingCount > 1)
                        return current;
                }

                current = node.ParentIndex;
            }

            return deepestIndex;
        }

        private bool SetContainingElementFold(int caretOffset, bool isFolded)
        {
            FoldingSection? best = null;

            foreach (var folding in _foldingManager.AllFoldings)
            {
                if (folding is null)
                    continue;

                if (caretOffset < folding.StartOffset || caretOffset > folding.EndOffset)
                    continue;

                if (best is null)
                {
                    best = folding;
                    continue;
                }

                var bestLen = best.EndOffset - best.StartOffset;
                var thisLen = folding.EndOffset - folding.StartOffset;
                if (thisLen < bestLen)
                    best = folding;
            }

            if (best is null)
                return false;

            best.IsFolded = isFolded;
            return true;
        }

        private int FindRightmostStartIndex(int caretOffset)
        {
            var lo = 0;
            var hi = _breadcrumbNodes.Count - 1;
            var result = -1;

            while (lo <= hi)
            {
                var mid = lo + (hi - lo) / 2;
                var start = _breadcrumbNodes[mid].StartOffset;

                if (start <= caretOffset)
                {
                    result = mid;
                    lo = mid + 1;
                }
                else
                {
                    hi = mid - 1;
                }
            }

            return result;
        }

        private void RebuildBreadcrumbNodes()
        {
            var doc = _editor.Document;
            if (doc is null)
            {
                _breadcrumbNodes = new List<FoldingNode>();
                return;
            }

            var foldings = _foldingManager.AllFoldings
                .Where(f => f is not null)
                .OrderBy(f => f.StartOffset)
                .ToList();

            var nodes = new List<FoldingNode>(foldings.Count);
            var stack = new List<int>();

            foreach (var f in foldings)
            {
                var name = TryReadStartTagNameAtOffset(doc, f.StartOffset);
                if (string.IsNullOrWhiteSpace(name))
                    continue;

                while (stack.Count > 0)
                {
                    var top = nodes[stack[stack.Count - 1]];
                    if (f.StartOffset <= top.EndOffset)
                        break;

                    stack.RemoveAt(stack.Count - 1);
                }

                var parentIndex = stack.Count > 0 ? stack[stack.Count - 1] : -1;

                var node = new FoldingNode(name, f.StartOffset, f.EndOffset, parentIndex);
                nodes.Add(node);

                stack.Add(nodes.Count - 1);
            }

            _breadcrumbNodes = nodes;
        }

        private static string? TryReadStartTagNameAtOffset(TextDocument doc, int startOffset)
        {
            if (startOffset < 0 || startOffset >= doc.TextLength)
                return null;

            var loc = doc.GetLocation(startOffset);
            var line = doc.GetLineByNumber(loc.Line);
            if (line is null)
                return null;

            var sliceLen = Math.Min(200, doc.TextLength - line.Offset);
            var slice = doc.GetText(line.Offset, sliceLen);

            return TryReadStartTagName(slice);
        }
        private void RebuildOutlineRoots()
        {
            if (_breadcrumbNodes.Count == 0)
            {
                _outlineRoots = Array.Empty<RawXmlOutlineEntry>();
                return;
            }

            var childrenByIndex = new List<List<int>>(_breadcrumbNodes.Count);
            for (var i = 0; i < _breadcrumbNodes.Count; i++)
                childrenByIndex.Add(new List<int>());

            var rootIndices = new List<int>();

            for (var i = 0; i < _breadcrumbNodes.Count; i++)
            {
                var parent = _breadcrumbNodes[i].ParentIndex;

                if (parent < 0)
                {
                    rootIndices.Add(i);
                    continue;
                }

                if (parent >= 0 && parent < childrenByIndex.Count)
                    childrenByIndex[parent].Add(i);
            }

            _outlineRoots = BuildOutlineLevel(rootIndices, childrenByIndex);
        }

        private IReadOnlyList<RawXmlOutlineEntry> BuildOutlineLevel(List<int> indices, List<List<int>> childrenByIndex)
        {
            if (indices.Count == 0)
                return Array.Empty<RawXmlOutlineEntry>();

            var counts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (var idx in indices)
            {
                var name = _breadcrumbNodes[idx].Name;
                if (!counts.TryAdd(name, 1))
                    counts[name] = counts[name] + 1;
            }

            var running = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            var result = new List<RawXmlOutlineEntry>(indices.Count);
            var doc = _editor.Document;

            foreach (var idx in indices)
            {
                var node = _breadcrumbNodes[idx];
                var name = node.Name;

                if (!running.TryAdd(name, 0))
                    running[name] = running[name] + 1;
                else
                    running[name] = 1;

                var fallback = name + "[" + running[name].ToString() + "]";
                var title = counts[name] > 1 ? ResolveOutlineTitle(doc, name, node.StartOffset, node.EndOffset, fallback) : name;

                var childIndices = childrenByIndex[idx];
                var children = BuildOutlineLevel(childIndices, childrenByIndex);

                result.Add(new RawXmlOutlineEntry(title, node.StartOffset, children));
            }

            return result;
        }
        private static string ResolveOutlineTitle(TextDocument? doc, string elementName, int startOffset, int endOffset, string fallback)
        {
            if (doc is null)
                return fallback;

            var attrValue = TryResolveFromStartTagAttributes(doc, startOffset);
            if (!string.IsNullOrWhiteSpace(attrValue))
                return attrValue;

            var innerValue = TryResolveFromInnerValue(doc, startOffset, endOffset);
            if (!string.IsNullOrWhiteSpace(innerValue))
                return innerValue;

            return fallback;
        }

        private static string? TryResolveFromStartTagAttributes(TextDocument doc, int startOffset)
        {
            if (startOffset < 0 || startOffset >= doc.TextLength)
                return null;

            var remaining = doc.TextLength - startOffset;
            var len = Math.Min(600, remaining);
            var text = doc.GetText(startOffset, len);

            var gt = text.IndexOf('>');
            if (gt > 0)
                text = text.Substring(0, gt);

            var candidates = new[]
            {
                "FullName",
                "DisplayName",
                "Name",
                "Title",
                "Label",
                "GroupName",
                "ModItemName",
                "ID",
                "Id",
                "Key"
            };

            foreach (var c in candidates)
            {
                var value = TryReadAttributeValue(text, c);
                if (!string.IsNullOrWhiteSpace(value))
                    return value;
            }

            return null;
        }

        private static string? TryResolveFromInnerValue(TextDocument doc, int startOffset, int endOffset)
        {
            if (startOffset < 0 || startOffset >= doc.TextLength)
                return null;

            if (endOffset < startOffset)
                return null;

            var maxLen = 6000;
            var len = Math.Min(maxLen, Math.Min(doc.TextLength - startOffset, endOffset - startOffset + 1));
            if (len <= 0)
                return null;

            var text = doc.GetText(startOffset, len);

            var candidates = new[]
            {
                "FullName",
                "DisplayName",
                "Name",
                "Title",
                "Description",
                "Label",
                "GroupName",
                "ModItemName",
                "ID",
                "Id",
                "Key"
            };

            foreach (var tag in candidates)
            {
                var value = TryReadSimpleElementValue(text, tag);
                if (!string.IsNullOrWhiteSpace(value))
                    return value;
            }

            return null;
        }

        private static string? TryReadAttributeValue(string startTagText, string attributeName)
        {
            var idx = IndexOfOrdinalIgnoreCase(startTagText, attributeName + "=");
            if (idx < 0)
                return null;

            idx += attributeName.Length + 1;

            while (idx < startTagText.Length && char.IsWhiteSpace(startTagText[idx]))
                idx++;

            if (idx >= startTagText.Length)
                return null;

            var quote = startTagText[idx];
            if (quote != '"' && quote != '\'')
                return null;

            idx++;

            var end = startTagText.IndexOf(quote, idx);
            if (end < 0)
                return null;

            var value = startTagText.Substring(idx, end - idx).Trim();
            if (value.Length == 0 || value.Length > 200)
                return null;

            return value;
        }

        private static string? TryReadSimpleElementValue(string text, string tag)
        {
            var open = "<" + tag + ">";
            var close = "</" + tag + ">";

            var start = IndexOfOrdinalIgnoreCase(text, open);
            if (start < 0)
                return null;

            start += open.Length;

            var end = IndexOfOrdinalIgnoreCase(text, close, start);
            if (end < 0)
                return null;

            var value = text.Substring(start, end - start).Trim();
            if (value.Length == 0 || value.Length > 200)
                return null;

            if (value.Contains("<", StringComparison.Ordinal))
                return null;

            return value;
        }

        private static int IndexOfOrdinalIgnoreCase(string text, string value)
        {
            return IndexOfOrdinalIgnoreCase(text, value, 0);
        }

        private static int IndexOfOrdinalIgnoreCase(string text, string value, int startIndex)
        {
            return text.IndexOf(value, startIndex, StringComparison.OrdinalIgnoreCase);
        }

        private sealed class FoldingNode
        {
            public FoldingNode(string name, int startOffset, int endOffset, int parentIndex)
            {
                Name = name;
                StartOffset = startOffset;
                EndOffset = endOffset;
                ParentIndex = parentIndex;
            }

            public string Name { get; }
            public int StartOffset { get; }
            public int EndOffset { get; }
            public int ParentIndex { get; }
        }
    }
}

