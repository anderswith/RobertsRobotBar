using System;
using System.Globalization;
using System.Windows.Data;
using RobotBarApp.Settings;

namespace RobotBarApp.Converters
{
    /// Shows whether the "decrease" button should be an X (remove) or a minus (decrease).
    /// - Normal ingredients: X at minimum (2cl).
    /// - Soda/Juice (Type == "Soda"): operates in 20cl chunks, so X when less than 2 chunks remain (&lt; 40cl).
    public sealed class ClToRemoveGlyphConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            // We sometimes bind the whole SelectedIngredientItem (preferred), but keep int support for safety.
            if (value is RobotBarApp.ViewModels.KundeMixSelvViewModel.SelectedIngredientItem item)
            {
                if (string.Equals(item.Type, "Soda", StringComparison.OrdinalIgnoreCase))
                    return item.Cl < (2 * MixSelvLimits.SodaChunkCl) ? "X" : "−";

                return item.Cl <= MixSelvLimits.StepCl ? "X" : "−";
            }

            if (value is int cl)
                return cl <= MixSelvLimits.StepCl ? "X" : "−";

            return "−";
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
