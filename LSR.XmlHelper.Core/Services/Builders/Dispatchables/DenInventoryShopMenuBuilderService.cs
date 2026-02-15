using LSR.XmlHelper.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace LSR.XmlHelper.Core.Services.Builders
{
    public sealed class DenInventoryShopMenuBuilderService
    {
        public XDocument CreateOrMergeInto(XDocument? existing, string groupId, string groupName, string menuId, string menuName, IReadOnlyList<DenInventoryMenuItem> items)
        {
            var doc = existing ?? CreateEmptyDocument();

            var menuList = doc.Root?.Element("ShopMenuList");
            if (menuList is null)
                throw new InvalidOperationException("ShopMenus document is missing ShopMenuList.");

            var existingMenu = menuList
                .Elements("ShopMenu")
                .FirstOrDefault(x => string.Equals(((string?)x.Element("ID") ?? "").Trim(), menuId.Trim(), StringComparison.OrdinalIgnoreCase));

            existingMenu?.Remove();

            menuList.Add(CreateMenu(menuId, menuName, items));

            return doc;
        }

        private static XDocument CreateEmptyDocument()
        {
            return new XDocument(
                new XDeclaration("1.0", "utf-8", "yes"),
                new XElement("ShopMenuTypes",
                    new XElement("ShopMenuGroupList"),
                    new XElement("ShopMenuList")
                )
            );
        }

        private static XElement CreateMenu(string menuId, string menuName, IReadOnlyList<DenInventoryMenuItem> items)
        {
            return new XElement("ShopMenu",
                new XElement("ID", menuId),
                new XElement("Name", menuName),
                new XElement("GroupName", ""),
                new XElement("Items",
                    items.Select(CreateMenuItem)
                )
            );
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
