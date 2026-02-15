using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace LSR.XmlHelper.Core.Services.Builders
{
    public sealed class GangDenLookupService
    {
        public List<XElement> GetGangDens(string rootFolderPath, string gangId)
        {
            var result = new List<XElement>();

            if (string.IsNullOrWhiteSpace(rootFolderPath))
                return result;

            if (!Directory.Exists(rootFolderPath))
                return result;

            if (string.IsNullOrWhiteSpace(gangId))
                return result;

            var resolver = new LSR.XmlHelper.Core.Services.LsrFileSetResolverService();
            var resolved = resolver.ResolveLocations(rootFolderPath, "Default");

            var files = resolved
                .EnumerateReadOrder()
                .Where(p => !string.IsNullOrWhiteSpace(p) && File.Exists(p))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var densByName = new Dictionary<string, XElement>(StringComparer.OrdinalIgnoreCase);

            foreach (var file in files)
            {
                try
                {
                    var doc = XDocument.Load(file, LoadOptions.None);

                    foreach (var den in doc.Descendants("GangDen"))
                    {
                        var assigned = (den.Element("AssignedAssociationID")?.Value ?? "").Trim();
                        if (!string.Equals(assigned, gangId, StringComparison.OrdinalIgnoreCase))
                            continue;

                        var name = ((string?)den.Element("Name") ?? "").Trim();
                        if (string.IsNullOrWhiteSpace(name))
                            name = Guid.NewGuid().ToString("N");

                        densByName[name] = new XElement(den);
                    }
                }
                catch
                {
                }
            }

            result.AddRange(densByName.Values);

            return result;
        }
    }
}
