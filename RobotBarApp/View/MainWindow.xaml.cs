using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Media;
using System.Windows.Controls;

namespace RobotBarApp.View
{
    public partial class MainWindow : Window
    {
        private bool _menuOpen;
        private const double MenuWidth = 260; // keep in sync with XAML
        private TranslateTransform? _menuTransform; // nullable until captured

        public MainWindow()
        {
            InitializeComponent();

            // Cache transform for performance / easier access
            _menuTransform = SideMenu.RenderTransform as TranslateTransform;

            // Show initial view
            ShowView(new EventListView());
        }

        private void ShowView(UserControl view)
        {
            MainContentHost.Content = view;
        }

        private void HomeButton_Click(object sender, RoutedEventArgs e) => ShowView(new EventListView());

        private void OpstartButton_Click(object sender, RoutedEventArgs e)
        {
            ShowView(new EventListView());
            ToggleMenu(false);
        }

        private void StatistikButton_Click(object sender, RoutedEventArgs e)
        {
            ShowView(new StatistikView());
            ToggleMenu(false);
        }

        private void KatalogButton_Click(object sender, RoutedEventArgs e)
        {
            ShowView(new KatalogView());
            ToggleMenu(false);
        }

        private void TilfoejIngrediensButton_Click(object sender, RoutedEventArgs e)
        {
            ShowView(new TilfoejIngrediensView());
            ToggleMenu(false);
        }

        private void TilfoejDrinkButton_Click(object sender, RoutedEventArgs e)
        {
            ShowView(new TilfoejDrinkView());
            ToggleMenu(false);
        }

        private void TilfoejMenuButton_Click(object sender, RoutedEventArgs e)
        {
            ShowView(new TilfoejMenuView());
            ToggleMenu(false);
        }

        private void TilfoejEventButton_Click(object sender, RoutedEventArgs e)
        {
            ShowView(new TilfoejEventView());
            ToggleMenu(false);
        }

        private void MenuButton_Click(object sender, RoutedEventArgs e) => ToggleMenu(!_menuOpen);

        private void MenuOverlay_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (_menuOpen)
            {
                ToggleMenu(false);
            }
        }

        private void ToggleMenu(bool open)
        {
            _menuOpen = open;

            if (_menuTransform == null)
            {
                _menuTransform = new TranslateTransform { X = MenuWidth };
                SideMenu.RenderTransform = _menuTransform;
            }

            // Animation duration & easing
            var duration = new Duration(System.TimeSpan.FromMilliseconds(280));
            var easing = new CubicEase { EasingMode = EasingMode.EaseOut };

            // Slide animation
            double from = _menuTransform.X;
            double to = open ? 0 : MenuWidth;
            var slideAnim = new DoubleAnimation(from, to, duration) { EasingFunction = easing };

            // Overlay fade animation
            double overlayFrom = MenuOverlay.Opacity;
            double overlayTo = open ? 0.35 : 0.0;
            var overlayAnim = new DoubleAnimation(overlayFrom, overlayTo, duration) { EasingFunction = easing };

            MenuOverlay.IsHitTestVisible = open;

            _menuTransform.BeginAnimation(TranslateTransform.XProperty, slideAnim);
            MenuOverlay.BeginAnimation(UIElement.OpacityProperty, overlayAnim);
        }
    }
}