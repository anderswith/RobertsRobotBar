using System;
using Microsoft.Extensions.DependencyInjection;
using RobotBarApp.Services.Interfaces;
using RobotBarApp.ViewModels;

namespace RobotBarApp.Services
{
    public class NavigationService : INavigationService
    {
        private readonly IServiceProvider _provider;

        public NavigationService(IServiceProvider provider)
        {
            _provider = provider;
        }

        public ViewModelBase CurrentViewModel { get; private set; }

        public event Action OnViewModelChanged;

        public void NavigateTo<TViewModel>() where TViewModel : ViewModelBase
        {
            CurrentViewModel = _provider.GetRequiredService<TViewModel>();
            OnViewModelChanged?.Invoke();
        }

        // NEW overload
        public void NavigateTo<TViewModel>(object parameter) where TViewModel : ViewModelBase
        {
            CurrentViewModel = (ViewModelBase)ActivatorUtilities.CreateInstance(
                _provider,
                typeof(TViewModel),
                parameter);

            OnViewModelChanged?.Invoke();
        }
    }
}