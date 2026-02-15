using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace LSR.XmlHelper.Core.Services.Reading
{
    public sealed class ShopMenusEffectiveDocumentService
    {
        public XDocument? LoadEffective(string shopMenusFilePath)
        {
            if (string.IsNullOrWhiteSpace(shopMenusFilePath))
                return null;

            if (!File.Exists(shopMenusFilePath))
                return null;

            var selectedFileName = Path.GetFileName(shopMenusFilePath) ?? "";
            var folder = Path.GetDirectoryName(shopMenusFilePath) ?? "";

            if (!IsAdditiveShopMenusFile(selectedFileName))
                return TryLoad(shopMenusFilePath);

            var basePath = Path.Combine(folder, "ShopMenus.xml");
            if (!File.Exists(basePath))
                return TryLoad(shopMenusFilePath);

            var baseDoc = TryLoad(basePath);
            if (baseDoc is null)
                return TryLoad(shopMenusFilePath);

            var additiveDoc = TryLoad(shopMenusFilePath);
            if (additiveDoc is null)
                return baseDoc;

            MergeShopMenuGroups(baseDoc, additiveDoc);
            return baseDoc;
        }

        private static bool IsAdditiveShopMenusFile(string fileName)
        {
            return fileName.StartsWith("ShopMenus+", StringComparison.OrdinalIgnoreCase);
        }

        private static XDocument? TryLoad(string path)
        {
            try
            {
                return XDocument.Load(path, LoadOptions.None);
            }
            catch
            {
                return null;
            }
        }

        private static void MergeShopMenuGroups(XDocument baseDoc, XDocument additiveDoc)
        {
            var baseGroups = baseDoc.Descendants("ShopMenuGroup").ToList();
            var baseContainer = baseGroups.FirstOrDefault()?.Parent ?? baseDoc.Root;

            if (baseContainer is null)
                return;

            var additiveGroups = additiveDoc.Descendants("ShopMenuGroup").ToList();
            foreach (var addGroup in additiveGroups)
            {
                var id = ((string?)addGroup.Element("ID") ?? "").Trim();
                if (string.IsNullOrWhiteSpace(id))
                    continue;

                var match = baseGroups.FirstOrDefault(x =>
                    string.Equals(((string?)x.Element("ID") ?? "").Trim(), id, StringComparison.OrdinalIgnoreCase));

                if (match is null)
                {
                    baseContainer.Add(new XElement(addGroup));
                    continue;
                }

                match.ReplaceWith(new XElement(addGroup));
            }
        }
    }
}
