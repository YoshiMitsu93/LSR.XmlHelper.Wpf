using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace LSR.XmlHelper.Core.Services.Builders
{
    public sealed class GangDealerMenuGroupLookupService
    {
        public string GetDealerMenuGroupId(string rootFolderPath, string gangId)
        {
            if (string.IsNullOrWhiteSpace(rootFolderPath) || !Directory.Exists(rootFolderPath))
                return "";

            if (string.IsNullOrWhiteSpace(gangId))
                return "";

            var resolver = new LSR.XmlHelper.Core.Services.LsrFileSetResolverService();
            var resolved = resolver.ResolveGangs(rootFolderPath, "Default");

            var files = resolved.EnumerateReadOrder()
                .Where(p => !string.IsNullOrWhiteSpace(p) && File.Exists(p))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            for (int i = files.Count - 1; i >= 0; i--)
            {
                var file = files[i];

                XDocument doc;
                try
                {
                    doc = XDocument.Load(file, LoadOptions.None);
                }
                catch
                {
                    continue;
                }

                var gang = doc.Descendants("Gang").FirstOrDefault(x =>
                    string.Equals(((string?)x.Element("ID") ?? "").Trim(), gangId.Trim(), StringComparison.OrdinalIgnoreCase));

                if (gang is null)
                    continue;

                return ((string?)gang.Element("DealerMenuGroupID") ?? "").Trim();
            }

            return "";
        }
    }
}
