using RobotBarApp.ViewModels;

namespace RobotBarApp.Services.Interfaces;

public interface INavigationService
{
    ViewModelBase CurrentViewModel { get; }
    void NavigateTo<TViewModel>() where TViewModel : ViewModelBase;
    event Action OnViewModelChanged;
    void NavigateTo<TViewModel>(object parameter) where TViewModel : ViewModelBase;
}