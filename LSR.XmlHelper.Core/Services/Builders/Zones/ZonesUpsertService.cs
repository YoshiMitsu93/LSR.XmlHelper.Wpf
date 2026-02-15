using LSR.XmlHelper.Core.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace LSR.XmlHelper.Core.Services.Builders.Zones
{
    public sealed class ZonesUpsertService
    {
        public IReadOnlyList<string> UpsertZonesIntoWinnerFile(
            string rootFolderPath,
            IReadOnlyCollection<string> zoneInternalNamesToApplyMenusTo,
            IReadOnlyCollection<ZoneDefinition> customZoneDefinitions,
            string dealerMenuContainerId,
            string customerMenuContainerId,
            bool allowCreateZonesFile)
        {
            if (string.IsNullOrWhiteSpace(rootFolderPath))
                return Array.Empty<string>();

            if (!Directory.Exists(rootFolderPath))
                return Array.Empty<string>();

            var resolver = new LsrFileSetResolverService();
            var resolved = resolver.ResolveZones(rootFolderPath, "Default");

            var winnerPath = resolved.BasePath;

            if (string.IsNullOrWhiteSpace(winnerPath))
            {
                if (!allowCreateZonesFile)
                    return Array.Empty<string>();

                winnerPath = Path.Combine(rootFolderPath, "Zones.xml");
            }

            var wantedNames = new HashSet<string>(
                (zoneInternalNamesToApplyMenusTo ?? Array.Empty<string>())
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(x => x.Trim()),
                StringComparer.OrdinalIgnoreCase);

            var customByName = (customZoneDefinitions ?? Array.Empty<ZoneDefinition>())
                .Where(z => z is not null && !string.IsNullOrWhiteSpace(z.InternalGameName))
                .GroupBy(z => z.InternalGameName.Trim(), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

            foreach (var k in customByName.Keys)
                wantedNames.Add(k);

            if (wantedNames.Count == 0)
                return Array.Empty<string>();

            var doc = LoadOrCreateZonesDocument(winnerPath, allowCreateZonesFile);
            if (doc.Root is null)
                return Array.Empty<string>();

            var changed = false;

            foreach (var wanted in wantedNames)
            {
                var existingZone = doc
                    .Descendants("Zone")
                    .FirstOrDefault(z =>
                        string.Equals(((string?)z.Element("InternalGameName") ?? "").Trim(), wanted, StringComparison.OrdinalIgnoreCase));

                if (existingZone is not null)
                {
                    if (!string.IsNullOrWhiteSpace(dealerMenuContainerId))
                        SetOrCreate(existingZone, "DealerMenuContainerID", dealerMenuContainerId);

                    if (!string.IsNullOrWhiteSpace(customerMenuContainerId))
                        SetOrCreate(existingZone, "CustomerMenuContainerID", customerMenuContainerId);

                    changed = true;
                    continue;
                }

                if (!customByName.TryGetValue(wanted, out var def))
                    continue;

                if (def.Boundaries is null || def.Boundaries.Count < 3)
                    continue;

                var newZone = BuildZoneElement(def, dealerMenuContainerId, customerMenuContainerId);
                doc.Root.Add(newZone);
                changed = true;
            }

            if (!changed)
                return Array.Empty<string>();

            try
            {
                doc.Save(winnerPath);
                return new[] { winnerPath };
            }
            catch
            {
                return Array.Empty<string>();
            }
        }

        private static XDocument LoadOrCreateZonesDocument(string winnerPath, bool allowCreateZonesFile)
        {
            if (File.Exists(winnerPath))
            {
                try
                {
                    return XDocument.Load(winnerPath, LoadOptions.None);
                }
                catch
                {
                    return new XDocument();
                }
            }

            if (!allowCreateZonesFile)
                return new XDocument();

            var root = new XElement("ArrayOfZone",
                new XAttribute(XNamespace.Xmlns + "xsd", "http://www.w3.org/2001/XMLSchema"),
                new XAttribute(XNamespace.Xmlns + "xsi", "http://www.w3.org/2001/XMLSchema-instance"));

            return new XDocument(root);
        }

        private static XElement BuildZoneElement(ZoneDefinition def, string dealerMenuContainerId, string customerMenuContainerId)
        {
            var zone = new XElement("Zone",
            new XElement("InternalGameName", def.InternalGameName ?? ""),
            new XElement("DisplayName", def.DisplayName ?? ""),
            new XElement("CountyID", def.CountyID ?? ""),
            new XElement("StateID", def.StateID ?? ""),
            new XElement("IsRestrictedDuringWanted", def.IsRestrictedDuringWanted ? "true" : "false"),
            new XElement("IsSpecificLocation", def.IsSpecificLocation ? "true" : "false"),
            new XElement("Boundaries", BuildBoundaries(def.Boundaries)),
            new XElement("Economy", def.Economy ?? ""),
            new XElement("Type", def.Type ?? ""),
            new XElement("DealerMenuContainerID", dealerMenuContainerId ?? ""),
            new XElement("CustomerMenuContainerID", customerMenuContainerId ?? "")
        );


            return zone;
        }

        private static IEnumerable<XElement> BuildBoundaries(IReadOnlyList<ZoneBoundaryPoint> points)
        {
            foreach (var p in points)
            {
                yield return new XElement("Vector2",
                    new XElement("X", p.X.ToString("0.################", CultureInfo.InvariantCulture)),
                    new XElement("Y", p.Y.ToString("0.################", CultureInfo.InvariantCulture)));
            }
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
