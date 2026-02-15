using LSR.XmlHelper.Wpf.Services.EditHistory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace LSR.XmlHelper.Wpf.Services.Compare
{
    public sealed class ShopMenusRawDiffService
    {
        public List<EditHistoryItem> BuildAddEdits(string currentXmlText, string externalXmlText, string currentFilePath)
        {
            var currentMenus = ExtractShopMenus(currentXmlText);
            var externalMenus = ExtractShopMenus(externalXmlText);

            var existingKeys = new HashSet<string>(currentMenus.Select(x => x.Key), StringComparer.OrdinalIgnoreCase);

            var occurrenceMap = currentMenus
                .GroupBy(x => x.Key, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.Count(), StringComparer.OrdinalIgnoreCase);

            var edits = new List<EditHistoryItem>();

            foreach (var m in externalMenus)
            {
                if (existingKeys.Contains(m.Key))
                    continue;

                occurrenceMap.TryGetValue(m.Key, out var occ);
                var occurrence = occ;
                occurrenceMap[m.Key] = occ + 1;

                edits.Add(new EditHistoryItem
                {
                    Operation = EditHistoryOperation.AddEntry,
                    FilePath = currentFilePath,
                    CollectionTitle = "ShopMenuList",
                    EntryKey = m.Key,
                    EntryOccurrence = occurrence,
                    FieldPath = "ADD_ENTRY",
                    OldValue = m.Display,
                    NewValue = m.Element.ToString(SaveOptions.DisableFormatting)
                });
            }

            return edits;
        }

        private static List<ShopMenuInfo> ExtractShopMenus(string xmlText)
        {
            XDocument doc;
            try
            {
                doc = XDocument.Parse(xmlText, LoadOptions.None);
            }
            catch
            {
                return new List<ShopMenuInfo>();
            }

            var root = doc.Root;
            if (root is null)
                return new List<ShopMenuInfo>();

            var shopMenus = new List<ShopMenuInfo>();

            foreach (var list in root.Descendants().Where(x => string.Equals(x.Name.LocalName, "ShopMenuList", StringComparison.OrdinalIgnoreCase)))
            {
                foreach (var menu in list.Elements().Where(x => string.Equals(x.Name.LocalName, "ShopMenu", StringComparison.OrdinalIgnoreCase)))
                {
                    var name = menu.Elements().FirstOrDefault(x => string.Equals(x.Name.LocalName, "Name", StringComparison.OrdinalIgnoreCase))?.Value ?? "";
                    var id = menu.Elements().FirstOrDefault(x => string.Equals(x.Name.LocalName, "ID", StringComparison.OrdinalIgnoreCase))?.Value ?? "";

                    var key = !string.IsNullOrWhiteSpace(id) ? id : name;
                    if (string.IsNullOrWhiteSpace(key))
                        key = "ShopMenu";

                    var display = !string.IsNullOrWhiteSpace(name) ? name : (!string.IsNullOrWhiteSpace(id) ? id : key);

                    shopMenus.Add(new ShopMenuInfo(key, display, menu));
                }
            }

            return shopMenus;
        }

        private sealed class ShopMenuInfo
        {
            public ShopMenuInfo(string key, string display, XElement element)
            {
                Key = key ?? "";
                Display = display ?? "";
                Element = element;
            }

            public string Key { get; }
            public string Display { get; }
            public XElement Element { get; }
        }
    }
}
