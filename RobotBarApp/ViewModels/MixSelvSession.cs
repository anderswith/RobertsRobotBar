using System;
using System.Collections.ObjectModel;

namespace RobotBarApp.ViewModels;

/// <summary>
/// Carries the MixSelv drink state between screens (selection -> pour).
/// Kept as a single object so it can be passed via INavigationService.NavigateTo&lt;T&gt;(object parameter).
/// </summary>
public sealed class MixSelvSession
{
    public MixSelvSession(
        ObservableCollection<KundeMixSelvViewModel.SelectedIngredientItem> selectedIngredients,
        ObservableCollection<KundeMixSelvViewModel.LiquidSegment> liquidSegments,
        Guid eventId = default)
    {
        SelectedIngredients = selectedIngredients ?? throw new ArgumentNullException(nameof(selectedIngredients));
        LiquidSegments = liquidSegments ?? throw new ArgumentNullException(nameof(liquidSegments));
        EventId = eventId;
    }

    public ObservableCollection<KundeMixSelvViewModel.SelectedIngredientItem> SelectedIngredients { get; }
    public ObservableCollection<KundeMixSelvViewModel.LiquidSegment> LiquidSegments { get; }

    /// <summary>
    /// Optional: lets MixSelv keep event context when it's created for a specific event.
    /// </summary>
    public Guid EventId { get; }
}

