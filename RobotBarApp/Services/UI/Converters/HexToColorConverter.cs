using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace RobotBarApp.Converters
{
    /// Converts a hex color string ("#RRGGBB" or "#AARRGGBB") into a Color.
    /// Returns Transparent if input is null/invalid.
    public sealed class HexToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var s = value as string;
            if (string.IsNullOrWhiteSpace(s))
                return Colors.Transparent;

            try
            {
                var c = (Color)ColorConverter.ConvertFromString(s.Trim());
                return c;
            }
            catch
            {
                return Colors.Transparent;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}

