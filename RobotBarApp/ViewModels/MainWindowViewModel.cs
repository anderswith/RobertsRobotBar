using System.Windows;
using System.Windows.Input;
using RobotBarApp.BLL.Interfaces;
using RobotBarApp.Services.Interfaces;

namespace RobotBarApp.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly INavigationService _navigationService;
        private readonly IRobotLogic _robotLogic;
        

        public ViewModelBase CurrentViewModel => _navigationService.CurrentViewModel;

        // Commands
        public ICommand NavigateHomeCommand { get; }
        public ICommand NavigateStatistikCommand { get; }
        public ICommand NavigateKatalogCommand { get; }
        public ICommand NavigateTilfoejIngrediensCommand { get; }
        public ICommand NavigateTilfoejEventCommand { get; }
        public ICommand NavigateCalibrationCommand { get; }

        public MainWindowViewModel(INavigationService navigationService, IRobotLogic robotLogic)
        {
            _navigationService = navigationService;
            _robotLogic = robotLogic;
            _robotLogic.ConnectionFailed += OnRobotConnectionFailed;
            if (_robotLogic.ConnectionFailedAlready)
            {
                OnRobotConnectionFailed();
            }

            // Reager på ViewModel-skift i NavigationService
            _navigationService.OnViewModelChanged += () =>
                OnPropertyChanged(nameof(CurrentViewModel));

            // Home / Opstart
            NavigateHomeCommand = new RelayCommand(_ =>
                _navigationService.NavigateTo<EventListViewModel>());

            // Statistik
            NavigateStatistikCommand = new RelayCommand(_ =>
                _navigationService.NavigateTo<StatistikViewModel>());

            // Katalog
            NavigateKatalogCommand = new RelayCommand(_ =>
                _navigationService.NavigateTo<KatalogViewModel>());

            // Tilføj Ingrediens
            NavigateTilfoejIngrediensCommand = new RelayCommand(_ =>
                _navigationService.NavigateTo<TilfoejIngrediensViewModel>());

            // Tilføj Event
            NavigateTilfoejEventCommand = new RelayCommand(_ =>
                _navigationService.NavigateTo<TilfoejEventViewModel>(Guid.NewGuid()));

            // Kalibrering
            NavigateCalibrationCommand = new RelayCommand(_ =>
                _navigationService.NavigateTo<CalibrationWizardViewModel>());

            // Set default view
            _navigationService.NavigateTo<EventListViewModel>();
        }
        private void OnRobotConnectionFailed()
        {
            _ = ShowRobotConnectionFailedAsync();
        }

        private async Task ShowRobotConnectionFailedAsync()
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                MessageBox.Show(
                    "Could not connect to the robot.\nThe application will continue without robot connection.",
                    "Robot connection failed",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
            });
        }
    }
}
