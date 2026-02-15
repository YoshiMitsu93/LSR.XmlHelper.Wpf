using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace LSR.XmlHelper.Wpf.Services.Parsing
{
    public sealed class SmartVector2PasteParser
    {
        private static readonly Regex NumberRegex = new Regex(@"(?<![A-Za-z_])[-+]?\d+(?:\.\d+)?", RegexOptions.Compiled);

        public bool TryParseManyXy(string input, out List<(double X, double Y)> points)
        {
            points = new List<(double X, double Y)>();

            if (string.IsNullOrWhiteSpace(input))
                return false;

            var lines = input.Replace("\r\n", "\n").Split('\n');

            foreach (var ln in lines)
            {
                var line = (ln ?? "").Trim();
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var matches = NumberRegex.Matches(line);
                if (matches.Count < 2)
                    continue;

                if (!double.TryParse(matches[0].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var x))
                    continue;

                if (!double.TryParse(matches[1].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var y))
                    continue;

                points.Add((x, y));
            }

            return points.Count >= 3;
        }
    }
}
