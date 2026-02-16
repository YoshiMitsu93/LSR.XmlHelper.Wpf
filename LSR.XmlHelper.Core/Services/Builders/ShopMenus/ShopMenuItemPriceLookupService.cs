using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace LSR.XmlHelper.Core.Services.Builders
{
    public sealed class ShopMenuItemPriceLookupService
    {
        public bool TryGetFirstPrices(string rootFolderPath, string modItemName, out int purchasePrice, out int salesPrice)
        {
            purchasePrice = 0;
            salesPrice = 0;

            if (string.IsNullOrWhiteSpace(rootFolderPath))
                return false;

            if (!Directory.Exists(rootFolderPath))
                return false;

            modItemName = (modItemName ?? "").Trim();
            if (string.IsNullOrWhiteSpace(modItemName))
                return false;

            var resolver = new LSR.XmlHelper.Core.Services.LsrFileSetResolverService();
            var resolved = resolver.ResolveShopMenus(rootFolderPath, "Default");

            var files = resolved.EnumerateReadOrder()
                .Where(p => !string.IsNullOrWhiteSpace(p) && File.Exists(p))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var foundAnyBuy = false;
            var foundAnySell = false;
            var firstBuy = 0;
            var firstSell = 0;

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

                foreach (var menuItem in doc.Descendants("MenuItem"))
                {
                    var itemName = ((string?)menuItem.Element("ModItemName") ?? "").Trim();
                    if (!string.Equals(itemName, modItemName, StringComparison.OrdinalIgnoreCase))
                        continue;

                    if (!foundAnyBuy)
                    {
                        if (TryReadInt(menuItem.Element("PurchasePrice")?.Value, out var buy))
                        {
                            firstBuy = buy;
                            foundAnyBuy = true;
                        }
                    }

                    if (!foundAnySell)
                    {
                        if (TryReadInt(menuItem.Element("SalesPrice")?.Value, out var sell) && sell >= 0)
                        {
                            firstSell = sell;
                            foundAnySell = true;
                        }
                    }

                    if (foundAnyBuy && foundAnySell)
                    {
                        purchasePrice = firstBuy;
                        salesPrice = firstSell;
                        return true;
                    }
                }
            }

            if (foundAnyBuy)
            {
                purchasePrice = firstBuy;
                salesPrice = foundAnySell ? firstSell : (int)Math.Round(firstBuy * 0.4, MidpointRounding.AwayFromZero);
                return true;
            }

            return false;
        }

        private static bool TryReadInt(string? value, out int result)
        {
            result = 0;
            var v = (value ?? "").Trim();
            if (string.IsNullOrWhiteSpace(v))
                return false;

            return int.TryParse(v, NumberStyles.Integer, CultureInfo.InvariantCulture, out result);
        }
    }
}
