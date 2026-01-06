using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Threading;

namespace LSR.XmlHelper.Wpf.Infrastructure
{
    public static class DataGridScrollIntoViewOnSelectionBehavior
    {
        public static readonly DependencyProperty EnableProperty =
            DependencyProperty.RegisterAttached(
                "Enable",
                typeof(bool),
                typeof(DataGridScrollIntoViewOnSelectionBehavior),
                new PropertyMetadata(false, OnEnableChanged));

        public static readonly DependencyProperty ScrollRequestIdProperty =
            DependencyProperty.RegisterAttached(
                "ScrollRequestId",
                typeof(int),
                typeof(DataGridScrollIntoViewOnSelectionBehavior),
                new PropertyMetadata(0, OnScrollRequestIdChanged));

        public static bool GetEnable(DependencyObject obj) => (bool)obj.GetValue(EnableProperty);

        public static void SetEnable(DependencyObject obj, bool value) => obj.SetValue(EnableProperty, value);

        public static int GetScrollRequestId(DependencyObject obj) => (int)obj.GetValue(ScrollRequestIdProperty);

        public static void SetScrollRequestId(DependencyObject obj, int value) => obj.SetValue(ScrollRequestIdProperty, value);

        private static void OnEnableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not DataGrid)
                return;
        }

        private static void OnScrollRequestIdChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not DataGrid grid)
                return;

            if (!GetEnable(grid))
                return;

            var requestId = e.NewValue is int id ? id : 0;
            RequestScroll(grid, true, requestId);
        }

        private static void RequestScroll(DataGrid grid, bool alignOuterToTop, int expectedRequestId)
        {
            RequestScroll(grid, alignOuterToTop, expectedRequestId, 0);
        }

        private static void RequestScroll(DataGrid grid, bool alignOuterToTop, int expectedRequestId, int attempt)
        {
            if (expectedRequestId >= 0 && GetScrollRequestId(grid) != expectedRequestId)
                return;

            if (!grid.IsLoaded || !grid.IsVisible || grid.ActualHeight <= 0)
            {
                if (attempt < 20)
                    grid.Dispatcher.BeginInvoke(() => RequestScroll(grid, alignOuterToTop, expectedRequestId, attempt + 1), DispatcherPriority.Background);

                return;
            }

            ScrollToSelected(grid, alignOuterToTop);
        }

        private static void ScrollToSelected(DataGrid grid, bool alignOuterToTop)
        {
            ScrollToSelected(grid, alignOuterToTop, 0);
        }

        private static void ScrollToSelected(DataGrid grid, bool alignOuterToTop, int attempt)
        {
            if (!alignOuterToTop)
                return;

            if (grid.SelectedItem is not ViewModels.XmlFriendlyLookupItemViewModel lookup)
                return;

            grid.Dispatcher.BeginInvoke(() =>
            {
                grid.UpdateLayout();

                var expander = FindLookupGroupExpander(grid, lookup.Item);
                if (expander is null)
                {
                    if (attempt < 12)
                        grid.Dispatcher.BeginInvoke(() => ScrollToSelected(grid, alignOuterToTop, attempt + 1), DispatcherPriority.Background);

                    return;
                }

                expander.BringIntoView();
                grid.Dispatcher.BeginInvoke(() => ScrollOuterToTargets(grid, expander, expander, true), DispatcherPriority.Background);
            }, DispatcherPriority.Loaded);
        }

        private static Expander? FindLookupGroupExpander(DependencyObject parent, string groupName)
        {
            var count = VisualTreeHelper.GetChildrenCount(parent);
            for (var i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                if (child is Expander expander)
                {
                    var header = expander.Header?.ToString();
                    if (!string.IsNullOrWhiteSpace(header) && string.Equals(header, groupName, StringComparison.OrdinalIgnoreCase))
                        return expander;
                }

                var match = FindLookupGroupExpander(child, groupName);
                if (match is not null)
                    return match;
            }

            return null;
        }

        private static void ScrollOuterToTargets(DataGrid grid, FrameworkElement innerTarget, FrameworkElement outerTarget, bool alignOuterToTop)
        {
            var outer = FindVisualAncestor<ScrollViewer>(grid);
            if (outer is null)
                return;

            ScrollViewerToTarget(outer, outerTarget, alignOuterToTop);
        }

        private static void ScrollViewerToTarget(ScrollViewer viewer, FrameworkElement target, bool alignToTop)
        {
            try
            {
                viewer.UpdateLayout();
                target.UpdateLayout();

                var viewportHeight = viewer.ViewportHeight;
                if (viewportHeight <= 0)
                    viewportHeight = viewer.ActualHeight;

                if (viewportHeight <= 0)
                    return;

                var transform = target.TransformToAncestor(viewer);
                var topLeft = transform.Transform(new System.Windows.Point(0, 0));
                var bottomLeft = transform.Transform(new System.Windows.Point(0, target.ActualHeight));

                var desiredOffset = viewer.VerticalOffset;

                if (alignToTop)
                {
                    var padding = 8d;
                    desiredOffset += topLeft.Y - padding;
                }
                else
                {
                    if (topLeft.Y < 0)
                        desiredOffset += topLeft.Y;
                    else if (bottomLeft.Y > viewportHeight)
                        desiredOffset += bottomLeft.Y - viewportHeight;
                    else
                        return;
                }

                if (desiredOffset < 0)
                    desiredOffset = 0;

                var maxOffset = viewer.ExtentHeight - viewportHeight;
                if (maxOffset < 0)
                    maxOffset = 0;

                if (desiredOffset > maxOffset)
                    desiredOffset = maxOffset;

                viewer.ScrollToVerticalOffset(desiredOffset);
            }
            catch
            {
            }
        }

        private static T? FindVisualAncestor<T>(DependencyObject start) where T : DependencyObject
        {
            var current = VisualTreeHelper.GetParent(start);

            while (current is not null)
            {
                if (current is T match)
                    return match;

                current = VisualTreeHelper.GetParent(current);
            }

            return null;
        }
    }
}
