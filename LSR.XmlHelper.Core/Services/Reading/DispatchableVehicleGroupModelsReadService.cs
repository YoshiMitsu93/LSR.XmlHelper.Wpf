using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using LSR.XmlHelper.Core.Models;

namespace LSR.XmlHelper.Core.Services.Reading
{
    public sealed class DispatchableVehicleGroupModelsReadService
    {
        private static bool IsVehicleGroupMatch(XElement group, string vehicleGroupId)
        {
            var id = ((string?)group.Element("DispatchableVehicleGroupID") ?? "").Trim();
            if (string.IsNullOrWhiteSpace(id))
                id = ((string?)group.Element("ID") ?? "").Trim();

            return string.Equals(id, (vehicleGroupId ?? "").Trim(), StringComparison.OrdinalIgnoreCase);
        }
        public IReadOnlyList<string> GetModelsForGroupId(string rootFolderPath, string vehicleGroupId)
        {
            if (string.IsNullOrWhiteSpace(rootFolderPath))
                return Array.Empty<string>();

            if (string.IsNullOrWhiteSpace(vehicleGroupId))
                return Array.Empty<string>();

            var results = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            var resolver = new LSR.XmlHelper.Core.Services.LsrFileSetResolverService();
            var resolved = resolver.ResolveDispatchableVehicles(rootFolderPath, "Default");

            var candidates = resolved
                .EnumerateReadOrder()
                .Where(p => !string.IsNullOrWhiteSpace(p) && File.Exists(p))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            foreach (var path in candidates)
            {
                try
                {
                    var doc = XDocument.Load(path, LoadOptions.None);

                    var group = doc
                        .Descendants("DispatchableVehicleGroup")
                        .FirstOrDefault(x => IsVehicleGroupMatch(x, vehicleGroupId));

                    if (group is null)
                        continue;

                    foreach (var dv in group.Descendants("DispatchableVehicle"))
                    {
                        var model = ((string?)dv.Element("ModelName") ?? "").Trim();
                        if (!string.IsNullOrWhiteSpace(model))
                            results.Add(model);
                    }
                }
                catch
                {
                }
            }

            return results
                .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }
        public IReadOnlyList<string> GetModelsForGroupIdResolved(string rootFolderPath, string vehicleGroupId)
        {
            if (string.IsNullOrWhiteSpace(rootFolderPath))
                return Array.Empty<string>();

            if (string.IsNullOrWhiteSpace(vehicleGroupId))
                return Array.Empty<string>();

            var resolver = new LSR.XmlHelper.Core.Services.LsrConfigFileResolverService();
            var path = resolver.ResolveDispatchableVehiclesFile(rootFolderPath, vehicleGroupId) ?? Path.Combine(rootFolderPath, "DispatchableVehicles.xml");

            if (!File.Exists(path))
                return Array.Empty<string>();

            try
            {
                var doc = XDocument.Load(path, LoadOptions.None);

                var group = doc
                    .Descendants("DispatchableVehicleGroup")
                    .FirstOrDefault(x => IsVehicleGroupMatch(x, vehicleGroupId));

                if (group is null)
                    return Array.Empty<string>();

                return group
                    .Descendants("DispatchableVehicle")
                    .Select(dv => ((string?)dv.Element("ModelName") ?? "").Trim())
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
                    .ToArray();
            }
            catch
            {
                return Array.Empty<string>();
            }
        }
    }
}
