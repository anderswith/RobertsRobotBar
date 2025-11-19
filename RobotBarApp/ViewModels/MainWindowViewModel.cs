using System.Windows.Input;

namespace RobotBarApp.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private object? _currentViewModel;
        private bool _isMenuOpen;

        public object? CurrentViewModel
        {
            get => _currentViewModel;
            set => SetProperty(ref _currentViewModel, value);
        }

        public bool IsMenuOpen
        {
            get => _isMenuOpen;
            set => SetProperty(ref _isMenuOpen, value);
        }

        public ICommand ToggleMenuCommand { get; }
        public ICommand CloseMenuCommand { get; }

        public MainWindowViewModel()
        {
            ToggleMenuCommand = new RelayCommand(_ => IsMenuOpen = !IsMenuOpen);
            CloseMenuCommand = new RelayCommand(_ => IsMenuOpen = false);
        }
    }
}
