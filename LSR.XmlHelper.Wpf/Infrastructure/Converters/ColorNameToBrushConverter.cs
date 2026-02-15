using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace LSR.XmlHelper.Wpf.Infrastructure
{
    public sealed class ColorNameToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not string name || string.IsNullOrWhiteSpace(name))
                return System.Windows.Media.Brushes.Transparent;

            try
            {
                var color = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(name);
                var brush = new System.Windows.Media.SolidColorBrush(color);
                brush.Freeze();
                return brush;
            }
            catch
            {
                return System.Windows.Media.Brushes.Transparent;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return System.Windows.Data.Binding.DoNothing;
        }
    }
}
