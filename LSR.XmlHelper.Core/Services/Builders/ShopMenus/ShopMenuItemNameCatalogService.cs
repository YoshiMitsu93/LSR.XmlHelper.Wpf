using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace LSR.XmlHelper.Core.Services.Builders
{
    public sealed class ShopMenuItemNameCatalogService
    {
        public IReadOnlyList<string> GetAllItemNames(string rootFolderPath)
        {
            if (string.IsNullOrWhiteSpace(rootFolderPath))
                return Array.Empty<string>();

            if (!Directory.Exists(rootFolderPath))
                return Array.Empty<string>();

            var resolver = new LSR.XmlHelper.Core.Services.LsrFileSetResolverService();
            var resolved = resolver.ResolveShopMenus(rootFolderPath, "Default");

            var files = resolved.EnumerateReadOrder()
                .Where(p => !string.IsNullOrWhiteSpace(p) && File.Exists(p))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

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

                foreach (var item in doc.Descendants("MenuItem"))
                {
                    var name = ((string?)item.Element("ModItemName") ?? "").Trim();
                    if (string.IsNullOrWhiteSpace(name))
                        continue;

                    names.Add(name);
                }
            }

            return names
                .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
    }
}
