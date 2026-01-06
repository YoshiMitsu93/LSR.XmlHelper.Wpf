using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;

namespace LSR.XmlHelper.Wpf.Infrastructure.Converters
{
    public sealed class RelativePathConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var fullPath = values.Length > 0 ? values[0] as string : null;
            var rootFolder = values.Length > 1 ? values[1] as string : null;

            if (string.IsNullOrWhiteSpace(fullPath))
                return "";

            if (string.IsNullOrWhiteSpace(rootFolder))
                return Path.GetFileName(fullPath);

            try
            {
                var root = rootFolder.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                if (fullPath.StartsWith(root, StringComparison.OrdinalIgnoreCase))
                {
                    var start = root.Length;
                    if (fullPath.Length > start)
                    {
                        var ch = fullPath[start];
                        if (ch == Path.DirectorySeparatorChar || ch == Path.AltDirectorySeparatorChar)
                            start++;
                    }

                    if (start >= 0 && start < fullPath.Length)
                        return fullPath.Substring(start);
                }
            }
            catch
            {
            }

            return Path.GetFileName(fullPath);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return Array.Empty<object>();
        }
    }
}
