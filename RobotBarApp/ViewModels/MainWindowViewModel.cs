using System.Windows.Input;
using RobotBarApp.Services.Interfaces;

namespace RobotBarApp.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly INavigationService _navigationService;

        public ViewModelBase CurrentViewModel => _navigationService.CurrentViewModel;

        // Commands
        public ICommand NavigateHomeCommand { get; }
        public ICommand NavigateStatistikCommand { get; }
        public ICommand NavigateKatalogCommand { get; }
        public ICommand NavigateTilfoejIngrediensCommand { get; }
        public ICommand NavigateTilfoejEventCommand { get; }

        public MainWindowViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;

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

            // Set default view
            _navigationService.NavigateTo<EventListViewModel>();
        }
    }
}
