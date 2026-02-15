using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace LSR.XmlHelper.Core.Services.Builders
{
    public sealed class DispatchablePeopleGroupReadService
    {
        public XElement? TryReadGroup(string rootFolderPath, string groupId)
        {
            if (string.IsNullOrWhiteSpace(rootFolderPath) || !Directory.Exists(rootFolderPath))
                return null;

            if (string.IsNullOrWhiteSpace(groupId))
                return null;

            var resolver = new LSR.XmlHelper.Core.Services.LsrFileSetResolverService();
            var resolved = resolver.ResolveDispatchablePeople(rootFolderPath, "Default");

            var files = resolved
                .EnumerateReadOrder()
                .Where(p => !string.IsNullOrWhiteSpace(p) && File.Exists(p))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            XElement? winningGroup = null;

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

                var group = doc.Descendants("DispatchablePersonGroup")
                    .FirstOrDefault(x => string.Equals(((string?)x.Element("DispatchablePersonGroupID") ?? "").Trim(), groupId, StringComparison.OrdinalIgnoreCase));

                if (group is null)
                    continue;

                winningGroup = new XElement(group);
            }

            return winningGroup;
        }
    }
}
