using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace LSR.XmlHelper.Core.Services.Builders
{
    public sealed class DispatchableVehicleModelReadService
    {
        public XElement? TryReadVehicle(string rootFolderPath, string modelName)
        {
            if (string.IsNullOrWhiteSpace(rootFolderPath) || !Directory.Exists(rootFolderPath))
                return null;

            if (string.IsNullOrWhiteSpace(modelName))
                return null;

            var resolver = new LSR.XmlHelper.Core.Services.LsrFileSetResolverService();
            var resolved = resolver.ResolveDispatchableVehicles(rootFolderPath, "Default");

            var files = resolved.EnumerateReadOrder()
                .Where(p => !string.IsNullOrWhiteSpace(p) && File.Exists(p))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            for (int i = files.Count - 1; i >= 0; i--)
            {
                var file = files[i];
                XDocument doc;
                try
                {
                    doc = XDocument.Load(file, LoadOptions.None);
                }
                catch
                {
                    continue;
                }

                var match = doc.Descendants("DispatchableVehicle")
                    .FirstOrDefault(x => string.Equals((((string?)x.Element("ModelName") ?? "").Trim()), modelName.Trim(), StringComparison.OrdinalIgnoreCase));

                if (match is null)
                    continue;

                return new XElement(match);
            }

            return null;
        }
    }
}
