using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace LSR.XmlHelper.Wpf.Services.Editing
{
    public sealed class LocationsDenDetailedChangeSummaryService
    {
        public IReadOnlyList<string> Summarize(string beforeXml, string afterXml, string gangId)
        {
            gangId = (gangId ?? "").Trim();

            if (string.IsNullOrWhiteSpace(beforeXml) || string.IsNullOrWhiteSpace(afterXml) || string.IsNullOrWhiteSpace(gangId))
                return Array.Empty<string>();

            XDocument beforeDoc;
            XDocument afterDoc;

            try
            {
                beforeDoc = XDocument.Parse(beforeXml, LoadOptions.None);
                afterDoc = XDocument.Parse(afterXml, LoadOptions.None);
            }
            catch
            {
                return Array.Empty<string>();
            }

            var beforeDen = FindDen(beforeDoc, gangId);
            var afterDen = FindDen(afterDoc, gangId);

            if (beforeDen is null || afterDen is null)
                return new[] { "Gang den: could not compare Locations.xml for '" + gangId + "'" };

            var lines = new List<string>();

            CompareSimpleField(lines, "Gang den: Name", beforeDen.Element("Name"), afterDen.Element("Name"));
            CompareSimpleField(lines, "Gang den: BannerImagePath", beforeDen.Element("BannerImagePath"), afterDen.Element("BannerImagePath"));
            CompareEntrance(lines, beforeDen, afterDen);

            CompareSpawnBlock(lines, "Gang den: Ped spawn", beforeDen.Element("PossiblePedSpawns"), afterDen.Element("PossiblePedSpawns"), includeVehicleGroups: false);
            CompareSpawnBlock(lines, "Gang den: Vehicle spawn", beforeDen.Element("PossibleVehicleSpawns"), afterDen.Element("PossibleVehicleSpawns"), includeVehicleGroups: true);

            if (lines.Count == 0)
                lines.Add("Gang den: no effective change");

            return lines;
        }

        private static XElement? FindDen(XDocument doc, string gangId)
        {
            return doc.Descendants("GangDen")
                .FirstOrDefault(x => string.Equals(((string?)x.Element("AssignedAssociationID") ?? "").Trim(), gangId, StringComparison.OrdinalIgnoreCase));
        }

        private static void CompareSimpleField(List<string> lines, string label, XElement? before, XElement? after)
        {
            var b = ((string?)before ?? "").Trim();
            var a = ((string?)after ?? "").Trim();

            if (!string.Equals(b, a, StringComparison.Ordinal))
                lines.Add(label + ": '" + b + "' -> '" + a + "'");
        }

        private static void CompareEntrance(List<string> lines, XElement beforeDen, XElement afterDen)
        {
            var bx = ReadDouble(beforeDen.Element("EntrancePosition")?.Element("X")?.Value);
            var by = ReadDouble(beforeDen.Element("EntrancePosition")?.Element("Y")?.Value);
            var bz = ReadDouble(beforeDen.Element("EntrancePosition")?.Element("Z")?.Value);
            var bh = ReadDouble(beforeDen.Element("EntranceHeading")?.Value);

            var ax = ReadDouble(afterDen.Element("EntrancePosition")?.Element("X")?.Value);
            var ay = ReadDouble(afterDen.Element("EntrancePosition")?.Element("Y")?.Value);
            var az = ReadDouble(afterDen.Element("EntrancePosition")?.Element("Z")?.Value);
            var ah = ReadDouble(afterDen.Element("EntranceHeading")?.Value);

            if (!NearlyEqual(bx, ax) || !NearlyEqual(by, ay) || !NearlyEqual(bz, az) || !NearlyEqual(bh, ah))
            {
                lines.Add("Gang den: Entrance changed");
            }
        }

        private static void CompareSpawnBlock(List<string> lines, string prefix, XElement? beforeContainer, XElement? afterContainer, bool includeVehicleGroups)
        {
            if (beforeContainer is null || afterContainer is null)
                return;

            var beforeRows = ReadRows(beforeContainer, includeVehicleGroups);
            var afterRows = ReadRows(afterContainer, includeVehicleGroups);

            foreach (var a in afterRows)
            {
                var match = beforeRows.FirstOrDefault(b => b.Matches(a));
                if (!match.IsValid)
                {
                    lines.Add(prefix + ": added spawn at " + a.KeyText());
                    continue;
                }

                var diffs = match.Diff(a);
                foreach (var d in diffs)
                    lines.Add(prefix + ": " + a.KeyText() + " " + d);
            }

            foreach (var b in beforeRows)
            {
                var stillExists = afterRows.Any(a => a.Matches(b));
                if (!stillExists)
                    lines.Add(prefix + ": removed spawn at " + b.KeyText());
            }
        }

        private static List<SpawnRow> ReadRows(XElement container, bool includeVehicleGroups)
        {
            var rows = new List<SpawnRow>();

            foreach (var cl in container.Elements("ConditionalLocation"))
            {
                var loc = cl.Element("Location");
                var x = ReadDouble(loc?.Element("X")?.Value);
                var y = ReadDouble(loc?.Element("Y")?.Value);
                var z = ReadDouble(loc?.Element("Z")?.Value);
                var heading = ReadDouble(cl.Element("Heading")?.Value);

                if (!x.HasValue || !y.HasValue || !z.HasValue || !heading.HasValue)
                    continue;

                var percentage = ((string?)cl.Element("Percentage") ?? "").Trim();
                var tasks = ((string?)cl.Element("Tasks") ?? "").Trim();
                var hours = ((string?)cl.Element("Hours") ?? "").Trim();
                var wl = ((string?)cl.Element("WantedLevel") ?? "").Trim();
                var longGun = ((string?)cl.Element("AlwaysHasLongGun") ?? "").Trim();

                var requiredGroup = includeVehicleGroups ? (((string?)cl.Element("RequiredVehicleGroup") ?? "").Trim()) : "";
                var forceGroup = includeVehicleGroups ? (((string?)cl.Element("ForceVehicleGroup") ?? "").Trim()) : "";

                rows.Add(new SpawnRow(x.Value, y.Value, z.Value, heading.Value, percentage, tasks, hours, wl, longGun, requiredGroup, forceGroup, includeVehicleGroups, true));
            }

            return rows;
        }

        private static double? ReadDouble(string? raw)
        {
            raw = (raw ?? "").Trim();
            if (string.IsNullOrWhiteSpace(raw))
                return null;

            if (double.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out var v))
                return v;

            if (double.TryParse(raw, NumberStyles.Float, CultureInfo.CurrentCulture, out v))
                return v;

            return null;
        }

        private static bool NearlyEqual(double? a, double? b)
        {
            if (!a.HasValue && !b.HasValue)
                return true;

            if (!a.HasValue || !b.HasValue)
                return false;

            return Math.Abs(a.Value - b.Value) < 0.01;
        }

        private readonly record struct SpawnRow(
            double X,
            double Y,
            double Z,
            double Heading,
            string Percentage,
            string Tasks,
            string Hours,
            string WantedLevel,
            string AlwaysHasLongGun,
            string RequiredVehicleGroup,
            string ForceVehicleGroup,
            bool IncludeVehicleGroups,
            bool IsValid)
        {
            public bool Matches(SpawnRow other)
            {
                return Math.Abs(X - other.X) < 0.01 &&
                       Math.Abs(Y - other.Y) < 0.01 &&
                       Math.Abs(Z - other.Z) < 0.01 &&
                       Math.Abs(Heading - other.Heading) < 0.01;
            }

            public IEnumerable<string> Diff(SpawnRow other)
            {
                if (!string.Equals(Percentage, other.Percentage, StringComparison.Ordinal))
                    yield return "Percentage: '" + Percentage + "' -> '" + other.Percentage + "'";

                if (!string.Equals(Tasks, other.Tasks, StringComparison.Ordinal))
                    yield return "Tasks: '" + Tasks + "' -> '" + other.Tasks + "'";

                if (!string.Equals(Hours, other.Hours, StringComparison.Ordinal))
                    yield return "Hours: '" + Hours + "' -> '" + other.Hours + "'";

                if (!string.Equals(WantedLevel, other.WantedLevel, StringComparison.Ordinal))
                    yield return "WantedLevel: '" + WantedLevel + "' -> '" + other.WantedLevel + "'";

                if (!string.Equals(AlwaysHasLongGun, other.AlwaysHasLongGun, StringComparison.Ordinal))
                    yield return "AlwaysHasLongGun: '" + AlwaysHasLongGun + "' -> '" + other.AlwaysHasLongGun + "'";

                if (IncludeVehicleGroups)
                {
                    if (!string.Equals(RequiredVehicleGroup, other.RequiredVehicleGroup, StringComparison.OrdinalIgnoreCase))
                        yield return "RequiredVehicleGroup: '" + RequiredVehicleGroup + "' -> '" + other.RequiredVehicleGroup + "'";

                    if (!string.Equals(ForceVehicleGroup, other.ForceVehicleGroup, StringComparison.OrdinalIgnoreCase))
                        yield return "ForceVehicleGroup: '" + ForceVehicleGroup + "' -> '" + other.ForceVehicleGroup + "'";
                }
            }

            public string KeyText()
            {
                return "X=" + X + ", Y=" + Y + ", Z=" + Z + ", Heading=" + Heading;
            }
        }
    }
}
