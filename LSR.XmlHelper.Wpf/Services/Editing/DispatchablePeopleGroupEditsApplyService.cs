using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using LSR.XmlHelper.Wpf.ViewModels.Builders;

namespace LSR.XmlHelper.Wpf.Services.Editing
{
    public sealed class DispatchablePeopleGroupEditsApplyService
    {
        public (bool Updated, IReadOnlyList<XmlFieldApplyIssue> XmlIssues) Apply(XDocument peopleDoc, string groupId, IReadOnlyCollection<DispatchablePersonEntryViewModel> entries)
        {
            if (peopleDoc?.Root is null)
                return (false, Array.Empty<XmlFieldApplyIssue>());

            groupId = (groupId ?? "").Trim();
            if (string.IsNullOrWhiteSpace(groupId))
                return (false, Array.Empty<XmlFieldApplyIssue>());

            entries ??= Array.Empty<DispatchablePersonEntryViewModel>();

            var normalizedGroupId = groupId.Trim();

            var group = peopleDoc
                .Descendants("DispatchablePersonGroup")
                .FirstOrDefault(g =>
                {
                    var id = ((string?)g.Element("ID") ?? (string?)g.Element("DispatchablePersonGroupID") ?? "").Trim();
                    return string.Equals(id, normalizedGroupId, StringComparison.OrdinalIgnoreCase);
                });

            if (group is null)
                return (false, Array.Empty<XmlFieldApplyIssue>());

            var people = group.Descendants("DispatchablePerson").ToList();
            if (people.Count == 0)
                return (false, Array.Empty<XmlFieldApplyIssue>());

            var before = peopleDoc.ToString(SaveOptions.DisableFormatting);
            var xmlIssues = new List<XmlFieldApplyIssue>();

            foreach (var entry in entries)
            {
                if (entry is null)
                    continue;

                if (entry.SourceIndex < 0 || entry.SourceIndex >= people.Count)
                    continue;

                var person = people[entry.SourceIndex];

                foreach (var field in entry.Fields)
                {
                    var name = (field?.Name ?? "").Trim();
                    if (string.IsNullOrWhiteSpace(name))
                        continue;

                    if (string.Equals(name, "ID", StringComparison.OrdinalIgnoreCase))
                        continue;

                    if (string.Equals(name, "DispatchablePersonGroupID", StringComparison.OrdinalIgnoreCase))
                        continue;

                    var desired = (field?.Value ?? "");

                    var existing = person.Element(name);
                    if (existing is null)
                    {
                        if (field is not null && field.IsXml)
                        {
                            if (TryParseSingleElement(desired, out var parsed) && parsed is not null)
                                person.Add(parsed);
                            else
                                xmlIssues.Add(new XmlFieldApplyIssue(entry.SourceIndex, name));
                        }
                        else
                        {
                            person.Add(new XElement(name, desired));
                        }

                        continue;
                    }

                    if (field is not null && field.IsXml)
                    {
                        if (TryParseSingleElement(desired, out var parsed) && parsed is not null)
                        {
                            if (!XNode.DeepEquals(existing, parsed))
                                existing.ReplaceWith(parsed);
                        }
                        else
                        {
                            xmlIssues.Add(new XmlFieldApplyIssue(entry.SourceIndex, name));
                        }
                    }
                    else
                    {
                        if (!string.Equals(existing.Value ?? "", desired, StringComparison.Ordinal))
                            existing.Value = desired;
                    }
                }
            }

            var after = peopleDoc.ToString(SaveOptions.DisableFormatting);
            var updated = !string.Equals(before, after, StringComparison.Ordinal);
            return (updated, xmlIssues);
        }

        private static bool TryParseSingleElement(string xml, out XElement? element)
        {
            element = null;

            if (string.IsNullOrWhiteSpace(xml))
                return false;

            try
            {
                element = XElement.Parse(xml, LoadOptions.None);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
