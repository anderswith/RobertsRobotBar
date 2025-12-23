using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace RobotBarApp.View
{
    public partial class KundeMixSelvView : UserControl
    {
        public event EventHandler? BackRequested;

        private string _selectedName = "";
        private int _selectedCl = 0;

        private const int MinCl = 2;
        private const int MaxCl = 10;

        public KundeMixSelvView()
        {
            InitializeComponent();
        }

        private void KundeMixSelvView_Loaded(object sender, RoutedEventArgs e)
        {
            LiquidLayer.Clip = CreateBowlClipGeometry();
        }

        private Geometry CreateBowlClipGeometry()
        {
            // Trapezoid clip approximating the inner walls of the glass (normalized 520x520).
            var topY = 210.0;
            var bottomY = 455.0;

            var topLeftX = 200.0;
            var topRightX = 315.0;

            var bottomLeftX = 235.0;
            var bottomRightX = 285.0;

            var g = new StreamGeometry();
            using (var ctx = g.Open())
            {
                ctx.BeginFigure(new Point(topLeftX, topY), isFilled: true, isClosed: true);
                ctx.LineTo(new Point(topRightX, topY), true, false);
                ctx.LineTo(new Point(bottomRightX, bottomY), true, false);
                ctx.LineTo(new Point(bottomLeftX, bottomY), true, false);
            }

            g.Freeze();
            return g;
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            BackRequested?.Invoke(this, EventArgs.Empty);
        }

        private void Category_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn) return;

            var category = btn.Tag?.ToString() ?? "Kategori";
            OverlayTitle.Text = $"Vælg {category.ToLower()}";

            OverlayItems.ItemsSource = new List<(string Name, string ImagePath)>
            {
                ("Vodka", "/Resources/IngredientPics/doge_20251210150457477.jpg"),
                ("Rom", "/Resources/IngredientPics/doge2_20251210150515992.jpg"),
                ("Tequila", "/Resources/IngredientPics/doge3_20251210150530232.jpg"),
                ("Dry gin", "/Resources/IngredientPics/ingredient_20251207221554255.jpg"),
                ("Champagne", "/Resources/IngredientPics/imagecheck8_20251207220129565.jpg")
            };

            OverlayItems.ItemTemplate = (DataTemplate)FindResource("IngredientCardTemplate");
            Overlay.Visibility = Visibility.Visible;
        }

        private void IngredientCard_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not FrameworkElement fe || fe.DataContext is not ValueTuple<string, string> data)
                return;

            _selectedName = data.Item1;
            _selectedCl = MinCl;

            ApplySelectedIngredientUI();

            Overlay.Visibility = Visibility.Collapsed;
        }

        private void ApplySelectedIngredientUI()
        {
            SelectedIngredientPanel.Visibility = Visibility.Visible;
            BestilButton.Visibility = Visibility.Visible;

            LiquidLayer.Visibility = Visibility.Visible;
            ConnectorLine.Visibility = Visibility.Visible;

            SelectedIngredientText.Text = $"{_selectedName} ({_selectedCl}cl)";
            UpdateDiscForCl(_selectedCl);
        }

        private void Minus_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedCl <= MinCl) return;
            _selectedCl--;

            SelectedIngredientText.Text = $"{_selectedName} ({_selectedCl}cl)";
            UpdateDiscForCl(_selectedCl);
        }

        private void Plus_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedCl >= MaxCl) return;
            _selectedCl++;

            SelectedIngredientText.Text = $"{_selectedName} ({_selectedCl}cl)";
            UpdateDiscForCl(_selectedCl);
        }

        private void UpdateDiscForCl(int cl)
        {
            var t = (cl - MinCl) / (double)(MaxCl - MinCl); // 0..1

            // Grow in-place (width + height)
            var minW = 150.0;
            var maxW = 185.0;
            var widthCurve = Math.Pow(t, 0.7); // < 1.0 = narrower top
            var w = minW + (maxW - minW) * widthCurve;

            var minH = 18.0;
            var maxH = 95.0;
            var h = minH + (maxH - minH) * t;

            // Center in glass
            var centerX = 260.0;
            var left = centerX - w / 2.0;

            // Anchor to a bottom Y inside the glass
            var bottomY = 370.0;
            var top = bottomY - h;

            LiquidDisc.Width = w;
            LiquidDisc.Height = h;
            LiquidDisc.Margin = new Thickness(left, top, 0, 0);

            // Highlight tracks disc
            LiquidHighlight.Width = w * 0.48;
            LiquidHighlight.Height = Math.Max(10.0, h * 0.18);
            LiquidHighlight.Margin = new Thickness(left + w * 0.2, top + h * 0.12, 0, 0);
        }

        private void Bestil_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show($"Bestilt: {_selectedName} ({_selectedCl}cl)");
        }

        private void OverlayScrollLeft_Click(object sender, RoutedEventArgs e)
            => OverlayScroll.ScrollToHorizontalOffset(OverlayScroll.HorizontalOffset - 700);

        private void OverlayScrollRight_Click(object sender, RoutedEventArgs e)
            => OverlayScroll.ScrollToHorizontalOffset(OverlayScroll.HorizontalOffset + 700);
    }
}
