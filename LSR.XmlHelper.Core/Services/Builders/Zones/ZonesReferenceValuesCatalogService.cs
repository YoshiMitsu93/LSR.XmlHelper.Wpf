using LSR.XmlHelper.Core.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace LSR.XmlHelper.Core.Services.Builders.Zones
{
    public sealed class ZonesReferenceValuesCatalogService
    {
        public (IReadOnlyList<string> CountyIds, IReadOnlyList<string> StateIds, IReadOnlyList<string> Economies, IReadOnlyList<string> Types) GetOptions(string rootFolderPath)
        {
            if (string.IsNullOrWhiteSpace(rootFolderPath) || !Directory.Exists(rootFolderPath))
                return (Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>());

            var resolver = new LsrFileSetResolverService();
            var resolved = resolver.ResolveZones(rootFolderPath, "Default");

            if (string.IsNullOrWhiteSpace(resolved.BasePath) || !File.Exists(resolved.BasePath))
                return (Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>());

            XDocument doc;
            try
            {
                doc = XDocument.Load(resolved.BasePath, LoadOptions.None);
            }
            catch
            {
                return (Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>());
            }

            var zones = doc.Descendants("Zone").ToArray();

            var countyIds = zones.Select(z => ((string?)z.Element("CountyID") ?? "").Trim())
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(s => s, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            var stateIds = zones.Select(z => ((string?)z.Element("StateID") ?? "").Trim())
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(s => s, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            var economies = zones.Select(z => ((string?)z.Element("Economy") ?? "").Trim())
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(s => s, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            var types = zones.Select(z => ((string?)z.Element("Type") ?? "").Trim())
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(s => s, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            return (countyIds, stateIds, economies, types);
        }
    }
}
