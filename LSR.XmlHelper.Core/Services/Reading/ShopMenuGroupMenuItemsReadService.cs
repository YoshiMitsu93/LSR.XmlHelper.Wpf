using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using LSR.XmlHelper.Core.Models;

namespace LSR.XmlHelper.Core.Services.Reading
{
    public sealed class ShopMenuGroupMenuItemsReadService
    {
        public IReadOnlyList<(int Index, string DisplayName)> GetMenusForGroupId(string rootFolderPath, string shopMenuGroupId)
        {
            if (string.IsNullOrWhiteSpace(rootFolderPath))
                return Array.Empty<(int, string)>();

            if (string.IsNullOrWhiteSpace(shopMenuGroupId))
                return Array.Empty<(int, string)>();

            if (!Directory.Exists(rootFolderPath))
                return Array.Empty<(int, string)>();

            var files = GetShopMenusFiles(rootFolderPath);

            for (int fileIndex = files.Length - 1; fileIndex >= 0; fileIndex--)
            {
                var file = files[fileIndex];
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

                var menus = group.Descendants("ShopMenu").ToArray();
                if (menus.Length == 0)
                    return Array.Empty<(int, string)>();

                var list = new List<(int, string)>();

                for (var i = 0; i < menus.Length; i++)
                {
                    var name = ((string?)menus[i].Element("Name") ?? "").Trim();
                    var id = ((string?)menus[i].Element("ID") ?? "").Trim();

                    if (string.IsNullOrWhiteSpace(name))
                        name = $"Dealer Menu {i + 1}";

                    var display = string.IsNullOrWhiteSpace(id)
                        ? $"{i + 1}: {name}"
                        : $"{i + 1}: {name} ({id})";

                    list.Add((i, display));
                }

                return list.ToArray();
            }

            return Array.Empty<(int, string)>();
        }

        public IReadOnlyList<DenInventoryMenuItem> GetItemsForGroupMenuIndex(string rootFolderPath, string shopMenuGroupId, int menuIndex)
        {
            if (string.IsNullOrWhiteSpace(rootFolderPath))
                return Array.Empty<DenInventoryMenuItem>();

            if (string.IsNullOrWhiteSpace(shopMenuGroupId))
                return Array.Empty<DenInventoryMenuItem>();

            if (!Directory.Exists(rootFolderPath))
                return Array.Empty<DenInventoryMenuItem>();

            var files = GetShopMenusFiles(rootFolderPath);

            for (int fileIndex = files.Length - 1; fileIndex >= 0; fileIndex--)
            {
                var file = files[fileIndex];
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

                var menus = group.Descendants("ShopMenu").ToArray();
                if (menus.Length == 0)
                    return Array.Empty<DenInventoryMenuItem>();

                var safeIndex = menuIndex;
                if (safeIndex < 0)
                    safeIndex = 0;
                if (safeIndex >= menus.Length)
                    safeIndex = menus.Length - 1;

                var menu = menus[safeIndex];

                var items = new List<DenInventoryMenuItem>();

                foreach (var mi in menu.Descendants("MenuItem"))
                {
                    var modItemName = ((string?)mi.Element("ModItemName") ?? "").Trim();
                    if (string.IsNullOrWhiteSpace(modItemName))
                        continue;

                    items.Add(new DenInventoryMenuItem
                    {
                        ModItemName = modItemName,
                        PurchasePrice = ReadInt(mi.Element("PurchasePrice")?.Value, 0),
                        SalesPrice = ReadInt(mi.Element("SalesPrice")?.Value, -1),
                        MinimumPurchaseAmount = ReadInt(mi.Element("MinimumPurchaseAmount")?.Value, 1),
                        MaximumPurchaseAmount = ReadInt(mi.Element("MaximumPurchaseAmount")?.Value, 10),
                        PurchaseIncrement = ReadInt(mi.Element("PurchaseIncrement")?.Value, 1),
                        NumberOfItemsToSellToPlayer = ReadInt(mi.Element("NumberOfItemsToSellToPlayer")?.Value, -1),
                        NumberOfItemsToPurchaseFromPlayer = ReadInt(mi.Element("NumberOfItemsToPurchaseFromPlayer")?.Value, -1),
                        IsIllicilt = ReadBool(mi.Element("IsIllicilt")?.Value, false),
                        IsFree = ReadBool(mi.Element("IsFree")?.Value, false),
                        SubPrice = ReadInt(mi.Element("SubPrice")?.Value, 1),
                        SubAmount = ReadInt(mi.Element("SubAmount")?.Value, 30),
                        NumberOfItemsSoldToPlayer = ReadInt(mi.Element("NumberOfItemsSoldToPlayer")?.Value, 0),
                        NumberOfItemsPurchasedByPlayer = ReadInt(mi.Element("NumberOfItemsPurchasedByPlayer")?.Value, 0)
                    });
                }

                return items
                    .OrderBy(x => x.ModItemName, StringComparer.OrdinalIgnoreCase)
                    .ToArray();
            }

            return Array.Empty<DenInventoryMenuItem>();
        }
        public IReadOnlyList<(int Index, string DisplayName)> GetMenusForGroupIdFromFile(string shopMenusFilePath, string shopMenuGroupId)
        {
            if (string.IsNullOrWhiteSpace(shopMenusFilePath))
                return Array.Empty<(int, string)>();

            if (string.IsNullOrWhiteSpace(shopMenuGroupId))
                return Array.Empty<(int, string)>();

            var effectiveLoader = new ShopMenusEffectiveDocumentService();
            var doc = effectiveLoader.LoadEffective(shopMenusFilePath);
            if (doc is null)
                return Array.Empty<(int, string)>();

            var group = doc
                .Descendants("ShopMenuGroup")
                .FirstOrDefault(x => string.Equals(((string?)x.Element("ID") ?? "").Trim(), shopMenuGroupId.Trim(), StringComparison.OrdinalIgnoreCase));

            if (group is null)
                return Array.Empty<(int, string)>();

            var menus = group.Descendants("ShopMenu").ToArray();
            if (menus.Length == 0)
                return Array.Empty<(int, string)>();

            var list = new List<(int, string)>();

            for (var i = 0; i < menus.Length; i++)
            {
                var name = ((string?)menus[i].Element("Name") ?? "").Trim();
                var id = ((string?)menus[i].Element("ID") ?? "").Trim();

                if (string.IsNullOrWhiteSpace(name))
                    name = $"Dealer Menu {i + 1}";

                var display = string.IsNullOrWhiteSpace(id)
                    ? $"{i + 1}: {name}"
                    : $"{i + 1}: {name} ({id})";

                list.Add((i, display));
            }

            return list.ToArray();
        }

