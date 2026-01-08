using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace LSR.XmlHelper.Wpf.Infrastructure
{
    public static class DataGridSelectedItemsBehavior
    {
        public static readonly DependencyProperty EnableProperty =
            DependencyProperty.RegisterAttached(
                "Enable",
                typeof(bool),
                typeof(DataGridSelectedItemsBehavior),
                new PropertyMetadata(false, OnEnableChanged));

        public static readonly DependencyProperty SelectedItemsProperty =
            DependencyProperty.RegisterAttached(
                "SelectedItems",
                typeof(IList),
                typeof(DataGridSelectedItemsBehavior),
                new PropertyMetadata(null));

        public static void SetEnable(DependencyObject element, bool value) => element.SetValue(EnableProperty, value);

        public static bool GetEnable(DependencyObject element) => (bool)element.GetValue(EnableProperty);

        public static void SetSelectedItems(DependencyObject element, IList value) => element.SetValue(SelectedItemsProperty, value);

        public static IList? GetSelectedItems(DependencyObject element) => element.GetValue(SelectedItemsProperty) as IList;

        private static void OnEnableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not DataGrid dg)
                return;

            if (e.NewValue is true)
            {
                dg.Loaded += DataGrid_Loaded;
                dg.SelectionChanged += DataGrid_SelectionChanged;

                dg.SelectionMode = DataGridSelectionMode.Extended;

                CopySelectionToBoundList(dg);
                return;
            }

            dg.Loaded -= DataGrid_Loaded;
            dg.SelectionChanged -= DataGrid_SelectionChanged;
        }

        private static void DataGrid_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is not DataGrid dg)
                return;

            if (!GetEnable(dg))
                return;

            dg.SelectionMode = DataGridSelectionMode.Extended;
        }

        private static void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is not DataGrid dg)
                return;

            CopySelectionToBoundList(dg, e);
        }

        private static void CopySelectionToBoundList(DataGrid dg, SelectionChangedEventArgs? e = null)
        {
            if (!GetEnable(dg))
                return;

            var target = GetSelectedItems(dg);
            if (target is null)
                return;

            if (e is null)
            {
                foreach (var item in dg.SelectedItems)
                {
                    if (!target.Contains(item))
                        target.Add(item);
                }

                return;
            }

            foreach (var item in e.RemovedItems)
            {
                while (target.Contains(item))
                    target.Remove(item);
            }

            foreach (var item in e.AddedItems)
            {
                if (!target.Contains(item))
                    target.Add(item);
            }
        }
    }
}
