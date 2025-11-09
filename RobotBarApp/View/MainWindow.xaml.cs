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
            var robo = new RoboComms("192.168.0.101");
            Console.WriteLine("entered mix drink");

            // Replace with your actual program name from the robot’s /programs folder
            robo.LoadAndRunProgram("test.urp");
            Console.WriteLine("Played script yay!");
            
            // Open the second window
            var mixWindow = new MixingWindow();
            mixWindow.ShowDialog();
        }
    }
}