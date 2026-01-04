using System.Collections.ObjectModel;
using RobotBarApp.Settings;

namespace RobotBarApp.ViewModels;

/// Central place for creating fresh MixSelv state (used when navigating back from the pour screen).
public static class KundeMixSelvViewModelFactory
{
    public static MixSelvSession CreateEmptySession()
    {
        var selected = new ObservableCollection<KundeMixSelvViewModel.SelectedIngredientItem>();

        var segments = new ObservableCollection<KundeMixSelvViewModel.LiquidSegment>();
        var segmentCount = MixSelvLimits.GlassMaxCl / MixSelvLimits.StepCl;
        for (var i = 0; i < segmentCount; i++)
            segments.Add(new KundeMixSelvViewModel.LiquidSegment(i));

        return new MixSelvSession(selected, segments);
    }
}

