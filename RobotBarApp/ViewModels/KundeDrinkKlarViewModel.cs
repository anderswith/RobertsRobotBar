using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Input;
using System.Windows.Threading;
using RobotBarApp.Services.Interfaces;

namespace RobotBarApp.ViewModels;

public class KundeDrinkKlarViewModel : ViewModelBase, IDisposable, INotifyPropertyChanged
{
    private readonly INavigationService _navigationService;
    private readonly DispatcherTimer _timer;
    private readonly Stopwatch _stopwatch = new();

    private const double TotalSeconds = 5.0;

    private double _progress = 1.0;
    public double Progress
    {
        get => _progress;
        private set
        {
            if (Math.Abs(_progress - value) < 0.0001) return;
            _progress = value;
            RaisePropertyChanged(nameof(Progress));
        }
    }

    private int _secondsRemaining = 5;
    public int SecondsRemaining
    {
        get => _secondsRemaining;
        private set
        {
            if (_secondsRemaining == value) return;
            _secondsRemaining = value;
            RaisePropertyChanged(nameof(SecondsRemaining));
        }
    }

    public ICommand DismissCommand { get; }

    public KundeDrinkKlarViewModel(INavigationService navigationService)
    {
        _navigationService = navigationService;

        DismissCommand = new DelegateCommand(Dismiss);

        // A smooth-ish UI update rate without being heavy.
        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(50)
        };

        _timer.Tick += OnTimerTick;

        // Initialize state
        Progress = 1.0;
        SecondsRemaining = (int)Math.Ceiling(TotalSeconds);

        _stopwatch.Start();
        _timer.Start();
    }

    private void OnTimerTick(object? sender, EventArgs e)
    {
        var elapsed = _stopwatch.Elapsed.TotalSeconds;
        var remaining = Math.Max(0.0, TotalSeconds - elapsed);

        // Progress drains from 1 -> 0
        Progress = remaining / TotalSeconds;

        // Countdown shows 5..1 (and reaches 0 at the very end)
        SecondsRemaining = (int)Math.Ceiling(remaining);

        if (remaining <= 0.0)
        {
            NavigateBack();
        }
    }

    private void Dismiss()
    {
        NavigateBack();
    }

    private void NavigateBack()
    {
        // Prevent double-navigation
        _timer.Stop();
        _timer.Tick -= OnTimerTick;
        _stopwatch.Stop();

        _navigationService.NavigateTo<KundeStartViewModel>();
    }

    public void Dispose()
    {
        _timer.Stop();
        _timer.Tick -= OnTimerTick;
        _stopwatch.Stop();
    }

    // Local, minimal ICommand implementation (keeps dependencies simple)
    private sealed class DelegateCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool>? _canExecute;

        public DelegateCommand(Action execute, Func<bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;

        public void Execute(object? parameter) => _execute();

        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }

    // Property change notifications (do not assume anything about ViewModelBase internals)
    public new event PropertyChangedEventHandler? PropertyChanged;

    private void RaisePropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
