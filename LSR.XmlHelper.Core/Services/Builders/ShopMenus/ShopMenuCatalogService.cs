using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace LSR.XmlHelper.Core.Services.Builders
{
    public sealed class ShopMenuCatalogService
    {
        public IReadOnlyList<(string Id, string Name)> GetShopMenusForGroup(string rootFolderPath, string groupId)
        {
            if (string.IsNullOrWhiteSpace(rootFolderPath))
                return Array.Empty<(string Id, string Name)>();

            if (!Directory.Exists(rootFolderPath))
                return Array.Empty<(string Id, string Name)>();

            if (string.IsNullOrWhiteSpace(groupId))
                return Array.Empty<(string Id, string Name)>();

            var resolver = new LSR.XmlHelper.Core.Services.LsrFileSetResolverService();
            var resolved = resolver.ResolveShopMenus(rootFolderPath, "Default");

            var files = resolved
                .EnumerateReadOrder()
                .Where(p => !string.IsNullOrWhiteSpace(p) && File.Exists(p))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var embeddedMenus = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var referencedIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

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

                var group = doc
                    .Descendants("ShopMenuGroup")
                    .FirstOrDefault(x => string.Equals(((string?)x.Element("ID") ?? "").Trim(), groupId.Trim(), StringComparison.OrdinalIgnoreCase));

                if (group is null)
                    continue;

                foreach (var menu in group.Descendants("ShopMenu"))
                {
                    var id = ((string?)menu.Element("ID") ?? "").Trim();
                    if (string.IsNullOrWhiteSpace(id))
                        continue;

                    var name = ((string?)menu.Element("Name") ?? "").Trim();
                    if (string.IsNullOrWhiteSpace(name))
                        name = id;

                    embeddedMenus[id] = name;
                }

                foreach (var e in group.Descendants())
                {
                    var local = e.Name.LocalName;
                    if (string.Equals(local, "ShopMenuID", StringComparison.OrdinalIgnoreCase) || string.Equals(local, "ShopMenuId", StringComparison.OrdinalIgnoreCase))
                    {
                        var v = (e.Value ?? "").Trim();
                        if (!string.IsNullOrWhiteSpace(v))
                            referencedIds.Add(v);
                    }
                }
            }

            var menuById = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var kvp in embeddedMenus)
                menuById[kvp.Key] = kvp.Value;

            if (referencedIds.Count > 0)
            {
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

                    foreach (var menu in doc.Descendants("ShopMenu"))
                    {
                        var id = ((string?)menu.Element("ID") ?? "").Trim();
                        if (string.IsNullOrWhiteSpace(id))
                            continue;

                        if (!referencedIds.Contains(id))
                            continue;

                        var name = ((string?)menu.Element("Name") ?? "").Trim();
                        if (string.IsNullOrWhiteSpace(name))
                            name = id;

                        menuById[id] = name;
                    }
                }
            }

            return menuById
                .Select(x => (x.Key, x.Value))
                .OrderBy(x => x.Value, StringComparer.OrdinalIgnoreCase)
                .ThenBy(x => x.Key, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
    }
}
