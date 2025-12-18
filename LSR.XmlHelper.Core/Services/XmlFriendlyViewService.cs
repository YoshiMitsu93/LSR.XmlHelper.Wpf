// LSR.XmlHelper.Core\Services\XmlFriendlyViewService.cs
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
                return null;

            var collections = DiscoverTopLevelCollections(doc);

            if (collections.Count == 0)
            {
                var single = new XmlFriendlyCollection(
                    title: doc.Root.Name.LocalName,
                    entries: new List<XmlFriendlyEntry> { BuildEntry(doc.Root, parentContext: null) }
                );
                collections.Add(single);
            }

            var primary = DeterminePrimaryCollectionKey(doc, collections) ?? collections[0].Title;

            return new XmlFriendlyDocument(doc, collections, primary);
        }

        public string ToXml(XmlFriendlyDocument friendly)
        {
            return friendly.Document.ToString(SaveOptions.DisableFormatting);
        }

        private static string? DeterminePrimaryCollectionKey(XDocument doc, List<XmlFriendlyCollection> collections)
        {
            var root = doc.Root;
            if (root == null)
                return null;

            var groups = root.Elements()
                .GroupBy(e => e.Name.LocalName, StringComparer.OrdinalIgnoreCase)
                .Select(g => new { Name = g.Key, Count = g.Count() })
                .Where(x => x.Count >= 2)
                .OrderByDescending(x => x.Count)
                .ThenBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (groups.Count == 0)
                return null;

            var rootPath = root.Name.LocalName;
            var desired = $"{rootPath}/{groups[0].Name}";

            var match = collections.FirstOrDefault(c => string.Equals(c.Title, desired, StringComparison.OrdinalIgnoreCase));
            return match?.Title;
        }

        private List<XmlFriendlyCollection> DiscoverTopLevelCollections(XDocument doc)
        {
            var root = doc.Root!;
            var collections = new List<XmlFriendlyCollection>();

            void AddGroupCollection(XElement parent, string parentPath)
            {
                var grouped = parent.Elements()
                    .GroupBy(e => e.Name.LocalName, StringComparer.OrdinalIgnoreCase)
                    .Select(g => new { Name = g.Key, Elements = g.ToList() })
                    .Where(x => x.Elements.Count >= 2)
                    .OrderByDescending(x => x.Elements.Count)
                    .ThenBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
                    .ToList();

                foreach (var g in grouped)
                {
                    var title = $"{parentPath}/{g.Name}";
                    var entries = g.Elements.Select(e => BuildEntry(e, FindAncestorContext(e))).ToList();
                    collections.Add(new XmlFriendlyCollection(title, entries));
                }
            }

            var rootPath = root.Name.LocalName;

            AddGroupCollection(root, rootPath);

            if (collections.Count == 0)
            {
                var onlyChild = root.Elements().FirstOrDefault();
                if (onlyChild != null && root.Elements().Skip(1).Any() == false)
                {
                    AddGroupCollection(onlyChild, $"{rootPath}/{onlyChild.Name.LocalName}");
                }
            }

            return collections
                .OrderByDescending(c => c.Entries.Count)
                .ThenBy(c => c.Title, StringComparer.OrdinalIgnoreCase)
                .ToList();
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
                        return v;
                }

                var attr = element.Attribute(name);
                if (attr != null)
                {
                    var v = (attr.Value ?? string.Empty).Trim();
                    if (!string.IsNullOrWhiteSpace(v))
                        return v;
                }
            }

            if (!element.HasElements)
            {
                var v = (element.Value ?? string.Empty).Trim();
                if (!string.IsNullOrWhiteSpace(v))
                    return v;
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
                        return v;
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
                    break;

                if (current.HasElements)
                {
                    foreach (var name in DisplayCandidates)
                    {
                        var match = current.Elements().FirstOrDefault(e => NameEquals(e, name));
                        if (match != null)
                        {
                            var v = (match.Value ?? string.Empty).Trim();
                            if (!string.IsNullOrWhiteSpace(v))
                                return v;
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

            void AddField(string key, XElement leaf)
            {
                if (!dict.ContainsKey(key))
                {
                    dict[key] = leaf;
                    return;
                }

                var i = 2;
                while (dict.ContainsKey($"{key} ({i})"))
                    i++;

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
                AddField("Value", element);

            return dict;
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
                    break;
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
