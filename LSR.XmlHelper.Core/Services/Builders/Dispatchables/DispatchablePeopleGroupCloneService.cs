using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace LSR.XmlHelper.Core.Services.Builders
{
    public sealed class DispatchablePeopleGroupCloneService
    {
        public (bool ok, string message, XDocument? doc) CloneToNewId(string rootFolderPath, string sourceGroupId, string newGroupId)
        {
            if (string.IsNullOrWhiteSpace(rootFolderPath) || !Directory.Exists(rootFolderPath))
                return (false, "Root folder is missing or invalid.", null);

            if (string.IsNullOrWhiteSpace(sourceGroupId))
                return (false, "Source DispatchablePersonGroupID is required.", null);

            if (string.IsNullOrWhiteSpace(newGroupId))
                return (false, "New DispatchablePersonGroupID is required.", null);

            var resolver = new LSR.XmlHelper.Core.Services.LsrFileSetResolverService();
            var resolved = resolver.ResolveDispatchablePeople(rootFolderPath, "Default");

            var files = resolved.EnumerateReadOrder()
                .Where(p => !string.IsNullOrWhiteSpace(p) && File.Exists(p))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            for (int i = files.Count - 1; i >= 0; i--)
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

                var group = doc
                    .Descendants("DispatchablePersonGroup")
                    .FirstOrDefault(x => string.Equals(((string?)x.Element("DispatchablePersonGroupID") ?? "").Trim(), sourceGroupId, StringComparison.OrdinalIgnoreCase));

                if (group is null)
                    continue;

                var cloned = new XElement(group);

                SetOrCreate(cloned, "DispatchablePersonGroupID", newGroupId);

                foreach (var person in cloned.Descendants("DispatchablePerson"))
                {
                    var groupName = person.Element("GroupName");
                    if (groupName is not null)
                        groupName.Value = newGroupId;
                }

                var root = doc.Root;
                if (root is null)
                    return (false, "DispatchablePeople xml root was missing.", null);

                var outDoc = new XDocument();
                if (doc.Declaration is not null)
                    outDoc.Declaration = new XDeclaration(doc.Declaration);

                outDoc.Add(new XElement(root.Name, cloned));

                return (true, "OK", outDoc);
            }

            return (false, $"Could not find DispatchablePersonGroupID '{sourceGroupId}' in any DispatchablePeople*.xml file.", null);
        }

        private static void SetOrCreate(XElement parent, string childName, string value)
        {
            var child = parent.Element(childName);
            if (child is null)
                parent.Add(new XElement(childName, value));
            else
                child.Value = value;
        }
    }
}
