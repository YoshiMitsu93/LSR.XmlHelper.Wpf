using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace LSR.XmlHelper.Core.Services.Builders
{
    public sealed class IssuableWeaponsGroupCatalogService
    {
        public IReadOnlyList<(string Id, string Name)> GetGroups(string rootFolderPath)
        {
            if (string.IsNullOrWhiteSpace(rootFolderPath))
                return Array.Empty<(string Id, string Name)>();

            if (!Directory.Exists(rootFolderPath))
                return Array.Empty<(string Id, string Name)>();

            var resolver = new LSR.XmlHelper.Core.Services.LsrFileSetResolverService();
            var resolved = resolver.ResolveIssuableWeapons(rootFolderPath, "Default");

            var files = resolved.EnumerateReadOrder()
                .Where(p => !string.IsNullOrWhiteSpace(p) && File.Exists(p))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var results = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

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

                foreach (var g in doc.Descendants("IssuableWeaponsGroup"))
                {
                    var id = ((string?)g.Element("ID") ?? (string?)g.Element("IssuableWeaponsID") ?? "").Trim();
                    if (string.IsNullOrWhiteSpace(id))
                        continue;

                    var name = ((string?)g.Element("Name") ?? "").Trim();
                    if (string.IsNullOrWhiteSpace(name))
                        name = id;

                    results[id] = name;
                }
            }

            return results
                .OrderBy(k => k.Key, StringComparer.OrdinalIgnoreCase)
                .Select(k => (k.Key, k.Value))
                .ToList();
        }
    }
}
