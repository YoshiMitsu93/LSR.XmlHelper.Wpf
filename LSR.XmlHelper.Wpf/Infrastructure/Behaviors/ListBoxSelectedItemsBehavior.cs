using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace LSR.XmlHelper.Wpf.Infrastructure
{
    public static class ListBoxSelectedItemsBehavior
    {
        public static readonly DependencyProperty EnableProperty =
            DependencyProperty.RegisterAttached(
                "Enable",
                typeof(bool),
                typeof(ListBoxSelectedItemsBehavior),
                new PropertyMetadata(false, OnEnableChanged));

        public static readonly DependencyProperty SelectedItemsProperty =
            DependencyProperty.RegisterAttached(
                "SelectedItems",
                typeof(IList),
                typeof(ListBoxSelectedItemsBehavior),
                new PropertyMetadata(null));

        public static void SetEnable(DependencyObject element, bool value) => element.SetValue(EnableProperty, value);

        public static bool GetEnable(DependencyObject element) => (bool)element.GetValue(EnableProperty);

        public static void SetSelectedItems(DependencyObject element, IList value) => element.SetValue(SelectedItemsProperty, value);

        public static IList? GetSelectedItems(DependencyObject element) => element.GetValue(SelectedItemsProperty) as IList;

        private static void OnEnableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not System.Windows.Controls.ListBox lb)
                return;

            if (e.NewValue is true)
            {
                lb.Loaded += ListBox_Loaded;
                lb.SelectionChanged += ListBox_SelectionChanged;

                lb.SelectionMode = System.Windows.Controls.SelectionMode.Extended;

                CopySelectionToBoundList(lb);
                return;
            }

            lb.Loaded -= ListBox_Loaded;
            lb.SelectionChanged -= ListBox_SelectionChanged;
        }

        private static void ListBox_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is not System.Windows.Controls.ListBox lb)
                return;

            if (!GetEnable(lb))
                return;

            lb.SelectionMode = System.Windows.Controls.SelectionMode.Extended;
        }

        private static void ListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (sender is not System.Windows.Controls.ListBox lb)
                return;

            CopySelectionToBoundList(lb);
        }

        private static void CopySelectionToBoundList(System.Windows.Controls.ListBox lb)
        {
            if (!GetEnable(lb))
                return;

            var target = GetSelectedItems(lb);
            if (target is null)
                return;

            target.Clear();

            foreach (var item in lb.SelectedItems)
                target.Add(item);
        }
    }
}
