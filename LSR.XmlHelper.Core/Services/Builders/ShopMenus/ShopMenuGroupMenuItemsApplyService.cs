using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using LSR.XmlHelper.Core.Models;

namespace LSR.XmlHelper.Core.Services.Builders
{
    public sealed class ShopMenuGroupMenuItemsApplyService
    {
        public void ApplyItemsToGroup(XDocument shopMenusDoc, string shopMenuGroupId, IReadOnlyList<DenInventoryMenuItem> items)
        {
            if (shopMenusDoc.Root is null)
                return;

            if (string.IsNullOrWhiteSpace(shopMenuGroupId))
                return;

            var safeItems = (items ?? Array.Empty<DenInventoryMenuItem>())
                .Where(x => !string.IsNullOrWhiteSpace(x.ModItemName))
                .ToArray();

            var group = shopMenusDoc
                .Descendants("ShopMenuGroup")
                .FirstOrDefault(x => string.Equals(((string?)x.Element("ID") ?? "").Trim(), shopMenuGroupId.Trim(), StringComparison.OrdinalIgnoreCase));

            if (group is not null)
            {
                foreach (var menu in group.Descendants("ShopMenu"))
                    ReplaceMenuItems(menu, safeItems);

                return;
            }

            foreach (var menu in shopMenusDoc.Descendants("ShopMenu"))
            {
                var groupName = ((string?)menu.Element("GroupName") ?? "").Trim();
                if (!string.Equals(groupName, shopMenuGroupId.Trim(), StringComparison.OrdinalIgnoreCase))
                    continue;

                ReplaceMenuItems(menu, safeItems);
            }
        }

        public void ApplyItemsToGroupMenuIndex(XDocument shopMenusDoc, string shopMenuGroupId, int menuIndex, IReadOnlyList<DenInventoryMenuItem> items)
        {
            if (shopMenusDoc.Root is null)
                return;

            if (string.IsNullOrWhiteSpace(shopMenuGroupId))
                return;

            var safeItems = (items ?? Array.Empty<DenInventoryMenuItem>())
                .Where(x => !string.IsNullOrWhiteSpace(x.ModItemName))
                .ToArray();

            var group = shopMenusDoc
                .Descendants("ShopMenuGroup")
                .FirstOrDefault(x => string.Equals(((string?)x.Element("ID") ?? "").Trim(), shopMenuGroupId.Trim(), StringComparison.OrdinalIgnoreCase));

            if (group is not null)
            {
                var menus = group.Descendants("ShopMenu").ToArray();
                var safeIndex = menuIndex;
                if (safeIndex < 0)
                    safeIndex = 0;
                if (safeIndex >= menus.Length)
                    safeIndex = menus.Length - 1;

                if (menus.Length > 0)
                    ReplaceMenuItems(menus[safeIndex], safeItems);

                return;
            }

            var groupedMenus = shopMenusDoc
                .Descendants("ShopMenu")
                .Where(m => string.Equals((((string?)m.Element("GroupName")) ?? "").Trim(), shopMenuGroupId.Trim(), StringComparison.OrdinalIgnoreCase))
                .ToArray();

            if (groupedMenus.Length == 0)
                return;

            var safeIndex2 = menuIndex;
            if (safeIndex2 < 0)
                safeIndex2 = 0;
            if (safeIndex2 >= groupedMenus.Length)
                safeIndex2 = groupedMenus.Length - 1;

            ReplaceMenuItems(groupedMenus[safeIndex2], safeItems);
        }

        private static void ReplaceMenuItems(XElement shopMenu, IReadOnlyList<DenInventoryMenuItem> items)
        {
            var itemsNode = shopMenu.Element("Items");
            if (itemsNode is null)
            {
                shopMenu.Add(new XElement("Items"));
                itemsNode = shopMenu.Element("Items");
            }

            if (itemsNode is null)
                return;

            itemsNode.RemoveNodes();

            foreach (var item in items)
                itemsNode.Add(CreateMenuItem(item));
        }

        private static XElement CreateMenuItem(DenInventoryMenuItem item)
        {
            return new XElement("MenuItem",
                new XElement("NumberOfItemsSoldToPlayer", item.NumberOfItemsSoldToPlayer),
                new XElement("NumberOfItemsPurchasedByPlayer", item.NumberOfItemsPurchasedByPlayer),
                new XElement("ModItemName", item.ModItemName),
                new XElement("PurchasePrice", item.PurchasePrice),
                new XElement("SalesPrice", item.SalesPrice),
                new XElement("IsIllicilt", item.IsIllicilt),
                new XElement("Extras"),
                new XElement("SubPrice", item.SubPrice),
                new XElement("SubAmount", item.SubAmount),
                new XElement("MinimumPurchaseAmount", item.MinimumPurchaseAmount),
                new XElement("MaximumPurchaseAmount", item.MaximumPurchaseAmount),
                new XElement("PurchaseIncrement", item.PurchaseIncrement),
                new XElement("NumberOfItemsToSellToPlayer", item.NumberOfItemsToSellToPlayer),
                new XElement("NumberOfItemsToPurchaseFromPlayer", item.NumberOfItemsToPurchaseFromPlayer),
                new XElement("IsFree", item.IsFree)
            );
        }
    }
}
