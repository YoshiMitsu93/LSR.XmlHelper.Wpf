using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using LSR.XmlHelper.Core.Services.Builders;
using LSR.XmlHelper.Wpf.ViewModels.Builders;

namespace LSR.XmlHelper.Wpf.Services.Editing
{
    public sealed class DispatchableVehicleGroupEditsApplyService
    {
        public (bool Updated, IReadOnlyList<string> MissingModels) Apply(
            string rootFolderPath,
            XDocument vehiclesDoc,
            string vehicleGroupId,
            IReadOnlyCollection<CustomDispatchableVehicleModelViewModel> customModels)
        {
            if (vehiclesDoc?.Root is null)
                return (false, Array.Empty<string>());

            vehicleGroupId = (vehicleGroupId ?? "").Trim();
            if (string.IsNullOrWhiteSpace(vehicleGroupId))
                return (false, Array.Empty<string>());

            if (customModels is null || customModels.Count == 0)
                return (false, Array.Empty<string>());

            var group = FindGroup(vehiclesDoc, vehicleGroupId);
            var created = false;

            if (group is null)
            {
                group = CreateGroupFromTemplateOrMinimal(vehiclesDoc, vehicleGroupId);
                created = group is not null;
            }

            if (group is null)
                return (false, Array.Empty<string>());

            var selections = new List<(string ModelName, string VariantKey, int? OverridePrimaryColorId, int? OverrideSecondaryColorId, IReadOnlyList<int> OverrideLiveries)>();

            foreach (var item in customModels)
            {
                var model = (item.ModelName ?? "").Trim();
                if (string.IsNullOrWhiteSpace(model))
                    continue;

                int? pri = null;
                if (item.TryGetOverridePrimaryColorId(out var priValue))
                    pri = priValue;

                int? sec = null;
                if (item.TryGetOverrideSecondaryColorId(out var secValue))
                    sec = secValue;

                var liveries = item.GetOverrideLiveryIds();

                selections.Add((model, (item.VariantKey ?? "").Trim(), pri, sec, liveries));
            }

            if (selections.Count == 0)
                return (false, Array.Empty<string>());

            var before = vehiclesDoc.ToString(SaveOptions.DisableFormatting);

            var adder = new GangDispatchableVehicleModelsAdderService();
            var missing = adder.AddSelections(rootFolderPath, vehiclesDoc, vehicleGroupId, selections);

            var after = vehiclesDoc.ToString(SaveOptions.DisableFormatting);

            var updated = created || !string.Equals(before, after, StringComparison.Ordinal);

            return (updated, missing);
        }

        private static XElement? FindGroup(XDocument doc, string groupId)
        {
            return doc.Descendants("DispatchableVehicleGroup")
                .FirstOrDefault(x => string.Equals(((string?)x.Element("DispatchableVehicleGroupID") ?? "").Trim(), groupId, StringComparison.OrdinalIgnoreCase));
        }

        private static XElement? CreateGroupFromTemplateOrMinimal(XDocument doc, string groupId)
        {
            if (doc.Root is null)
                return null;

            var template = doc.Descendants("DispatchableVehicleGroup").FirstOrDefault();
            XElement group;

            if (template is not null)
            {
                group = new XElement(template);
                SetOrCreate(group, "DispatchableVehicleGroupID", groupId);

                var container = group.Element("DispatchableVehicles");
                if (container is null)
                {
                    container = new XElement("DispatchableVehicles");
                    group.Add(container);
                }
                else
                {
                    container.RemoveNodes();
                }
            }
            else
            {
                group = new XElement("DispatchableVehicleGroup",
                    new XElement("DispatchableVehicleGroupID", groupId),
                    new XElement("DispatchableVehicles"));
            }

            doc.Root.Add(group);
            return group;
        }

        private static void SetOrCreate(XElement parent, string childName, string value)
        {
            var child = parent.Element(childName);
            if (child is null)
                parent.Add(new XElement(childName, value));
            else
                child.Value = value;
        }
    }
}
