using System;
using System.Globalization;
using System.Windows.Data;

namespace RobotBarApp.Converters
{
    /// <summary>
    /// Subtracts the converter parameter from a numeric value.
    /// Supports double/int inputs. Returns double.
    /// </summary>
    public sealed class SubtractConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return 0d;

            if (!TryToDouble(value, out var v))
                return 0d;

            if (!TryToDouble(parameter, out var sub))
                sub = 0d;

            return v - sub;
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
                case float f:
                    result = f;
                    return true;
                case int i:
                    result = i;
                    return true;
                case long l:
                    result = l;
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

