using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LSR.XmlHelper.Wpf.Infrastructure
{
    public static class TreeViewBringIntoViewSuppressionBehavior
    {
        public static readonly DependencyProperty EnableProperty =
            DependencyProperty.RegisterAttached(
                "Enable",
                typeof(bool),
                typeof(TreeViewBringIntoViewSuppressionBehavior),
                new PropertyMetadata(false, OnEnableChanged));

        public static bool GetEnable(DependencyObject obj) => (bool)obj.GetValue(EnableProperty);

        public static void SetEnable(DependencyObject obj, bool value) => obj.SetValue(EnableProperty, value);

        private static void OnEnableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not System.Windows.Controls.TreeView tree)
                return;

            if (e.NewValue is bool enabled && enabled)
                tree.AddHandler(System.Windows.FrameworkElement.RequestBringIntoViewEvent, new System.Windows.RequestBringIntoViewEventHandler(OnRequestBringIntoView), true);
            else
                tree.RemoveHandler(System.Windows.FrameworkElement.RequestBringIntoViewEvent, new System.Windows.RequestBringIntoViewEventHandler(OnRequestBringIntoView));
        }

        private static void OnRequestBringIntoView(object sender, System.Windows.RequestBringIntoViewEventArgs e)
        {
            if (sender is not System.Windows.Controls.TreeView tree)
                return;

            if (e.OriginalSource is not System.Windows.Controls.TreeViewItem)
                return;

            var mouseDown =
                System.Windows.Input.Mouse.LeftButton == System.Windows.Input.MouseButtonState.Pressed ||
                System.Windows.Input.Mouse.RightButton == System.Windows.Input.MouseButtonState.Pressed ||
                System.Windows.Input.Mouse.MiddleButton == System.Windows.Input.MouseButtonState.Pressed;

            if (mouseDown && tree.IsMouseOver)
                return;

            e.Handled = true;
        }
    }
}
