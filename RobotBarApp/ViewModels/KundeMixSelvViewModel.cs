namespace RobotBarApp.ViewModels;

using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

public sealed class KundeMixSelvViewModel : ViewModelBase
{
    public sealed class IngredientChoice
    {
        public IngredientChoice(string name, string imagePath)
        {
            Name = name;
            ImagePath = imagePath;
        }

        public string Name { get; }
        public string ImagePath { get; }
    }

    public event EventHandler? BackRequested;

    private const int MinCl = 2;
    private const int MaxCl = 10;

    // Overlay
    private bool _isOverlayOpen;
    public bool IsOverlayOpen
    {
        get => _isOverlayOpen;
        set
        {
            _isOverlayOpen = value;
            OnPropertyChanged();
        }
    }

    private string _overlayTitle = "";
    public string OverlayTitle
    {
        get => _overlayTitle;
        set
        {
            _overlayTitle = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<IngredientChoice> OverlayIngredients { get; } = new();

    // Selection
    private IngredientChoice? _selectedIngredient;
    public IngredientChoice? SelectedIngredient
    {
        get => _selectedIngredient;
        private set
        {
            _selectedIngredient = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(SelectedIngredientDisplayText));
            OnPropertyChanged(nameof(SelectedIngredientName));

            RaiseCanExecute();
        }
    }

    public string SelectedIngredientName => SelectedIngredient?.Name ?? string.Empty;

    private int _selectedCl;
    public int SelectedCl
    {
        get => _selectedCl;
        private set
        {
            _selectedCl = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(SelectedIngredientDisplayText));

            UpdateLiquidVisuals();
            RaiseCanExecute();
        }
    }

    public string SelectedIngredientDisplayText
        => SelectedIngredient == null ? string.Empty : $"{SelectedIngredient.Name} ({SelectedCl}cl)";

    // Liquid visuals (bound directly from VM)
    private double _liquidDiscWidth;
    public double LiquidDiscWidth
    {
        get => _liquidDiscWidth;
        private set { _liquidDiscWidth = value; OnPropertyChanged(); }
    }

    private double _liquidDiscHeight;
    public double LiquidDiscHeight
    {
        get => _liquidDiscHeight;
        private set { _liquidDiscHeight = value; OnPropertyChanged(); }
    }

    private Thickness _liquidDiscMargin;
    public Thickness LiquidDiscMargin
    {
        get => _liquidDiscMargin;
        private set { _liquidDiscMargin = value; OnPropertyChanged(); }
    }

    private double _liquidHighlightWidth;
    public double LiquidHighlightWidth
    {
        get => _liquidHighlightWidth;
        private set { _liquidHighlightWidth = value; OnPropertyChanged(); }
    }

    private double _liquidHighlightHeight;
    public double LiquidHighlightHeight
    {
        get => _liquidHighlightHeight;
        private set { _liquidHighlightHeight = value; OnPropertyChanged(); }
    }

    private Thickness _liquidHighlightMargin;
    public Thickness LiquidHighlightMargin
    {
        get => _liquidHighlightMargin;
        private set { _liquidHighlightMargin = value; OnPropertyChanged(); }
    }

    // Commands
    public ICommand BackCommand { get; }
    public ICommand SelectCategoryCommand { get; }
    public ICommand SelectIngredientCommand { get; }
    public ICommand IncreaseClCommand { get; }
    public ICommand DecreaseClCommand { get; }
    public ICommand BestilCommand { get; }

    public KundeMixSelvViewModel()
    {
        BackCommand = new RelayCommand(_ => BackRequested?.Invoke(this, EventArgs.Empty));

        SelectCategoryCommand = new RelayCommand(param => SelectCategory(param?.ToString()));

        SelectIngredientCommand = new RelayCommand(
            param =>
            {
                if (param is IngredientChoice choice)
                    SelectIngredient(choice);
            });

        IncreaseClCommand = new RelayCommand(_ => IncreaseCl(), _ => CanIncreaseCl());
        DecreaseClCommand = new RelayCommand(_ => DecreaseCl(), _ => CanDecreaseCl());

        BestilCommand = new RelayCommand(_ => Bestil(), _ => SelectedIngredient != null);

        // Set defaults for bindings
        SelectedCl = MinCl;
        UpdateLiquidVisuals();
    }

    private void SelectCategory(string? category)
    {
        var cat = string.IsNullOrWhiteSpace(category) ? "Kategori" : category;
        OverlayTitle = $"Vælg {cat.ToLower()}";

        OverlayIngredients.Clear();

        // TODO: Replace with real data source (BLL/DB) when ready.
        OverlayIngredients.Add(new IngredientChoice("Vodka", "/Resources/IngredientPics/doge_20251210150457477.jpg"));
        OverlayIngredients.Add(new IngredientChoice("Rom", "/Resources/IngredientPics/doge2_20251210150515992.jpg"));
        OverlayIngredients.Add(new IngredientChoice("Tequila", "/Resources/IngredientPics/doge3_20251210150530232.jpg"));
        OverlayIngredients.Add(new IngredientChoice("Dry gin", "/Resources/IngredientPics/ingredient_20251207221554255.jpg"));
        OverlayIngredients.Add(new IngredientChoice("Champagne", "/Resources/IngredientPics/imagecheck8_20251207220129565.jpg"));

        IsOverlayOpen = true;
    }

    private void SelectIngredient(IngredientChoice choice)
    {

        SelectedIngredient = choice;
        SelectedCl = MinCl;
        IsOverlayOpen = false;

        RaiseCanExecute();
    }

    private bool CanIncreaseCl() => SelectedIngredient != null && SelectedCl < MaxCl;
    private bool CanDecreaseCl() => SelectedIngredient != null && SelectedCl > MinCl;

    private void IncreaseCl()
    {
        if (!CanIncreaseCl()) return;
        SelectedCl++;
    }

    private void DecreaseCl()
    {
        if (!CanDecreaseCl()) return;
        SelectedCl--;
    }

    private void Bestil()
    {
        // Placeholder. Later: call ordering service.
        // Keep UI out of VM? If you prefer, we can raise an event instead.
        MessageBox.Show($"Bestilt: {SelectedIngredientName} ({SelectedCl}cl)");
    }

    private void UpdateLiquidVisuals()
    {
        // Mirror the old code-behind calculation.
        var cl = Math.Max(MinCl, Math.Min(MaxCl, SelectedCl));
        var t = (cl - MinCl) / (double)(MaxCl - MinCl); // 0..1

        var minW = 150.0;
        var maxW = 185.0;
        var widthCurve = Math.Pow(t, 0.7);
        var w = minW + (maxW - minW) * widthCurve;

        var minH = 18.0;
        var maxH = 95.0;
        var h = minH + (maxH - minH) * t;

        var centerX = 260.0;
        var left = centerX - w / 2.0;

        var bottomY = 370.0;
        var top = bottomY - h;

        LiquidDiscWidth = w;
        LiquidDiscHeight = h;
        LiquidDiscMargin = new Thickness(left, top, 0, 0);

        LiquidHighlightWidth = w * 0.48;
        LiquidHighlightHeight = Math.Max(10.0, h * 0.18);
        LiquidHighlightMargin = new Thickness(left + w * 0.2, top + h * 0.12, 0, 0);
    }

    private void RaiseCanExecute()
    {
        if (IncreaseClCommand is RelayCommand rc1) rc1.RaiseCanExecuteChanged();
        if (DecreaseClCommand is RelayCommand rc2) rc2.RaiseCanExecuteChanged();
        if (BestilCommand is RelayCommand rc3) rc3.RaiseCanExecuteChanged();
    }
}