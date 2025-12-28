using System;
using System.Collections.Generic;
using System.Linq;
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

            var rootChildren = root.Elements().ToList();
            if (rootChildren.Count == 0)
                return null;

            var collections = new List<XmlFriendlyCollection>();
            var directEntries = new List<XElement>();

            foreach (var child in rootChildren)
            {
                var grandchildren = child.Elements().ToList();
                if (grandchildren.Count < 2)
                {
                    directEntries.Add(child);
                    continue;
                }

                var groupedGrand = grandchildren
                    .GroupBy(e => e.Name.LocalName, StringComparer.OrdinalIgnoreCase)
                    .ToList();

                var hasRepeatingGroup = groupedGrand.Any(g => g.Count() >= 2);
                if (!hasRepeatingGroup)
                {
                    directEntries.Add(child);
                    continue;
                }

                foreach (var g in groupedGrand.Where(g => g.Count() >= 2))
                {
                    var title = groupedGrand.Count == 1
                        ? child.Name.LocalName
                        : $"{child.Name.LocalName}/{g.Key}";

                    var entries = BuildEntries(g.ToList());
                    collections.Add(new XmlFriendlyCollection(title, entries));
                }
            }

            if (directEntries.Count > 0)
            {
                var groupedDirect = directEntries
                    .GroupBy(e => e.Name.LocalName, StringComparer.OrdinalIgnoreCase)
                    .ToList();

                foreach (var g in groupedDirect)
                {
                    var entries = BuildEntries(g.ToList());
                    collections.Add(new XmlFriendlyCollection(g.Key, entries));
                }
            }

            if (collections.Count == 0)
                return null;

            var primaryKey = collections
                .OrderByDescending(c => c.Entries.Count)
                .Select(c => c.Title)
                .FirstOrDefault() ?? collections[0].Title;

            return new XmlFriendlyDocument(doc, collections, primaryKey);
        }

        public string ToXml(XmlFriendlyDocument document)
        {
            return document.Document.ToString(SaveOptions.DisableFormatting);
        }

        public bool TryDuplicateEntry(
        XmlFriendlyDocument document,
        XmlFriendlyEntry sourceEntry,
        bool insertAfter,
        out XmlFriendlyEntry? duplicatedEntry,
        out string? error)
        {
            duplicatedEntry = null;
            error = null;

            if (document is null)
            {
                error = "Document is null.";
                return false;
            }

            if (sourceEntry is null)
            {
                error = "Source entry is null.";
                return false;
            }

            var sourceElement = sourceEntry.Element;
            var parent = sourceElement.Parent;

            if (parent is null)
            {
                error = "Cannot duplicate. Entry has no parent in the XML.";
                return false;
            }

            var clone = new XElement(sourceElement);

            try
            {
                if (insertAfter)
                    AddAfterPreservingWhitespace(sourceElement, clone);
                else
                    AddToEndPreservingWhitespace(parent, clone);
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }

            try
            {
                var siblings = parent.Elements(sourceElement.Name).ToList();
                var siblingIndex = siblings.IndexOf(clone);
                var displayIndex = siblingIndex >= 0 ? siblingIndex + 1 : siblings.Count;
                var key = ResolveKey(clone, displayIndex);

                duplicatedEntry = new XmlFriendlyEntry(key, string.Empty, clone);
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        private static void AddAfterPreservingWhitespace(XElement sourceElement, XElement clone)
        {
            var wsAfter = sourceElement.NextNode as XText;

            if (wsAfter is not null && string.IsNullOrWhiteSpace(wsAfter.Value))
            {
                if (wsAfter.NextNode is XElement)
                {
                    wsAfter.AddAfterSelf(clone, new XText(wsAfter.Value));
                    return;
                }

                sourceElement.AddAfterSelf(new XText(wsAfter.Value), clone);
                return;
            }

            var wsBefore = sourceElement.PreviousNode as XText;
            var separator = (wsBefore is not null && string.IsNullOrWhiteSpace(wsBefore.Value))
                ? wsBefore.Value
                : Environment.NewLine;

            sourceElement.AddAfterSelf(new XText(separator), clone);

            if (clone.NextNode is XElement)
                clone.AddAfterSelf(new XText(separator));
        }

        private static void AddToEndPreservingWhitespace(XElement parent, XElement clone)
        {
            var trailing = parent.LastNode as XText;

            if (trailing is not null && string.IsNullOrWhiteSpace(trailing.Value))
            {
                trailing.AddBeforeSelf(new XText(trailing.Value), clone);
                return;
            }

            parent.Add(clone);
        }


        private static List<XmlFriendlyEntry> BuildEntries(List<XElement> elements)
        {
            var entries = new List<XmlFriendlyEntry>(elements.Count);
            var index = 0;

            foreach (var element in elements)
            {
                index++;

                var key = ResolveKey(element, index);
                var display = string.Empty;

                entries.Add(new XmlFriendlyEntry(key, display, element));
            }

            return entries;
        }

        internal static Dictionary<string, XmlFriendlyField> FlattenFields(XElement element)
        {
            var result = new Dictionary<string, XmlFriendlyField>(StringComparer.OrdinalIgnoreCase);

            void Walk(XElement el, string prefix)
            {
                var basePrefix = string.IsNullOrEmpty(prefix) ? "" : prefix + "/";

                foreach (var attr in el.Attributes())
                {
                    var key = $"{basePrefix}@{attr.Name.LocalName}";
                    result[key] = new XmlFriendlyField(key, attr.Value, attr);
                }

                var hasAnyChild = false;
                var counts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

                foreach (var child in el.Elements())
                {
                    hasAnyChild = true;

                    var name = child.Name.LocalName;
                    counts.TryGetValue(name, out var c);
                    counts[name] = c + 1;
                }

                if (!hasAnyChild)
                {
                    if (string.IsNullOrEmpty(prefix))
                    {
                        var key = el.Name.LocalName;
                        result[key] = new XmlFriendlyField(key, el.Value, el);
                    }
                    else
                    {
                        result[prefix] = new XmlFriendlyField(prefix, el.Value, el);
                    }

                    return;
                }

                Dictionary<string, int>? multiIndex = null;

                foreach (var child in el.Elements())
                {
                    var name = child.Name.LocalName;

                    if (counts[name] == 1)
                    {
                        var childPrefix = string.IsNullOrEmpty(prefix)
                            ? name
                            : $"{prefix}/{name}";

                        Walk(child, childPrefix);
                        continue;
                    }

                    multiIndex ??= new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

                    multiIndex.TryGetValue(name, out var idx);
                    idx++;
                    multiIndex[name] = idx;

                    var indexedName = $"{name}[{idx}]";
                    var childPrefix2 = string.IsNullOrEmpty(prefix)
                        ? indexedName
                        : $"{prefix}/{indexedName}";

                    Walk(child, childPrefix2);
                }
            }

            Walk(element, "");
            return result;
        }

        private static string ResolveKey(XElement element, int index)
        {
            var best = FindBestIdentifier(element, preferNameLike: false);
            if (!string.IsNullOrWhiteSpace(best))
                return best;

            var anyIdLike = element.Elements()
                .FirstOrDefault(e =>
                    e.Name.LocalName.EndsWith("ID", StringComparison.OrdinalIgnoreCase) &&
                    !string.IsNullOrWhiteSpace(e.Value));

            if (anyIdLike is not null)
                return anyIdLike.Value.Trim();

            return $"{element.Name.LocalName}[{index}]";
        }

        internal static string ResolveDisplay(XElement element, string fallback)
        {
            var bestName = FindBestIdentifier(element, preferNameLike: true);
            if (!string.IsNullOrWhiteSpace(bestName))
                return bestName;

            var bestId = FindBestIdentifier(element, preferNameLike: false);
            if (!string.IsNullOrWhiteSpace(bestId))
                return bestId;

            return fallback;
        }

        private static string? FindBestIdentifier(XElement element, bool preferNameLike)
        {
            static IEnumerable<(string name, string value)> EnumerateCandidates(XElement el)
            {
                foreach (var a in el.Attributes())
                {
                    var name = a.Name.LocalName;
                    var value = a.Value;
                    if (!string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(value))
                        yield return (name, value.Trim());
                }

                foreach (var c in el.Elements())
                {
                    if (c.HasElements)
                        continue;

                    var name = c.Name.LocalName;
                    var value = c.Value;
                    if (!string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(value))
                        yield return (name, value.Trim());
                }

                if (!el.HasElements)
                {
                    var value = el.Value;
                    if (!string.IsNullOrWhiteSpace(value))
                        yield return (el.Name.LocalName, value.Trim());
                }
            }

            static bool LooksNumeric(string v)
            {
                if (string.IsNullOrWhiteSpace(v))
                    return true;

                var hasDigit = false;
                foreach (var ch in v)
                {
                    if (char.IsDigit(ch))
                    {
                        hasDigit = true;
                        continue;
                    }

                    if (ch == '.' || ch == '-' || ch == '+' || ch == ',')
                        continue;

                    return false;
                }

                return hasDigit;
            }

            static bool LooksLikeNiceName(string v)
            {
                if (string.IsNullOrWhiteSpace(v))
                    return false;

                if (!v.Any(char.IsLetter))
                    return false;

                if (v.Length < 2 || v.Length > 120)
                    return false;

                return true;
            }

            static int NamePriority(string fieldUpper, bool preferName)
            {
                if (preferName)
                {
                    if (fieldUpper == "FULLNAME")
                        return 5000;
                    if (fieldUpper == "DISPLAYNAME")
                        return 4500;
                    if (fieldUpper == "NAME")
                        return 4000;
                    if (fieldUpper == "TITLE")
                        return 3800;
                    if (fieldUpper == "DESCRIPTION")
                        return 3600;
                    if (fieldUpper == "LABEL")
                        return 3400;
                    if (fieldUpper == "GROUPNAME")
                        return 3300;
                    if (fieldUpper == "MODITEMNAME")
                        return 3200;

                    if (fieldUpper.EndsWith("DISPLAYNAME", StringComparison.Ordinal))
                        return 3000;
                    if (fieldUpper.EndsWith("FULLNAME", StringComparison.Ordinal))
                        return 2950;
                    if (fieldUpper.EndsWith("NAME", StringComparison.Ordinal))
                        return 2800;
                    if (fieldUpper.EndsWith("TITLE", StringComparison.Ordinal))
                        return 2700;
                    if (fieldUpper.EndsWith("DESCRIPTION", StringComparison.Ordinal))
                        return 2600;

                    if (fieldUpper == "AGENCYID" || fieldUpper.EndsWith("AGENCYID", StringComparison.Ordinal))
                        return 3500;

                    if (fieldUpper == "INTERNALGAMENAME" || fieldUpper.EndsWith("INTERNALGAMENAME", StringComparison.Ordinal))
                        return -2500;

                    if (fieldUpper == "ZONEINTERNALGAMENAME" || fieldUpper.EndsWith("ZONEINTERNALGAMENAME", StringComparison.Ordinal))
                        return -2500;

                    if (fieldUpper == "TYPENAME" || fieldUpper.EndsWith("TYPENAME", StringComparison.Ordinal))
                        return -2000;

                    if (fieldUpper == "MEASUREMENTNAME" || fieldUpper.EndsWith("MEASUREMENTNAME", StringComparison.Ordinal))
                        return -1800;

                    if (fieldUpper.Contains("TEMPERATURE", StringComparison.Ordinal) ||
                        fieldUpper.Contains("WINDSPEED", StringComparison.Ordinal) ||
                        fieldUpper.Contains("WINDDIRECTION", StringComparison.Ordinal) ||
                        fieldUpper.Contains("DATETIME", StringComparison.Ordinal))
                        return -1600;

                    if (fieldUpper == "ID" || fieldUpper.EndsWith("ID", StringComparison.Ordinal) || fieldUpper.Contains("KEY", StringComparison.Ordinal))
                        return 800;
                }
                else
                {
                    if (fieldUpper == "ID")
                        return 5000;
                    if (fieldUpper.EndsWith("ID", StringComparison.Ordinal))
                        return 4200;
                    if (fieldUpper == "KEY" || fieldUpper.EndsWith("KEY", StringComparison.Ordinal))
                        return 3800;
                    if (fieldUpper.Contains("GUID", StringComparison.Ordinal))
                        return 3400;
                    if (fieldUpper.Contains("HASH", StringComparison.Ordinal))
                        return 3200;

                    if (fieldUpper == "TYPENAME" || fieldUpper.EndsWith("TYPENAME", StringComparison.Ordinal))
                        return -1200;

                    if (fieldUpper == "MEASUREMENTNAME" || fieldUpper.EndsWith("MEASUREMENTNAME", StringComparison.Ordinal))
                        return -1200;
                }

                return 0;
            }

            static int Score(string fieldName, string value, bool preferName)
            {
                if (string.IsNullOrWhiteSpace(fieldName) || string.IsNullOrWhiteSpace(value))
                    return int.MinValue;

                if (value.Length > 500)
                    return -5000;

                var fieldUpper = fieldName.ToUpperInvariant();
                var score = 0;

                score += NamePriority(fieldUpper, preferName);

                if (preferName)
                {
                    if (LooksLikeNiceName(value))
                        score += 600;

                    if (LooksNumeric(value))
                        score -= 900;
                }
                else
                {
                    if (LooksNumeric(value))
                        score += 150;
                }

                if (value.Length >= 3 && value.Length <= 120)
                    score += 50;

                if (fieldUpper.StartsWith("IS", StringComparison.Ordinal))
                    score -= 800;

                if (fieldUpper.Contains("ENABLED", StringComparison.Ordinal) ||
                    fieldUpper.Contains("DISABLED", StringComparison.Ordinal) ||
                    fieldUpper.Contains("FLAGS", StringComparison.Ordinal))
                    score -= 800;

                return score;
            }

            (string value, int score) best = ("", int.MinValue);

            foreach (var (name, value) in EnumerateCandidates(element))
            {
                var s = Score(name, value, preferNameLike);
                if (s > best.score)
                    best = (value, s);
            }

            return string.IsNullOrWhiteSpace(best.value) ? null : best.value;
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
        private Dictionary<string, XmlFriendlyField>? _fields;
        private string? _display;

        public XmlFriendlyEntry(string key, string display, XElement element)
        {
            Key = key;
            _display = string.IsNullOrWhiteSpace(display) ? null : display;
            Element = element;
        }

        public string Key { get; }
        public string Display => _display ??= XmlFriendlyViewService.ResolveDisplay(Element, Key);
        public XElement Element { get; }

        public Dictionary<string, XmlFriendlyField> Fields
        {
            get
            {
                _fields ??= XmlFriendlyViewService.FlattenFields(Element);
                return _fields;
            }
        }

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
