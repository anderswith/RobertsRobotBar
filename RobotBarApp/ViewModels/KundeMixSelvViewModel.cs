namespace RobotBarApp.ViewModels;

using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using RobotBarApp.BLL.Interfaces;
using RobotBarApp.BE;

public sealed class KundeMixSelvViewModel : ViewModelBase
{
    public sealed class IngredientChoice
    {
        public IngredientChoice(string name, string imagePath, string? colorHex = null, bool isSelectable = true)
        {
            Name = name;
            ImagePath = imagePath;
            ColorHex = string.IsNullOrWhiteSpace(colorHex) ? "#7FD0D8" : colorHex;
            IsSelectable = isSelectable;
        }

        public string Name { get; }
        public string ImagePath { get; }
        public string ColorHex { get; }
        public bool IsSelectable { get; }
    }

    public sealed class SelectedIngredientItem : ViewModelBase
    {
        private int _cl;

        public SelectedIngredientItem(string name, string colorHex, int cl)
        {
            Name = name;
            ColorHex = string.IsNullOrWhiteSpace(colorHex) ? "#7FD0D8" : colorHex;
            _cl = cl;
        }

        public string Name { get; }
        public string ColorHex { get; }

        public int Cl
        {
            get => _cl;
            set
            {
                if (_cl == value) return;
                _cl = value;
                OnPropertyChanged();
            }
        }

        public string DisplayText => $"{Name} ({Cl}cl)";
    }

    public sealed class LiquidSegment : ViewModelBase
    {
        public LiquidSegment(int index)
        {
            Index = index;
        }

        public int Index { get; }

        private string _colorHex = "#00000000"; // transparent
        public string ColorHex
        {
            get => _colorHex;
            set
            {
                if (_colorHex == value) return;
                _colorHex = value;
                OnPropertyChanged();
            }
        }
    }

    private const int GlassMaxCl = 30;
    private const int SegmentCl = 2;
    private const int SegmentCount = GlassMaxCl / SegmentCl; // 15

    public event EventHandler? BackRequested;

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

    public ObservableCollection<SelectedIngredientItem> SelectedIngredients { get; } = new();
    public ObservableCollection<LiquidSegment> LiquidSegments { get; } = new();

    public IEnumerable<SelectedIngredientItem> SelectedIngredientsForDisplay => SelectedIngredients.Reverse();

    public bool HasSelectedIngredients => SelectedIngredients.Count > 0;

