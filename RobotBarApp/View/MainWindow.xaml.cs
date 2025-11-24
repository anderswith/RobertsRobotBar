using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using RobotBarApp.ViewModels;

namespace RobotBarApp.View
{
    public partial class MainWindow : Window
    {
        private bool _menuOpen;
        private const double MenuWidth = 260;
        private TranslateTransform? _menuTransform;

        // Strongly typed ViewModel from DI container
        private MainWindowViewModel ViewModel => (MainWindowViewModel)DataContext;

        // ✔ DI constructor – no default constructor needed
        public MainWindow(MainWindowViewModel viewModel)
        {
            InitializeComponent();

            // Set the injected ViewModel from DI
            DataContext = viewModel;

            // Cache transform for animations (SideMenu is loaded in XAML)
            Loaded += (_, __) =>
            {
                _menuTransform = SideMenu.RenderTransform as TranslateTransform;
            };
        }

        // --- UI Animation / Menu Logic ---

        private void MenuButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleMenu(!_menuOpen);
        }

        private void MenuOverlay_MouseLeftButtonDown(
            object sender,
            System.Windows.Input.MouseButtonEventArgs e)
        {
            if (_menuOpen)
                ToggleMenu(false);
        }

        private void ToggleMenu(bool open)
        {
            _menuOpen = open;

            if (_menuTransform == null)
            {
                _menuTransform = new TranslateTransform { X = MenuWidth };
                SideMenu.RenderTransform = _menuTransform;
            }

            var duration = new Duration(TimeSpan.FromMilliseconds(280));
            var easing = new CubicEase { EasingMode = EasingMode.EaseOut };

            double from = _menuTransform.X;
            double to = open ? 0 : MenuWidth;

            var slideAnim = new DoubleAnimation(from, to, duration)
            {
                EasingFunction = easing
            };

            double overlayFrom = MenuOverlay.Opacity;
            double overlayTo = open ? 0.35 : 0.0;

            var overlayAnim = new DoubleAnimation(overlayFrom, overlayTo, duration)
            {
                EasingFunction = easing
            };

            MenuOverlay.IsHitTestVisible = open;

            _menuTransform.BeginAnimation(TranslateTransform.XProperty, slideAnim);
            MenuOverlay.BeginAnimation(OpacityProperty, overlayAnim);
        }
    }
}
