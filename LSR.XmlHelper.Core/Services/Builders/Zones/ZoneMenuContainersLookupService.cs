using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace LSR.XmlHelper.Core.Services.Builders
{
    public sealed class ZoneMenuContainersLookupService
    {
        public IReadOnlyList<(string ZoneInternalName, string DealerMenuContainerId, string CustomerMenuContainerId)> GetZoneMenuContainers(string rootFolderPath, IReadOnlyCollection<string> zoneInternalNames)
        {
            if (string.IsNullOrWhiteSpace(rootFolderPath))
                return Array.Empty<(string ZoneInternalName, string DealerMenuContainerId, string CustomerMenuContainerId)>();

            if (!Directory.Exists(rootFolderPath))
                return Array.Empty<(string ZoneInternalName, string DealerMenuContainerId, string CustomerMenuContainerId)>();


            var wanted = new HashSet<string>(
    zoneInternalNames.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()),
    StringComparer.OrdinalIgnoreCase);
            if (wanted.Count == 0)
                return Array.Empty<(string ZoneInternalName, string DealerMenuContainerId, string CustomerMenuContainerId)>();

            var resolver = new LSR.XmlHelper.Core.Services.LsrFileSetResolverService();
            var resolved = resolver.ResolveZones(rootFolderPath, "Default");

            var byInternalName = new Dictionary<string, (string ZoneInternalName, string DealerMenuContainerId, string CustomerMenuContainerId)>(StringComparer.OrdinalIgnoreCase);

            foreach (var file in resolved.EnumerateReadOrder())
            {
                if (string.IsNullOrWhiteSpace(file) || !File.Exists(file))
                    continue;

                XDocument doc;

                try
                {
                    doc = XDocument.Load(file, LoadOptions.None);
                }
                catch
                {
                    continue;
                }

                foreach (var zone in doc.Descendants("Zone"))
                {
                    var internalName = ((string?)zone.Element("InternalGameName") ?? "").Trim();
                    if (string.IsNullOrWhiteSpace(internalName))
                        continue;

                    if (!wanted.Contains(internalName))
                        continue;

                    var dealerId = ((string?)zone.Element("DealerMenuContainerID") ?? "").Trim();
                    var customerId = ((string?)zone.Element("CustomerMenuContainerID") ?? "").Trim();

                    byInternalName[internalName] = (internalName, dealerId, customerId);
                }
            }

            return byInternalName.Values.ToArray();

        }
    }
}
