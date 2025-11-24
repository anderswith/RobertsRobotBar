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
            // Resolve ViewModel via DI
            CurrentViewModel = _provider.GetRequiredService<TViewModel>();

            // Fire event so MainWindowViewModel updates UI
            OnViewModelChanged?.Invoke();
        }
    }
}