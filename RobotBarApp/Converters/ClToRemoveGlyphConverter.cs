using System;
using System.Globalization;
using System.Windows.Data;

namespace RobotBarApp.Converters
{
    /// <summary>
    /// If the ingredient is at minimum volume (2cl), show an 'X' so user can remove it.
    /// Otherwise show the minus sign.
    /// </summary>
    public sealed class ClToRemoveGlyphConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int cl && cl <= 2)
                return "X";

            return "−";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}

