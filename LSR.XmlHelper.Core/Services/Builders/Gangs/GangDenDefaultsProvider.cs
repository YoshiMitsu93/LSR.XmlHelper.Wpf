using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace LSR.XmlHelper.Core.Services.Builders
{
    public static class GangDenDefaultsProvider
    {
        public static XElement? TryLoadDefaultDen(string rootFolderPath)
        {
            if (string.IsNullOrWhiteSpace(rootFolderPath) || !Directory.Exists(rootFolderPath))
                return null;

            var resolver = new LSR.XmlHelper.Core.Services.LsrFileSetResolverService();
            var resolved = resolver.ResolveLocations(rootFolderPath, "Default");

            foreach (var file in resolved.EnumerateReadOrder())
            {
                if (string.IsNullOrWhiteSpace(file) || !File.Exists(file))
                    continue;

                try
                {
                    var doc = XDocument.Load(file, LoadOptions.None);
                    var gangDens = doc.Descendants("GangDens").FirstOrDefault();
                    var den = gangDens?.Elements("GangDen").FirstOrDefault();
                    if (den is not null)
                        return new XElement(den);
                }
                catch
                {
                }
            }

            return null;
        }

        public static void ApplyDefaults(XElement targetDen, XElement? defaultsDen)
        {
            if (targetDen is null)
                throw new ArgumentNullException(nameof(targetDen));

            SetOrCreate(targetDen, "TypeName", GetValue(defaultsDen, "TypeName") ?? "Gang Den");

            var defaultMenuId = GetValue(defaultsDen, "MenuID");
            if (string.IsNullOrWhiteSpace(defaultMenuId))
                defaultMenuId = "FamiliesDenMenu";

            SetOrCreate(targetDen, "MenuID", defaultMenuId);

            SetOrCreate(targetDen, "IsBlipEnabled", GetValue(defaultsDen, "IsBlipEnabled") ?? "true");
            SetOrCreate(targetDen, "MapIcon", GetValue(defaultsDen, "MapIcon") ?? "378");
            SetOrCreate(targetDen, "MapIconColorString", GetValue(defaultsDen, "MapIconColorString") ?? "White");
            SetOrCreate(targetDen, "MapIconScale", GetValue(defaultsDen, "MapIconScale") ?? "0.5");
            SetOrCreate(targetDen, "MapIconRadius", GetValue(defaultsDen, "MapIconRadius") ?? "1");
            SetOrCreate(targetDen, "MapOpenIconAlpha", GetValue(defaultsDen, "MapOpenIconAlpha") ?? "1");
            SetOrCreate(targetDen, "MapClosedIconAlpha", GetValue(defaultsDen, "MapClosedIconAlpha") ?? "0.25");
        }

        private static string? GetValue(XElement? parent, string childName)
        {
            return parent?.Element(childName)?.Value;
        }

        private static void SetOrCreate(XElement parent, string childName, string value)
        {
            var element = parent.Element(childName);
            if (element is null)
            {
                parent.Add(new XElement(childName, value));
                return;
            }

            element.Value = value;
        }
    }
}
