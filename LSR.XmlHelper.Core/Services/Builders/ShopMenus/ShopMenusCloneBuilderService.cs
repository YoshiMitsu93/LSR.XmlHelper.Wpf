using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace LSR.XmlHelper.Core.Services.Builders
{
    public sealed class ShopMenusCloneBuilderService
    {
        public (XDocument doc, string clonedGroupId, int clonedMenusCount) CloneDealerGroup(string rootFolderPath, string sourceGroupId, string desiredClonedGroupId, string menuIdSuffix = "")
        {
            if (string.IsNullOrWhiteSpace(rootFolderPath) || !Directory.Exists(rootFolderPath))
                return (CreateEmptyRoot(), "", 0);

            if (string.IsNullOrWhiteSpace(sourceGroupId))
                return (CreateEmptyRoot(), "", 0);

            var files = EnumerateFiles(rootFolderPath, "ShopMenus*.xml").ToArray();

            XElement? templateRoot = null;
            XElement? sourceGroup = null;
            var referencedMenuIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var clonedMenus = new List<XElement>();

            foreach (var file in files)
            {
                XDocument doc;
                try
                {
                    doc = XDocument.Load(file, LoadOptions.None);
                }
                catch
                {
                    continue;
                }

                if (templateRoot is null && doc.Root is not null)
                    templateRoot = doc.Root;

                if (sourceGroup is null)
                {
                    sourceGroup = doc.Descendants("ShopMenuGroup")
                        .FirstOrDefault(x => string.Equals((string?)x.Element("ID"), sourceGroupId, StringComparison.OrdinalIgnoreCase));

                    if (sourceGroup is not null)
                    {
                        foreach (var e in sourceGroup.Descendants())
                        {
                            var name = e.Name.LocalName;

                            if (string.Equals(name, "ShopMenuID", StringComparison.OrdinalIgnoreCase)
                                || string.Equals(name, "ShopMenuId", StringComparison.OrdinalIgnoreCase))
                            {
                                var v = (e.Value ?? "").Trim();
                                if (!string.IsNullOrWhiteSpace(v))
                                    referencedMenuIds.Add(v);
                            }
                        }
                    }
                }
            }

            if (sourceGroup is null)
                return (CreateEmptyRoot(templateRoot), "", 0);

            var existingGroupIds = GetExistingGroupIds(files);

            var finalGroupId = MakeUniqueId(
                string.IsNullOrWhiteSpace(desiredClonedGroupId) ? $"{sourceGroupId}_CLONE" : desiredClonedGroupId,
                existingGroupIds);
            var menuIdMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var suffix = (menuIdSuffix ?? "").Trim();

            if (!string.IsNullOrWhiteSpace(suffix))
            {
                foreach (var id in referencedMenuIds)
                    menuIdMap[id] = $"{id}_{suffix}";
            }

            foreach (var file in files)
            {
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
                    var id = (string?)menu.Element("ID");
                    if (string.IsNullOrWhiteSpace(id))
                        continue;

                    if (!referencedMenuIds.Contains(id))
                        continue;

                    var groupName = ((string?)menu.Element("GroupName") ?? "").Trim();
                    if (!string.Equals(groupName, sourceGroupId, StringComparison.OrdinalIgnoreCase))
                        continue;

                    var cloned = new XElement(menu);

                    if (menuIdMap.TryGetValue(id, out var mappedId))
                        SetOrCreate(cloned, "ID", mappedId);

                    SetOrCreate(cloned, "GroupName", finalGroupId);

                    clonedMenus.Add(cloned);
                }
            }

            if (templateRoot is null)
                templateRoot = CreateEmptyRoot().Root;

            var outputRoot = new XElement(templateRoot!.Name);

            foreach (var attr in templateRoot.Attributes())
                outputRoot.SetAttributeValue(attr.Name, attr.Value);

            var groupList = new XElement("ShopMenuGroupList");
            var clonedGroup = new XElement(sourceGroup);
            SetOrCreate(clonedGroup, "ID", finalGroupId);

            if (menuIdMap.Count > 0)
            {
                foreach (var e in clonedGroup.Descendants())
                {
                    var name = e.Name.LocalName;
                    if (string.Equals(name, "ShopMenuID", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(name, "ShopMenuId", StringComparison.OrdinalIgnoreCase))
                    {
                        var v = (e.Value ?? "").Trim();
                        if (menuIdMap.TryGetValue(v, out var mapped))
                            e.Value = mapped;
                    }
                }
            }

            groupList.Add(clonedGroup);

            var menuList = new XElement("ShopMenuList");
            foreach (var m in clonedMenus)
                menuList.Add(m);

            outputRoot.Add(groupList);
            outputRoot.Add(menuList);

            return (new XDocument(outputRoot), finalGroupId, clonedMenus.Count);
        }

        private static IEnumerable<string> EnumerateFiles(string rootFolderPath, string searchPattern)
        {
            var resolver = new LSR.XmlHelper.Core.Services.LsrFileSetResolverService();
            var resolved = resolver.ResolveShopMenus(rootFolderPath, "Default");

            return resolved.EnumerateReadOrder()
                .Where(p => !string.IsNullOrWhiteSpace(p) && File.Exists(p))
                .Distinct(StringComparer.OrdinalIgnoreCase);
        }

        private static HashSet<string> GetExistingGroupIds(string[] files)
        {
            var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var file in files)
            {
                XDocument doc;
                try
                {
                    doc = XDocument.Load(file, LoadOptions.None);
                }
                catch
                {
                    continue;
                }

                foreach (var g in doc.Descendants("ShopMenuGroup"))
                {
                    var id = (string?)g.Element("ID");
                    if (!string.IsNullOrWhiteSpace(id))
                        set.Add(id);
                }
            }

            return set;
        }

        private static string MakeUniqueId(string desired, HashSet<string> existing)
        {
            if (!existing.Contains(desired))
                return desired;

            var i = 2;
            while (true)
            {
                var next = $"{desired}{i}";
                if (!existing.Contains(next))
                    return next;

                i++;
            }
        }

        private static void SetOrCreate(XElement parent, string childName, string value)
        {
            var child = parent.Element(childName);
            if (child is null)
                parent.Add(new XElement(childName, value));
            else
                child.Value = value;
        }

        private static XDocument CreateEmptyRoot(XElement? templateRoot = null)
        {
            if (templateRoot is not null)
                return new XDocument(new XElement(templateRoot.Name));

            return new XDocument(
                new XElement("ArrayOfShopMenu",
                    new XAttribute(XNamespace.Xmlns + "xsd", "http://www.w3.org/2001/XMLSchema"),
                    new XAttribute(XNamespace.Xmlns + "xsi", "http://www.w3.org/2001/XMLSchema-instance")));
        }
    }
}
