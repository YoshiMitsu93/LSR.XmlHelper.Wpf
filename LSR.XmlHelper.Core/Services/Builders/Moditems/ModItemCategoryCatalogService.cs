using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace LSR.XmlHelper.Core.Services.Builders
{
    public sealed class ModItemCategoryCatalogService
    {
        public IReadOnlyList<(string Name, string Category)> GetAllItemsWithCategories(string rootFolderPath)
        {
            if (string.IsNullOrWhiteSpace(rootFolderPath))
                return Array.Empty<(string Name, string Category)>();

            if (!Directory.Exists(rootFolderPath))
                return Array.Empty<(string Name, string Category)>();

            var files = CollectFiles(rootFolderPath);

            var byName = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            LoadFromModAndPhysicalItems(files, byName);
            ImproveUsingShopMenus(files, byName);

            return byName
                .Select(x => (x.Key, x.Value))
                .OrderBy(x => x.Key, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private static List<string> CollectFiles(string rootFolderPath)
        {
            var files = new List<string>();

            var resolver = new LSR.XmlHelper.Core.Services.LsrFileSetResolverService();

            var modItems = resolver.ResolveModItems(rootFolderPath, "Default");
            var physicalItems = resolver.ResolvePhysicalItems(rootFolderPath, "Default");
            var shopMenus = resolver.ResolveShopMenus(rootFolderPath, "Default");

            files.AddRange(modItems.EnumerateReadOrder());
            files.AddRange(physicalItems.EnumerateReadOrder());
            files.AddRange(shopMenus.EnumerateReadOrder());

            return files
                .Select(p => new FileInfo(p))
                .Where(f => f.Exists)
                .OrderByDescending(f => f.Name, StringComparer.OrdinalIgnoreCase)
                .Select(f => f.FullName)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private static void LoadFromModAndPhysicalItems(List<string> files, Dictionary<string, string> byName)
        {
            foreach (var file in files)
            {
                var fn = Path.GetFileName(file);
                if (!fn.StartsWith("ModItems", StringComparison.OrdinalIgnoreCase) && !fn.StartsWith("PhysicalItems", StringComparison.OrdinalIgnoreCase))
                    continue;

                XDocument doc;
                try
                {
                    doc = XDocument.Load(file, LoadOptions.None);
                }
                catch
                {
                    continue;
                }

                foreach (var item in doc.Descendants())
                {
                    var name = ((string?)item.Element("Name") ?? "").Trim();
                    if (string.IsNullOrWhiteSpace(name))
                        continue;

                    var itemType = ((string?)item.Element("ItemType") ?? "").Trim();
                    var category = NormalizeCategoryFromItemType(itemType);

                    if (string.IsNullOrWhiteSpace(category))
                        category = "Other";

                    if (!byName.TryGetValue(name, out var existing))
                    {
                        byName[name] = category;
                        continue;
                    }

                    if (IsHigherPriority(category, existing))
                        byName[name] = category;
                }
            }
        }

        private static void ImproveUsingShopMenus(List<string> files, Dictionary<string, string> byName)
        {
            foreach (var file in files)
            {
                var fn = Path.GetFileName(file);
                if (!fn.StartsWith("ShopMenus", StringComparison.OrdinalIgnoreCase))
                    continue;

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
                    var menuId = ((string?)menu.Element("ID") ?? "").Trim();
                    var menuName = ((string?)menu.Element("Name") ?? "").Trim();
                    var groupName = ((string?)menu.Element("GroupName") ?? "").Trim();

                    var intent = InferCategoryFromShopMenuIntent($"{menuId} {menuName} {groupName}");
                    if (string.IsNullOrWhiteSpace(intent))
                        continue;

                    foreach (var menuItem in menu.Descendants("MenuItem"))
                    {
                        var itemName = ((string?)menuItem.Element("ModItemName") ?? "").Trim();
                        if (string.IsNullOrWhiteSpace(itemName))
                            continue;

                        if (!byName.TryGetValue(itemName, out var existing))
                        {
                            byName[itemName] = intent;
                            continue;
                        }

                        if (IsHigherPriority(intent, existing))
                            byName[itemName] = intent;
                    }
                }
            }
        }

        private static string NormalizeCategoryFromItemType(string itemType)
        {
            if (string.IsNullOrWhiteSpace(itemType))
                return "Other";

            if (itemType.IndexOf("Vehicle", StringComparison.OrdinalIgnoreCase) >= 0)
                return "Vehicles";

            if (itemType.IndexOf("Ammo", StringComparison.OrdinalIgnoreCase) >= 0 || itemType.IndexOf("Ammunition", StringComparison.OrdinalIgnoreCase) >= 0)
                return "Weapons";

            if (itemType.IndexOf("Weapon", StringComparison.OrdinalIgnoreCase) >= 0)
                return "Weapons";

            if (itemType.IndexOf("Drug", StringComparison.OrdinalIgnoreCase) >= 0 || itemType.IndexOf("Intoxic", StringComparison.OrdinalIgnoreCase) >= 0)
                return "Drugs";

            if (itemType.IndexOf("Food", StringComparison.OrdinalIgnoreCase) >= 0 || itemType.IndexOf("Drink", StringComparison.OrdinalIgnoreCase) >= 0)
                return "Food";

            if (itemType.IndexOf("Tool", StringComparison.OrdinalIgnoreCase) >= 0)
                return "Tools";

            if (itemType.IndexOf("Equipment", StringComparison.OrdinalIgnoreCase) >= 0)
                return "Equipment";

            return "Other";
        }

        private static string InferCategoryFromShopMenuIntent(string context)
        {
            if (string.IsNullOrWhiteSpace(context))
                return "";

            if (context.IndexOf("Ammunition", StringComparison.OrdinalIgnoreCase) >= 0 || context.IndexOf("Ammo", StringComparison.OrdinalIgnoreCase) >= 0)
                return "Weapons";

            if (context.IndexOf("Weapon", StringComparison.OrdinalIgnoreCase) >= 0 || context.IndexOf("Gun", StringComparison.OrdinalIgnoreCase) >= 0 || context.IndexOf("Armory", StringComparison.OrdinalIgnoreCase) >= 0)
                return "Weapons";

            if (context.IndexOf("Vehicle", StringComparison.OrdinalIgnoreCase) >= 0 || context.IndexOf("Garage", StringComparison.OrdinalIgnoreCase) >= 0)
                return "Vehicles";

            if (context.IndexOf("Drug", StringComparison.OrdinalIgnoreCase) >= 0 || context.IndexOf("Dealer", StringComparison.OrdinalIgnoreCase) >= 0)
                return "Drugs";

            if (context.IndexOf("Food", StringComparison.OrdinalIgnoreCase) >= 0 || context.IndexOf("Restaurant", StringComparison.OrdinalIgnoreCase) >= 0 || context.IndexOf("Diner", StringComparison.OrdinalIgnoreCase) >= 0 || context.IndexOf("Burger", StringComparison.OrdinalIgnoreCase) >= 0)
                return "Food";

            if (context.IndexOf("Tool", StringComparison.OrdinalIgnoreCase) >= 0 || context.IndexOf("Hardware", StringComparison.OrdinalIgnoreCase) >= 0 || context.IndexOf("Repair", StringComparison.OrdinalIgnoreCase) >= 0)
                return "Tools";

            if (context.IndexOf("Equipment", StringComparison.OrdinalIgnoreCase) >= 0 || context.IndexOf("Armor", StringComparison.OrdinalIgnoreCase) >= 0)
                return "Equipment";

            return "";
        }

        private static bool IsHigherPriority(string candidate, string existing)
        {
            return CategoryPriority(candidate) > CategoryPriority(existing);
        }

        private static int CategoryPriority(string category)
        {
            if (string.Equals(category, "Vehicles", StringComparison.OrdinalIgnoreCase))
                return 80;

            if (string.Equals(category, "Weapons", StringComparison.OrdinalIgnoreCase))
                return 60;

            if (string.Equals(category, "Drugs", StringComparison.OrdinalIgnoreCase))
                return 50;

            if (string.Equals(category, "Food", StringComparison.OrdinalIgnoreCase))
                return 40;

            if (string.Equals(category, "Tools", StringComparison.OrdinalIgnoreCase))
                return 30;

            if (string.Equals(category, "Equipment", StringComparison.OrdinalIgnoreCase))
                return 20;

            return 10;
        }
    }
}
