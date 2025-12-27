﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace RobotBarApp.Behaviors
{
    /// <summary>
    /// Attached behavior for a horizontal ScrollViewer hosting an ItemsControl.
    /// 
    /// Features:
    /// - Debounced snap-to-center (after user stops scrolling)
    /// - Scales the centered item container
    /// - Two-step tap: tap off-center => center it; tap centered => executes command
    /// 
    /// Requirements:
    /// - ScrollViewer.Content contains (or is) an ItemsControl.
    /// - ItemsPanel is horizontal (StackPanel is fine).
    /// - CanContentScroll should be false for best pixel-perfect snapping.
    /// </summary>
    public static class CenteredSnapCarouselBehavior
    {
        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.RegisterAttached(
                "IsEnabled",
                typeof(bool),
                typeof(CenteredSnapCarouselBehavior),
                new PropertyMetadata(false, OnIsEnabledChanged));

        public static void SetIsEnabled(DependencyObject obj, bool value) => obj.SetValue(IsEnabledProperty, value);
        public static bool GetIsEnabled(DependencyObject obj) => (bool)obj.GetValue(IsEnabledProperty);

        public static readonly DependencyProperty ItemTemplateRootNameProperty =
            DependencyProperty.RegisterAttached(
                "ItemTemplateRootName",
                typeof(string),
                typeof(CenteredSnapCarouselBehavior),
                new PropertyMetadata(string.Empty));

        /// <summary>
        /// Optional: name of the root element inside your DataTemplate to scale.
        /// If empty, the behavior scales the item container (ContentPresenter).
        /// </summary>
        public static void SetItemTemplateRootName(DependencyObject obj, string value) => obj.SetValue(ItemTemplateRootNameProperty, value);
        public static string GetItemTemplateRootName(DependencyObject obj) => (string)obj.GetValue(ItemTemplateRootNameProperty);

        public static readonly DependencyProperty HighlightScaleProperty =
            DependencyProperty.RegisterAttached(
                "HighlightScale",
                typeof(double),
                typeof(CenteredSnapCarouselBehavior),
                new PropertyMetadata(1.15));

        public static void SetHighlightScale(DependencyObject obj, double value) => obj.SetValue(HighlightScaleProperty, value);
        public static double GetHighlightScale(DependencyObject obj) => (double)obj.GetValue(HighlightScaleProperty);

        public static readonly DependencyProperty SnapDebounceMsProperty =
            DependencyProperty.RegisterAttached(
                "SnapDebounceMs",
                typeof(int),
                typeof(CenteredSnapCarouselBehavior),
                new PropertyMetadata(200));

        public static void SetSnapDebounceMs(DependencyObject obj, int value) => obj.SetValue(SnapDebounceMsProperty, value);
        public static int GetSnapDebounceMs(DependencyObject obj) => (int)obj.GetValue(SnapDebounceMsProperty);

        public static readonly DependencyProperty SnapAnimationMsProperty =
            DependencyProperty.RegisterAttached(
                "SnapAnimationMs",
                typeof(int),
                typeof(CenteredSnapCarouselBehavior),
                new PropertyMetadata(250));

        public static void SetSnapAnimationMs(DependencyObject obj, int value) => obj.SetValue(SnapAnimationMsProperty, value);
        public static int GetSnapAnimationMs(DependencyObject obj) => (int)obj.GetValue(SnapAnimationMsProperty);

        public static readonly DependencyProperty TapCommandProperty =
            DependencyProperty.RegisterAttached(
                "TapCommand",
                typeof(ICommand),
                typeof(CenteredSnapCarouselBehavior),
                new PropertyMetadata(null));

        public static void SetTapCommand(DependencyObject obj, ICommand? value) => obj.SetValue(TapCommandProperty, value);
        public static ICommand? GetTapCommand(DependencyObject obj) => (ICommand?)obj.GetValue(TapCommandProperty);

        public static readonly DependencyProperty DoubleTapThresholdMsProperty =
            DependencyProperty.RegisterAttached(
                "DoubleTapThresholdMs",
                typeof(int),
                typeof(CenteredSnapCarouselBehavior),
                new PropertyMetadata(650));

        public static void SetDoubleTapThresholdMs(DependencyObject obj, int value) => obj.SetValue(DoubleTapThresholdMsProperty, value);
        public static int GetDoubleTapThresholdMs(DependencyObject obj) => (int)obj.GetValue(DoubleTapThresholdMsProperty);

        // Private attached state container
        private static readonly DependencyProperty StateProperty =
            DependencyProperty.RegisterAttached(
                "State",
                typeof(State),
                typeof(CenteredSnapCarouselBehavior),
                new PropertyMetadata(null));

        private sealed class State
        {
            public required ScrollViewer ScrollViewer { get; init; }
            public required ItemsControl ItemsControl { get; init; }
            public DispatcherTimer? DebounceTimer { get; set; }

            public int CenteredIndex { get; set; } = -1;
            public FrameworkElement? CenteredElement { get; set; }

            public bool IsProgrammaticScroll { get; set; }

            public object? LastTappedItem { get; set; }
            public DateTime LastTapTimeUtc { get; set; }
        }

        private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not ScrollViewer sv)
                return;

            if ((bool)e.NewValue)
                Attach(sv);
            else
                Detach(sv);
        }

        private static void Attach(ScrollViewer sv)
        {
            if (sv.GetValue(StateProperty) is State)
                return;

            // Find ItemsControl inside ScrollViewer
            sv.Loaded += SvOnLoaded;
        }

        private static void SvOnLoaded(object sender, RoutedEventArgs e)
        {
            if (sender is not ScrollViewer sv)
                return;

            sv.Loaded -= SvOnLoaded;

            var ic = FindItemsControl(sv);
            if (ic == null)
                return;

            // Strongly recommend pixel scrolling.
            sv.CanContentScroll = false;

            var state = new State
            {
                ScrollViewer = sv,
                ItemsControl = ic,
            };
            sv.SetValue(StateProperty, state);

            sv.ScrollChanged += SvOnScrollChanged;

            // Intercept taps (use Down so we beat inner Buttons)
            sv.PreviewMouseLeftButtonDown += SvOnPreviewMouseLeftButtonDown;

            // Initial highlight once layout is ready
            sv.Dispatcher.BeginInvoke(new Action(() =>
            {
                EnsureEdgePadding(state);
                var idx = FindNearestIndexToCenter(state);
                ApplyCenter(state, idx, animate: false);
            }), DispatcherPriority.Loaded);
        }

        private static void Detach(ScrollViewer sv)
        {
            sv.Loaded -= SvOnLoaded;
            sv.ScrollChanged -= SvOnScrollChanged;
            sv.PreviewMouseLeftButtonDown -= SvOnPreviewMouseLeftButtonDown;

            if (sv.GetValue(StateProperty) is State state)
            {
                state.DebounceTimer?.Stop();
            }

            sv.ClearValue(StateProperty);
        }

        private static void SvOnScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (sender is not ScrollViewer sv)
                return;

            if (sv.GetValue(StateProperty) is not State state)
                return;

            if (state.IsProgrammaticScroll)
                return;

            // Keep edge padding up to date when viewport size changes or items re-measure.
            if (e.ViewportWidthChange != 0 || e.ExtentWidthChange != 0)
                EnsureEdgePadding(state);

            // Debounce snap
            state.DebounceTimer ??= new DispatcherTimer();
            state.DebounceTimer.Interval = TimeSpan.FromMilliseconds(GetSnapDebounceMs(sv));
            state.DebounceTimer.Tick -= DebounceTick;
            state.DebounceTimer.Tick += DebounceTick;
            state.DebounceTimer.Stop();
            state.DebounceTimer.Start();

            void DebounceTick(object? s, EventArgs _)
            {
                state.DebounceTimer?.Stop();

                var idx = FindNearestIndexToCenter(state);
                ApplyCenter(state, idx, animate: true);
            }
        }

        // NEW: handle taps early so inner Buttons don't re-route the click to the previously centered item
        private static void SvOnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is not ScrollViewer sv)
                return;

            if (sv.GetValue(StateProperty) is not State state)
                return;

            var element = e.OriginalSource as DependencyObject;
            if (element == null)
                return;

            // Only handle taps that originated from an item container.
            var container = state.ItemsControl.ContainerFromElement(element) as FrameworkElement;
            if (container == null)
                return;

            var item = container.DataContext;
            if (item == null)
                return;

            var tappedIndex = state.ItemsControl.ItemContainerGenerator.IndexFromContainer(container);
            if (tappedIndex < 0)
                return;

            if (tappedIndex != state.CenteredIndex)
            {
                ApplyCenter(state, tappedIndex, animate: true);
                e.Handled = true;
                return;
            }

            // Centered item: execute on single tap.
            var cmd = GetTapCommand(sv);
            if (cmd != null && cmd.CanExecute(item))
            {
                cmd.Execute(item);
                e.Handled = true;
            }
        }

        private static void SvOnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            // We handle on MouseDown now; keep this as a no-op safety net.
            // (Leaving the method present avoids re-wiring work if we later add touch handling.)
        }

        private static int FindNearestIndexToCenter(State state)
        {
            var ic = state.ItemsControl;
            if (ic.Items.Count == 0)
                return -1;

            var sv = state.ScrollViewer;
            var viewportCenter = sv.HorizontalOffset + sv.ViewportWidth / 2.0;

            var bestIndex = -1;
            var bestDist = double.MaxValue;

            for (var i = 0; i < ic.Items.Count; i++)
            {
                var container = ic.ItemContainerGenerator.ContainerFromIndex(i) as FrameworkElement;
                if (container == null)
                    continue;

                var itemCenter = GetElementCenterXInScrollViewer(sv, container);
                var dist = Math.Abs(itemCenter - viewportCenter);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    bestIndex = i;
                }
            }

            return bestIndex;
        }

        private static void ApplyCenter(State state, int index, bool animate)
        {
            if (index < 0 || index >= state.ItemsControl.Items.Count)
                return;

            state.CenteredIndex = index;

            // Reset tap-confirmation state when center changes.
            state.LastTappedItem = null;
            state.LastTapTimeUtc = default;

            var container = state.ItemsControl.ItemContainerGenerator.ContainerFromIndex(index) as FrameworkElement;
            if (container == null)
                return;

            // Scale highlight
            UpdateHighlight(state, container);

            // Snap
            var sv = state.ScrollViewer;
            var desiredOffset = ComputeOffsetToCenter(sv, container);
            AnimateScrollTo(sv, desiredOffset, animate ? GetSnapAnimationMs(sv) : 0, state);
        }

        private static void UpdateHighlight(State state, FrameworkElement newCenteredContainer)
        {
            if (state.CenteredElement != null)
                SetScale(state.ScrollViewer, state.CenteredElement, 1.0);

            state.CenteredElement = newCenteredContainer;
            SetScale(state.ScrollViewer, newCenteredContainer, GetHighlightScale(state.ScrollViewer));
        }

        private static void SetScale(ScrollViewer sv, FrameworkElement container, double scale)
        {
            FrameworkElement target = container;
            var rootName = GetItemTemplateRootName(sv);
            if (!string.IsNullOrWhiteSpace(rootName))
            {
                var found = FindNamedDescendant(container, rootName);
                if (found != null)
                    target = found;
            }

            if (target.RenderTransform is not ScaleTransform st)
            {
                st = new ScaleTransform(1.0, 1.0);
                target.RenderTransform = st;
                target.RenderTransformOrigin = new Point(0.5, 0.5);
            }

            st.BeginAnimation(ScaleTransform.ScaleXProperty, new DoubleAnimation(scale, TimeSpan.FromMilliseconds(120))
            {
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            });
            st.BeginAnimation(ScaleTransform.ScaleYProperty, new DoubleAnimation(scale, TimeSpan.FromMilliseconds(120))
            {
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            });
        }

        private static double ComputeOffsetToCenter(ScrollViewer sv, FrameworkElement container)
        {
            var itemCenter = GetElementCenterXInScrollViewer(sv, container);
            var viewportCenter = sv.HorizontalOffset + sv.ViewportWidth / 2.0;
            var delta = itemCenter - viewportCenter;

            var target = sv.HorizontalOffset + delta;
            if (target < 0) target = 0;
            if (target > sv.ScrollableWidth) target = sv.ScrollableWidth;

            return target;
        }

        private static double GetElementCenterXInScrollViewer(ScrollViewer sv, FrameworkElement element)
        {
            var point = element.TransformToAncestor(sv).Transform(new Point(0, 0));
            var left = point.X + sv.HorizontalOffset;
            return left + element.ActualWidth / 2.0;
        }

        private static void AnimateScrollTo(ScrollViewer sv, double targetOffset, int durationMs, State state)
        {
            if (durationMs <= 0)
            {
                state.IsProgrammaticScroll = true;
                sv.ScrollToHorizontalOffset(targetOffset);
                state.IsProgrammaticScroll = false;
                return;
            }

            state.IsProgrammaticScroll = true;

            var anim = new DoubleAnimation(sv.HorizontalOffset, targetOffset, TimeSpan.FromMilliseconds(durationMs))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut },
                FillBehavior = FillBehavior.Stop
            };

            var helper = new OffsetAnimationHelper(sv, state);
            anim.Completed += (_, _) =>
            {
                sv.ScrollToHorizontalOffset(targetOffset);
                state.IsProgrammaticScroll = false;
            };
            helper.BeginAnimation(OffsetAnimationHelper.AnimatedHorizontalOffsetProperty, anim);
        }

        private sealed class OffsetAnimationHelper : Animatable
        {
            private readonly ScrollViewer _sv;

            public OffsetAnimationHelper(ScrollViewer sv, State state)
            {
                _sv = sv;
            }

            public static readonly DependencyProperty AnimatedHorizontalOffsetProperty =
                DependencyProperty.Register(
                    nameof(AnimatedHorizontalOffset),
                    typeof(double),
                    typeof(OffsetAnimationHelper),
                    new PropertyMetadata(0.0, OnAnimatedHorizontalOffsetChanged));

            public double AnimatedHorizontalOffset
            {
                get => (double)GetValue(AnimatedHorizontalOffsetProperty);
                set => SetValue(AnimatedHorizontalOffsetProperty, value);
            }

            private static void OnAnimatedHorizontalOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
            {
                if (d is not OffsetAnimationHelper h)
                    return;

                h._sv.ScrollToHorizontalOffset((double)e.NewValue);
            }

            protected override Freezable CreateInstanceCore()
                => new OffsetAnimationHelper(_sv, null!);
        }

        private static ItemsControl? FindItemsControl(DependencyObject root)
        {
            if (root is ItemsControl ic)
                return ic;

            var count = VisualTreeHelper.GetChildrenCount(root);
            for (var i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(root, i);
                var found = FindItemsControl(child);
                if (found != null)
                    return found;
            }

            return null;
        }

        private static FrameworkElement? FindNamedDescendant(DependencyObject root, string name)
        {
            var count = VisualTreeHelper.GetChildrenCount(root);
            for (var i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(root, i);
                if (child is FrameworkElement fe)
                {
                    if (fe.Name == name)
                        return fe;

                    var found = FindNamedDescendant(fe, name);
                    if (found != null)
                        return found;
                }
            }
            return null;
        }

        private static void EnsureEdgePadding(State state)
        {
            var sv = state.ScrollViewer;
            var ic = state.ItemsControl;

            if (ic.Items.Count == 0)
                return;

            var first = ic.ItemContainerGenerator.ContainerFromIndex(0) as FrameworkElement;
            if (first == null || first.ActualWidth <= 0 || sv.ViewportWidth <= 0)
                return;

            // Required padding so first/last can reach center.
            var pad = Math.Max(0, (sv.ViewportWidth - first.ActualWidth) / 2.0);

            // Find the items panel (typically the StackPanel) and set its margin.
            var panel = FindItemsPanel(ic);
            if (panel == null)
                return;

            // Preserve any vertical margin already set.
            var current = panel.Margin;
            var desired = new Thickness(pad, current.Top, pad, current.Bottom);
            if (!ThicknessEquals(current, desired))
                panel.Margin = desired;
        }

        private static Panel? FindItemsPanel(ItemsControl ic)
        {
            if (VisualTreeHelper.GetChildrenCount(ic) == 0)
                return null;

            // ItemsPresenter -> Panel
            var presenter = FindVisualChild<ItemsPresenter>(ic);
            if (presenter == null)
                return null;

            presenter.ApplyTemplate();
            if (VisualTreeHelper.GetChildrenCount(presenter) == 0)
                return null;

            return VisualTreeHelper.GetChild(presenter, 0) as Panel;
        }

        private static T? FindVisualChild<T>(DependencyObject root) where T : DependencyObject
        {
            var count = VisualTreeHelper.GetChildrenCount(root);
            for (var i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(root, i);
                if (child is T t)
                    return t;

                var found = FindVisualChild<T>(child);
                if (found != null)
                    return found;
            }
            return null;
        }

        private static bool ThicknessEquals(Thickness a, Thickness b)
            => Math.Abs(a.Left - b.Left) < 0.5
               && Math.Abs(a.Top - b.Top) < 0.5
               && Math.Abs(a.Right - b.Right) < 0.5
               && Math.Abs(a.Bottom - b.Bottom) < 0.5;
    }
}
