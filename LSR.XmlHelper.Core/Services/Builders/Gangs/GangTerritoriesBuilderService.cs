using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace LSR.XmlHelper.Core.Services.Builders
{
    public sealed class GangTerritoriesBuilderService
    {
        public XDocument Build(string rootFolderPath, string gangId, IReadOnlyCollection<string> zoneInternalNames)
        {
            var templateRoot = LoadTemplateRoot(rootFolderPath);

            var root = new XElement(templateRoot.Name);

            foreach (var attr in templateRoot.Attributes())
                root.SetAttributeValue(attr.Name, attr.Value);

            foreach (var zone in zoneInternalNames.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(zone))
                    continue;

                var territory = new XElement("GangTerritory",
                    new XElement("ZoneInternalGameName", zone),
                    new XElement("GangID", gangId),
                    new XElement("Priority", 0),
                    new XElement("AmbientSpawnChance", 100));

                root.Add(territory);
            }

            return new XDocument(root);
        }

        private static XElement LoadTemplateRoot(string rootFolderPath)
        {
            try
            {
                if (Directory.Exists(rootFolderPath))
                {
                    var resolver = new LSR.XmlHelper.Core.Services.LsrFileSetResolverService();
                    var resolved = resolver.ResolveGangTerritories(rootFolderPath, "Default");

                    var file = resolved.EnumerateReadOrder()
                        .Where(p => !string.IsNullOrWhiteSpace(p) && File.Exists(p))
                        .FirstOrDefault();

                    if (!string.IsNullOrWhiteSpace(file))
                    {
                        var doc = XDocument.Load(file, LoadOptions.None);
                        if (doc.Root is not null)
                            return doc.Root;
                    }
                }
            }
            catch
            {
            }

            return new XElement("ArrayOfGangTerritory",
                new XAttribute(XNamespace.Xmlns + "xsd", "http://www.w3.org/2001/XMLSchema"),
                new XAttribute(XNamespace.Xmlns + "xsi", "http://www.w3.org/2001/XMLSchema-instance"));
        }
    }
}
