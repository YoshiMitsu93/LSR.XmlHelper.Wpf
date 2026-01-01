using System;
using System.Linq;

namespace LSR.XmlHelper.Wpf.Services
{
    public static class LookupFieldPathParser
    {
        public static bool TryParseLookupField(string name, out string groupTitle, out string itemName, out string leafField)
        {
            groupTitle = "";
            itemName = "";
            leafField = "";

            if (string.IsNullOrWhiteSpace(name))
                return false;

            var parts = name.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 3)
                return false;

            var first = parts[0];
            var second = parts[1];

            if (string.IsNullOrWhiteSpace(first) || string.IsNullOrWhiteSpace(second))
                return false;

            var lb = second.IndexOf('[', StringComparison.Ordinal);
            var rb = second.EndsWith("]", StringComparison.Ordinal);

            if (lb <= 0 || !rb)
                return false;

            groupTitle = first;
            itemName = second;
            leafField = string.Join("/", parts.Skip(2));
            return true;
        }
    }
}
