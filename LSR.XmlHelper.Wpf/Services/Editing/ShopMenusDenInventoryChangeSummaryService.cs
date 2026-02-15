using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace LSR.XmlHelper.Wpf.Services.Editing
{
    public sealed class ShopMenusDenInventoryChangeSummaryService
    {
        public IReadOnlyList<string> Summarize(string beforeXml, string afterXml, string menuId)
        {
            menuId = (menuId ?? "").Trim();

            if (string.IsNullOrWhiteSpace(beforeXml) || string.IsNullOrWhiteSpace(afterXml) || string.IsNullOrWhiteSpace(menuId))
                return Array.Empty<string>();

            XDocument beforeDoc;
            XDocument afterDoc;

            try
            {
                beforeDoc = XDocument.Parse(beforeXml, LoadOptions.None);
                afterDoc = XDocument.Parse(afterXml, LoadOptions.None);
            }
            catch
            {
                return Array.Empty<string>();
            }

            var beforeMenu = FindMenu(beforeDoc, menuId);
            var afterMenu = FindMenu(afterDoc, menuId);

            if (beforeMenu is null || afterMenu is null)
                return new[] { "Den inventory: ShopMenu '" + menuId + "' could not be compared" };

            var beforeByItem = IndexByModItemName(beforeMenu);
            var afterByItem = IndexByModItemName(afterMenu);

            var allItems = beforeByItem.Keys.Union(afterByItem.Keys, StringComparer.OrdinalIgnoreCase)
                .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
                .ToList();

            var lines = new List<string>();

            foreach (var itemName in allItems)
            {
                beforeByItem.TryGetValue(itemName, out var b);
                afterByItem.TryGetValue(itemName, out var a);

                b ??= new List<MenuItemSnapshot>();
                a ??= new List<MenuItemSnapshot>();

                var beforeCounts = ToSignatureCounts(b);
                var afterCounts = ToSignatureCounts(a);

                var allSigs = beforeCounts.Keys.Union(afterCounts.Keys, StringComparer.OrdinalIgnoreCase)
                    .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
                    .ToList();

                foreach (var sig in allSigs)
                {
                    beforeCounts.TryGetValue(sig, out var bc);
                    afterCounts.TryGetValue(sig, out var ac);

                    if (bc < ac)
                    {
                        for (var i = 0; i < ac - bc; i++)
                            lines.Add("Den inventory[" + menuId + "]: added item " + itemName + " | " + sig);
                    }
                    else if (bc > ac)
                    {
                        for (var i = 0; i < bc - ac; i++)
                            lines.Add("Den inventory[" + menuId + "]: removed item " + itemName + " | " + sig);
                    }
                }

                if (b.Count == 1 && a.Count == 1)
                {
                    AddFieldDiff(lines, menuId, itemName, "PurchasePrice", b[0].PurchasePrice, a[0].PurchasePrice);
                    AddFieldDiff(lines, menuId, itemName, "SalesPrice", b[0].SalesPrice, a[0].SalesPrice);
                    AddFieldDiff(lines, menuId, itemName, "MinimumPurchaseAmount", b[0].MinimumPurchaseAmount, a[0].MinimumPurchaseAmount);
                    AddFieldDiff(lines, menuId, itemName, "MaximumPurchaseAmount", b[0].MaximumPurchaseAmount, a[0].MaximumPurchaseAmount);
                    AddFieldDiff(lines, menuId, itemName, "PurchaseIncrement", b[0].PurchaseIncrement, a[0].PurchaseIncrement);
                }
            }

            if (lines.Count == 0)
                lines.Add("Den inventory[" + menuId + "]: no effective change");

            return lines;
        }

        private static void AddFieldDiff(List<string> lines, string menuId, string itemName, string fieldName, string beforeValue, string afterValue)
        {
            beforeValue ??= "";
            afterValue ??= "";

            if (!string.Equals(beforeValue, afterValue, StringComparison.Ordinal))
                lines.Add("Den inventory[" + menuId + "]: " + itemName + " " + fieldName + ": '" + beforeValue + "' -> '" + afterValue + "'");
        }

        private static XElement? FindMenu(XDocument doc, string menuId)
        {
            return doc.Descendants("ShopMenu")
                .FirstOrDefault(x => string.Equals(((string?)x.Element("ID") ?? "").Trim(), menuId, StringComparison.OrdinalIgnoreCase));
        }

        private static Dictionary<string, List<MenuItemSnapshot>> IndexByModItemName(XElement menu)
        {
            var dict = new Dictionary<string, List<MenuItemSnapshot>>(StringComparer.OrdinalIgnoreCase);

            foreach (var mi in menu.Descendants("MenuItem"))
            {
                var modItem = ((string?)mi.Element("ModItemName") ?? "").Trim();
                if (string.IsNullOrWhiteSpace(modItem))
                    continue;

                var snap = new MenuItemSnapshot(
                    modItem,
                    ((string?)mi.Element("PurchasePrice") ?? "").Trim(),
                    ((string?)mi.Element("SalesPrice") ?? "").Trim(),
                    ((string?)mi.Element("MinimumPurchaseAmount") ?? "").Trim(),
                    ((string?)mi.Element("MaximumPurchaseAmount") ?? "").Trim(),
                    ((string?)mi.Element("PurchaseIncrement") ?? "").Trim());

                if (!dict.TryGetValue(modItem, out var list))
                {
                    list = new List<MenuItemSnapshot>();
                    dict[modItem] = list;
                }

                list.Add(snap);
            }

            return dict;
        }

        private static Dictionary<string, int> ToSignatureCounts(List<MenuItemSnapshot> items)
        {
            var dict = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            foreach (var i in items)
            {
                var sig =
                    "Min=" + (i.MinimumPurchaseAmount ?? "") +
                    " | Max=" + (i.MaximumPurchaseAmount ?? "") +
                    " | Inc=" + (i.PurchaseIncrement ?? "") +
                    " | Sell=" + (i.SalesPrice ?? "") +
                    " | PurchasePrice=" + (i.PurchasePrice ?? "");

                if (!dict.TryGetValue(sig, out var count))
                    count = 0;

                dict[sig] = count + 1;
            }

            return dict;
        }

        private sealed class MenuItemSnapshot
        {
            public MenuItemSnapshot(string modItemName, string purchasePrice, string salesPrice, string min, string max, string inc)
            {
                ModItemName = modItemName;
                PurchasePrice = purchasePrice;
                SalesPrice = salesPrice;
                MinimumPurchaseAmount = min;
                MaximumPurchaseAmount = max;
                PurchaseIncrement = inc;
            }

            public string ModItemName { get; }
            public string PurchasePrice { get; }
            public string SalesPrice { get; }
            public string MinimumPurchaseAmount { get; }
            public string MaximumPurchaseAmount { get; }
            public string PurchaseIncrement { get; }
        }
    }
}
