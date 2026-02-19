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

            var before = peopleDoc.ToString(SaveOptions.DisableFormatting);
            var xmlIssues = new List<XmlFieldApplyIssue>();

            var peopleContainer = group.Element("DispatchablePeople") ?? group;

            if (!ReferenceEquals(peopleContainer, group))
            {
                var directPeople = group.Elements("DispatchablePerson").ToList();
                foreach (var p in directPeople)
                    p.Remove();
            }

            var existingPeople = peopleContainer.Elements("DispatchablePerson").ToList();
            foreach (var p in existingPeople)
                p.Remove();

            var entryList = entries.Where(x => x is not null).ToList();

            for (var i = 0; i < entryList.Count; i++)
            {
                var entryids = entryList[i];
                var person = new XElement("DispatchablePerson");

                foreach (var field in entryList[i].Fields)
                {
                    var name = (field?.Name ?? "").Trim();
                    if (string.IsNullOrWhiteSpace(name))
                        continue;

                    if (string.Equals(name, "ID", StringComparison.OrdinalIgnoreCase))
                        continue;

                    if (string.Equals(name, "DispatchablePersonGroupID", StringComparison.OrdinalIgnoreCase))
                        continue;

                    var desired = field?.Value ?? "";

                    if (field is not null && field.IsXml)
                    {
                        if (string.IsNullOrWhiteSpace(desired))
                            continue;

                        if (TryParseSingleElement(desired, out var parsed) && parsed is not null)
                            person.Add(parsed);
                        else
                            xmlIssues.Add(new XmlFieldApplyIssue(i, name));

                        continue;
                    }

                    person.Add(new XElement(name, desired));
                }

                peopleContainer.Add(person);
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
