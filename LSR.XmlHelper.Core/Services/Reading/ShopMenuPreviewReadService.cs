using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace LSR.XmlHelper.Core.Services.Reading
{
    public sealed class ShopMenuPreviewReadService
    {
        public IReadOnlyList<string> GetShopMenuItemsForMenuId(string rootFolderPath, string shopMenuId)
        {
            if (string.IsNullOrWhiteSpace(rootFolderPath))
                return Array.Empty<string>();

            if (string.IsNullOrWhiteSpace(shopMenuId))
                return Array.Empty<string>();

            var path = Path.Combine(rootFolderPath, "ShopMenus.xml");
            if (!File.Exists(path))
                return Array.Empty<string>();

            try
            {
                var doc = XDocument.Load(path, LoadOptions.None);

                var menu = doc
                    .Descendants("ShopMenu")
                    .FirstOrDefault(x => string.Equals(((string?)x.Element("ID") ?? "").Trim(), shopMenuId, StringComparison.OrdinalIgnoreCase));

                if (menu is null)
                    return Array.Empty<string>();

                return menu
                    .Descendants("MenuItem")
                    .Select(x => ((string?)x.Element("ModItemName") ?? "").Trim())
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
                    .ToArray();
            }
            catch
            {
                return Array.Empty<string>();
            }
        }

        public IReadOnlyList<string> GetShopMenuGroupPreviewLines(string rootFolderPath, string shopMenuGroupId)
        {
            if (string.IsNullOrWhiteSpace(rootFolderPath))
                return Array.Empty<string>();

            if (string.IsNullOrWhiteSpace(shopMenuGroupId))
                return Array.Empty<string>();

            var path = Path.Combine(rootFolderPath, "ShopMenus.xml");
            if (!File.Exists(path))
                return Array.Empty<string>();

            try
            {
                var doc = XDocument.Load(path, LoadOptions.None);

                var group = doc
                    .Descendants("ShopMenuGroup")
                    .FirstOrDefault(x => string.Equals(((string?)x.Element("ID") ?? "").Trim(), shopMenuGroupId, StringComparison.OrdinalIgnoreCase));

                if (group is null)
                    return Array.Empty<string>();

                var menus = group
                    .Descendants("ShopMenu")
                    .Select(x =>
                    {
                        var id = ((string?)x.Element("ID") ?? "").Trim();
                        var name = ((string?)x.Element("Name") ?? "").Trim();
                        var itemCount = x.Descendants("MenuItem").Count();
                        return new { id, name, itemCount };
                    })
                    .Where(x => !string.IsNullOrWhiteSpace(x.id))
                    .ToList();

                var lines = new List<string>();

                foreach (var m in menus.OrderBy(x => x.name, StringComparer.OrdinalIgnoreCase))
                {
                    var title = string.IsNullOrWhiteSpace(m.name) ? m.id : $"{m.name} ({m.id})";
                    lines.Add($"{title} - {m.itemCount} items");
                }

                return lines.ToArray();
            }
            catch
            {
                return Array.Empty<string>();
            }
        }
    }
}
