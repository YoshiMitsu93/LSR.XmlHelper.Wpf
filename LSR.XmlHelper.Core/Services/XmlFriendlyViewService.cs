using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace LSR.XmlHelper.Core.Services
{
    public sealed class XmlFriendlyViewService
    {
        private static readonly string[] KeyCandidates =
        {
            "ID", "Id", "Key", "Guid",
            "Name", "FullName", "ShortName", "DisplayName",
            "ModItemName", "ContactName", "PlayerName",
            "ModelName", "ModelHash",
            "GroupName", "TypeName"
        };

        private static readonly string[] DisplayCandidates =
        {
            "FullName", "DisplayName", "ShortName", "Name",
            "ModItemName", "ContactName", "PlayerName",
            "GroupName", "TypeName",
            "ModelName"
        };

        public XmlFriendlyDocument? TryBuild(string xmlText)
        {
            XDocument doc;
            try
            {
                doc = XDocument.Parse(xmlText, LoadOptions.PreserveWhitespace | LoadOptions.SetLineInfo);
            }
            catch
            {
                return null;
            }

            if (doc.Root == null)
            {
                return null;
            }

            var collections = DiscoverCollections(doc);

            if (collections.Count == 0)
            {
                var single = new XmlFriendlyCollection(
                    title: doc.Root.Name.LocalName,
                    entries: new List<XmlFriendlyEntry> { BuildEntry(doc.Root, parentContext: null) }
                );
                collections.Add(single);
            }

            var primary = collections[0].Title;

            return new XmlFriendlyDocument(doc, collections, primary);
        }

        public string ToXml(XmlFriendlyDocument friendly)
        {
            return friendly.Document.ToString(SaveOptions.DisableFormatting);
        }

        private List<XmlFriendlyCollection> DiscoverCollections(XDocument doc)
        {
            var root = doc.Root!;
            var collectionsByTitle = new Dictionary<string, XmlFriendlyCollection>(StringComparer.OrdinalIgnoreCase);

            void AddOrMergeCollection(string title, IEnumerable<XElement> elements)
            {
                var entries = elements.Select(e => BuildEntry(e, FindAncestorContext(e))).ToList();

                if (collectionsByTitle.TryGetValue(title, out var existing))
                {
                    existing.Entries.AddRange(entries);
                    return;
                }

                collectionsByTitle[title] = new XmlFriendlyCollection(title, entries);
            }

            void Visit(XElement node, string path, bool isDirectRootChild)
            {
                var children = node.Elements().ToList();
                if (children.Count == 0)
                {
                    return;
                }

                var groups = children.GroupBy(e => e.Name.LocalName).ToList();

                foreach (var g in groups)
                {
                    var groupElements = g.ToList();
                    if (groupElements.Count >= 2)
                    {
                        var title = $"{path}/{g.Key}";
                        AddOrMergeCollection(title, groupElements);
                    }
                    else if (isDirectRootChild)
                    {
                        var only = groupElements[0];
                        if (only.HasElements || !string.IsNullOrWhiteSpace(only.Value))
                        {
                            var title = $"{path}/{g.Key}";
                            AddOrMergeCollection(title, groupElements);
                        }
                    }
                }

                foreach (var child in children)
                {
                    Visit(child, $"{path}/{child.Name.LocalName}", isDirectRootChild: false);
                }
            }

            var rootPath = root.Name.LocalName;

            foreach (var child in root.Elements().ToList())
            {
                Visit(child, $"{rootPath}/{child.Name.LocalName}", isDirectRootChild: true);
            }

            var ordered = collectionsByTitle.Values
                .OrderByDescending(c => c.Entries.Count)
                .ThenBy(c => c.Title, StringComparer.OrdinalIgnoreCase)
                .ToList();

            return ordered;
        }

        private XmlFriendlyEntry BuildEntry(XElement element, string? parentContext)
        {
            var key = ResolveKey(element);
            var display = ResolveDisplay(element);

            if (!string.IsNullOrWhiteSpace(parentContext))
            {
                var trimmed = display.Trim();
                if (string.Equals(trimmed, element.Name.LocalName, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(trimmed, "string", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(trimmed, "value", StringComparison.OrdinalIgnoreCase))
                {
                    display = $"{parentContext} / {trimmed}";
                }
            }

            var fields = BuildFields(element);

            return new XmlFriendlyEntry(key, display, element, fields);
        }

        private string ResolveKey(XElement element)
        {
            foreach (var name in KeyCandidates)
            {
                var match = element.Elements().FirstOrDefault(e => NameEquals(e, name));
                if (match != null)
                {
                    var v = (match.Value ?? string.Empty).Trim();
                    if (!string.IsNullOrWhiteSpace(v))
                    {
                        return v;
                    }
                }

                var attr = element.Attribute(name);
                if (attr != null)
                {
                    var v = (attr.Value ?? string.Empty).Trim();
                    if (!string.IsNullOrWhiteSpace(v))
                    {
                        return v;
                    }
                }
            }

            if (!element.HasElements)
            {
                var v = (element.Value ?? string.Empty).Trim();
                if (!string.IsNullOrWhiteSpace(v))
                {
                    return v;
                }
            }

            return Guid.NewGuid().ToString("N");
        }

        private string ResolveDisplay(XElement element)
        {
            if (!element.HasElements)
            {
                var v = (element.Value ?? string.Empty).Trim();
                return string.IsNullOrWhiteSpace(v) ? element.Name.LocalName : v;
            }

            foreach (var name in DisplayCandidates)
            {
                var match = element.Elements().FirstOrDefault(e => NameEquals(e, name));
                if (match != null)
                {
                    var v = (match.Value ?? string.Empty).Trim();
                    if (!string.IsNullOrWhiteSpace(v))
                    {
                        return v;
                    }
                }
            }

            return element.Name.LocalName;
        }

        private string? FindAncestorContext(XElement element)
        {
            var current = element.Parent;
            while (current != null)
            {
                if (current.Parent == null)
                {
                    break;
                }

                if (current.HasElements)
                {
                    foreach (var name in DisplayCandidates)
                    {
                        var match = current.Elements().FirstOrDefault(e => NameEquals(e, name));
                        if (match != null)
                        {
                            var v = (match.Value ?? string.Empty).Trim();
                            if (!string.IsNullOrWhiteSpace(v))
                            {
                                return v;
                            }
                        }
                    }
                }

                current = current.Parent;
            }

            return null;
        }

        private Dictionary<string, XElement> BuildFields(XElement element)
        {
            if (!element.HasElements)
            {
                return new Dictionary<string, XElement>(StringComparer.OrdinalIgnoreCase)
                {
                    ["Value"] = element
                };
            }

            var dict = new Dictionary<string, XElement>(StringComparer.OrdinalIgnoreCase);
            var stopNodes = new HashSet<XElement>();

            foreach (var n in element.Descendants().Where(HasRepeatedChildren))
            {
                stopNodes.Add(n);
            }

            void AddField(string key, XElement leaf)
            {
                if (!dict.ContainsKey(key))
                {
                    dict[key] = leaf;
                    return;
                }

                var i = 2;
                while (dict.ContainsKey($"{key} ({i})"))
                {
                    i++;
                }
                dict[$"{key} ({i})"] = leaf;
            }

            void Walk(XElement node, string path)
            {
                foreach (var child in node.Elements())
                {
                    var indexedName = GetIndexedName(child);

                    var childPath = string.IsNullOrEmpty(path)
                        ? indexedName
                        : $"{path}/{indexedName}";

                    if (stopNodes.Contains(child))
                    {
                        var count = child.Elements().Count();
                        if (count > 0)
                        {
                            var synthetic = new XElement(child.Name, count.ToString());
                            AddField($"{childPath}/Count", synthetic);
                        }
                        continue;
                    }

                    if (!child.HasElements)
                    {
                        AddField(childPath, child);
                        continue;
                    }

                    Walk(child, childPath);
                }
            }

            Walk(element, "");

            if (dict.Count == 0)
            {
                AddField("Value", element);
            }

            return dict;
        }

        private static bool HasRepeatedChildren(XElement element)
        {
            var children = element.Elements().ToList();
            if (children.Count < 2) return false;

            return children.GroupBy(e => e.Name.LocalName).Any(g => g.Count() >= 2);
        }

        private static string GetIndexedName(XElement element)
        {
            var parent = element.Parent;
            if (parent == null) return element.Name.LocalName;

            var same = parent.Elements(element.Name).ToList();
            if (same.Count <= 1) return element.Name.LocalName;

            var index = 1;
            foreach (var s in same)
            {
                if (ReferenceEquals(s, element))
                {
                    break;
                }
                index++;
            }

            return $"{element.Name.LocalName}[{index}]";
        }

        private static bool NameEquals(XElement element, string candidate)
        {
            return string.Equals(element.Name.LocalName, candidate, StringComparison.OrdinalIgnoreCase);
        }
    }

    public sealed class XmlFriendlyDocument
    {
        public XmlFriendlyDocument(XDocument document, List<XmlFriendlyCollection> collections, string primaryCollectionKey)
        {
            Document = document;
            Collections = collections;
            PrimaryCollectionKey = primaryCollectionKey;
        }

        public XDocument Document { get; }
        public List<XmlFriendlyCollection> Collections { get; }
        public string PrimaryCollectionKey { get; }
    }

    public sealed class XmlFriendlyCollection
    {
        public XmlFriendlyCollection(string title, List<XmlFriendlyEntry> entries)
        {
            Title = title;
            Entries = entries;
        }

        public string Title { get; }
        public List<XmlFriendlyEntry> Entries { get; }
    }

    public sealed class XmlFriendlyEntry
    {
        public XmlFriendlyEntry(string key, string display, XElement element, Dictionary<string, XElement> fields)
        {
            Key = key;
            Display = display;
            Element = element;
            Fields = fields;
        }

        public string Key { get; }
        public string Display { get; }
        public XElement Element { get; }
        public Dictionary<string, XElement> Fields { get; }

        public bool TrySetField(string fieldKey, string newValue, out string? error)
        {
            error = null;

            if (!Fields.TryGetValue(fieldKey, out var el))
            {
                error = $"Field '{fieldKey}' was not found.";
                return false;
            }

            try
            {
                el.Value = newValue ?? string.Empty;
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }
    }
}
