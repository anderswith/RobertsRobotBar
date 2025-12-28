using RobotBarApp.Services.Interfaces;

namespace RobotBarApp.ViewModels
{
    public class KundeMainViewModel : ViewModelBase
    {
        private readonly INavigationService _navigationService;

        public ViewModelBase? CurrentViewModel
            => _navigationService.CurrentViewModel;

        public KundeMainViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;

            // React to navigation changes
            _navigationService.OnViewModelChanged += () =>
                OnPropertyChanged(nameof(CurrentViewModel));

            // Default customer screen
            _navigationService.NavigateTo<KundeStartViewModel>();
        }
    }
}