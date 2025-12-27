using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace RobotBarApp.Services.UI
{
    /// <summary>
    /// Holds tweakable sizing values for customer-facing carousels.
    /// Initialized once from the primary screen size (kiosk) and then reused.
    /// </summary>
    public sealed class CarouselSizingSettings : INotifyPropertyChanged
    {
        private bool _isInitialized;

        public event PropertyChangedEventHandler? PropertyChanged;

        public bool IsInitialized
        {
            get => _isInitialized;
            private set
            {
                if (_isInitialized == value) return;
                _isInitialized = value;
                OnPropertyChanged();
            }
        }

        // Base screen size we derived numbers from (for troubleshooting)
        public double ScreenWidth { get; private set; }
        public double ScreenHeight { get; private set; }

        // Multipliers (easy to tweak later)
        public double DrinkHeightRatio { get; set; } = 0.56;        // % of screen height (smaller so more cards fit)
        public double DrinkWidthFromHeight { get; set; } = 0.62;    // width = height * this
        public double IngredientHeightRatio { get; set; } = 0.42;
        public double IngredientWidthFromHeight { get; set; } = 0.52;

        public double HorizontalItemSpacingRatio { get; set; } = 0.012; // % of screen width

        // Computed sizes (bind to these)
        public double DrinkCardHeight { get; private set; }
        public double DrinkCardWidth { get; private set; }
        public Thickness DrinkCardMargin { get; private set; }

        public double IngredientCardHeight { get; private set; }
        public double IngredientCardWidth { get; private set; }
        public Thickness IngredientCardMargin { get; private set; }

        /// <summary>
        /// Global instance for XAML bindings. Initialized from DI in KundeStartView.
        /// </summary>
        public static CarouselSizingSettings Instance { get; } = new();

        public void InitializeFromScreenSize(double width, double height)
        {
            if (width <= 0 || height <= 0)
                throw new ArgumentOutOfRangeException(nameof(width), "Screen size must be positive.");

            ScreenWidth = width;
            ScreenHeight = height;

            Recalculate();
            IsInitialized = true;
        }

        public void Recalculate()
        {
            // Drinks
            DrinkCardHeight = Math.Round(ScreenHeight * DrinkHeightRatio, 0);
            DrinkCardWidth = Math.Round(DrinkCardHeight * DrinkWidthFromHeight, 0);

            // Shared spacing
            var spacing = Math.Round(ScreenWidth * HorizontalItemSpacingRatio, 0);
            DrinkCardMargin = new Thickness(spacing, 0, spacing, 0);

            // Ingredients
            IngredientCardHeight = Math.Round(ScreenHeight * IngredientHeightRatio, 0);
            IngredientCardWidth = Math.Round(IngredientCardHeight * IngredientWidthFromHeight, 0);
            IngredientCardMargin = new Thickness(spacing, 0, spacing, 0);

            OnPropertyChanged(nameof(ScreenWidth));
            OnPropertyChanged(nameof(ScreenHeight));

            OnPropertyChanged(nameof(DrinkCardHeight));
            OnPropertyChanged(nameof(DrinkCardWidth));
            OnPropertyChanged(nameof(DrinkCardMargin));

            OnPropertyChanged(nameof(IngredientCardHeight));
            OnPropertyChanged(nameof(IngredientCardWidth));
            OnPropertyChanged(nameof(IngredientCardMargin));
        }

        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
