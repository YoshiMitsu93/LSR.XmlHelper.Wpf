using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace LSR.XmlHelper.Core.Services.Builders
{
    public sealed class GangDispatchableVehiclesAugmenterService
    {
        public void AugmentWithVehicleGroups(string rootFolderPath, XDocument vehiclesDoc, string gangVehicleGroupId, IReadOnlyList<string> requiredVehicleGroupIds)
        {
            if (vehiclesDoc?.Root is null)
                return;

            if (string.IsNullOrWhiteSpace(gangVehicleGroupId))
                return;

            if (requiredVehicleGroupIds is null || requiredVehicleGroupIds.Count == 0)
                return;

            var gangGroup = vehiclesDoc.Descendants("DispatchableVehicleGroup")
                .FirstOrDefault(x => string.Equals(((string?)x.Element("DispatchableVehicleGroupID") ?? "").Trim(), gangVehicleGroupId, StringComparison.OrdinalIgnoreCase));

            if (gangGroup is null)
                return;

            var gangVehiclesContainer = gangGroup.Element("DispatchableVehicles");
            if (gangVehiclesContainer is null)
            {
                gangVehiclesContainer = new XElement("DispatchableVehicles");
                gangGroup.Add(gangVehiclesContainer);
            }

            var existingKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var dv in gangVehiclesContainer.Elements("DispatchableVehicle"))
            {
                var model = ((string?)dv.Element("ModelName") ?? "").Trim();
                var groupName = ((string?)dv.Element("GroupName") ?? "").Trim();
                existingKeys.Add($"{model}|{groupName}");
            }

            var reader = new DispatchableVehicleGroupReadService();

            foreach (var groupIdRaw in requiredVehicleGroupIds)
            {
                var groupId = (groupIdRaw ?? "").Trim();
                if (string.IsNullOrWhiteSpace(groupId))
                    continue;

                var sourceGroup = reader.TryReadGroup(rootFolderPath, groupId);
                if (sourceGroup is null)
                    continue;

                var sourceVehiclesContainer = sourceGroup.Element("DispatchableVehicles");
                if (sourceVehiclesContainer is null)
                    continue;

                foreach (var dv in sourceVehiclesContainer.Elements("DispatchableVehicle"))
                {
                    var copy = new XElement(dv);

                    var model = ((string?)copy.Element("ModelName") ?? "").Trim();
                    if (string.IsNullOrWhiteSpace(model))
                        continue;

                    SetOrCreate(copy, "GroupName", groupId);

                    var key = $"{model}|{groupId}";
                    if (existingKeys.Contains(key))
                        continue;

                    existingKeys.Add(key);
                    gangVehiclesContainer.Add(copy);
                }
            }
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
