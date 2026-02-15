using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace LSR.XmlHelper.Core.Services.Editing
{
    public sealed class GangTerritoriesReplaceService
    {
        public bool ReplaceForGang(XDocument territoriesDoc, string gangId, IReadOnlyCollection<string> zoneInternalNames)
        {
            if (territoriesDoc?.Root is null)
                return false;

            if (string.IsNullOrWhiteSpace(gangId))
                return false;

            zoneInternalNames ??= Array.Empty<string>();

            var root = territoriesDoc.Root;
            var normalizedGangId = gangId.Trim();

            var existing = root
                .Elements("GangTerritory")
                .Where(t => string.Equals(((string?)t.Element("GangID") ?? "").Trim(), normalizedGangId, StringComparison.OrdinalIgnoreCase))
                .ToList();

            var existingMetaByZone = existing
                .Select(t =>
                {
                    var zone = ((string?)t.Element("ZoneInternalGameName") ?? "").Trim();
                    var priority = ((string?)t.Element("Priority") ?? "").Trim();
                    var ambient = ((string?)t.Element("AmbientSpawnChance") ?? "").Trim();
                    return new
                    {
                        Zone = zone,
                        Priority = string.IsNullOrWhiteSpace(priority) ? "0" : priority,
                        Ambient = string.IsNullOrWhiteSpace(ambient) ? "100" : ambient
                    };
                })
                .Where(x => !string.IsNullOrWhiteSpace(x.Zone))
                .GroupBy(x => x.Zone, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

            foreach (var e in existing)
                e.Remove();

            var normalizedZones = zoneInternalNames
                .Select(z => (z ?? "").Trim())
                .Where(z => !string.IsNullOrWhiteSpace(z))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(z => z, StringComparer.OrdinalIgnoreCase)
                .ToList();

            foreach (var zone in normalizedZones)
            {
                var meta = existingMetaByZone.TryGetValue(zone, out var m)
                    ? m
                    : null;

                var territory = new XElement("GangTerritory",
                    new XElement("ZoneInternalGameName", zone),
                    new XElement("GangID", normalizedGangId),
                    new XElement("Priority", meta?.Priority ?? "0"),
                    new XElement("AmbientSpawnChance", meta?.Ambient ?? "100"));

                root.Add(territory);
            }

            return true;
        }
    }
}

