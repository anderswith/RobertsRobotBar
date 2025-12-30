using System.Windows.Threading;
using RobotBarApp.Services.Interfaces;

namespace RobotBarApp.ViewModels;

public class KundeDrinkKlarViewModel : ViewModelBase, IDisposable
{
    private readonly INavigationService _navigationService;
    private readonly DispatcherTimer _timer;

    public string Message => "Din drink er klar – du må nu fjerne glasset";

    public KundeDrinkKlarViewModel(INavigationService navigationService)
    {
        _navigationService = navigationService;

        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(5) // 5 sec
        };

        _timer.Tick += OnTimerTick;
        _timer.Start();
    }

    private void OnTimerTick(object? sender, EventArgs e)
    {
        _timer.Stop();
        _timer.Tick -= OnTimerTick;

        _navigationService.NavigateTo<KundeStartViewModel>();
    }

    public void Dispose()
    {
        _timer.Stop();
        _timer.Tick -= OnTimerTick;
    }
}