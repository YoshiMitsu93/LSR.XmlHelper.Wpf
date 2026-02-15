using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace LSR.XmlHelper.Core.Services.Builders
{
    public sealed class ZoneCatalogService
    {
        public IReadOnlyList<(string InternalGameName, string DisplayName)> GetZones(string rootFolderPath)
        {
            if (string.IsNullOrWhiteSpace(rootFolderPath))
                return Array.Empty<(string InternalGameName, string DisplayName)>();

            if (!Directory.Exists(rootFolderPath))
                return Array.Empty<(string InternalGameName, string DisplayName)>();

            var resolver = new LSR.XmlHelper.Core.Services.LsrFileSetResolverService();
            var resolved = resolver.ResolveZones(rootFolderPath, "Default");

            var byInternalNameMerged = new Dictionary<string, (string InternalGameName, string DisplayName)>(StringComparer.OrdinalIgnoreCase);

            foreach (var file in resolved.EnumerateReadOrder())
            {
                if (string.IsNullOrWhiteSpace(file) || !File.Exists(file))
                    continue;

                XDocument doc;

                try
                {
                    doc = XDocument.Load(file, LoadOptions.None);
                }
                catch
                {
                    continue;
                }

                foreach (var zone in doc.Descendants("Zone"))
                {
                    var internalName = ((string?)zone.Element("InternalGameName") ?? "").Trim();
                    if (string.IsNullOrWhiteSpace(internalName))
                        continue;

                    var displayName = ((string?)zone.Element("DisplayName") ?? "").Trim();
                    if (string.IsNullOrWhiteSpace(displayName))
                        displayName = internalName;

                    byInternalNameMerged[internalName] = (internalName, displayName);
                }
            }

            return byInternalNameMerged.Values
                .OrderBy(z => z.DisplayName, StringComparer.OrdinalIgnoreCase)
                .ThenBy(z => z.InternalGameName, StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }
    }
}
