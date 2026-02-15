using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace LSR.XmlHelper.Wpf.Services.Editing
{
    public sealed class ShopMenusDealerGroupChangeSummaryService
    {
        public IReadOnlyList<string> Summarize(string beforeXml, string afterXml, string groupId)
        {
            groupId = (groupId ?? "").Trim();

            if (string.IsNullOrWhiteSpace(beforeXml) || string.IsNullOrWhiteSpace(afterXml) || string.IsNullOrWhiteSpace(groupId))
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

            var beforeGroup = FindGroup(beforeDoc, groupId);
            var afterGroup = FindGroup(afterDoc, groupId);

            if (beforeGroup is null || afterGroup is null)
                return new[] { "Dealer menus: ShopMenuGroup '" + groupId + "' could not be compared" };

            var beforeMenus = beforeGroup.Descendants("PercentageSelectShopMenu").ToList();
            var afterMenus = afterGroup.Descendants("PercentageSelectShopMenu").ToList();

            var count = Math.Min(beforeMenus.Count, afterMenus.Count);
            var lines = new List<string>();

            for (var i = 0; i < count; i++)
            {
                var bMenu = beforeMenus[i].Element("ShopMenu");
                var aMenu = afterMenus[i].Element("ShopMenu");

                if (bMenu is null || aMenu is null)
                    continue;

                var beforeItems = IndexMenuItems(bMenu);
                var afterItems = IndexMenuItems(aMenu);

                var allKeys = beforeItems.Keys.Union(afterItems.Keys, StringComparer.OrdinalIgnoreCase).OrderBy(x => x).ToList();

                foreach (var key in allKeys)
                {
                    beforeItems.TryGetValue(key, out var b);
                    afterItems.TryGetValue(key, out var a);

                    if (b is null && a is not null)
                    {
                        lines.Add("Dealer menus[" + groupId + "]/MenuIndex " + i + ": added item " + key);
                        continue;
                    }

                    if (b is not null && a is null)
                    {
                        lines.Add("Dealer menus[" + groupId + "]/MenuIndex " + i + ": removed item " + key);
                        continue;
                    }

                    if (b is null || a is null)
                        continue;

                    foreach (var field in a.Keys.OrderBy(x => x))
                    {
                        var av = a[field];
                        b.TryGetValue(field, out var bv);
                        bv ??= "";

                        if (!string.Equals(bv, av, StringComparison.Ordinal))
                            lines.Add("Dealer menus[" + groupId + "]/MenuIndex " + i + ": " + key + " " + field + ": '" + bv + "' -> '" + av + "'");
                    }
                }
            }

            if (lines.Count == 0)
                lines.Add("Dealer menus[" + groupId + "]: no effective change");

            return lines;
        }

        private static XElement? FindGroup(XDocument doc, string groupId)
        {
            return doc.Descendants("ShopMenuGroup")
                .FirstOrDefault(x => string.Equals(((string?)x.Element("ID") ?? "").Trim(), groupId, StringComparison.OrdinalIgnoreCase));
        }

        private static Dictionary<string, Dictionary<string, string>> IndexMenuItems(XElement menu)
        {
            var dict = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);

            foreach (var mi in menu.Descendants("MenuItem"))
            {
                var modItem = ((string?)mi.Element("ModItemName") ?? "").Trim();
                var min = ((string?)mi.Element("Min") ?? "").Trim();
                var max = ((string?)mi.Element("Max") ?? "").Trim();
                var inc = ((string?)mi.Element("Increment") ?? "").Trim();

                var key = (modItem + " | Min=" + min + " | Max=" + max + " | Inc=" + inc).Trim();
                if (string.IsNullOrWhiteSpace(modItem))
                    continue;

                var fields = mi.Elements()
                    .ToDictionary(e => e.Name.LocalName, e => (e.Value ?? "").Trim(), StringComparer.OrdinalIgnoreCase);

                dict[key] = fields;
            }

            return dict;
        }
    }
}
