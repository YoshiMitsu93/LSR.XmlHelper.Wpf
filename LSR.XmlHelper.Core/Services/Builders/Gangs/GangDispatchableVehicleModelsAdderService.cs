using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace LSR.XmlHelper.Core.Services.Builders
{
    public sealed class GangDispatchableVehicleModelsAdderService
    {
        public IReadOnlyList<string> AddSelections(
            string rootFolderPath,
            XDocument vehiclesDoc,
            string gangVehicleGroupId,
            IReadOnlyList<(string ModelName, string VariantKey, int? OverridePrimaryColorId, int? OverrideSecondaryColorId, IReadOnlyList<int> OverrideLiveries)> selections)
        {
            if (vehiclesDoc?.Root is null)
                return Array.Empty<string>();

            if (string.IsNullOrWhiteSpace(gangVehicleGroupId))
                return Array.Empty<string>();

            if (selections is null || selections.Count == 0)
                return Array.Empty<string>();

            var gangGroup = vehiclesDoc.Descendants("DispatchableVehicleGroup")
                .FirstOrDefault(x => string.Equals(((string?)x.Element("DispatchableVehicleGroupID") ?? "").Trim(), gangVehicleGroupId, StringComparison.OrdinalIgnoreCase));

            if (gangGroup is null)
                return selections.Select(x => (x.ModelName ?? "").Trim()).Where(x => !string.IsNullOrWhiteSpace(x)).ToList();

            var gangVehiclesContainer = gangGroup.Element("DispatchableVehicles");
            if (gangVehiclesContainer is null)
            {
                gangVehiclesContainer = new XElement("DispatchableVehicles");
                gangGroup.Add(gangVehiclesContainer);
            }

            var existingModels = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var dv in gangVehiclesContainer.Elements("DispatchableVehicle"))
            {
                var model = ((string?)dv.Element("ModelName") ?? "").Trim();
                if (!string.IsNullOrWhiteSpace(model))
                    existingModels.Add(model);
            }

            XElement? templateFromGang = gangVehiclesContainer.Elements("DispatchableVehicle").FirstOrDefault();
            var modelReader = new DispatchableVehicleModelReadService();
            var variantReader = new DispatchableVehicleVariantReadService();

            var missing = new List<string>();

            foreach (var sel in selections)
            {
                var model = (sel.ModelName ?? "").Trim();
                if (string.IsNullOrWhiteSpace(model))
                    continue;

                if (existingModels.Contains(model))
                    continue;

                XElement? toAdd = null;

                if (!string.IsNullOrWhiteSpace(sel.VariantKey))
                    toAdd = variantReader.TryReadVehicleByVariantKey(rootFolderPath, model, sel.VariantKey);

                if (toAdd is null)
                    toAdd = modelReader.TryReadVehicle(rootFolderPath, model);

                if (toAdd is null && templateFromGang is not null)
                {
                    toAdd = new XElement(templateFromGang);
                    SetOrCreate(toAdd, "ModelName", model);
                }

                if (toAdd is null)
                {
                    missing.Add(model);
                    continue;
                }

                SetOrCreate(toAdd, "ModelName", model);
                SetOrCreate(toAdd, "RequiredPedGroup", "");
                SetOrCreate(toAdd, "GroupName", "");

                if (sel.OverridePrimaryColorId.HasValue)
                    SetOrCreate(toAdd, "RequiredPrimaryColorID", sel.OverridePrimaryColorId.Value.ToString());

                if (sel.OverrideSecondaryColorId.HasValue)
                    SetOrCreate(toAdd, "RequiredSecondaryColorID", sel.OverrideSecondaryColorId.Value.ToString());

                if (sel.OverrideLiveries is not null && sel.OverrideLiveries.Count > 0)
                    SetRequiredLiveries(toAdd, sel.OverrideLiveries);

                existingModels.Add(model);
                gangVehiclesContainer.Add(toAdd);
            }

            return missing;
        }

        private static void SetRequiredLiveries(XElement dv, IReadOnlyList<int> liveries)
        {
            var rl = dv.Element("RequiredLiveries");
            if (rl is null)
            {
                rl = new XElement("RequiredLiveries");
                dv.Add(rl);
            }

            rl.RemoveNodes();

            foreach (var id in liveries.Distinct())
                rl.Add(new XElement("int", id));
        }

        private static void SetOrCreate(XElement parent, string elementName, string value)
        {
            var existing = parent.Element(elementName);
            if (existing is null)
                parent.Add(new XElement(elementName, value));
            else
                existing.Value = value;
        }
    }
}
