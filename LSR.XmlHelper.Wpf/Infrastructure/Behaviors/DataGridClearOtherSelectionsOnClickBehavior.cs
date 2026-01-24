using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace LSR.XmlHelper.Wpf.Infrastructure
{
    public static class DataGridClearOtherSelectionsOnClickBehavior
    {
        public static readonly DependencyProperty EnableProperty =
            DependencyProperty.RegisterAttached(
                "Enable",
                typeof(bool),
                typeof(DataGridClearOtherSelectionsOnClickBehavior),
                new PropertyMetadata(false, OnEnableChanged));

        public static void SetEnable(DependencyObject element, bool value) => element.SetValue(EnableProperty, value);

        public static bool GetEnable(DependencyObject element) => (bool)element.GetValue(EnableProperty);

        private static void OnEnableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not DataGrid dg)
                return;

            if (e.NewValue is true)
                dg.PreviewMouseLeftButtonDown += Dg_PreviewMouseLeftButtonDown;
            else
                dg.PreviewMouseLeftButtonDown -= Dg_PreviewMouseLeftButtonDown;
        }

        private static void Dg_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is not DataGrid dg)
                return;

            if ((Keyboard.Modifiers & (ModifierKeys.Control | ModifierKeys.Shift)) != ModifierKeys.None)
                return;

            if (e.OriginalSource is not DependencyObject dep)
                return;

            var row = FindAncestor<DataGridRow>(dep);
            if (row is null)
                return;

            var sharedSelection = DataGridSelectedItemsBehavior.GetSelectedItems(dg);
            if (sharedSelection is null)
                return;

            var root = FindAncestor<Window>(dg);
            if (root is null)
                return;

            foreach (var other in FindVisualChildren<DataGrid>(root))
            {
                if (ReferenceEquals(other, dg))
                    continue;

                if (!DataGridSelectedItemsBehavior.GetEnable(other))
                    continue;

                var otherSelection = DataGridSelectedItemsBehavior.GetSelectedItems(other);
                if (!ReferenceEquals(otherSelection, sharedSelection))
                    continue;

                if (other.SelectedItems.Count == 0)
                    continue;

                other.UnselectAll();
            }
        }

        private static T? FindAncestor<T>(DependencyObject dep) where T : DependencyObject
        {
            while (dep is not null)
            {
                if (dep is T t)
                    return t;

                dep = VisualTreeHelper.GetParent(dep);
            }

            return null;
        }

        private static IEnumerable<T> FindVisualChildren<T>(DependencyObject dep) where T : DependencyObject
        {
            if (dep is null)
                yield break;

            var count = VisualTreeHelper.GetChildrenCount(dep);
            for (var i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(dep, i);
                if (child is T t)
                    yield return t;

                foreach (var nested in FindVisualChildren<T>(child))
                    yield return nested;
            }
        }
    }
}
