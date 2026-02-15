using LSR.XmlHelper.Wpf.Services.EditHistory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace LSR.XmlHelper.Wpf.Services.Compare
{
    public sealed class CompareSelectionDetailsService
    {
        public CompareSelectionDetails Build(EditHistoryItem? item)
        {
            if (item is null)
                return CompareSelectionDetails.Empty;

            if (item.Operation != EditHistoryOperation.AddEntry)
                return CompareSelectionDetails.Empty;

            var raw = item.NewValue ?? "";
            if (string.IsNullOrWhiteSpace(raw))
                return CompareSelectionDetails.Empty;

            XElement rootEl;
            try
            {
                rootEl = XElement.Parse(raw);
            }
            catch
            {
                return CompareSelectionDetails.Empty;
            }

            var shopMenu = FindFirstDescendantIgnoreCase(rootEl, "ShopMenu") ?? (string.Equals(rootEl.Name.LocalName, "ShopMenu", StringComparison.OrdinalIgnoreCase) ? rootEl : null);
            if (shopMenu != null)
                return BuildShopMenuDetails(shopMenu);

            var name = FindFirstValueIgnoreCase(rootEl, new[] { "Name", "DisplayName", "Title" });
            var id = FindFirstValueIgnoreCase(rootEl, new[] { "ID" });
            var header = !string.IsNullOrWhiteSpace(name) ? name : (!string.IsNullOrWhiteSpace(id) ? id : rootEl.Name.LocalName);

            return new CompareSelectionDetails(header, new List<string>());
        }

        private CompareSelectionDetails BuildShopMenuDetails(XElement shopMenu)
        {
            var name = FindFirstValueIgnoreCase(shopMenu, new[] { "Name", "DisplayName", "Title" });
            var id = FindFirstValueIgnoreCase(shopMenu, new[] { "ID" });

            var header = !string.IsNullOrWhiteSpace(name) ? name : (!string.IsNullOrWhiteSpace(id) ? id : "ShopMenu");

            var itemsEl = FindFirstDescendantIgnoreCase(shopMenu, "Items");
            if (itemsEl is null)
                return new CompareSelectionDetails(header, new List<string>());

            var lines = new List<string>();
            foreach (var itemEl in itemsEl.Elements())
            {
                var line = FindFirstValueIgnoreCase(itemEl, new[] { "ModItemName", "ItemName", "Name", "DisplayName", "Title", "ModelName", "ID" });
                if (string.IsNullOrWhiteSpace(line))
                    line = itemEl.Name.LocalName;

                lines.Add(line.Trim());
            }

            return new CompareSelectionDetails(header, lines);
        }

        private static XElement? FindFirstDescendantIgnoreCase(XElement el, string localName)
        {
            foreach (var d in el.Descendants())
            {
                if (string.Equals(d.Name.LocalName, localName, StringComparison.OrdinalIgnoreCase))
                    return d;
            }

            return null;
        }

        private static string FindFirstValueIgnoreCase(XElement el, IEnumerable<string> localNames)
        {
            foreach (var wanted in localNames)
            {
                foreach (var d in el.DescendantsAndSelf())
                {
                    if (string.Equals(d.Name.LocalName, wanted, StringComparison.OrdinalIgnoreCase))
                        return d.Value ?? "";
                }

                foreach (var a in el.DescendantsAndSelf().Attributes())
                {
                    if (string.Equals(a.Name.LocalName, wanted, StringComparison.OrdinalIgnoreCase))
                        return a.Value ?? "";
                }
            }

            return "";
        }
    }

    public sealed class CompareSelectionDetails
    {
        public static CompareSelectionDetails Empty { get; } = new CompareSelectionDetails("", new List<string>());

        public CompareSelectionDetails(string header, List<string> lines)
        {
            Header = header ?? "";
            Lines = lines ?? new List<string>();
        }

        public string Header { get; }
        public List<string> Lines { get; }
    }
}
