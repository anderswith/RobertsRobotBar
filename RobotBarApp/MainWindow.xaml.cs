using System.Windows;

namespace RobotBarApp
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void MixDrink_Click(object sender, RoutedEventArgs e)
        {
            // Open the second window
            var mixWindow = new MixingWindow();
            mixWindow.ShowDialog();
        }
    }
}