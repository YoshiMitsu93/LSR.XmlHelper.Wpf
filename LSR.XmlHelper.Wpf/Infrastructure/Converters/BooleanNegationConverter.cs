using System;
using System.Globalization;
using System.Windows.Data;

namespace LSR.XmlHelper.Wpf.Infrastructure
{
    public sealed class BooleanNegationConverter : IValueConverter
    {
        public static BooleanNegationConverter Instance { get; } = new BooleanNegationConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is bool b ? !b : value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is bool b ? !b : value;
        }
    }
}