        public IReadOnlyList<DenInventoryMenuItem> GetItemsForGroupMenuIndexFromFile(string shopMenusFilePath, string shopMenuGroupId, int menuIndex)
        {
            if (string.IsNullOrWhiteSpace(shopMenusFilePath))
                return Array.Empty<DenInventoryMenuItem>();

            if (string.IsNullOrWhiteSpace(shopMenuGroupId))
                return Array.Empty<DenInventoryMenuItem>();

            var effectiveLoader = new ShopMenusEffectiveDocumentService();
            var doc = effectiveLoader.LoadEffective(shopMenusFilePath);
            if (doc is null)
                return Array.Empty<DenInventoryMenuItem>();

            var group = doc
                .Descendants("ShopMenuGroup")
                .FirstOrDefault(x => string.Equals(((string?)x.Element("ID") ?? "").Trim(), shopMenuGroupId.Trim(), StringComparison.OrdinalIgnoreCase));

            if (group is null)
                return Array.Empty<DenInventoryMenuItem>();

            var menus = group.Descendants("ShopMenu").ToArray();
            if (menus.Length == 0)
                return Array.Empty<DenInventoryMenuItem>();

            var safeIndex = menuIndex;
            if (safeIndex < 0)
                safeIndex = 0;
            if (safeIndex >= menus.Length)
                safeIndex = menus.Length - 1;

            var menu = menus[safeIndex];

            var items = new List<DenInventoryMenuItem>();

            foreach (var mi in menu.Descendants("MenuItem"))
            {
                var modItemName = ((string?)mi.Element("ModItemName") ?? "").Trim();
                if (string.IsNullOrWhiteSpace(modItemName))
                    continue;

                items.Add(new DenInventoryMenuItem
                {
                    ModItemName = modItemName,
                    PurchasePrice = ReadInt(mi.Element("PurchasePrice")?.Value, 0),
                    SalesPrice = ReadInt(mi.Element("SalesPrice")?.Value, -1),
                    MinimumPurchaseAmount = ReadInt(mi.Element("MinimumPurchaseAmount")?.Value, 1),
                    MaximumPurchaseAmount = ReadInt(mi.Element("MaximumPurchaseAmount")?.Value, 10),
                    PurchaseIncrement = ReadInt(mi.Element("PurchaseIncrement")?.Value, 1),
                    NumberOfItemsToSellToPlayer = ReadInt(mi.Element("NumberOfItemsToSellToPlayer")?.Value, -1),
                    NumberOfItemsToPurchaseFromPlayer = ReadInt(mi.Element("NumberOfItemsToPurchaseFromPlayer")?.Value, -1),
                    IsIllicilt = ReadBool(mi.Element("IsIllicilt")?.Value, false),
                    IsFree = ReadBool(mi.Element("IsFree")?.Value, false),
                    SubPrice = ReadInt(mi.Element("SubPrice")?.Value, 1),
                    SubAmount = ReadInt(mi.Element("SubAmount")?.Value, 30),
                    NumberOfItemsSoldToPlayer = ReadInt(mi.Element("NumberOfItemsSoldToPlayer")?.Value, 0),
                    NumberOfItemsPurchasedByPlayer = ReadInt(mi.Element("NumberOfItemsPurchasedByPlayer")?.Value, 0)
                });
            }

            return items
                .OrderBy(x => x.ModItemName, StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }

        public IReadOnlyList<DenInventoryMenuItem> GetTemplateItemsForGroupId(string rootFolderPath, string shopMenuGroupId)
        {
            return GetItemsForGroupMenuIndex(rootFolderPath, shopMenuGroupId, 0);
        }
        private static string[] GetShopMenusFiles(string rootFolderPath)
        {
            var resolver = new LSR.XmlHelper.Core.Services.LsrFileSetResolverService();
            var resolved = resolver.ResolveShopMenus(rootFolderPath, "Default");

            return resolved.EnumerateReadOrder()
                .Where(p => !string.IsNullOrWhiteSpace(p) && File.Exists(p))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }
        private static int ReadInt(string? value, int fallback)
        {
            var v = (value ?? "").Trim();
            if (string.IsNullOrWhiteSpace(v))
                return fallback;

            return int.TryParse(v, NumberStyles.Integer, CultureInfo.InvariantCulture, out var i) ? i : fallback;
        }

        private static bool ReadBool(string? value, bool fallback)
        {
            var v = (value ?? "").Trim();
            if (string.IsNullOrWhiteSpace(v))
                return fallback;

            return bool.TryParse(v, out var b) ? b : fallback;
        }
    }
}
