using LSR.XmlHelper.Core.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace LSR.XmlHelper.Core.Services.Builders.Zones
{
    public sealed class ZoneDefinitionLookupService
    {
        public bool TryGetZoneDefinition(string rootFolderPath, string internalGameName, out ZoneDefinition definition)
        {
            definition = null!;

            if (string.IsNullOrWhiteSpace(rootFolderPath) || !Directory.Exists(rootFolderPath))
                return false;

            if (string.IsNullOrWhiteSpace(internalGameName))
                return false;

            var resolver = new LsrFileSetResolverService();
            var resolved = resolver.ResolveZones(rootFolderPath, "Default");

            if (string.IsNullOrWhiteSpace(resolved.BasePath) || !File.Exists(resolved.BasePath))
                return false;

            XDocument doc;
            try
            {
                doc = XDocument.Load(resolved.BasePath, LoadOptions.None);
            }
            catch
            {
                return false;
            }

            var zone = doc.Descendants("Zone")
                .FirstOrDefault(z =>
                    string.Equals(((string?)z.Element("InternalGameName") ?? "").Trim(), internalGameName.Trim(), StringComparison.OrdinalIgnoreCase));

            if (zone is null)
                return false;

            var displayName = ((string?)zone.Element("DisplayName") ?? "").Trim();
            var countyId = ((string?)zone.Element("CountyID") ?? "").Trim();
            var stateId = ((string?)zone.Element("StateID") ?? "").Trim();
            var economy = ((string?)zone.Element("Economy") ?? "").Trim();
            var type = ((string?)zone.Element("Type") ?? "").Trim();

            var isRestrictedDuringWanted = ParseBool(((string?)zone.Element("IsRestrictedDuringWanted") ?? "").Trim());
            var isSpecificLocation = ParseBool(((string?)zone.Element("IsSpecificLocation") ?? "").Trim());

            var boundaries = new List<ZoneBoundaryPoint>();
            var boundaryRoot = zone.Element("Boundaries");
            if (boundaryRoot is not null)
            {
                foreach (var v in boundaryRoot.Elements())
                {
                    if (!string.Equals(v.Name.LocalName, "Vector2", StringComparison.OrdinalIgnoreCase))
                        continue;

                    var xText = ((string?)v.Element("X") ?? "").Trim();
                    var yText = ((string?)v.Element("Y") ?? "").Trim();

                    if (!double.TryParse(xText, NumberStyles.Float, CultureInfo.InvariantCulture, out var x))
                        continue;

                    if (!double.TryParse(yText, NumberStyles.Float, CultureInfo.InvariantCulture, out var y))
                        continue;

                    boundaries.Add(new ZoneBoundaryPoint(x, y));
                }
            }

            definition = new ZoneDefinition(
                internalGameName.Trim(),
                displayName,
                countyId,
                stateId,
                isRestrictedDuringWanted,
                isSpecificLocation,
                economy,
                type,
                boundaries);

            return true;
        }

        private static bool ParseBool(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            return value.Equals("true", StringComparison.OrdinalIgnoreCase)
                || value.Equals("1", StringComparison.OrdinalIgnoreCase)
                || value.Equals("yes", StringComparison.OrdinalIgnoreCase);
        }
    }
}
