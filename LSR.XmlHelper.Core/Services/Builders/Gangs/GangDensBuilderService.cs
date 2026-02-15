using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace LSR.XmlHelper.Core.Services.Builders
{
    public sealed class GangDensBuilderService
    {
        public (XDocument doc, int clonedCount) BuildClone(string rootFolderPath, string sourceGangId, string newGangId, bool keepSourceDenTypeName, string? menuIdOverride = null, string? bannerImagePathOverride = null)
        {
            if (string.IsNullOrWhiteSpace(rootFolderPath) || !Directory.Exists(rootFolderPath))
                return (CreateEmptyLocationsRoot(), 0);

            if (string.IsNullOrWhiteSpace(sourceGangId))
                return (CreateEmptyLocationsRoot(), 0);

            if (string.IsNullOrWhiteSpace(newGangId))
                return (CreateEmptyLocationsRoot(), 0);

            var resolver = new LSR.XmlHelper.Core.Services.LsrFileSetResolverService();
            var resolved = resolver.ResolveLocations(rootFolderPath, "Default");

            var files = resolved
                .EnumerateReadOrder()
                .Where(p => !string.IsNullOrWhiteSpace(p) && File.Exists(p))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            XElement? templateRoot = null;
            var clonedDens = new List<XElement>();
            var defaultsDen = GangDenDefaultsProvider.TryLoadDefaultDen(rootFolderPath);

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

                var gangDens = doc.Descendants("GangDens").FirstOrDefault();
                if (gangDens is null)
                    continue;

                foreach (var den in gangDens.Elements("GangDen"))
                {
                    var assigned = (string?)den.Element("AssignedAssociationID");
                    if (!string.Equals(assigned, sourceGangId, StringComparison.OrdinalIgnoreCase))
                        continue;

                    var cloned = new XElement(den);
                    SetOrCreate(cloned, "AssignedAssociationID", newGangId);

                    if (!keepSourceDenTypeName)
                    {
                        var typeName = defaultsDen?.Element("TypeName")?.Value;
                        if (string.IsNullOrWhiteSpace(typeName))
                            typeName = "Gang Den";

                        SetOrCreate(cloned, "TypeName", typeName);
                    }

                    EnsureMenuIdIfMissing(cloned, defaultsDen);
                    ApplyDenOverrides(cloned, menuIdOverride, bannerImagePathOverride);
                    clonedDens.Add(cloned);
                }
            }

            if (templateRoot is null)
                templateRoot = CreateEmptyLocationsRoot().Root;

            var outputRoot = new XElement(templateRoot!.Name);

            foreach (var attr in templateRoot.Attributes())
                outputRoot.SetAttributeValue(attr.Name, attr.Value);

            var outputGangDens = new XElement("GangDens");
            foreach (var den in clonedDens)
                outputGangDens.Add(den);

            outputRoot.Add(outputGangDens);

            return (new XDocument(outputRoot), clonedDens.Count);
        }

        public (XDocument doc, int createdCount) BuildNewDen(string rootFolderPath, string newGangId, string denName, double x, double y, double z, double heading, Models.GangDenBlipSettings blipSettings, string? menuIdOverride = null, string? bannerImagePathOverride = null)
        {
            if (string.IsNullOrWhiteSpace(rootFolderPath) || !Directory.Exists(rootFolderPath))
                return (CreateEmptyLocationsRoot(), 0);

            if (string.IsNullOrWhiteSpace(newGangId))
                return (CreateEmptyLocationsRoot(), 0);

            if (string.IsNullOrWhiteSpace(denName))
                return (CreateEmptyLocationsRoot(), 0);

            var resolver = new LSR.XmlHelper.Core.Services.LsrFileSetResolverService();
            var resolved = resolver.ResolveLocations(rootFolderPath, "Default");

            var files = resolved
                .EnumerateReadOrder()
                .Where(p => !string.IsNullOrWhiteSpace(p) && File.Exists(p))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            XElement? templateRoot = null;
            XElement? templateDen = null;

            var defaultsDen = GangDenDefaultsProvider.TryLoadDefaultDen(rootFolderPath);

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

                if (templateDen is not null)
                    continue;

                var gangDens = doc.Descendants("GangDens").FirstOrDefault();
                if (gangDens is null)
                    continue;

                templateDen = gangDens.Elements("GangDen").FirstOrDefault();
            }

            if (templateRoot is null)
                templateRoot = CreateEmptyLocationsRoot().Root;

            var outputRoot = new XElement(templateRoot!.Name);

            foreach (var attr in templateRoot.Attributes())
                outputRoot.SetAttributeValue(attr.Name, attr.Value);

            var outputGangDens = new XElement("GangDens");

            var newDen = templateDen is not null ? new XElement(templateDen) : new XElement("GangDen");

            SetOrCreate(newDen, "Name", denName);
            SetOrCreate(newDen, "FullName", denName);
            SetOrCreate(newDen, "AssignedAssociationID", newGangId);

            var entrancePosition = newDen.Element("EntrancePosition");
            if (entrancePosition is null)
            {
                entrancePosition = new XElement("EntrancePosition");
                newDen.Add(entrancePosition);
            }

            SetOrCreate(entrancePosition, "X", x.ToString(System.Globalization.CultureInfo.InvariantCulture));
            SetOrCreate(entrancePosition, "Y", y.ToString(System.Globalization.CultureInfo.InvariantCulture));
            SetOrCreate(entrancePosition, "Z", z.ToString(System.Globalization.CultureInfo.InvariantCulture));

            SetOrCreate(newDen, "EntranceHeading", heading.ToString(System.Globalization.CultureInfo.InvariantCulture));

            GangDenDefaultsProvider.ApplyDefaults(newDen, defaultsDen);
            EnsureMenuIdIfMissing(newDen, defaultsDen);
            ApplyDenOverrides(newDen, menuIdOverride, bannerImagePathOverride);
            SetOrCreate(newDen, "IsBlipEnabled", blipSettings.IsBlipEnabled ? "true" : "false");
            SetOrCreate(newDen, "MapIcon", blipSettings.MapIcon);
            SetOrCreate(newDen, "MapIconColorString", blipSettings.MapIconColorString);
            SetOrCreate(newDen, "MapIconScale", blipSettings.MapIconScale);
            SetOrCreate(newDen, "MapIconRadius", blipSettings.MapIconRadius);
            SetOrCreate(newDen, "MapOpenIconAlpha", blipSettings.MapOpenIconAlpha);
            SetOrCreate(newDen, "MapClosedIconAlpha", blipSettings.MapClosedIconAlpha);

            outputGangDens.Add(newDen);
            outputRoot.Add(outputGangDens);

            return (new XDocument(outputRoot), 1);
        }

        private static void SetOrCreate(XElement parent, string childName, string value)
        {
            var child = parent.Element(childName);
            if (child is null)
                parent.Add(new XElement(childName, value));
            else
                child.Value = value;
        }
        private static void EnsureMenuIdIfMissing(XElement den, XElement? defaultsDen)
        {
            var currentMenuId = (string?)den.Element("MenuID");
            if (!string.IsNullOrWhiteSpace(currentMenuId))
                return;

            var defaultMenuId = defaultsDen?.Element("MenuID")?.Value;
            if (string.IsNullOrWhiteSpace(defaultMenuId))
                defaultMenuId = "FamiliesDenMenu";

            SetOrCreate(den, "MenuID", defaultMenuId);
        }
        private static void ApplyDenOverrides(XElement den, string? menuIdOverride, string? bannerImagePathOverride)
        {
            if (!string.IsNullOrWhiteSpace(menuIdOverride))
                SetOrCreate(den, "MenuID", menuIdOverride.Trim());

            if (!string.IsNullOrWhiteSpace(bannerImagePathOverride))
                SetOrCreate(den, "BannerImagePath", bannerImagePathOverride.Trim());
        }
        private static XDocument CreateEmptyLocationsRoot()
        {
            return new XDocument(
                new XElement("PossibleLocations",
                    new XAttribute(XNamespace.Xmlns + "xsd", "http://www.w3.org/2001/XMLSchema"),
                    new XAttribute(XNamespace.Xmlns + "xsi", "http://www.w3.org/2001/XMLSchema-instance"),
                    new XElement("GangDens")));
        }
    }
}
