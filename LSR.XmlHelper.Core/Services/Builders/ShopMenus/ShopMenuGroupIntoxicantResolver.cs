using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace LSR.XmlHelper.Core.Services.Builders.ShopMenus
{
    public sealed class ShopMenuGroupIntoxicantResolver
    {
        public HashSet<string> Resolve(string rootFolderPath, string shopMenuGroupId, HashSet<string> intoxicantNames)
        {
            var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (string.IsNullOrWhiteSpace(shopMenuGroupId))
                return result;

            if (intoxicantNames.Count == 0)
                return result;

            var resolver = new LSR.XmlHelper.Core.Services.LsrFileSetResolverService();
            var resolved = resolver.ResolveShopMenus(rootFolderPath, "Default");

            var files = resolved.EnumerateReadOrder()
                .Where(p => !string.IsNullOrWhiteSpace(p) && File.Exists(p))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

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

                var candidateGroupIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                foreach (var container in doc.Descendants("ShopMenuGroupContainer"))
                {
                    var containerId = ((string?)container.Element("ID"))?.Trim();
                    if (!string.Equals(containerId, shopMenuGroupId, StringComparison.OrdinalIgnoreCase))
                        continue;

                    foreach (var groupIdNode in container.Descendants("ShopMenuGroupID"))
                    {
                        var gid = ((string?)groupIdNode)?.Trim();
                        if (!string.IsNullOrWhiteSpace(gid))
                            candidateGroupIds.Add(gid);
                    }
                }

                if (candidateGroupIds.Count == 0)
                    candidateGroupIds.Add(shopMenuGroupId);

                foreach (var group in doc.Descendants("ShopMenuGroup"))
                {
                    var id = ((string?)group.Element("ID"))?.Trim();
                    if (string.IsNullOrWhiteSpace(id))
                        continue;

                    if (!candidateGroupIds.Contains(id))
                        continue;

                    foreach (var item in group.Descendants("MenuItem"))
                    {
                        var name = ((string?)item.Element("ModItemName"))?.Trim();
                        if (string.IsNullOrWhiteSpace(name))
                            continue;

                        if (intoxicantNames.Contains(name))
                            result.Add(name);
                    }
                }
            }

            return result;
        }
    }
}
