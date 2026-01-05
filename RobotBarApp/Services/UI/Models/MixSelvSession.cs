using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace RobotBarApp.ViewModels;

/// Carries the MixSelv drink state between screens (selection -> pour).
/// Kept as a single object so it can be passed via INavigationService.NavigateTo<T>(object parameter).
public sealed class MixSelvSession
{
    public MixSelvSession(
        ObservableCollection<KundeMixSelvViewModel.SelectedIngredientItem> selectedIngredients,
        ObservableCollection<KundeMixSelvViewModel.LiquidSegment> liquidSegments,
        Guid eventId = default)
    {
        SelectedIngredients = selectedIngredients 
                              ?? throw new ArgumentNullException(nameof(selectedIngredients));

        LiquidSegments = liquidSegments 
                         ?? throw new ArgumentNullException(nameof(liquidSegments));

        EventId = eventId;

        // ✅ THIS IS THE IMPORTANT PART
        Order = selectedIngredients
            .Select(i => (i.IngredientId, i.Cl))
            .ToList();
    }

    public ObservableCollection<KundeMixSelvViewModel.SelectedIngredientItem> SelectedIngredients { get; }
    public ObservableCollection<KundeMixSelvViewModel.LiquidSegment> LiquidSegments { get; }

    /// Resolved robot execution order (UI-independent)
    public List<(Guid IngredientId, int Cl)> Order { get; }

    /// Optional: lets MixSelv keep event context when it's created for a specific event.
    public Guid EventId { get; }
}