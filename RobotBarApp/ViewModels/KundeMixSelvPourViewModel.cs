﻿using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using RobotBarApp.BLL.Interfaces;
using RobotBarApp.Services.Interfaces;

namespace RobotBarApp.ViewModels
{
    /// Pour screen for MixSelv. Reuses the selected ingredients + liquid segments from the MixSelv session,
    /// and adds a progress value (0..100) to drive a progress bar.
    public sealed class KundeMixSelvPourViewModel : ViewModelBase
    {
        private readonly IRobotLogic _robotLogic;
        public event EventHandler? BackRequested;

        public ObservableCollection<KundeMixSelvViewModel.SelectedIngredientItem> SelectedIngredients { get; }
        public ObservableCollection<KundeMixSelvViewModel.LiquidSegment> LiquidSegments { get; }

        public IEnumerable<KundeMixSelvViewModel.SelectedIngredientItem> SelectedIngredientsForDisplay
            => SelectedIngredients.Reverse();

        public bool HasSelectedIngredients => SelectedIngredients.Count > 0;

        private double _pourProgress;
        public double PourProgress
        {
            get => _pourProgress;
            set { _pourProgress = value; OnPropertyChanged(); }
        }
        
        public string StepCounterText => $"{CurrentStep}/{TotalSteps}";

        private int _currentStep;
        public int CurrentStep
        {
            get => _currentStep;
            set
            {
                if (SetProperty(ref _currentStep, value))
                    OnPropertyChanged(nameof(StepCounterText));
            }
        }

        private int _totalSteps;
        public int TotalSteps
        {
            get => _totalSteps;
            set
            {
                if (SetProperty(ref _totalSteps, value))
                    OnPropertyChanged(nameof(StepCounterText));
            }
        }

        public ICommand BackCommand { get; }

        private readonly INavigationService? _navigation;
        private readonly CancellationTokenSource _cts = new();

        // Constructor used by NavigationService (single parameter)
        public KundeMixSelvPourViewModel(MixSelvSession session, INavigationService navigation, IRobotLogic robotLogic)
        {
            _navigation = navigation;
            _robotLogic = robotLogic;
            _robotLogic.ScriptFinished += OnScriptFinished;
            _robotLogic.DrinkFinished += OnDrinkFinished;
            _robotLogic.ScriptsStarted += OnScriptsStarted;

            SelectedIngredients = session.SelectedIngredients;
            LiquidSegments = session.LiquidSegments;

            SelectedIngredients.CollectionChanged += (_, _) =>
            {
                OnPropertyChanged(nameof(HasSelectedIngredients));
                OnPropertyChanged(nameof(SelectedIngredientsForDisplay));
            };

            BackCommand = new RelayCommand(_ => NavigateBackFresh());

            // Temporary demo progress (replace with robot pour progress later).
            _ = RunFakeProgressAsync(_cts.Token);
        }
        private void OnScriptsStarted(int total)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                TotalSteps = total;
                CurrentStep = 1; 
            });
        }
        

        // Fallback for designer/legacy usage
        public KundeMixSelvPourViewModel(
            ObservableCollection<KundeMixSelvViewModel.SelectedIngredientItem> selectedIngredients,
            ObservableCollection<KundeMixSelvViewModel.LiquidSegment> liquidSegments)
        {
            SelectedIngredients = selectedIngredients;
            LiquidSegments = liquidSegments;

            SelectedIngredients.CollectionChanged += (_, _) =>
            {
                OnPropertyChanged(nameof(HasSelectedIngredients));
                OnPropertyChanged(nameof(SelectedIngredientsForDisplay));
            };

            BackCommand = new RelayCommand(_ => BackRequested?.Invoke(this, EventArgs.Empty));

            // Temporary demo progress (replace with robot pour progress later).
            _ = RunFakeProgressAsync(_cts.Token);
        }
        
        private void OnDrinkFinished()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                _robotLogic.DrinkFinished -= OnDrinkFinished;

                _navigation!.NavigateTo<KundeDrinkKlarViewModel>();
            });
        }

        private void OnScriptFinished(int finished, int total)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                CurrentStep = finished;
                TotalSteps = total;
            });
        }

        private void NavigateBackFresh()
        {
            _cts.Cancel();

            if (_robotLogic != null)
                _robotLogic.DrinkFinished -= OnDrinkFinished;

            if (_navigation == null)
            {
                BackRequested?.Invoke(this, EventArgs.Empty);
                return;
            }

            var emptySession = KundeMixSelvViewModelFactory.CreateEmptySession();
            _navigation.NavigateTo<KundeMixSelvViewModel>(emptySession);
        }

        private async Task RunFakeProgressAsync(CancellationToken ct)
        {
            PourProgress = 0;
            while (PourProgress < 100 && !ct.IsCancellationRequested)
            {
                await Task.Delay(80, ct);
                PourProgress += 2;
            }
        }
    }
}
