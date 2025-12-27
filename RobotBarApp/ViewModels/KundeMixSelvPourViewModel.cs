using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RobotBarApp.ViewModels
{
    /// <summary>
    /// Pour screen for MixSelv. Reuses the selected ingredients + liquid segments from the MixSelv session,
    /// and adds a progress value (0..100) to drive a progress bar.
    /// </summary>
    public sealed class KundeMixSelvPourViewModel : ViewModelBase
    {
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

        public ICommand BackCommand { get; }

        private readonly CancellationTokenSource _cts = new();

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

