using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Media;
using RobotBarApp.ViewModels;

namespace RobotBarApp.View
{
    public partial class MainWindow : Window
    {
        private bool _menuOpen;
        private const double MenuWidth = 260; // keep in sync with XAML
        private TranslateTransform? _menuTransform; // nullable until captured

        private MainWindowViewModel ViewModel => (MainWindowViewModel)DataContext;

        public MainWindow()
        {
            InitializeComponent();

            // Set runtime DataContext for now (later can use DI)
            if (DataContext is not MainWindowViewModel vm)
            {
                vm = new MainWindowViewModel();
                DataContext = vm;
            }

            // Ensure initial view is EventListViewModel
            ViewModel.CurrentViewModel = new EventListViewModel();

            // Cache transform for performance / easier access
            _menuTransform = SideMenu.RenderTransform as TranslateTransform;
        }

        private void HomeButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.CurrentViewModel = new EventListViewModel();
        }

        private void OpstartButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.CurrentViewModel = new EventListViewModel();
            ToggleMenu(false);
        }

        private void StatistikButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.CurrentViewModel = new StatistikViewModel();
            ToggleMenu(false);
        }

        private void KatalogButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.CurrentViewModel = new KatalogViewModel();
            ToggleMenu(false);
        }

        private void TilfoejIngrediensButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.CurrentViewModel = new TilfoejIngrediensViewModel();
            ToggleMenu(false);
        }

        private void TilfoejDrinkButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.CurrentViewModel = new TilfoejDrinkViewModel();
            ToggleMenu(false);
        }

        private void TilfoejMenuButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.CurrentViewModel = new TilfoejMenuViewModel();
            ToggleMenu(false);
        }

        private void TilfoejEventButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.CurrentViewModel = new TilfoejEventViewModel();
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