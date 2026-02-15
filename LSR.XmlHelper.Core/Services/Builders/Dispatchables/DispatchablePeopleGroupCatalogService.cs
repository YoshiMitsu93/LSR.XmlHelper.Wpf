using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace LSR.XmlHelper.Core.Services.Builders
{
    public sealed class DispatchablePeopleGroupCatalogService
    {
        public IReadOnlyList<(string Id, int Count)> GetGroups(string rootFolderPath)
        {
            if (string.IsNullOrWhiteSpace(rootFolderPath))
                return Array.Empty<(string Id, int Count)>();

            if (!Directory.Exists(rootFolderPath))
                return Array.Empty<(string Id, int Count)>();

            var resolver = new LSR.XmlHelper.Core.Services.LsrFileSetResolverService();
            var resolved = resolver.ResolveDispatchablePeople(rootFolderPath, "Default");

            var byIdMerged = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            foreach (var file in resolved.EnumerateReadOrder())
            {
                if (string.IsNullOrWhiteSpace(file) || !File.Exists(file))
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

                foreach (var g in doc.Descendants("DispatchablePersonGroup"))
                {
                    var id = ((string?)g.Element("DispatchablePersonGroupID") ?? "").Trim();
                    if (string.IsNullOrWhiteSpace(id))
                        continue;

                    var count = g.Descendants("DispatchablePerson").Count();
                    byIdMerged[id] = count;
                }
            }

            return byIdMerged
                .OrderBy(k => k.Key, StringComparer.OrdinalIgnoreCase)
                .Select(k => (k.Key, k.Value))
                .ToList();
        }
    }
}
