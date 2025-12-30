using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace LSR.XmlHelper.Wpf.Infrastructure
{
    public static class ScrollViewerMouseWheelBehavior
    {
        public static readonly DependencyProperty EnableProperty =
            DependencyProperty.RegisterAttached(
                "Enable",
                typeof(bool),
                typeof(ScrollViewerMouseWheelBehavior),
                new PropertyMetadata(false, OnEnableChanged));

        public static bool GetEnable(DependencyObject obj) => (bool)obj.GetValue(EnableProperty);
        public static void SetEnable(DependencyObject obj, bool value) => obj.SetValue(EnableProperty, value);

        private static void OnEnableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not UIElement element)
                return;

            if (e.NewValue is bool enabled && enabled)
                element.PreviewMouseWheel += ElementOnPreviewMouseWheel;
            else
                element.PreviewMouseWheel -= ElementOnPreviewMouseWheel;
        }

        private static void ElementOnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (sender is not DependencyObject origin)
                return;

            var scrollViewer = FindParentScrollViewer(origin);
            if (scrollViewer is null)
                return;

            var current = scrollViewer.VerticalOffset;
            var delta = -e.Delta / 12.0;
            var target = current + delta;

            if (target < 0)
                target = 0;

            if (target > scrollViewer.ScrollableHeight)
                target = scrollViewer.ScrollableHeight;

            if (Math.Abs(target - current) < 0.01)
                return;

            scrollViewer.ScrollToVerticalOffset(target);
            e.Handled = true;
        }

        private static ScrollViewer? FindParentScrollViewer(DependencyObject start)
        {
            var current = start;

            while (current is not null)
            {
                if (current is ScrollViewer sv)
                    return sv;

                current = VisualTreeHelper.GetParent(current);
            }

            return null;
        }
    }
}
