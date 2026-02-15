using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace LSR.XmlHelper.Wpf.Services.Editing
{
    public sealed class GangTerritoriesChangeSummaryService
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

            var beforeZones = GetZones(beforeDoc, gangId);
            var afterZones = GetZones(afterDoc, gangId);

            var added = afterZones.Except(beforeZones, StringComparer.OrdinalIgnoreCase).OrderBy(x => x).ToList();
            var removed = beforeZones.Except(afterZones, StringComparer.OrdinalIgnoreCase).OrderBy(x => x).ToList();

            var lines = new List<string>();

            if (added.Count > 0)
                lines.Add("Territories: added zones: " + string.Join(", ", added));

            if (removed.Count > 0)
                lines.Add("Territories: removed zones: " + string.Join(", ", removed));

            if (lines.Count == 0)
                lines.Add("Territories: no effective change");

            return lines;
        }

        private static HashSet<string> GetZones(XDocument doc, string gangId)
        {
            return doc.Descendants("GangTerritory")
                .Where(t => string.Equals(((string?)t.Element("GangID") ?? "").Trim(), gangId, StringComparison.OrdinalIgnoreCase))
                .Select(t => ((string?)t.Element("ZoneInternalGameName") ?? "").Trim())
                .Where(z => !string.IsNullOrWhiteSpace(z))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
        }
    }
}
