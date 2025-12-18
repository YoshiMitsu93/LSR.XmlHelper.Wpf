using System.Xml.Linq;

namespace LSR.XmlHelper.Core.Services
{
    public sealed class XmlFriendlyViewService
    {
        public XmlFriendlyDocument? TryBuild(string xmlText)
        {
            if (string.IsNullOrWhiteSpace(xmlText))
                return null;

            XDocument doc;
            try
            {
                doc = XDocument.Parse(xmlText, LoadOptions.PreserveWhitespace);
            }
            catch
            {
                return null;
            }

            var root = doc.Root;
            if (root is null)
                return null;

            var collections = new List<XmlFriendlyCollection>();

            var directChildren = root.Elements().ToList();
            if (directChildren.Count == 0)
                return null;

            var grouped = directChildren
                .GroupBy(e => e.Name.LocalName, StringComparer.OrdinalIgnoreCase)
                .ToList();

            foreach (var g in grouped)
            {
                var entries = new List<XmlFriendlyEntry>();
                var index = 0;

                foreach (var element in g)
                {
                    index++;

                    var key = ResolveKey(element, index);
                    var display = ResolveDisplay(element, key);

                    var fields = FlattenFields(element);

                    entries.Add(new XmlFriendlyEntry(key, display, element, fields));
                }

                var title = g.Key;
                collections.Add(new XmlFriendlyCollection(title, entries));
            }

            var primaryKey = grouped
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .FirstOrDefault() ?? grouped[0].Key;

            return new XmlFriendlyDocument(doc, collections, primaryKey);
        }

        public string ToXml(XmlFriendlyDocument document)
        {
            return document.Document.ToString(SaveOptions.DisableFormatting);
        }

        private static Dictionary<string, XmlFriendlyField> FlattenFields(XElement element)
        {
            var result = new Dictionary<string, XmlFriendlyField>(StringComparer.OrdinalIgnoreCase);

            void Walk(XElement el, string prefix)
            {
                foreach (var attr in el.Attributes())
                {
                    var key = string.IsNullOrEmpty(prefix)
                        ? $"@{attr.Name.LocalName}"
                        : $"{prefix}/@{attr.Name.LocalName}";

                    result[key] = new XmlFriendlyField(key, attr.Value, attr);
                }

                var children = el.Elements().ToList();

                if (children.Count == 0)
                {
                    if (!string.IsNullOrEmpty(prefix))
                        result[prefix] = new XmlFriendlyField(prefix, el.Value, el);

                    return;
                }

                var grouped = children.GroupBy(c => c.Name.LocalName, StringComparer.OrdinalIgnoreCase);

                foreach (var group in grouped)
                {
                    var list = group.ToList();
                    if (list.Count == 1)
                    {
                        var child = list[0];
                        var childPrefix = string.IsNullOrEmpty(prefix)
                            ? child.Name.LocalName
                            : $"{prefix}/{child.Name.LocalName}";

                        Walk(child, childPrefix);
                    }
                    else
                    {
                        var idx = 0;
                        foreach (var child in list)
                        {
                            idx++;

                            var childPrefix = string.IsNullOrEmpty(prefix)
                                ? $"{child.Name.LocalName}[{idx}]"
                                : $"{prefix}/{child.Name.LocalName}[{idx}]";

                            Walk(child, childPrefix);
                        }
                    }
                }
            }

            Walk(element, "");
            return result;
        }

        private static string ResolveKey(XElement element, int index)
        {
            var keyCandidates = new[]
            {
                "ID",
                "Id",
                "Key",
                "Name",
                "Type",
                "Hash",
                "Guid"
            };

            foreach (var candidate in keyCandidates)
            {
                var child = element.Elements().FirstOrDefault(e => string.Equals(e.Name.LocalName, candidate, StringComparison.OrdinalIgnoreCase));
                if (child is not null && !string.IsNullOrWhiteSpace(child.Value))
                    return child.Value.Trim();

                var attr = element.Attributes().FirstOrDefault(a => string.Equals(a.Name.LocalName, candidate, StringComparison.OrdinalIgnoreCase));
                if (attr is not null && !string.IsNullOrWhiteSpace(attr.Value))
                    return attr.Value.Trim();
            }

            var anyIdLike = element.Elements()
                .Select(e => e)
                .FirstOrDefault(e =>
                    e.Name.LocalName.EndsWith("ID", StringComparison.OrdinalIgnoreCase) &&
                    !string.IsNullOrWhiteSpace(e.Value));

            if (anyIdLike is not null)
                return anyIdLike.Value.Trim();

            return $"{element.Name.LocalName}[{index}]";
        }

        private static string ResolveDisplay(XElement element, string fallback)
        {
            var displayCandidates = new[]
            {
                "Name",
                "DisplayName",
                "Title",
                "Label",
                "ModelName",
                "GroupName",
                "TypeName",
                "ModItemName"
            };

            foreach (var candidate in displayCandidates)
            {
                var child = element.Elements().FirstOrDefault(e => string.Equals(e.Name.LocalName, candidate, StringComparison.OrdinalIgnoreCase));
                if (child is not null && !string.IsNullOrWhiteSpace(child.Value))
                    return child.Value.Trim();

                var attr = element.Attributes().FirstOrDefault(a => string.Equals(a.Name.LocalName, candidate, StringComparison.OrdinalIgnoreCase));
                if (attr is not null && !string.IsNullOrWhiteSpace(attr.Value))
                    return attr.Value.Trim();
            }

            var anyNameLike = element.Elements()
                .FirstOrDefault(e =>
                    e.Name.LocalName.EndsWith("Name", StringComparison.OrdinalIgnoreCase) &&
                    !string.IsNullOrWhiteSpace(e.Value));

            if (anyNameLike is not null)
                return anyNameLike.Value.Trim();

            var anyIdLike = element.Elements()
                .FirstOrDefault(e =>
                    e.Name.LocalName.EndsWith("ID", StringComparison.OrdinalIgnoreCase) &&
                    !string.IsNullOrWhiteSpace(e.Value));

            if (anyIdLike is not null)
                return anyIdLike.Value.Trim();

            return fallback;
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
        public XmlFriendlyEntry(string key, string display, XElement element, Dictionary<string, XmlFriendlyField> fields)
        {
            Key = key;
            Display = display;
            Element = element;
            Fields = fields;
        }

        public string Key { get; }
        public string Display { get; }
        public XElement Element { get; }
        public Dictionary<string, XmlFriendlyField> Fields { get; }

        public bool TrySetField(string fieldPath, string newValue, out string? error)
        {
            error = null;

            if (!Fields.TryGetValue(fieldPath, out var field))
            {
                error = "Field not found.";
                return false;
            }

            try
            {
                if (field.BoundTo is XAttribute attr)
                {
                    attr.Value = newValue;
                    field.Value = newValue;
                    return true;
                }

                if (field.BoundTo is XElement el)
                {
                    el.Value = newValue;
                    field.Value = newValue;
                    return true;
                }

                error = "Field cannot be updated.";
                return false;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }
    }

    public sealed class XmlFriendlyField
    {
        public XmlFriendlyField(string key, string value, XObject boundTo)
        {
            Key = key;
            Value = value;
            BoundTo = boundTo;
        }

        public string Key { get; }
        public string Value { get; set; }
        public XObject BoundTo { get; }
    }
}
