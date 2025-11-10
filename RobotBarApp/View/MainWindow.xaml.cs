using System.Windows;

namespace RobotBarApp
{
    public partial class MainWindow : Window
    {
        private bool _menuOpen;

        public MainWindow()
        {
            InitializeComponent();

            // Sample data for the cards:
            FestivalItems.ItemsSource = new[]
            {
                new { Title = "Tønder Festival", ImagePath = "Images/TonderFestival.jpg" },
                new { Title = "Naturvidenskabsfestival", ImagePath = "Images/Naturvidenskab.jpg" }
            };
        }

        private void HomeButton_Click(object sender, RoutedEventArgs e)
        {
            // Navigate to main screen (this one) – your logic here
        }

        private void MenuButton_Click(object sender, RoutedEventArgs e)
        {
            _menuOpen = !_menuOpen;
            MenuColumn.Width = _menuOpen ? new GridLength(260) : new GridLength(0);
        }
    }

}