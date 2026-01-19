using System.Windows.Media;

namespace LSR.XmlHelper.Wpf.Services.Appearance
{
    public static class HexColorParser
    {
        public static bool TryParseColor(string? hex, out System.Windows.Media.Color color)
        {
            color = default;

            if (string.IsNullOrWhiteSpace(hex))
                return false;

            try
            {
                var obj = System.Windows.Media.ColorConverter.ConvertFromString(hex);
                if (obj is System.Windows.Media.Color c)
                {
                    color = c;
                    return true;
                }
            }
            catch
            {
            }

            return false;
        }
    }
}
