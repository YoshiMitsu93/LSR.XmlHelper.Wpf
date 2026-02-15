using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace LSR.XmlHelper.Core.Services.Builders
{
    public sealed class GangIdLookupService
    {
        public bool TryFindGangId(string rootFolderPath, string gangId, out string foundInFileName)
        {
            foundInFileName = "";

            if (string.IsNullOrWhiteSpace(rootFolderPath) || !Directory.Exists(rootFolderPath))
                return false;

            if (string.IsNullOrWhiteSpace(gangId))
                return false;

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

                var exists = doc
                    .Descendants("Gang")
                    .Select(x => ((string?)x.Element("ID") ?? "").Trim())
                    .Any(x => string.Equals(x, gangId, StringComparison.OrdinalIgnoreCase));

                if (exists)
                {
                    foundInFileName = Path.GetFileName(file);
                    return true;
                }
            }

            return false;
        }
    }
}
