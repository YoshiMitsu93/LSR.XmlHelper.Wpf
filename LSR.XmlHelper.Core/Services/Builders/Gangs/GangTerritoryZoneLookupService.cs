using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace LSR.XmlHelper.Core.Services.Builders
{
    public sealed class GangTerritoryZoneLookupService
    {
        public IReadOnlyList<string> GetZoneInternalNamesForGang(string rootFolderPath, string gangId)
        {
            gangId = (gangId ?? "").Trim();

            if (string.IsNullOrWhiteSpace(rootFolderPath) || string.IsNullOrWhiteSpace(gangId))
                return Array.Empty<string>();

            if (!Directory.Exists(rootFolderPath))
                return Array.Empty<string>();

            var resolver = new LSR.XmlHelper.Core.Services.LsrFileSetResolverService();
            var resolved = resolver.ResolveGangTerritories(rootFolderPath, "Default");

            var winnerGangByZone = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

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

                foreach (var t in doc.Descendants("GangTerritory"))
                {
                    var zone = ((string?)t.Element("ZoneInternalGameName") ?? "").Trim();
                    if (string.IsNullOrWhiteSpace(zone))
                        continue;

                    var gid = ((string?)t.Element("GangID") ?? "").Trim();
                    winnerGangByZone[zone] = gid;
                }
            }

            return winnerGangByZone
                .Where(kvp => string.Equals(kvp.Value, gangId, StringComparison.OrdinalIgnoreCase))
                .Select(kvp => kvp.Key)
                .Where(z => !string.IsNullOrWhiteSpace(z))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(z => z, StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }
    }
}
