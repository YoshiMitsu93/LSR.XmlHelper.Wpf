using System.Windows;
using System.Windows.Threading;

namespace LSR.XmlHelper.Wpf.Infrastructure
{
    public static class ListBoxScrollIntoViewOnSelectionBehavior
    {
        public static readonly DependencyProperty EnableProperty =
            DependencyProperty.RegisterAttached(
                "Enable",
                typeof(bool),
                typeof(ListBoxScrollIntoViewOnSelectionBehavior),
                new PropertyMetadata(false, OnEnableChanged));

        public static bool GetEnable(DependencyObject obj) => (bool)obj.GetValue(EnableProperty);

        public static void SetEnable(DependencyObject obj, bool value) => obj.SetValue(EnableProperty, value);

        private static void OnEnableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not System.Windows.Controls.ListBox listBox)
                return;

            if (e.NewValue is bool enabled && enabled)
                listBox.SelectionChanged += OnSelectionChanged;
            else
                listBox.SelectionChanged -= OnSelectionChanged;
        }

        private static void OnSelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (sender is not System.Windows.Controls.ListBox listBox)
                return;

            var selected = listBox.SelectedItem;
            if (selected is null)
                return;

            listBox.Dispatcher.BeginInvoke(() =>
            {
                listBox.UpdateLayout();
                listBox.ScrollIntoView(selected);
            }, DispatcherPriority.Background);
        }
    }
}
