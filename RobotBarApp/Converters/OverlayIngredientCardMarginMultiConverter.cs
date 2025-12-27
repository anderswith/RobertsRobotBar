using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace RobotBarApp.Converters
{
    /// <summary>
    /// Computes horizontal margin (Thickness) for overlay ingredient cards.
    /// values[0] = overlay width
    /// </summary>
    public sealed class OverlayIngredientCardMarginMultiConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length < 1)
                return new Thickness(0);

            if (!TryToDouble(values[0], out var w))
                return new Thickness(0);

            // Small spacing relative to overlay width.
            var spacing = Math.Round(Math.Max(6, w * 0.015), 0);
            return new Thickness(spacing, 0, spacing, 0);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
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

