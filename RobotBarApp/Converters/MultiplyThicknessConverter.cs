using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace RobotBarApp.Converters
{
    /// <summary>
    /// Builds a symmetric horizontal Thickness based on a numeric value * factor.
    /// Usage: Width/Height -> returns new Thickness(spacing,0,spacing,0)
    /// </summary>
    public sealed class MultiplyThicknessConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return new Thickness(0);

            if (!TryToDouble(value, out var v))
                return new Thickness(0);

            if (!TryToDouble(parameter, out var factor))
                return new Thickness(0);

            var s = Math.Max(0, v * factor);
            return new Thickness(s, 0, s, 0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();

        private static bool TryToDouble(object value, out double result)
        {
            switch (value)
            {
                case double d:
                    result = d;
                    return true;
                case int i:
                    result = i;
                    return true;
                case string s when double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed):
                    result = parsed;
                    return true;
                default:
                    try
                    {
                        result = System.Convert.ToDouble(value, CultureInfo.InvariantCulture);
                        return true;
                    }
                    catch
                    {
                        result = 0;
                        return false;
                    }
            }
        }
    }
}

