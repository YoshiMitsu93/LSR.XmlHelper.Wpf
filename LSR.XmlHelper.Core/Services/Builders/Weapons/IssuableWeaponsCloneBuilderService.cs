using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace LSR.XmlHelper.Core.Services.Builders
{
    public sealed class IssuableWeaponsCloneBuilderService
    {
        public (XDocument doc, IReadOnlyDictionary<string, string> clonedIdsBySourceId, int clonedGroupsCount) CloneGroups(
            string rootFolderPath,
            IReadOnlyDictionary<string, string> desiredIdsBySourceId)
        {
            if (string.IsNullOrWhiteSpace(rootFolderPath) || !Directory.Exists(rootFolderPath))
                return (CreateEmptyRoot(), new Dictionary<string, string>(), 0);

            if (desiredIdsBySourceId is null || desiredIdsBySourceId.Count == 0)
                return (CreateEmptyRoot(), new Dictionary<string, string>(), 0);

            var files = EnumerateFiles(rootFolderPath, "IssuableWeapons*.xml").ToArray();

            XElement? templateRoot = null;

            var sourceGroupById = new Dictionary<string, XElement>(StringComparer.OrdinalIgnoreCase);

            for (int i = files.Length - 1; i >= 0; i--)
            {
                var file = files[i];
                XDocument doc;
                try
                {
                    doc = XDocument.Load(file, LoadOptions.None);
                }
                catch
                {
                    continue;
                }

                if (templateRoot is null && doc.Root is not null)
                    templateRoot = doc.Root;

                foreach (var group in doc.Descendants("IssuableWeaponsGroup"))
                {
                    var id = ((string?)group.Element("IssuableWeaponsID") ?? "").Trim();
                    if (string.IsNullOrWhiteSpace(id))
                        continue;

                    if (!desiredIdsBySourceId.ContainsKey(id))
                        continue;

                    if (sourceGroupById.ContainsKey(id))
                        continue;

                    sourceGroupById[id] = new XElement(group);
                }
            }

            if (sourceGroupById.Count == 0)
                return (CreateEmptyRoot(templateRoot), new Dictionary<string, string>(), 0);

            var existingIds = GetExistingGroupIds(files);

            var clonedIdsBySourceId = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var clonedGroups = new List<XElement>();

            foreach (var kvp in sourceGroupById)
            {
                var sourceId = kvp.Key;
                var clonedGroup = new XElement(kvp.Value);

                var desired = desiredIdsBySourceId.TryGetValue(sourceId, out var d) ? d : $"{sourceId}_CLONE";
                if (string.IsNullOrWhiteSpace(desired))
                    desired = $"{sourceId}_CLONE";

                var finalId = MakeUniqueId(desired, existingIds);
                existingIds.Add(finalId);

                SetOrCreate(clonedGroup, "IssuableWeaponsID", finalId);

                clonedIdsBySourceId[sourceId] = finalId;
                clonedGroups.Add(clonedGroup);
            }

            if (templateRoot is null)
                templateRoot = CreateEmptyRoot().Root;

            var outputRoot = new XElement(templateRoot!.Name);

            foreach (var attr in templateRoot.Attributes())
                outputRoot.SetAttributeValue(attr.Name, attr.Value);

            foreach (var g in clonedGroups)
                outputRoot.Add(g);

            return (new XDocument(outputRoot), clonedIdsBySourceId, clonedGroups.Count);
        }

        private static IEnumerable<string> EnumerateFiles(string rootFolderPath, string searchPattern)
        {
            var resolver = new LSR.XmlHelper.Core.Services.LsrFileSetResolverService();
            var resolved = resolver.ResolveIssuableWeapons(rootFolderPath, "Default");

            return resolved.EnumerateReadOrder()
                .Where(p => !string.IsNullOrWhiteSpace(p) && File.Exists(p))
                .Distinct(StringComparer.OrdinalIgnoreCase);
        }

        private static HashSet<string> GetExistingGroupIds(string[] files)
        {
            var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var file in files)
            {
                XDocument doc;
                try
                {
                    doc = XDocument.Load(file, LoadOptions.None);
                }
                catch
                {
                    continue;
                }

                foreach (var g in doc.Descendants("IssuableWeaponsGroup"))
                {
                    var id = (string?)g.Element("IssuableWeaponsID");
                    if (!string.IsNullOrWhiteSpace(id))
                        set.Add(id);
                }
            }

            return set;
        }

        private static string MakeUniqueId(string desired, HashSet<string> existing)
        {
            if (!existing.Contains(desired))
                return desired;

            var i = 2;
            while (true)
            {
                var next = $"{desired}{i}";
                if (!existing.Contains(next))
                    return next;

                i++;
            }
        }

        private static void SetOrCreate(XElement parent, string childName, string value)
        {
            var child = parent.Element(childName);
            if (child is null)
                parent.Add(new XElement(childName, value));
            else
                child.Value = value;
        }

        private static XDocument CreateEmptyRoot(XElement? templateRoot = null)
        {
            if (templateRoot is not null)
                return new XDocument(new XElement(templateRoot.Name));

            return new XDocument(
                new XElement("ArrayOfIssuableWeaponsGroup",
                    new XAttribute(XNamespace.Xmlns + "xsd", "http://www.w3.org/2001/XMLSchema"),
                    new XAttribute(XNamespace.Xmlns + "xsi", "http://www.w3.org/2001/XMLSchema-instance")));
        }
    }
}
