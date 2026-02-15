using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace LSR.XmlHelper.Core.Services.Builders
{
    public sealed class ZoneMenuContainersUpdaterService
    {
        public XDocument BuildAdditiveZonesDocument(string rootFolderPath, IReadOnlyCollection<string> zoneInternalNames, string dealerMenuContainerId, string customerMenuContainerId)
        {
            var (templateRoot, zonesDoc) = LoadZonesDocument(rootFolderPath);

            var outputRoot = new XElement(templateRoot.Name);

            foreach (var attr in templateRoot.Attributes())
                outputRoot.SetAttributeValue(attr.Name, attr.Value);

            foreach (var internalName in zoneInternalNames.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(internalName))
                    continue;

                var zone = zonesDoc
                    .Descendants("Zone")
                    .FirstOrDefault(x => string.Equals(((string?)x.Element("InternalGameName") ?? "").Trim(), internalName.Trim(), StringComparison.OrdinalIgnoreCase));

                if (zone is null)
                    continue;

                var clonedZone = new XElement(zone);

                SetOrCreate(clonedZone, "DealerMenuContainerID", dealerMenuContainerId);
                SetOrCreate(clonedZone, "CustomerMenuContainerID", customerMenuContainerId);

                outputRoot.Add(clonedZone);
            }

            return new XDocument(outputRoot);
        }

        public void ApplyToZonesFile(string zonesFilePath, IReadOnlyCollection<string> zoneInternalNames, string dealerMenuContainerId, string customerMenuContainerId)
        {
            if (string.IsNullOrWhiteSpace(zonesFilePath) || !File.Exists(zonesFilePath))
                return;

            XDocument doc;
            try
            {
                doc = XDocument.Load(zonesFilePath, LoadOptions.None);
            }
            catch
            {
                return;
            }

            foreach (var internalName in zoneInternalNames.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(internalName))
                    continue;

                var zone = doc
                    .Descendants("Zone")
                    .FirstOrDefault(x => string.Equals(((string?)x.Element("InternalGameName") ?? "").Trim(), internalName.Trim(), StringComparison.OrdinalIgnoreCase));

                if (zone is null)
                    continue;

                SetOrCreate(zone, "DealerMenuContainerID", dealerMenuContainerId);
                SetOrCreate(zone, "CustomerMenuContainerID", customerMenuContainerId);
            }

            doc.Save(zonesFilePath);
        }

        private static (XElement templateRoot, XDocument doc) LoadZonesDocument(string rootFolderPath)
        {
            var defaultRoot = new XElement("Zones");
            var defaultDoc = new XDocument(defaultRoot);

            if (string.IsNullOrWhiteSpace(rootFolderPath) || !Directory.Exists(rootFolderPath))
                return (defaultRoot, defaultDoc);

            var resolver = new LSR.XmlHelper.Core.Services.LsrConfigFileResolverService();
            var file = resolver.ResolveZonesFile(rootFolderPath);

            if (string.IsNullOrWhiteSpace(file) || !File.Exists(file))
                return (defaultRoot, defaultDoc);

            XDocument doc;
            try
            {
                doc = XDocument.Load(file, LoadOptions.None);
            }
            catch
            {
                return (defaultRoot, defaultDoc);
            }

            var root = doc.Root ?? defaultRoot;
            return (root, doc);
        }

        private static void SetOrCreate(XElement parent, string childName, string value)
        {
            var existing = parent.Elements().FirstOrDefault(x => string.Equals(x.Name.LocalName, childName, StringComparison.OrdinalIgnoreCase));
            if (existing is not null)
            {
                existing.Value = value ?? "";
                return;
            }

            parent.Add(new XElement(childName, value ?? ""));
        }
    }
}