    // Selection (keep for now but now points at the last-added ingredient)
    private SelectedIngredientItem? _selectedIngredient;
    public SelectedIngredientItem? SelectedIngredient
    {
        get => _selectedIngredient;
        private set
        {
            _selectedIngredient = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(SelectedIngredientName));
            OnPropertyChanged(nameof(SelectedIngredientColorHex));
            RaiseCanExecute();
        }
    }

    public string SelectedIngredientName => SelectedIngredient?.Name ?? string.Empty;
    public string SelectedIngredientColorHex => SelectedIngredient?.ColorHex ?? "#7FD0D8";

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

    private readonly IIngredientLogic? _ingredientLogic;
    private readonly Guid _eventId;

    public KundeMixSelvViewModel()
    {
        BackCommand = new RelayCommand(_ => BackRequested?.Invoke(this, EventArgs.Empty));

        SelectCategoryCommand = new RelayCommand(param => SelectCategory(param?.ToString()));

        SelectIngredientCommand = new RelayCommand(param =>
        {
            if (param is IngredientChoice choice)
                SelectIngredient(choice);
        });

        IncreaseClCommand = new RelayCommand(param =>
        {
            if (param is SelectedIngredientItem item)
                IncreaseCl(item);
        }, param => param is SelectedIngredientItem item && CanIncreaseCl(item));

        DecreaseClCommand = new RelayCommand(param =>
        {
            if (param is SelectedIngredientItem item)
                DecreaseCl(item);
        }, param => param is SelectedIngredientItem item && CanDecreaseCl(item));

        BestilCommand = new RelayCommand(_ => Bestil(), _ => SelectedIngredients.Count > 0);

        SelectedIngredients.CollectionChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(HasSelectedIngredients));
            OnPropertyChanged(nameof(SelectedIngredientsForDisplay));
            RaiseCanExecute();
        };

        // init 15 segments
        for (var i = 0; i < SegmentCount; i++)
            LiquidSegments.Add(new LiquidSegment(i));

        UpdateLiquidVisuals();
        UpdateLiquidSegments();
    }

    public KundeMixSelvViewModel(IIngredientLogic ingredientLogic, Guid eventId = default)
        : this()
    {
        _ingredientLogic = ingredientLogic;
        _eventId = eventId;
    }

    private Guid EffectiveEventId => _eventId == Guid.Empty ? Guid.Empty : _eventId;

    private void SelectCategory(string? category)
    {
        var cat = string.IsNullOrWhiteSpace(category) ? "Kategori" : category;
        OverlayTitle = $"Vælg {cat.ToLower()}";

        OverlayIngredients.Clear();

        if (_ingredientLogic != null)
        {
            var hasEventContext = EffectiveEventId != Guid.Empty;

            // Map UI button labels to Ingredient.Type values in DB.
            // NOTE: Your DB uses types like "Alkohol", "Syrup", "Soda" (see IngredientLogic).
            string? desiredType = cat switch
            {
                "Alkohol" => "Alkohol",
                "Mockohol" => "Mockohol",
                "Sirup" => "Syrup",
                "Sodavand" => "Soda",
                _ => null
            };

            IEnumerable<Ingredient> ingredients;
            if (hasEventContext)
            {
                ingredients = cat switch
                {
                    "Alkohol" => _ingredientLogic.GetAlcohol(EffectiveEventId),
                    // If Mockohol is event-scoped in your DB via BarSetups, this should ideally be a dedicated BLL method.
                    // For now, filter all ingredients by the DB type.
                    "Mockohol" => _ingredientLogic.GetAllIngredients().Where(i => i.Type == "Mockohol"),
                    "Sirup" => _ingredientLogic.GetSyrups(EffectiveEventId),
                    "Sodavand" => _ingredientLogic.GetSoda(EffectiveEventId),
                    _ => _ingredientLogic.GetAllIngredients()
                };
            }
            else
            {
                // No event context: still filter correctly by type.
                ingredients = _ingredientLogic.GetAllIngredients();
                if (!string.IsNullOrWhiteSpace(desiredType))
                    ingredients = ingredients.Where(i => i.Type == desiredType);
            }

            foreach (var i in ingredients)
            {
                if (string.IsNullOrWhiteSpace(i.Name))
                    continue;

                var img = i.Image ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(img) && !img.StartsWith("/"))
                    img = "/" + img;

                // Disable already-added ingredients
                var isSelectable = !SelectedIngredients.Any(si => si.Name == i.Name);
                OverlayIngredients.Add(new IngredientChoice(i.Name, img, i.Color, isSelectable));
            }
        }
        else
        {
            // Fallback placeholders (designer/dev mode)
            OverlayIngredients.Add(new IngredientChoice("Vodka", "/Resources/IngredientPics/doge_20251210150457477.jpg"));
            OverlayIngredients.Add(new IngredientChoice("Rom", "/Resources/IngredientPics/doge2_20251210150515992.jpg"));
            OverlayIngredients.Add(new IngredientChoice("Tequila", "/Resources/IngredientPics/doge3_20251210150530232.jpg"));
        }

        IsOverlayOpen = true;
    }

    private void SelectIngredient(IngredientChoice choice)
    {
        if (!choice.IsSelectable)
            return;

        // Safety net: never add duplicates by name.
        if (SelectedIngredients.Any(i => i.Name == choice.Name))
        {
            IsOverlayOpen = false;
            return;
        }

        // Add ingredient as 2cl by default, stacked order.
        var item = new SelectedIngredientItem(choice.Name, choice.ColorHex, SegmentCl);
        SelectedIngredients.Add(item);
        OnPropertyChanged(nameof(HasSelectedIngredients));
        OnPropertyChanged(nameof(SelectedIngredientsForDisplay));

        SelectedIngredient = item;

        // close overlay
        IsOverlayOpen = false;

        UpdateLiquidSegments();
        UpdateLiquidVisuals();
        RaiseCanExecute();
    }

    private int CurrentTotalCl => SelectedIngredients.Sum(i => i.Cl);

    private bool CanIncreaseCl(SelectedIngredientItem? item)
        => item != null && CurrentTotalCl + SegmentCl <= GlassMaxCl;

    private bool CanDecreaseCl(SelectedIngredientItem? item)
        => item != null;

    private void IncreaseCl(SelectedIngredientItem item)
    {
        if (!CanIncreaseCl(item)) return;
        item.Cl += SegmentCl;
        UpdateLiquidSegments();
        UpdateLiquidVisuals();
        RaiseCanExecute();
        OnPropertyChanged(nameof(SelectedIngredientsForDisplay));
    }

    private void DecreaseCl(SelectedIngredientItem item)
    {
        // At minimum volume: remove ingredient entirely.
        if (item.Cl <= SegmentCl)
        {
            SelectedIngredients.Remove(item);
            if (ReferenceEquals(SelectedIngredient, item))
                SelectedIngredient = SelectedIngredients.LastOrDefault();

            UpdateLiquidSegments();
            UpdateLiquidVisuals();
            RaiseCanExecute();
            OnPropertyChanged(nameof(SelectedIngredientsForDisplay));

            // If overlay is open, refresh its items so the removed ingredient becomes selectable again.
            if (IsOverlayOpen)
            {
                // Re-run the last category filter by parsing title ("Vælg xxx").
                var cat = OverlayTitle.Replace("Vælg ", string.Empty, StringComparison.OrdinalIgnoreCase).Trim();
                SelectCategory(string.IsNullOrWhiteSpace(cat) ? null : CultureInfo.CurrentCulture.TextInfo.ToTitleCase(cat));
            }

            return;
        }

        // Otherwise decrease by 2cl.
        item.Cl -= SegmentCl;
        UpdateLiquidSegments();
        UpdateLiquidVisuals();
        RaiseCanExecute();
        OnPropertyChanged(nameof(SelectedIngredientsForDisplay));
    }

    private void Bestil()
    {
        // Placeholder. Later: call ordering service.
        MessageBox.Show($"Bestilt: {string.Join(", ", SelectedIngredients.Select(i => i.DisplayText))}");
    }

    private void UpdateLiquidSegments()
    {
        // StackPanel renders items top->bottom.
        // Because the panel is bottom-aligned, we want filled segments to appear at the bottom and grow upward.
        // So we fill from the end of the collection backwards.

        // Clear all.
        for (var i = 0; i < SegmentCount; i++)
            LiquidSegments[i].ColorHex = "#00000000";

        var writeIndex = SegmentCount - 1;
        foreach (var ing in SelectedIngredients)
        {
            var segs = Math.Max(1, ing.Cl / SegmentCl);
            for (var s = 0; s < segs && writeIndex >= 0; s++)
            {
                LiquidSegments[writeIndex].ColorHex = ing.ColorHex;
                writeIndex--;
            }
        }
    }

    // Update visuals based on total fill (reuse old math but use total cl)
    private void UpdateLiquidVisuals()
    {
        var cl = Math.Max(SegmentCl, Math.Min(GlassMaxCl, CurrentTotalCl));
        var t = (cl - SegmentCl) / (double)(GlassMaxCl - SegmentCl); // 0..1

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

        // plus/minus per ingredient uses CanExecute via RelayCommand<T>; if not present, UI will still be correct via IsEnabled bindings.
    }
}