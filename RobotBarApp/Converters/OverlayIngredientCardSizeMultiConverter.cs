using System;
using System.Globalization;
using System.Windows.Data;

namespace RobotBarApp.Converters
{
    /// <summary>
    /// Computes an ingredient card size (width or height) from overlay available size.
    /// values[0] = overlay width (double)
    /// values[1] = overlay height (double)
    /// parameter = "W" or "H"
    /// </summary>
    public sealed class OverlayIngredientCardSizeMultiConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length < 2)
                return 0d;

            if (!TryToDouble(values[0], out var w))
                return 0d;
            if (!TryToDouble(values[1], out var h))
                return 0d;

            // Leave room for arrows and margins.
            // Overlay border is ~1500x720, inner grid has Margin=30.
            // We choose height-driven sizing so cards fit nicely.
            var availableH = Math.Max(0, h);

            // Card height: about 70% of overlay height
            var cardH = Math.Round(availableH * 0.70, 0);
            // Card width: slightly narrower than height
            var cardW = Math.Round(cardH * 0.55, 0);

            var mode = parameter?.ToString();
            return string.Equals(mode, "H", StringComparison.OrdinalIgnoreCase) ? cardH : cardW;
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
                case float f:
                    result = f;
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

