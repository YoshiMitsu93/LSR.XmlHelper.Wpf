using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using LSR.XmlHelper.Wpf.ViewModels.Builders;

namespace LSR.XmlHelper.Wpf.Services.Editing
{
    public sealed class DispatchableVehiclesChangeSummaryService
    {
        public IReadOnlyList<string> Summarize(
            string beforeXml,
            string afterXml,
            string groupId,
            IReadOnlyCollection<CustomDispatchableVehicleModelViewModel> requestedAdds,
            IReadOnlyCollection<string> requestedRemoves)
        {
            groupId = (groupId ?? "").Trim();
            requestedAdds ??= Array.Empty<CustomDispatchableVehicleModelViewModel>();
            requestedRemoves ??= Array.Empty<string>();

            if (string.IsNullOrWhiteSpace(beforeXml) || string.IsNullOrWhiteSpace(afterXml) || string.IsNullOrWhiteSpace(groupId))
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

            var beforeGroup = FindGroup(beforeDoc, groupId);
            var afterGroup = FindGroup(afterDoc, groupId);

            if (beforeGroup is null || afterGroup is null)
                return new[] { "DispatchableVehicles: group '" + groupId + "' could not be compared" };

            var beforeModels = beforeGroup
                .Descendants("DispatchableVehicle")
                .Select(v => ((string?)v.Element("ModelName") ?? "").Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var afterModels = afterGroup
                .Descendants("DispatchableVehicle")
                .Select(v => ((string?)v.Element("ModelName") ?? "").Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var lines = new List<string>();

            foreach (var vm in requestedAdds)
            {
                var model = (vm?.ModelName ?? "").Trim();
                if (string.IsNullOrWhiteSpace(model))
                    continue;

                var existedBefore = beforeModels.Contains(model);
                var existsAfter = afterModels.Contains(model);

                if (!existedBefore && existsAfter)
                    lines.Add("DispatchableVehicles[" + groupId + "]: added model '" + model + "'");
            }

            foreach (var modelRaw in requestedRemoves)
            {
                var model = (modelRaw ?? "").Trim();
                if (string.IsNullOrWhiteSpace(model))
                    continue;

                var existedBefore = beforeModels.Contains(model);
                var existsAfter = afterModels.Contains(model);

                if (existedBefore && !existsAfter)
                    lines.Add("DispatchableVehicles[" + groupId + "]: removed model '" + model + "'");
            }

            if (lines.Count == 0)
                lines.Add("DispatchableVehicles[" + groupId + "]: no effective change");

            return lines;
        }

        public IReadOnlyList<string> Summarize(string beforeXml, string afterXml, string groupId, IReadOnlyCollection<CustomDispatchableVehicleModelViewModel> requestedAdds)
        {
            groupId = (groupId ?? "").Trim();
            requestedAdds ??= Array.Empty<CustomDispatchableVehicleModelViewModel>();

            if (string.IsNullOrWhiteSpace(beforeXml) || string.IsNullOrWhiteSpace(afterXml) || string.IsNullOrWhiteSpace(groupId))
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

            var beforeGroup = FindGroup(beforeDoc, groupId);
            var afterGroup = FindGroup(afterDoc, groupId);

            if (beforeGroup is null || afterGroup is null)
                return new[] { "DispatchableVehicles: group '" + groupId + "' could not be compared" };

            var beforeModels = beforeGroup
                .Descendants("DispatchableVehicle")
                .Select(v => ((string?)v.Element("ModelName") ?? "").Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var afterModels = afterGroup
                .Descendants("DispatchableVehicle")
                .Select(v => ((string?)v.Element("ModelName") ?? "").Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var lines = new List<string>();

            foreach (var vm in requestedAdds)
            {
                var model = (vm?.ModelName ?? "").Trim();
                if (string.IsNullOrWhiteSpace(model))
                    continue;

                var existedBefore = beforeModels.Contains(model);
                var existsAfter = afterModels.Contains(model);

                if (!existedBefore && existsAfter)
                    lines.Add("DispatchableVehicles[" + groupId + "]: added model '" + model + "'");
            }

            if (lines.Count == 0)
                lines.Add("DispatchableVehicles[" + groupId + "]: no effective change");

            return lines;
        }

        private static XElement? FindGroup(XDocument doc, string groupId)
        {
            return doc.Descendants("DispatchableVehicleGroup")
                .FirstOrDefault(x => string.Equals(((string?)x.Element("DispatchableVehicleGroupID") ?? "").Trim(), groupId, StringComparison.OrdinalIgnoreCase));
        }
    }
}
