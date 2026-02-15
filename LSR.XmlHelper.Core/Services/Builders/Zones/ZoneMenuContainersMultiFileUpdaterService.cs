using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace LSR.XmlHelper.Core.Services.Builders
{
    public sealed class ZoneMenuContainersMultiFileUpdaterService
    {
        public IReadOnlyList<string> ApplyToFirstMatchingZonesFiles(string rootFolderPath, IReadOnlyCollection<string> zoneInternalNames, string dealerMenuContainerId, string customerMenuContainerId)
        {
            if (string.IsNullOrWhiteSpace(rootFolderPath))
                return Array.Empty<string>();

            if (!Directory.Exists(rootFolderPath))
                return Array.Empty<string>();

            var zonesFiles = GetCandidateZonesFiles(rootFolderPath);
            if (zonesFiles.Count == 0)
                return Array.Empty<string>();

            var wanted = new HashSet<string>(
                zoneInternalNames.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()),
                StringComparer.OrdinalIgnoreCase);

            if (wanted.Count == 0)
                return Array.Empty<string>();

            var updatedFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var handledZones = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var file in zonesFiles.AsEnumerable().Reverse())
            {
                if (handledZones.Count == wanted.Count)
                    break;

                XDocument doc;
                try
                {
                    doc = XDocument.Load(file, LoadOptions.None);
                }
                catch
                {
                    continue;
                }

                var changed = false;

                foreach (var zone in doc.Descendants("Zone"))
                {
                    var internalName = ((string?)zone.Element("InternalGameName") ?? "").Trim();
                    if (string.IsNullOrWhiteSpace(internalName))
                        continue;

                    if (!wanted.Contains(internalName))
                        continue;

                    if (handledZones.Contains(internalName))
                        continue;

                    SetOrCreate(zone, "DealerMenuContainerID", dealerMenuContainerId);
                    SetOrCreate(zone, "CustomerMenuContainerID", customerMenuContainerId);

                    changed = true;
                    handledZones.Add(internalName);
                }

                if (changed)
                {
                    try
                    {
                        doc.Save(file);
                        updatedFiles.Add(file);
                    }
                    catch
                    {
                    }
                }
            }

            return updatedFiles.ToList();
        }

        private static List<string> GetCandidateZonesFiles(string rootFolderPath)
        {
            var resolver = new LSR.XmlHelper.Core.Services.LsrFileSetResolverService();
            var resolved = resolver.ResolveZones(rootFolderPath, "Default");

            return resolved
                .EnumerateReadOrder()
                .Where(p => !string.IsNullOrWhiteSpace(p) && File.Exists(p))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private static void SetOrCreate(XElement parent, string childName, string value)
        {
            var existing = parent.Elements().FirstOrDefault(x => string.Equals(x.Name.LocalName, childName, StringComparison.OrdinalIgnoreCase));
            if (existing is not null)
            {
                existing.Value = value ?? "";
                return;
            }

            parent.Add(new XElement(childName, value ?? ""));
        }
    }
}
