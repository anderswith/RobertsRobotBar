using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace RobotBarApp.View
{
    public partial class KundeMixSelvView : UserControl
    {
        public event EventHandler? BackRequested;

        public KundeMixSelvView()
        {
            InitializeComponent();
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            BackRequested?.Invoke(this, EventArgs.Empty);
        }

        private void Category_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn) return;

            OverlayTitle.Text = $"Vælg {btn.Tag}";
            Overlay.Visibility = Visibility.Visible;

            OverlayItems.ItemsSource = new List<(string Name, string ImagePath)>
            {
                ("Vodka", "/Resources/IngredientPics/doge_20251210150457477.jpg"),
                ("Rom", "/Resources/IngredientPics/doge2_20251210150515992.jpg"),
                ("Tequila", "/Resources/IngredientPics/doge3_20251210150530232.jpg"),
                ("Dry gin", "/Resources/IngredientPics/ingredient_20251207221554255.jpg"),
                ("Champagne", "/Resources/IngredientPics/imagecheck8_20251207220129565.jpg")
            };
        }

        private void IngredientCard_Click(object sender, RoutedEventArgs e)
        {
            Overlay.Visibility = Visibility.Collapsed;
        }

        private void OverlayScrollLeft_Click(object sender, RoutedEventArgs e)
            => OverlayScroll.ScrollToHorizontalOffset(OverlayScroll.HorizontalOffset - 700);

        private void OverlayScrollRight_Click(object sender, RoutedEventArgs e)
            => OverlayScroll.ScrollToHorizontalOffset(OverlayScroll.HorizontalOffset + 700);
    }
}