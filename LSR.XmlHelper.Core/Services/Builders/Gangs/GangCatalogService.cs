using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace LSR.XmlHelper.Core.Services.Builders
{
    public sealed class GangCatalogService
    {
        public IReadOnlyList<(string Id, string FullName)> GetGangs(string rootFolderPath)
        {
            if (string.IsNullOrWhiteSpace(rootFolderPath))
                return Array.Empty<(string Id, string FullName)>();

            if (!Directory.Exists(rootFolderPath))
                return Array.Empty<(string Id, string FullName)>();

            var resolver = new LSR.XmlHelper.Core.Services.LsrFileSetResolverService();
            var resolved = resolver.ResolveGangs(rootFolderPath, "Default");

            var byIdMerged = new Dictionary<string, (string Id, string FullName)>(StringComparer.OrdinalIgnoreCase);

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

                foreach (var gangElement in doc.Descendants("Gang"))
                {
                    var id = ((string?)gangElement.Element("ID") ?? "").Trim();
                    if (string.IsNullOrWhiteSpace(id))
                        continue;

                    var fullName = ((string?)gangElement.Element("FullName") ?? "").Trim();
                    if (string.IsNullOrWhiteSpace(fullName))
                        fullName = id;

                    byIdMerged[id] = (id, fullName);
                }
            }

            return byIdMerged.Values
                .OrderBy(g => g.FullName, StringComparer.OrdinalIgnoreCase)
                .ThenBy(g => g.Id, StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }
    }
}
