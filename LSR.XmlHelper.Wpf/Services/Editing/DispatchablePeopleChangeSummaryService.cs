using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace LSR.XmlHelper.Wpf.Services.Editing
{
    public sealed class DispatchablePeopleChangeSummaryService
    {
        public IReadOnlyList<string> Summarize(string beforeXml, string afterXml, string groupId)
        {
            groupId = (groupId ?? "").Trim();

            if (string.IsNullOrWhiteSpace(beforeXml) || string.IsNullOrWhiteSpace(afterXml) || string.IsNullOrWhiteSpace(groupId))
                return Array.Empty<string>();

            XDocument beforeDoc;
            XDocument afterDoc;

            try
            {
                beforeDoc = XDocument.Parse(beforeXml, LoadOptions.None);
                afterDoc = XDocument.Parse(afterXml, LoadOptions.None);
            }
            catch
            {
                return Array.Empty<string>();
            }

            var beforeGroup = FindGroup(beforeDoc, groupId);
            var afterGroup = FindGroup(afterDoc, groupId);

            if (beforeGroup is null || afterGroup is null)
                return new[] { "DispatchablePeople: group '" + groupId + "' could not be compared" };

            var beforePeople = beforeGroup.Descendants("DispatchablePerson").ToDictionary(GetKey, x => x, StringComparer.OrdinalIgnoreCase);
            var afterPeople = afterGroup.Descendants("DispatchablePerson").ToDictionary(GetKey, x => x, StringComparer.OrdinalIgnoreCase);

            var lines = new List<string>();

            foreach (var key in afterPeople.Keys.OrderBy(x => x))
            {
                if (!beforePeople.TryGetValue(key, out var beforePerson))
                    continue;

                var afterPerson = afterPeople[key];

                var beforeFields = beforePerson.Elements().ToDictionary(e => e.Name.LocalName, e => (e.Value ?? "").Trim(), StringComparer.OrdinalIgnoreCase);
                var afterFields = afterPerson.Elements().ToDictionary(e => e.Name.LocalName, e => (e.Value ?? "").Trim(), StringComparer.OrdinalIgnoreCase);

                foreach (var field in afterFields.Keys.OrderBy(x => x))
                {
                    var afterValue = afterFields[field];

                    beforeFields.TryGetValue(field, out var beforeValue);
                    beforeValue ??= "";

                    if (string.Equals(field, "DebugName", StringComparison.OrdinalIgnoreCase))
                        continue;

                    if (!string.Equals(beforeValue, afterValue, StringComparison.Ordinal))
                        lines.Add("DispatchablePeople[" + groupId + "]/" + key + " " + field + ": '" + beforeValue + "' -> '" + afterValue + "'");
                }
            }

            if (lines.Count == 0)
                lines.Add("DispatchablePeople: no effective change for group '" + groupId + "'");

            return lines;
        }

        private static XElement? FindGroup(XDocument doc, string groupId)
        {
            return doc.Descendants("DispatchablePersonGroup")
                .FirstOrDefault(g =>
                {
                    var id = ((string?)g.Element("ID") ?? (string?)g.Element("DispatchablePersonGroupID") ?? "").Trim();
                    return string.Equals(id, groupId, StringComparison.OrdinalIgnoreCase);
                });
        }

        private static string GetKey(XElement person)
        {
            var key = ((string?)person.Element("DebugName") ?? "").Trim();
            return string.IsNullOrWhiteSpace(key) ? Guid.NewGuid().ToString("N") : key;
        }
    }
}
