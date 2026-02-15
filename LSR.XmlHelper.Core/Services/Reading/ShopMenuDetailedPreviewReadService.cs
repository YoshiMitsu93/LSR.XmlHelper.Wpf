using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace LSR.XmlHelper.Core.Services.Reading
{
    public sealed class ShopMenuDetailedPreviewReadService
    {
        public IReadOnlyList<string> GetShopMenuItemDetailLinesForMenuId(string rootFolderPath, string shopMenuId)
        {
            if (string.IsNullOrWhiteSpace(rootFolderPath))
                return Array.Empty<string>();

            if (string.IsNullOrWhiteSpace(shopMenuId))
                return Array.Empty<string>();

            var files = GetShopMenusFiles(rootFolderPath);
            if (files.Count == 0)
                return Array.Empty<string>();

            var lines = new List<string>();

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

                var menu = doc
                    .Descendants("ShopMenu")
                    .FirstOrDefault(x => string.Equals(((string?)x.Element("ID") ?? "").Trim(), shopMenuId.Trim(), StringComparison.OrdinalIgnoreCase));

                if (menu is null)
                    continue;

                foreach (var mi in menu.Descendants("MenuItem"))
                {
                    var itemName = ((string?)mi.Element("ModItemName") ?? "").Trim();
                    if (string.IsNullOrWhiteSpace(itemName))
                        continue;

                    var buy = ReadInt(mi.Element("PurchasePrice")?.Value);
                    var sell = ReadInt(mi.Element("SalesPrice")?.Value);

                    var buyText = buy.HasValue ? buy.Value.ToString(CultureInfo.InvariantCulture) : "?";
                    var sellText = sell.HasValue ? sell.Value.ToString(CultureInfo.InvariantCulture) : "?";

                    lines.Add($"{itemName} | Buy: {buyText} | Sell: {sellText}");
                }

                break;
            }

            return lines
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }

        public IReadOnlyList<string> GetShopMenuGroupItemDetailLines(string rootFolderPath, string shopMenuGroupId)
        {
            if (string.IsNullOrWhiteSpace(rootFolderPath))
                return Array.Empty<string>();

            if (string.IsNullOrWhiteSpace(shopMenuGroupId))
                return Array.Empty<string>();

            var files = GetShopMenusFiles(rootFolderPath);
            if (files.Count == 0)
                return Array.Empty<string>();

            var lines = new List<string>();

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

                var group = doc
                    .Descendants("ShopMenuGroup")
                    .FirstOrDefault(x => string.Equals(((string?)x.Element("ID") ?? "").Trim(), shopMenuGroupId.Trim(), StringComparison.OrdinalIgnoreCase));

                if (group is null)
                    continue;

                var menus = group
                    .Descendants("ShopMenu")
                    .Select(m => new
                    {
                        Id = ((string?)m.Element("ID") ?? "").Trim(),
                        Name = ((string?)m.Element("Name") ?? "").Trim(),
                        Menu = m
                    })
                    .Where(x => !string.IsNullOrWhiteSpace(x.Id))
                    .ToList();

                foreach (var menu in menus.OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase))
                {
                    var title = string.IsNullOrWhiteSpace(menu.Name) ? menu.Id : $"{menu.Name} ({menu.Id})";
                    lines.Add(title);

                    foreach (var mi in menu.Menu.Descendants("MenuItem"))
                    {
                        var itemName = ((string?)mi.Element("ModItemName") ?? "").Trim();
                        if (string.IsNullOrWhiteSpace(itemName))
                            continue;

                        var buy = ReadInt(mi.Element("PurchasePrice")?.Value);
                        var sell = ReadInt(mi.Element("SalesPrice")?.Value);

                        var buyText = buy.HasValue ? buy.Value.ToString(CultureInfo.InvariantCulture) : "?";
                        var sellText = sell.HasValue ? sell.Value.ToString(CultureInfo.InvariantCulture) : "?";

                        lines.Add($"  - {itemName} | Buy: {buyText} | Sell: {sellText}");
                    }
                }

                break;
            }

            return lines.ToArray();
        }

        private static List<string> GetShopMenusFiles(string rootFolderPath)
        {
            if (string.IsNullOrWhiteSpace(rootFolderPath) || !Directory.Exists(rootFolderPath))
                return new List<string>();

            var resolver = new LSR.XmlHelper.Core.Services.LsrFileSetResolverService();
            var resolved = resolver.ResolveShopMenus(rootFolderPath, "Default");

            return resolved.EnumerateReadOrder()
                .Where(p => !string.IsNullOrWhiteSpace(p) && File.Exists(p))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private static int? ReadInt(string? value)
        {
            var v = (value ?? "").Trim();
            if (string.IsNullOrWhiteSpace(v))
                return null;

            return int.TryParse(v, NumberStyles.Integer, CultureInfo.InvariantCulture, out var i) ? i : null;
        }
    }
}
