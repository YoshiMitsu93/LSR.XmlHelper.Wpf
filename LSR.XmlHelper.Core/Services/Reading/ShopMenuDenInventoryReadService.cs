using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using LSR.XmlHelper.Core.Models;

namespace LSR.XmlHelper.Core.Services.Reading
{
    public sealed class ShopMenuDenInventoryReadService
    {
        public IReadOnlyList<DenInventoryMenuItem> GetDenInventoryItemsForMenuId(string rootFolderPath, string shopMenuId)
        {
            if (string.IsNullOrWhiteSpace(rootFolderPath))
                return Array.Empty<DenInventoryMenuItem>();

            if (string.IsNullOrWhiteSpace(shopMenuId))
                return Array.Empty<DenInventoryMenuItem>();

            var files = GetShopMenusFiles(rootFolderPath);
            if (files.Count == 0)
                return Array.Empty<DenInventoryMenuItem>();

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
                        SalesPrice = ReadInt(mi.Element("SalesPrice")?.Value, 0),
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

                return items.ToArray();
            }

            return Array.Empty<DenInventoryMenuItem>();
        }

        private static List<string> GetShopMenusFiles(string rootFolderPath)
        {
            var resolver = new LSR.XmlHelper.Core.Services.LsrFileSetResolverService();
            var resolved = resolver.ResolveShopMenus(rootFolderPath, "Default");

            return resolved.EnumerateReadOrder()
                .Where(p => !string.IsNullOrWhiteSpace(p) && File.Exists(p))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private static int ReadInt(string? value, int fallback)
        {
            var v = (value ?? "").Trim();
            if (string.IsNullOrWhiteSpace(v))
                return fallback;

            return int.TryParse(v, out var i) ? i : fallback;
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
