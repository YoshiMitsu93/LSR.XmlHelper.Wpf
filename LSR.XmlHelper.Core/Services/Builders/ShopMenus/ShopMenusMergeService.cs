using System;
using System.Linq;
using System.Xml.Linq;

namespace LSR.XmlHelper.Core.Services.Builders
{
    public sealed class ShopMenusMergeService
    {
        public XDocument MergeNew(XDocument baseDoc, XDocument addDoc)
        {
            if (baseDoc.Root is null)
                return baseDoc;

            if (addDoc.Root is null)
                return baseDoc;

            var baseRoot = baseDoc.Root;
            var addRoot = addDoc.Root;

            var baseGroupList = baseRoot.Element("ShopMenuGroupList");
            if (baseGroupList is null)
            {
                baseGroupList = new XElement("ShopMenuGroupList");
                baseRoot.Add(baseGroupList);
            }

            var baseMenuList = baseRoot.Element("ShopMenuList");
            if (baseMenuList is null)
            {
                baseMenuList = new XElement("ShopMenuList");
                baseRoot.Add(baseMenuList);
            }

            var addGroupList = addRoot.Element("ShopMenuGroupList");
            if (addGroupList is not null)
                MergeListById(baseGroupList, addGroupList.Elements("ShopMenuGroup"));

            var addMenuList = addRoot.Element("ShopMenuList");
            if (addMenuList is not null)
                MergeListById(baseMenuList, addMenuList.Elements("ShopMenu"));

            return baseDoc;
        }

        private static void MergeListById(XElement baseList, IEnumerable<XElement> incomingItems)
        {
            foreach (var incoming in incomingItems)
            {
                var incomingId = ((string?)incoming.Element("ID") ?? "").Trim();
                if (string.IsNullOrWhiteSpace(incomingId))
                    continue;

                var existing = baseList.Elements(incoming.Name.LocalName)
                    .FirstOrDefault(x => string.Equals(((string?)x.Element("ID") ?? "").Trim(), incomingId, StringComparison.OrdinalIgnoreCase));

                if (existing is not null)
                    existing.ReplaceWith(new XElement(incoming));
                else
                    baseList.Add(new XElement(incoming));
            }
        }
    }
}
