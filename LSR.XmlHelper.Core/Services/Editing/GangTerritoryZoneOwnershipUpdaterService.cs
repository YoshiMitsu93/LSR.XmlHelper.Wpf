using System;
using System.Linq;
using System.Xml.Linq;

namespace LSR.XmlHelper.Core.Services.Editing
{
    public sealed class GangTerritoryZoneOwnershipUpdaterService
    {
        public bool Apply(XDocument territoriesDoc, string zoneInternalGameName, string gangId, bool shouldOwnZone)
        {
            if (territoriesDoc?.Root is null)
                return false;

            zoneInternalGameName = (zoneInternalGameName ?? "").Trim();
            gangId = (gangId ?? "").Trim();

            if (string.IsNullOrWhiteSpace(zoneInternalGameName) || string.IsNullOrWhiteSpace(gangId))
                return false;

            var root = territoriesDoc.Root;

            var matches = root
                .Elements("GangTerritory")
                .Where(t =>
                    string.Equals((((string?)t.Element("ZoneInternalGameName") ?? "").Trim()), zoneInternalGameName, StringComparison.OrdinalIgnoreCase)
                    && string.Equals((((string?)t.Element("GangID") ?? "").Trim()), gangId, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (!shouldOwnZone)
            {
                if (matches.Count == 0)
                    return false;

                foreach (var m in matches)
                    m.Remove();

                return true;
            }

            if (matches.Count > 0)
                return false;

            var priority = "0";
            var ambient = "100";

            var territory = new XElement("GangTerritory",
                new XElement("ZoneInternalGameName", zoneInternalGameName),
                new XElement("GangID", gangId),
                new XElement("Priority", priority),
                new XElement("AmbientSpawnChance", ambient));

            root.Add(territory);

            return true;
        }
    }
}
