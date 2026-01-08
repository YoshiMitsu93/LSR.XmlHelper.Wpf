using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace LSR.XmlHelper.Wpf.Infrastructure
{
    public static class DataGridPreserveMultiSelectionOnEditBehavior
    {
        public static readonly DependencyProperty EnableProperty =
            DependencyProperty.RegisterAttached(
                "Enable",
                typeof(bool),
                typeof(DataGridPreserveMultiSelectionOnEditBehavior),
                new PropertyMetadata(false, OnEnableChanged));

        public static void SetEnable(DependencyObject element, bool value) => element.SetValue(EnableProperty, value);

        public static bool GetEnable(DependencyObject element) => (bool)element.GetValue(EnableProperty);

        private static void OnEnableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not DataGrid dg)
                return;

            if ((bool)e.NewValue)
                dg.PreviewMouseLeftButtonDown += Dg_PreviewMouseLeftButtonDown;
            else
                dg.PreviewMouseLeftButtonDown -= Dg_PreviewMouseLeftButtonDown;
        }

        private static void Dg_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is not DataGrid dg)
                return;

            if (dg.SelectedItems is null || dg.SelectedItems.Count <= 1)
                return;

            if ((Keyboard.Modifiers & (ModifierKeys.Control | ModifierKeys.Shift)) != ModifierKeys.None)
                return;

            if (e.OriginalSource is not DependencyObject dep)
                return;

            var cell = FindAncestor<DataGridCell>(dep);
            if (cell is null)
                return;

            if (cell.IsEditing)
                return;

            var row = FindAncestor<DataGridRow>(cell);
            if (row is null)
                return;

            var item = row.Item;
            if (item is null)
                return;

            if (!dg.SelectedItems.Contains(item))
                return;

            if (cell.Column is null)
                return;

            var wasCurrentCell =
                Equals(dg.CurrentCell.Item, item) &&
                Equals(dg.CurrentCell.Column, cell.Column);

            dg.CurrentCell = new DataGridCellInfo(item, cell.Column);

            if (!cell.IsFocused)
                cell.Focus();

            if (wasCurrentCell)
                dg.BeginEdit(e);

            e.Handled = true;
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
    }
}
