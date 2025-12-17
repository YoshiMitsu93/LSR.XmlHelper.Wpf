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
            "ID",
            "InternalGameName",
            "ModelName"
        };

        private static readonly string[] DisplayCandidates =
        {
            "Name",
            "DisplayName",
            "FullName",
            "ShortName"
        };

        public XmlFriendlyDocument? TryBuild(string xml)
        {
            if (string.IsNullOrWhiteSpace(xml))
                return null;

            XDocument doc;

            try
            {
                doc = XDocument.Parse(xml, LoadOptions.PreserveWhitespace | LoadOptions.SetLineInfo);
            }
            catch
            {
                return null;
            }

            var collections = DiscoverCollections(doc);
            if (collections.Count == 0)
                return null;

            return new XmlFriendlyDocument(doc, collections);
        }

        public string ToXml(XmlFriendlyDocument document)
        {
            return document.Document.ToString(SaveOptions.DisableFormatting);
        }

        private static List<XmlFriendlyCollection> DiscoverCollections(XDocument doc)
        {
            var root = doc.Root;
            if (root is null)
                return new List<XmlFriendlyCollection>();

            var topLevelGroups = GroupRepeatedChildren(root);
            if (topLevelGroups.Count > 0)
                return topLevelGroups;

            var best = root.Elements()
                .Select(e => new { Groups = GroupRepeatedChildren(e) })
                .OrderByDescending(x => x.Groups.Sum(g => g.Entries.Count))
                .FirstOrDefault();

            return best?.Groups ?? new List<XmlFriendlyCollection>();
        }

        private static List<XmlFriendlyCollection> GroupRepeatedChildren(XElement parent)
        {
            var byName = parent.Elements()
                .GroupBy(e => e.Name.LocalName, StringComparer.OrdinalIgnoreCase)
                .Where(g => g.Count() >= 2)
                .ToList();

            var result = new List<XmlFriendlyCollection>();

            foreach (var group in byName)
            {
                var entries = group
                    .Select(BuildEntry)
                    .Where(e => e is not null)
                    .Cast<XmlFriendlyEntry>()
                    .ToList();

                if (entries.Count == 0)
                    continue;

                result.Add(new XmlFriendlyCollection(group.Key, entries));
            }

            return result;
        }

        private static XmlFriendlyEntry? BuildEntry(XElement element)
        {
            var leafElements = element.Elements()
                .Where(e => !e.HasElements)
                .ToList();

            var primaryFields = new Dictionary<string, XElement>(StringComparer.OrdinalIgnoreCase);
            var allFields = new Dictionary<string, XElement>(StringComparer.OrdinalIgnoreCase);

            foreach (var group in leafElements.GroupBy(e => e.Name.LocalName, StringComparer.OrdinalIgnoreCase))
            {
                var name = group.Key;
                var items = group.ToList();

                if (!primaryFields.ContainsKey(name))
                    primaryFields[name] = items[0];

                if (items.Count == 1)
                {
                    allFields[name] = items[0];
                    continue;
                }

                for (var i = 0; i < items.Count; i++)
                {
                    var indexedName = $"{name}[{i + 1}]";
                    allFields[indexedName] = items[i];
                }
            }

            var entryKey = PickFirstValue(primaryFields, KeyCandidates) ?? PickEndsWithId(primaryFields);
            var display = PickFirstValue(primaryFields, DisplayCandidates);

            if (string.IsNullOrWhiteSpace(display))
                display = entryKey;

            if (string.IsNullOrWhiteSpace(display))
                display = $"{element.Name.LocalName}";

            return new XmlFriendlyEntry(element, entryKey, display, allFields);
        }

        private static string? PickEndsWithId(Dictionary<string, XElement> fields)
        {
            foreach (var kv in fields)
            {
                if (kv.Key.EndsWith("ID", StringComparison.OrdinalIgnoreCase))
                {
                    var v = kv.Value.Value?.Trim();
                    if (!string.IsNullOrWhiteSpace(v))
                        return v;
                }
            }

            return null;
        }

        private static string? PickFirstValue(Dictionary<string, XElement> fields, string[] candidates)
        {
            foreach (var c in candidates)
            {
                if (!fields.TryGetValue(c, out var el))
                    continue;

                var v = el.Value?.Trim();
                if (!string.IsNullOrWhiteSpace(v))
                    return v;
            }

            return null;
        }
    }

    public sealed class XmlFriendlyDocument
    {
        internal XmlFriendlyDocument(XDocument document, IReadOnlyList<XmlFriendlyCollection> collections)
        {
            Document = document;
            Collections = collections;
        }

        public XDocument Document { get; }
        public IReadOnlyList<XmlFriendlyCollection> Collections { get; }
    }

    public sealed class XmlFriendlyCollection
    {
        public XmlFriendlyCollection(string title, IReadOnlyList<XmlFriendlyEntry> entries)
        {
            Title = title;
            Entries = entries;
        }

        public string Title { get; }
        public IReadOnlyList<XmlFriendlyEntry> Entries { get; }
    }

    public sealed class XmlFriendlyEntry
    {
        internal XmlFriendlyEntry(XElement element, string? key, string display, IReadOnlyDictionary<string, XElement> fields)
        {
            Element = element;
            Key = key;
            Display = display;
            Fields = fields;
        }

        public XElement Element { get; }
        public string? Key { get; }
        public string Display { get; }
        public IReadOnlyDictionary<string, XElement> Fields { get; }

        public bool TrySetField(string fieldName, string value)
        {
            if (!Fields.TryGetValue(fieldName, out var el))
                return false;

            el.Value = value ?? "";
            return true;
        }
    }
}
