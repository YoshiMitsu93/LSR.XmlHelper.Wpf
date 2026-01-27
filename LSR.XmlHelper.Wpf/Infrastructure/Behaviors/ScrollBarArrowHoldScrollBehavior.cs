using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace LSR.XmlHelper.Wpf.Infrastructure
{
    public static class ScrollBarArrowHoldScrollBehavior
    {
        private static readonly Dictionary<System.Windows.Controls.Primitives.ButtonBase, HoldState> HoldStatesByButton = new Dictionary<System.Windows.Controls.Primitives.ButtonBase, HoldState>();
        private static readonly HashSet<FrameworkElement> HookedRoots = new HashSet<FrameworkElement>();

        public static readonly DependencyProperty EnableProperty =
            DependencyProperty.RegisterAttached(
                "Enable",
                typeof(bool),
                typeof(ScrollBarArrowHoldScrollBehavior),
                new PropertyMetadata(false, OnEnableChanged));

        public static bool GetEnable(DependencyObject obj) => (bool)obj.GetValue(EnableProperty);
        public static void SetEnable(DependencyObject obj, bool value) => obj.SetValue(EnableProperty, value);

        public static readonly DependencyProperty SpeedScaleProperty =
            DependencyProperty.RegisterAttached(
                "SpeedScale",
                typeof(double),
                typeof(ScrollBarArrowHoldScrollBehavior),
                new FrameworkPropertyMetadata(1d, FrameworkPropertyMetadataOptions.Inherits));

        public static double GetSpeedScale(DependencyObject obj) => (double)obj.GetValue(SpeedScaleProperty);
        public static void SetSpeedScale(DependencyObject obj, double value) => obj.SetValue(SpeedScaleProperty, value);

        private static double GetEffectiveSpeedScale(DependencyObject startingAt)
        {
            var current = startingAt;

            while (current != null)
            {
                var source = DependencyPropertyHelper.GetValueSource(current, SpeedScaleProperty);
                if (source.BaseValueSource != BaseValueSource.Default)
                {
                    var value = (double)current.GetValue(SpeedScaleProperty);
                    if (value > 0)
                    {
                        return value;
                    }
                }

                var visualParent = VisualTreeHelper.GetParent(current);
                if (visualParent != null)
                {
                    current = visualParent;
                    continue;
                }

                if (current is FrameworkElement fe)
                {
                    if (fe.Parent != null)
                    {
                        current = fe.Parent;
                        continue;
                    }

                    if (fe.TemplatedParent != null)
                    {
                        current = fe.TemplatedParent;
                        continue;
                    }
                }

                current = LogicalTreeHelper.GetParent(current);
            }

            return 1d;
        }
        private static void OnEnableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not FrameworkElement root)
            {
                return;
            }

            var enabled = e.NewValue is bool b && b;

            if (enabled)
            {
                root.Loaded -= RootOnLoaded;
                root.Unloaded -= RootOnUnloaded;
                root.Loaded += RootOnLoaded;
                root.Unloaded += RootOnUnloaded;

                if (root.IsLoaded)
                {
                    AttachToRoot(root);
                }
            }
            else
            {
                root.Loaded -= RootOnLoaded;
                root.Unloaded -= RootOnUnloaded;
                DetachFromRoot(root);
            }
        }

        private static void RootOnLoaded(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement root)
            {
                AttachToRoot(root);
            }
        }

        private static void RootOnUnloaded(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement root)
            {
                DetachFromRoot(root);
            }
        }

        private static void AttachToRoot(FrameworkElement root)
        {
            if (!HookedRoots.Add(root))
            {
                return;
            }

            root.LayoutUpdated -= RootOnLayoutUpdated;
            root.LayoutUpdated += RootOnLayoutUpdated;

            AttachToScrollBarsUnder(root);
        }

        private static void DetachFromRoot(FrameworkElement root)
        {
            if (!HookedRoots.Remove(root))
            {
                return;
            }

            root.LayoutUpdated -= RootOnLayoutUpdated;
            DetachButtonsUnder(root);
        }

        private static void RootOnLayoutUpdated(object? sender, EventArgs e)
        {
            if (sender is FrameworkElement root)
            {
                AttachToScrollBarsUnder(root);
            }
        }

        private static void AttachToScrollBarsUnder(FrameworkElement root)
        {
            foreach (var scrollBar in FindVisualDescendants<System.Windows.Controls.Primitives.ScrollBar>(root))
            {
                AttachToScrollBar(scrollBar);
            }
        }

        private static void AttachToScrollBar(System.Windows.Controls.Primitives.ScrollBar scrollBar)
        {
            foreach (var button in FindVisualDescendants<System.Windows.Controls.Primitives.ButtonBase>(scrollBar))
            {
                if (HoldStatesByButton.ContainsKey(button))
                {
                    continue;
                }

                var cmd = button.Command;

                if (cmd != null)
                {
                    if (!IsSupportedScrollCommand(cmd))
                    {
                        continue;
                    }
                }
                else
                {
                    if (FindVisualAncestor<Track>(button) != null)
                    {
                        continue;
                    }

                    if (scrollBar.ActualWidth <= 0 || scrollBar.ActualHeight <= 0 || button.ActualWidth <= 0 || button.ActualHeight <= 0)
                    {
                        continue;
                    }

                    var center = button.TransformToAncestor(scrollBar).Transform(new System.Windows.Point(button.ActualWidth / 2, button.ActualHeight / 2));

                    if (scrollBar.Orientation == System.Windows.Controls.Orientation.Vertical)
                    {
                        cmd = center.Y < (scrollBar.ActualHeight / 2)
                            ? System.Windows.Controls.Primitives.ScrollBar.LineUpCommand
                            : System.Windows.Controls.Primitives.ScrollBar.LineDownCommand;
                    }
                    else
                    {
                        cmd = center.X < (scrollBar.ActualWidth / 2)
                            ? System.Windows.Controls.Primitives.ScrollBar.LineLeftCommand
                            : System.Windows.Controls.Primitives.ScrollBar.LineRightCommand;
                    }
                }

                var state = new HoldState(scrollBar, button, cmd);
                HoldStatesByButton.Add(button, state);

                button.PreviewMouseLeftButtonDown += ButtonOnPreviewMouseLeftButtonDown;
                button.PreviewMouseLeftButtonUp += ButtonOnPreviewMouseLeftButtonUp;
                button.MouseLeave += ButtonOnMouseLeave;
                button.Unloaded += ButtonOnUnloaded;
            }
        }

        private static bool IsSupportedScrollCommand(ICommand command)
        {
            return ReferenceEquals(command, System.Windows.Controls.Primitives.ScrollBar.LineDownCommand)
                || ReferenceEquals(command, System.Windows.Controls.Primitives.ScrollBar.LineUpCommand)
                || ReferenceEquals(command, System.Windows.Controls.Primitives.ScrollBar.LineLeftCommand)
                || ReferenceEquals(command, System.Windows.Controls.Primitives.ScrollBar.LineRightCommand);
        }

        private static void DetachButtonsUnder(FrameworkElement root)
        {
            var buttonsToRemove = new List<System.Windows.Controls.Primitives.ButtonBase>();

            foreach (var kvp in HoldStatesByButton)
            {
                var button = kvp.Key;
                if (!IsUnderRoot(root, button))
                {
                    continue;
                }

                buttonsToRemove.Add(button);
            }

            foreach (var button in buttonsToRemove)
            {
                DetachButton(button);
            }
        }

        private static bool IsUnderRoot(FrameworkElement root, DependencyObject obj)
        {
            var current = obj;
            while (current != null)
            {
                if (ReferenceEquals(current, root))
                {
                    return true;
                }

                current = VisualTreeHelper.GetParent(current);
            }

            return false;
        }

        private static void DetachButton(System.Windows.Controls.Primitives.ButtonBase button)
        {
            if (!HoldStatesByButton.TryGetValue(button, out var state))
            {
                return;
            }

            state.Stop();
            HoldStatesByButton.Remove(button);

            button.PreviewMouseLeftButtonDown -= ButtonOnPreviewMouseLeftButtonDown;
            button.PreviewMouseLeftButtonUp -= ButtonOnPreviewMouseLeftButtonUp;
            button.MouseLeave -= ButtonOnMouseLeave;
            button.Unloaded -= ButtonOnUnloaded;
        }

        private static void ButtonOnUnloaded(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Primitives.ButtonBase button)
            {
                DetachButton(button);
            }
        }

        private static void ButtonOnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is not System.Windows.Controls.Primitives.ButtonBase button)
            {
                return;
            }

            if (!HoldStatesByButton.TryGetValue(button, out var state))
            {
                return;
            }

            state.Start();
            e.Handled = true;
        }

        private static void ButtonOnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is not System.Windows.Controls.Primitives.ButtonBase button)
            {
                return;
            }

            if (!HoldStatesByButton.TryGetValue(button, out var state))
            {
                return;
            }

            state.Stop();
            e.Handled = true;
        }

        private static void ButtonOnMouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (sender is not System.Windows.Controls.Primitives.ButtonBase button)
            {
                return;
            }

            if (!HoldStatesByButton.TryGetValue(button, out var state))
            {
                return;
            }

            if (e.LeftButton != MouseButtonState.Pressed)
            {
                state.Stop();
            }
        }

        private sealed class HoldState
        {
            private readonly System.Windows.Controls.Primitives.ScrollBar _scrollBar;
            private readonly System.Windows.Controls.Primitives.ButtonBase _button;
            private readonly ICommand _command;
            private readonly DispatcherTimer _timer;
            private readonly int _direction;
            private DateTime _lastTickUtc;
            private ScrollViewer? _scrollViewer;
            private bool _restoreCanContentScroll;
            private bool _previousCanContentScroll;
            private double _velocity;

            public HoldState(System.Windows.Controls.Primitives.ScrollBar scrollBar, System.Windows.Controls.Primitives.ButtonBase button, ICommand command)
            {
                _scrollBar = scrollBar;
                _button = button;
                _command = command;

                _direction =
                    ReferenceEquals(command, System.Windows.Controls.Primitives.ScrollBar.LineDownCommand) || ReferenceEquals(command, System.Windows.Controls.Primitives.ScrollBar.LineRightCommand)
                        ? 1
                        : -1;

                _timer = new DispatcherTimer(DispatcherPriority.Render)
                {
                    Interval = TimeSpan.FromMilliseconds(16)
                };
                _timer.Tick += TimerOnTick;
            }

            public void Start()
            {
                _lastTickUtc = DateTime.UtcNow;
                _scrollViewer = _scrollBar.TemplatedParent as ScrollViewer ?? FindVisualAncestor<ScrollViewer>(_scrollBar);

                if (_scrollViewer != null)
                {
                    _previousCanContentScroll = _scrollViewer.CanContentScroll;
                }

                _restoreCanContentScroll = false;

                var scaleSource = Window.GetWindow(_scrollBar) ?? (DependencyObject)_scrollBar;
                var scale = GetSpeedScale(scaleSource);
                if (scale <= 0)
                {
                    scale = 1;
                }

                var baseSpeed = 700d * scale;
                _velocity = baseSpeed;

                Step(0.016, baseSpeed, 4200d * scale, 2800d * scale);

                _timer.Start();
            }

            public void Stop()
            {
                _timer.Stop();
                _restoreCanContentScroll = false;
                _velocity = 0;
            }

            private void TimerOnTick(object? sender, EventArgs e)
            {
                _scrollViewer = _scrollBar.TemplatedParent as ScrollViewer ?? FindVisualAncestor<ScrollViewer>(_scrollBar);
                var now = DateTime.UtcNow;
                var dt = (now - _lastTickUtc).TotalSeconds;
                _lastTickUtc = now;

                if (dt <= 0)
                {
                    return;
                }

                var scaleSource = Window.GetWindow(_scrollBar) ?? (DependencyObject)_scrollBar;
                var scale = GetSpeedScale(scaleSource);
                if (scale <= 0)
                {
                    scale = 1;
                }

                Step(dt, 700d * scale, 4200d * scale, 2800d * scale);
            }

            private void Step(double dt, double baseSpeed, double accel, double maxSpeed)
            {
                _velocity = Math.Min(maxSpeed, _velocity + (accel * dt));

                var delta = _direction * _velocity * dt;

                if (_scrollViewer != null)
                {
                    if (_scrollBar.Orientation == System.Windows.Controls.Orientation.Vertical)
                    {
                        var next = _scrollViewer.VerticalOffset + delta;
                        if (next < 0)
                        {
                            next = 0;
                        }

                        if (next > _scrollViewer.ScrollableHeight)
                        {
                            next = _scrollViewer.ScrollableHeight;
                        }

                        _scrollViewer.ScrollToVerticalOffset(next);
                    }
                    else
                    {
                        var next = _scrollViewer.HorizontalOffset + delta;
                        if (next < 0)
                        {
                            next = 0;
                        }

                        if (next > _scrollViewer.ScrollableWidth)
                        {
                            next = _scrollViewer.ScrollableWidth;
                        }

                        _scrollViewer.ScrollToHorizontalOffset(next);
                    }

                    return;
                }

                var nextValue = _scrollBar.Value + delta;

                if (nextValue < _scrollBar.Minimum)
                {
                    nextValue = _scrollBar.Minimum;
                }

                if (nextValue > _scrollBar.Maximum)
                {
                    nextValue = _scrollBar.Maximum;
                }

                _scrollBar.Value = nextValue;
            }
        }

        private static IEnumerable<T> FindVisualDescendants<T>(DependencyObject root) where T : DependencyObject
        {
            if (root == null)
            {
                yield break;
            }

            var count = VisualTreeHelper.GetChildrenCount(root);
            for (var i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(root, i);
                if (child is T typed)
                {
                    yield return typed;
                }

                foreach (var descendant in FindVisualDescendants<T>(child))
                {
                    yield return descendant;
                }
            }
        }

        private static T? FindVisualAncestor<T>(DependencyObject? startingAt) where T : DependencyObject
        {
            var current = startingAt;
            while (current != null)
            {
                if (current is T typed)
                {
                    return typed;
                }

                current = VisualTreeHelper.GetParent(current);
            }

            return null;
        }
    }
}
