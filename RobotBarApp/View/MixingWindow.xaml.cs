using System.Windows;
using System.Windows.Threading;

namespace RobotBarApp
{
    public partial class MixingWindow : Window
    {
        private DispatcherTimer _timer;
        private int _progress = 0;

        public MixingWindow()
        {
            InitializeComponent();
            StartMixing();
        }

        private void StartMixing()
        {
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(100);
            _timer.Tick += (s, e) =>
            {
                _progress++;
                MixProgress.Value = _progress;
                if (_progress >= 100)
                {
                    _timer.Stop();
                    this.Title = "Drink Ready!";
                }
            };
            _timer.Start();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}