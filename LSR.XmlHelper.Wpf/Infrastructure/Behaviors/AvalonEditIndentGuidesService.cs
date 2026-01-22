using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;
using ICSharpCode.AvalonEdit.Editing;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace LSR.XmlHelper.Wpf.Infrastructure.Behaviors
{
    public sealed class AvalonEditIndentGuidesService : IBackgroundRenderer
    {
        private readonly TextDocument _document;
        private readonly IRawXmlLineDepthProvider _depthProvider;
        private TextView? _lastTextView;
        private bool _isEnabled;
        private bool _isTextViewInvalidationHooked;
        private bool _isDarkMode;
        private bool _isIndentGuidesEnabled = true;
        private string _guidesColor = "";
        private System.Windows.Media.Pen? _pen;
        private int _spacesPerIndentLevel = 4;
        private int _detectedSpacesPerIndentLevel;
        private readonly Dictionary<int, double> _lastXByLevelCache = new Dictionary<int, double>();
        private TextView? _lastXCacheTextView;
        private double _lastXCacheHorizontalOffset = double.NaN;
        private int _caretLine;
        private int _caretVisualColumn;
        private bool _isCurrentIndentHighlightEnabled = true;
        private System.Windows.Media.Pen? _highlightPen;
       
        public AvalonEditIndentGuidesService(TextDocument document, IRawXmlLineDepthProvider depthProvider)
        {
            _document = document;
            _depthProvider = depthProvider;
            _document.Changed += (_, __) => _detectedSpacesPerIndentLevel = 0;
        }

        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                if (_isEnabled == value)
                    return;

                _isEnabled = value;
                Invalidate();
            }
        }

        public bool IsDarkMode
        {
            get => _isDarkMode;
            set
            {
                if (_isDarkMode == value)
                    return;

                _isDarkMode = value;
                _pen = null;
                _highlightPen = null;
                Invalidate();
            }
        }

        public bool IsIndentGuidesEnabled
        {
            get => _isIndentGuidesEnabled;
            set
            {
                if (_isIndentGuidesEnabled == value)
                    return;

                _isIndentGuidesEnabled = value;
                Invalidate();
            }
        }

        public string GuidesColor
        {
            get => _guidesColor;
            set
            {
                if (string.Equals(_guidesColor, value, StringComparison.Ordinal))
                    return;

                _guidesColor = value ?? "";
                _pen = null;
                _highlightPen = null;
                Invalidate();
            }
        }

        public int SpacesPerIndentLevel
        {
            get => _spacesPerIndentLevel;
            set
            {
                if (_spacesPerIndentLevel == value)
                    return;

                _spacesPerIndentLevel = Math.Max(1, value);
                Invalidate();
            }
        }
        public int CaretLine
        {
            get => _caretLine;
            set
            {
                if (_caretLine == value)
                    return;

                _caretLine = value;
                Invalidate();
            }
        }

        public int CaretVisualColumn
        {
            get => _caretVisualColumn;
            set
            {
                if (_caretVisualColumn == value)
                    return;

                _caretVisualColumn = value;
                Invalidate();
            }
        }

        public bool IsCurrentIndentHighlightEnabled
        {
            get => _isCurrentIndentHighlightEnabled;
            set
            {
                if (_isCurrentIndentHighlightEnabled == value)
                    return;

                _isCurrentIndentHighlightEnabled = value;
                Invalidate();
            }
        }
 
        public KnownLayer Layer => KnownLayer.Background;

        private void Invalidate()
        {
            if (_lastTextView is null)
                return;

            _lastTextView.InvalidateLayer(Layer);
            _lastTextView.InvalidateVisual();
        }
        private void TrackTextView(TextView textView)
        {
            if (ReferenceEquals(_lastTextView, textView))
                return;

            UnhookTextView();
            _lastTextView = textView;

            if (_lastTextView is null)
                return;

            _lastTextView.ScrollOffsetChanged += TextView_ScrollOffsetChanged;
            _lastTextView.VisualLinesChanged += TextView_VisualLinesChanged;
            _isTextViewInvalidationHooked = true;
        }

        private void UnhookTextView()
        {
            if (!_isTextViewInvalidationHooked)
                return;

            if (_lastTextView is null)
                return;

            _lastTextView.ScrollOffsetChanged -= TextView_ScrollOffsetChanged;
            _lastTextView.VisualLinesChanged -= TextView_VisualLinesChanged;
            _isTextViewInvalidationHooked = false;
        }

        private void TextView_ScrollOffsetChanged(object? sender, EventArgs e)
        {
            Invalidate();
        }

        private void TextView_VisualLinesChanged(object? sender, EventArgs e)
        {
            Invalidate();
        }

        public void Draw(TextView textView, DrawingContext drawingContext)
        {
            if (!_isEnabled)
                return;

            TrackTextView(textView);

            if (_document is null || textView?.Document is null)
                return;

            if (!textView.VisualLinesValid)
                return;

            if (!_isIndentGuidesEnabled)
                return;

            var pen = GetPen();
            if (pen is null)
                return;

            textView.EnsureVisualLines();

            var spacesPerIndentLevel = GetSpacesPerIndentLevel(textView);
            var tabSize = GetTabSize(textView);
            var caretIndentLevel = GetCaretIndentLevel(spacesPerIndentLevel);

            var lastIndentLevel = 0;
            var visibleDocLines = 0;
            var visibleWithGuides = 0;
            var visibleWithoutGuides = 0;
            var minVisibleMaxLevel = int.MaxValue;
            var maxVisibleMaxLevel = 0;
            var firstVisibleLineNumber = 0;
            var lastVisibleLineNumber = 0;

            if (!ReferenceEquals(_lastXCacheTextView, textView))
            {
                _lastXCacheTextView = textView;
                _lastXCacheHorizontalOffset = double.NaN;
                _lastXByLevelCache.Clear();
            }

            if (double.IsNaN(_lastXCacheHorizontalOffset) || Math.Abs(_lastXCacheHorizontalOffset - textView.HorizontalOffset) > 0.1)
            {
                _lastXCacheHorizontalOffset = textView.HorizontalOffset;
                _lastXByLevelCache.Clear();
            }

            foreach (var visualLine in textView.VisualLines)
            {
                var docLine = visualLine.FirstDocumentLine;
                var lineNumber = docLine.LineNumber;

                if (firstVisibleLineNumber == 0)
                    firstVisibleLineNumber = lineNumber;

                visibleDocLines += 1;

                var lineText = _document.GetText(docLine.Offset, docLine.Length);

                var maxLevel = GetIndentLevel(docLine, spacesPerIndentLevel, tabSize, ref lastIndentLevel);

                if (_depthProvider.TryGetLineDepth(lineNumber, out var depth) && depth > 0)
                    maxLevel = depth;

                if (maxLevel <= 0)
                {
                    visibleWithoutGuides += 1;
                    UpdateDebugText(textView, spacesPerIndentLevel, tabSize, caretIndentLevel, lineNumber, maxLevel, visibleDocLines, visibleWithGuides, visibleWithoutGuides, firstVisibleLineNumber, lastVisibleLineNumber, minVisibleMaxLevel, maxVisibleMaxLevel);
                    lastVisibleLineNumber = lineNumber;
                    continue;
                }

                visibleWithGuides += 1;

                if (maxLevel < minVisibleMaxLevel)
                    minVisibleMaxLevel = maxLevel;

                if (maxLevel > maxVisibleMaxLevel)
                    maxVisibleMaxLevel = maxLevel;

                UpdateDebugText(textView, spacesPerIndentLevel, tabSize, caretIndentLevel, lineNumber, maxLevel, visibleDocLines, visibleWithGuides, visibleWithoutGuides, firstVisibleLineNumber, lastVisibleLineNumber, minVisibleMaxLevel, maxVisibleMaxLevel);
                lastVisibleLineNumber = lineNumber;

                var yTop = visualLine.GetTextLineVisualYPosition(visualLine.TextLines[0], VisualYPosition.LineTop) - textView.ScrollOffset.Y;
                var yBottom = visualLine.GetTextLineVisualYPosition(visualLine.TextLines[0], VisualYPosition.LineBottom) - textView.ScrollOffset.Y;

                if (yBottom < yTop)
                {
                    var tmp = yTop;
                    yTop = yBottom;
                    yBottom = tmp;
                }

                for (var level = 1; level <= maxLevel; level++)
                {
                    try
                    {
                        var hasColumn = TryGetIndentGuideColumn(docLine, spacesPerIndentLevel, tabSize, level, out var column);

                        double x;
                        if (hasColumn)
                        {
                            var xPoint = textView.GetVisualPosition(new TextViewPosition(lineNumber, column), VisualYPosition.TextTop);
                            x = xPoint.X - textView.ScrollOffset.X;
                            _lastXByLevelCache[level] = x;
                        }
                        else if (string.IsNullOrWhiteSpace(lineText) && _lastXByLevelCache.TryGetValue(level, out var cachedX))
                            x = cachedX;
                        else
                            continue;

                        var penToUse = pen;
                        if (_isCurrentIndentHighlightEnabled && caretIndentLevel == level)
                        {
                            var highlightPen = GetHighlightPen();
                            if (highlightPen is not null)
                                penToUse = highlightPen;
                        }

                        var xSnapped = Math.Floor(x) + 0.5;

                        if (double.IsNaN(xSnapped) || double.IsNaN(yTop) || double.IsNaN(yBottom))
                            continue;

                        drawingContext.DrawLine(penToUse, new System.Windows.Point(xSnapped, yTop), new System.Windows.Point(xSnapped, yBottom));
                    }
                    catch
                    {
                    }
                }
            }
        }

        private void UpdateDebugText(TextView textView, int spacesPerIndentLevel, int tabSize, int caretIndentLevel, int currentLineNumber, int currentLineMaxLevel, int visibleDocLines, int visibleWithGuides, int visibleWithoutGuides, int firstVisibleLineNumber, int lastVisibleLineNumber, int minVisibleMaxLevel, int maxVisibleMaxLevel)
        {
            var debugMinVisibleMaxLevel = minVisibleMaxLevel == int.MaxValue ? 0 : minVisibleMaxLevel;

            var caretDocLineLevel = 0;
            if (_caretLine > 0 && _caretLine <= _document.LineCount)
            {
                var caretDocLine = _document.GetLineByNumber(_caretLine);
                var tmpIndent = 0;
                caretDocLineLevel = GetIndentLevel(caretDocLine, spacesPerIndentLevel, tabSize, ref tmpIndent);
            }

            var newDebugText =
                "IndentGuides" + Environment.NewLine +
                "SpacesPerIndent=" + spacesPerIndentLevel + " Detected=" + _detectedSpacesPerIndentLevel + " TabSize=" + tabSize + Environment.NewLine +
                "CaretLine=" + _caretLine + " CaretVisualColumn=" + _caretVisualColumn + " CaretIndentLevel=" + caretIndentLevel + " CaretDocLineLevel=" + caretDocLineLevel + Environment.NewLine +
                "CurrentLine=" + currentLineNumber + " CurrentMaxLevel=" + currentLineMaxLevel + Environment.NewLine +
                "VisibleLines=" + textView.VisualLines.Count + " VisibleDocLines=" + visibleDocLines + " First=" + firstVisibleLineNumber + " Last=" + lastVisibleLineNumber + Environment.NewLine +
                "WithGuides=" + visibleWithGuides + " WithoutGuides=" + visibleWithoutGuides + " MinMaxLevel=" + debugMinVisibleMaxLevel + " MaxMaxLevel=" + maxVisibleMaxLevel;

        }

        private int GetCaretIndentLevel(int spacesPerIndentLevel)
        {
            if (spacesPerIndentLevel <= 0)
                return 0;

            var level = _caretVisualColumn / spacesPerIndentLevel;
            if (level <= 0)
                return 0;

            return level;
        }

        private System.Windows.Media.Pen? GetHighlightPen()
        {
            if (_highlightPen is not null)
                return _highlightPen;

            System.Windows.Media.Color baseColor;

            if (string.IsNullOrWhiteSpace(_guidesColor))
            {
                baseColor = _isDarkMode ? System.Windows.Media.Colors.White : System.Windows.Media.Colors.Black;
            }
            else
            {
                try
                {
                    baseColor = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(_guidesColor);
                }
                catch
                {
                    baseColor = _isDarkMode ? System.Windows.Media.Colors.White : System.Windows.Media.Colors.Black;
                }
            }

            var luminance = (baseColor.R * 299 + baseColor.G * 587 + baseColor.B * 114) / 1000;

            if (!_isDarkMode && luminance > 200)
                baseColor = System.Windows.Media.Colors.Black;

            if (_isDarkMode && luminance < 55)
                baseColor = System.Windows.Media.Colors.White;

            byte alpha = _isDarkMode ? (byte)190 : (byte)220;
            var c = System.Windows.Media.Color.FromArgb(alpha, baseColor.R, baseColor.G, baseColor.B);

            var brush = new SolidColorBrush(c);
            brush.Freeze();

            var pen = new System.Windows.Media.Pen(brush, 1.0);
            pen.Freeze();

            _highlightPen = pen;
            return pen;
        }

        private int GetTabSize(TextView textView)
        {
            var options = textView.Options;
            if (options is null)
                return 4;

            var type = options.GetType();

            var tabSizeProp = type.GetProperty("TabSize");
            if (tabSizeProp is not null && tabSizeProp.PropertyType == typeof(int))
            {
                var value = tabSizeProp.GetValue(options);
                if (value is int i && i > 0)
                    return i;
            }

            var indentationSizeProp = type.GetProperty("IndentationSize");
            if (indentationSizeProp is not null && indentationSizeProp.PropertyType == typeof(int))
            {
                var value = indentationSizeProp.GetValue(options);
                if (value is int i && i > 0)
                    return i;
            }

            return 4;
        }

        private int GetSpacesPerIndentLevel(TextView textView)
        {
            if (_detectedSpacesPerIndentLevel > 0)
                return _detectedSpacesPerIndentLevel;

            var tabSize = GetTabSize(textView);
            var inspected = 0;
            var widths = new System.Collections.Generic.List<int>(256);

            var maxLines = Math.Min(_document.LineCount, 4000);
            var lineNumber = 1;

            while (lineNumber <= maxLines && inspected < 800)
            {
                var docLine = _document.GetLineByNumber(lineNumber);
                var text = _document.GetText(docLine.Offset, docLine.Length);

                if (!string.IsNullOrWhiteSpace(text))
                {
                    var spaceCount = 0;
                    var idx = 0;

                    while (idx < text.Length)
                    {
                        var ch = text[idx];

                        if (ch == ' ')
                            spaceCount += 1;
                        else if (ch == '\t')
                            spaceCount += tabSize;
                        else
                            break;

                        idx += 1;
                    }

                    if (spaceCount > 0)
                    {
                        widths.Add(spaceCount);
                        inspected += 1;
                    }
                }

                lineNumber += 1;
            }

            if (widths.Count == 0)
            {
                _detectedSpacesPerIndentLevel = _spacesPerIndentLevel;
                return _detectedSpacesPerIndentLevel;
            }

            var bestCandidate = 0;
            var bestScore = -1;

            for (var candidate = 2; candidate <= 8; candidate += 1)
            {
                var score = 0;
                for (var i = 0; i < widths.Count; i += 1)
                {
                    if (widths[i] % candidate == 0)
                        score += 1;
                }

                if (score > bestScore || (score == bestScore && candidate > bestCandidate))
                {
                    bestScore = score;
                    bestCandidate = candidate;
                }
            }

            if (bestCandidate < 2)
                _detectedSpacesPerIndentLevel = _spacesPerIndentLevel;
            else
                _detectedSpacesPerIndentLevel = bestCandidate;

            return _detectedSpacesPerIndentLevel;
        }

        private static int Gcd(int a, int b)
        {
            a = Math.Abs(a);
            b = Math.Abs(b);

            while (b != 0)
            {
                var t = a % b;
                a = b;
                b = t;
            }

            return a;
        }

        private int GetIndentLevel(DocumentLine docLine, int spacesPerIndentLevel, int tabSize, ref int lastIndentLevel)
        {
            var text = _document.GetText(docLine.Offset, docLine.Length);

            if (string.IsNullOrWhiteSpace(text))
                return lastIndentLevel;

            var spaceCount = 0;
            var charIndex = 0;

            while (charIndex < text.Length)
            {
                var ch = text[charIndex];

                if (ch == ' ')
                {
                    spaceCount += 1;
                }
                else if (ch == '\t')
                {
                    var step = tabSize - (spaceCount % tabSize);
                    if (step <= 0)
                        step = tabSize;

                    spaceCount += step;
                }
                else
                {
                    break;
                }

                charIndex += 1;
            }

            var level = spaceCount / spacesPerIndentLevel;

            if (level > lastIndentLevel)
                lastIndentLevel = level;
            else if (charIndex > 0)
                lastIndentLevel = level;

            return level;
        }
        private int GetMaxIndentLevel(DocumentLine docLine, int spacesPerIndentLevel, int tabSize, ref int lastIndentLevel)
        {
            if (_depthProvider.TryGetLineDepth(docLine.LineNumber, out var depth) && depth > 0)
                return depth;

            return GetIndentLevel(docLine, spacesPerIndentLevel, tabSize, ref lastIndentLevel);
        }

        private bool TryGetIndentGuideColumn(DocumentLine docLine, int spacesPerIndentLevel, int tabSize, int level, out int column)
        {
            var text = _document.GetText(docLine.Offset, docLine.Length);
            var targetSpaces = level * spacesPerIndentLevel;
            var spaceCount = 0;
            var charIndex = 0;

            while (charIndex < text.Length)
            {
                var ch = text[charIndex];

                if (ch == ' ')
                {
                    spaceCount += 1;
                }
                else if (ch == '\t')
                {
                    var step = tabSize - (spaceCount % tabSize);
                    if (step <= 0)
                        step = tabSize;

                    spaceCount += step;
                }
                else
                {
                    break;
                }

                charIndex += 1;

                if (spaceCount >= targetSpaces)
                {
                    column = charIndex + 1;
                    return true;
                }
            }

            column = 0;
            return false;
        }

        private System.Windows.Media.Pen? GetPen()
        {
            if (_pen is not null)
                return _pen;

            System.Windows.Media.Color baseColor;

            if (string.IsNullOrWhiteSpace(_guidesColor))
            {
                baseColor = _isDarkMode ? System.Windows.Media.Colors.White : System.Windows.Media.Colors.Black;
            }
            else
            {
                try
                {
                    baseColor = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(_guidesColor);
                }
                catch
                {
                    baseColor = _isDarkMode ? System.Windows.Media.Colors.White : System.Windows.Media.Colors.Black;
                }
            }

            var luminance = (baseColor.R * 299 + baseColor.G * 587 + baseColor.B * 114) / 1000;

            if (!_isDarkMode && luminance > 200)
                baseColor = System.Windows.Media.Colors.Black;

            if (_isDarkMode && luminance < 55)
                baseColor = System.Windows.Media.Colors.White;

            byte alpha = _isDarkMode ? (byte)120 : (byte)160;
            var c = System.Windows.Media.Color.FromArgb(alpha, baseColor.R, baseColor.G, baseColor.B);

            var brush = new SolidColorBrush(c);
            brush.Freeze();

            var pen = new System.Windows.Media.Pen(brush, 1.0);
            pen.Freeze();

            _pen = pen;
            return pen;
        }
    }
}
