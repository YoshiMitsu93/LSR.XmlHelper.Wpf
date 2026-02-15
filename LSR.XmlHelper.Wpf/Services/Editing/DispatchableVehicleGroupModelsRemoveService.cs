using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace LSR.XmlHelper.Wpf.Services.Editing
{
    public sealed class DispatchableVehicleGroupModelsRemoveService
    {
        public bool RemoveModels(XDocument vehiclesDoc, string vehicleGroupId, IReadOnlyCollection<string> modelNamesToRemove)
        {
            if (vehiclesDoc?.Root is null)
                return false;

            vehicleGroupId = (vehicleGroupId ?? "").Trim();
            if (string.IsNullOrWhiteSpace(vehicleGroupId))
                return false;

            if (modelNamesToRemove is null || modelNamesToRemove.Count == 0)
                return false;

            var models = modelNamesToRemove
                .Select(x => (x ?? "").Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            if (models.Count == 0)
                return false;

            var group = vehiclesDoc
                .Descendants("DispatchableVehicleGroup")
                .FirstOrDefault(x => string.Equals(((string?)x.Element("DispatchableVehicleGroupID") ?? "").Trim(), vehicleGroupId, StringComparison.OrdinalIgnoreCase));

            if (group is null)
                return false;

            var vehicles = group
                .Descendants("DispatchableVehicle")
                .Where(v => models.Contains(((string?)v.Element("ModelName") ?? "").Trim()))
                .ToList();

            if (vehicles.Count == 0)
                return false;

            foreach (var v in vehicles)
                v.Remove();

            return true;
        }
    }
}
