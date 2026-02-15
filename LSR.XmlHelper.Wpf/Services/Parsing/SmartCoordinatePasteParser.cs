using System.Globalization;
using System.Text.RegularExpressions;

namespace LSR.XmlHelper.Wpf.Services.Parsing
{
    public sealed class SmartCoordinatePasteParser
    {
        private static readonly Regex NumberRegex = new Regex(@"(?<![A-Za-z_])[-+]?\d+(?:\.\d+)?", RegexOptions.Compiled);

        public bool TryParseFirstXyzHeading(string input, out double x, out double y, out double z, out double heading)
        {
            x = 0;
            y = 0;
            z = 0;
            heading = 0;

            if (string.IsNullOrWhiteSpace(input))
                return false;

            var matches = NumberRegex.Matches(input);
            if (matches.Count < 4)
                return false;

            if (!double.TryParse(matches[0].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out x))
                return false;

            if (!double.TryParse(matches[1].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out y))
                return false;

            if (!double.TryParse(matches[2].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out z))
                return false;

            if (!double.TryParse(matches[3].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out heading))
                return false;

            return true;
        }
    }
}
