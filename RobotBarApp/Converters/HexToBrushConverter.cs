using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace RobotBarApp.Converters
{
    /// <summary>
    /// Converts a hex color string into a SolidColorBrush.
    /// Returns Transparent brush if input is null/invalid.
    /// </summary>
    public sealed class HexToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var s = value as string;
            if (string.IsNullOrWhiteSpace(s))
                return Brushes.Transparent;

            try
            {
                var c = (Color)ColorConverter.ConvertFromString(s.Trim());
                var b = new SolidColorBrush(c);
                b.Freeze();
                return b;
            }
            catch
            {
                return Brushes.Transparent;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}

