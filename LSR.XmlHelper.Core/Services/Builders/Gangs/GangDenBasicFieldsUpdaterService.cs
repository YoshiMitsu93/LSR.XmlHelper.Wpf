using System;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace LSR.XmlHelper.Core.Services.Editing
{
    public sealed class GangDenBasicFieldsUpdaterService
    {
        public bool Apply(XDocument locationsDoc, string gangId, string denName, string x, string y, string z, string heading, string menuId, string bannerImagePath)
        {
            if (locationsDoc?.Root is null)
                return false;

            if (string.IsNullOrWhiteSpace(gangId))
                return false;

            var dens = locationsDoc
                .Descendants("GangDen")
                .Where(d => string.Equals((d.Element("AssignedAssociationID")?.Value ?? "").Trim(), gangId.Trim(), StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (dens.Count == 0)
                return false;

            var changedAny = false;

            foreach (var den in dens)
            {
                changedAny |= Set(den, "Name", denName);
                changedAny |= Set(den, "FullName", denName);

                if (!string.IsNullOrWhiteSpace(x) || !string.IsNullOrWhiteSpace(y) || !string.IsNullOrWhiteSpace(z))
                {
                    var pos = den.Element("EntrancePosition");
                    if (pos is null)
                    {
                        pos = new XElement("EntrancePosition");
                        den.Add(pos);
                        changedAny = true;
                    }

                    changedAny |= Set(pos, "X", NormalizeNumberText(x));
                    changedAny |= Set(pos, "Y", NormalizeNumberText(y));
                    changedAny |= Set(pos, "Z", NormalizeNumberText(z));
                }

                changedAny |= Set(den, "EntranceHeading", NormalizeNumberText(heading));
                changedAny |= Set(den, "MenuID", menuId);
                changedAny |= Set(den, "BannerImagePath", bannerImagePath);
            }

            return changedAny;
        }

        private static bool Set(XElement parent, string elementName, string value)
        {
            var desired = (value ?? "").Trim();

            var existing = parent.Element(elementName);
            if (existing is null)
            {
                parent.Add(new XElement(elementName, desired));
                return true;
            }

            var current = (existing.Value ?? "").Trim();
            if (string.Equals(current, desired, StringComparison.Ordinal))
                return false;

            existing.Value = desired;
            return true;
        }

        private static string NormalizeNumberText(string text)
        {
            var raw = (text ?? "").Trim();
            if (string.IsNullOrWhiteSpace(raw))
                return raw;

            var style = NumberStyles.Float | NumberStyles.AllowThousands;
            var culture = CultureInfo.InvariantCulture;

            if (!double.TryParse(raw, style, culture, out var parsed))
                return raw;

            return parsed.ToString(culture);
        }
    }
}
