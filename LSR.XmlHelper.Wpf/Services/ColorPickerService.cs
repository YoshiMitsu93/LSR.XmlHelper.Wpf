using System.Globalization;
using WpfMedia = System.Windows.Media;
using WinForms = System.Windows.Forms;

namespace LSR.XmlHelper.Wpf.Services
{
    public sealed class ColorPickerService
    {
        public bool TryPick(string? initialHex, out string pickedHex)
        {
            pickedHex = "";

            var initial = TryParseWpfColor(initialHex, out var wpf)
                ? wpf
                : WpfMedia.Colors.White;

            using var dlg = new WinForms.ColorDialog
            {
                FullOpen = true,
                Color = System.Drawing.Color.FromArgb(initial.A, initial.R, initial.G, initial.B)
            };

            var result = dlg.ShowDialog();
            if (result != WinForms.DialogResult.OK)
                return false;

            var c = dlg.Color;
            pickedHex = ToHexArgb(c.A, c.R, c.G, c.B);
            return true;
        }

        private static bool TryParseWpfColor(string? s, out WpfMedia.Color c)
        {
            c = default;

            if (string.IsNullOrWhiteSpace(s))
                return false;

            try
            {
                var obj = WpfMedia.ColorConverter.ConvertFromString(s);
                if (obj is WpfMedia.Color col)
                {
                    c = col;
                    return true;
                }
            }
            catch
            {
            }

            return false;
        }

        private static string ToHexArgb(byte a, byte r, byte g, byte b)
        {
            return string.Create(CultureInfo.InvariantCulture, $"#{a:X2}{r:X2}{g:X2}{b:X2}");
        }
    }
}