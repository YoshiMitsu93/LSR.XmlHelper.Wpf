using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace LSR.XmlHelper.Core.Services.Builders.Intoxicants
{
    public sealed class IntoxicantCatalogService
    {
        public HashSet<string> GetIntoxicantNames(string rootFolderPath)
        {
            var results = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (string.IsNullOrWhiteSpace(rootFolderPath))
                return results;

            var resolver = new LSR.XmlHelper.Core.Services.LsrFileSetResolverService();
            var resolved = resolver.ResolveIntoxicants(rootFolderPath, "Default");

            var files = resolved.EnumerateReadOrder()
                .Where(p => !string.IsNullOrWhiteSpace(p) && File.Exists(p))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            foreach (var file in files)
            {
                XDocument doc;
                try
                {
                    doc = XDocument.Load(file, LoadOptions.None);
                }
                catch
                {
                    continue;
                }

                foreach (var intoxicant in doc.Descendants("Intoxicant"))
                {
                    var name = ((string?)intoxicant.Element("Name"))?.Trim();
                    if (!string.IsNullOrWhiteSpace(name))
                        results.Add(name);
                }
            }

            return results;
        }
    }
}
