using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace LSR.XmlHelper.Core.Services.Builders
{
    public sealed class DispatchableVehicleModelCatalogService
    {
        public IReadOnlyList<(string ModelName, int Count)> GetModels(string rootFolderPath)
        {
            if (string.IsNullOrWhiteSpace(rootFolderPath))
                return Array.Empty<(string ModelName, int Count)>();

            if (!Directory.Exists(rootFolderPath))
                return Array.Empty<(string ModelName, int Count)>();

            var resolver = new LSR.XmlHelper.Core.Services.LsrFileSetResolverService();
            var resolved = resolver.ResolveDispatchableVehicles(rootFolderPath, "Default");

            var files = resolved.EnumerateReadOrder()
                .Where(p => !string.IsNullOrWhiteSpace(p) && File.Exists(p))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var results = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            foreach (var file in files)
            {
                XDocument doc;
                try
                {
                    doc = XDocument.Load(file);
                }
                catch
                {
                    continue;
                }

                foreach (var v in doc.Descendants("DispatchableVehicle"))
                {
                    var model = ((string?)v.Element("ModelName") ?? "").Trim();
                    if (string.IsNullOrWhiteSpace(model))
                        continue;

                    results.TryGetValue(model, out var count);
                    results[model] = count + 1;
                }
            }

            return results
                .OrderBy(k => k.Key, StringComparer.OrdinalIgnoreCase)
                .Select(k => (k.Key, k.Value))
                .ToList();
        }
    }
}
