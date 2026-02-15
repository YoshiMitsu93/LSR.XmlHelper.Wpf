using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace LSR.XmlHelper.Core.Services.Builders
{
    public sealed class ShopMenuGroupCatalogService
    {
        public IReadOnlyList<(string Id, string Name)> GetShopMenuGroups(string rootFolderPath)
        {
            if (string.IsNullOrWhiteSpace(rootFolderPath))
                return Array.Empty<(string Id, string Name)>();

            if (!Directory.Exists(rootFolderPath))
                return Array.Empty<(string Id, string Name)>();


            var resolver = new LSR.XmlHelper.Core.Services.LsrFileSetResolverService();
            var resolved = resolver.ResolveShopMenus(rootFolderPath, "Default");

            var results = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

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

                foreach (var g in doc.Descendants("ShopMenuGroup"))
                {
                    var id = ((string?)g.Element("ID") ?? "").Trim();
                    if (string.IsNullOrWhiteSpace(id))
                        continue;

                    var name = ((string?)g.Element("Name") ?? "").Trim();
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
