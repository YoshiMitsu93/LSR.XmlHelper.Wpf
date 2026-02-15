using System.Collections;
using System.Collections.Specialized;
using System.Windows;

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
                new PropertyMetadata(null, OnSelectedItemsChanged));

        private static readonly DependencyProperty IsSyncingProperty =
            DependencyProperty.RegisterAttached(
                "IsSyncing",
                typeof(bool),
                typeof(ListBoxSelectedItemsBehavior),
                new PropertyMetadata(false));

        private static readonly DependencyProperty BoundCollectionProperty =
            DependencyProperty.RegisterAttached(
                "BoundCollection",
                typeof(INotifyCollectionChanged),
                typeof(ListBoxSelectedItemsBehavior),
                new PropertyMetadata(null));

        private static readonly DependencyProperty BoundCollectionHandlerProperty =
            DependencyProperty.RegisterAttached(
                "BoundCollectionHandler",
                typeof(NotifyCollectionChangedEventHandler),
                typeof(ListBoxSelectedItemsBehavior),
                new PropertyMetadata(null));

        public static void SetEnable(DependencyObject element, bool value) => element.SetValue(EnableProperty, value);

        public static bool GetEnable(DependencyObject element) => (bool)element.GetValue(EnableProperty);

        public static void SetSelectedItems(DependencyObject element, IList value) => element.SetValue(SelectedItemsProperty, value);

        public static IList? GetSelectedItems(DependencyObject element) => element.GetValue(SelectedItemsProperty) as IList;

        private static bool GetIsSyncing(DependencyObject element) => (bool)element.GetValue(IsSyncingProperty);

        private static void SetIsSyncing(DependencyObject element, bool value) => element.SetValue(IsSyncingProperty, value);

        private static INotifyCollectionChanged? GetBoundCollection(DependencyObject element) => element.GetValue(BoundCollectionProperty) as INotifyCollectionChanged;

        private static void SetBoundCollection(DependencyObject element, INotifyCollectionChanged? value) => element.SetValue(BoundCollectionProperty, value);

        private static NotifyCollectionChangedEventHandler? GetBoundCollectionHandler(DependencyObject element) => element.GetValue(BoundCollectionHandlerProperty) as NotifyCollectionChangedEventHandler;

        private static void SetBoundCollectionHandler(DependencyObject element, NotifyCollectionChangedEventHandler? value) => element.SetValue(BoundCollectionHandlerProperty, value);

        private static void OnEnableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not System.Windows.Controls.ListBox lb)
                return;

            if (e.NewValue is true)
            {
                lb.Loaded += ListBox_Loaded;
                lb.Unloaded += ListBox_Unloaded;
                lb.SelectionChanged += ListBox_SelectionChanged;
                lb.SelectionMode = System.Windows.Controls.SelectionMode.Multiple;

                SubscribeToBoundCollection(lb);
                ApplyBoundSelectionToListBox(lb);
                return;
            }

            lb.Loaded -= ListBox_Loaded;
            lb.Unloaded -= ListBox_Unloaded;
            lb.SelectionChanged -= ListBox_SelectionChanged;

            UnsubscribeFromBoundCollection(lb);
        }

        private static void OnSelectedItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not System.Windows.Controls.ListBox lb)
                return;

            if (!GetEnable(lb))
                return;

            SubscribeToBoundCollection(lb);
            ApplyBoundSelectionToListBox(lb);
        }

        private static void ListBox_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is not System.Windows.Controls.ListBox lb)
                return;

            if (!GetEnable(lb))
                return;

            lb.SelectionMode = System.Windows.Controls.SelectionMode.Multiple;

            SubscribeToBoundCollection(lb);
            ApplyBoundSelectionToListBox(lb);
        }

        private static void ListBox_Unloaded(object sender, RoutedEventArgs e)
        {
            if (sender is not System.Windows.Controls.ListBox lb)
                return;

            UnsubscribeFromBoundCollection(lb);
        }

        private static void ListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (sender is not System.Windows.Controls.ListBox lb)
                return;

            if (GetIsSyncing(lb))
                return;

            CopySelectionToBoundList(lb);
        }

        private static void SubscribeToBoundCollection(System.Windows.Controls.ListBox lb)
        {
            UnsubscribeFromBoundCollection(lb);

            var list = GetSelectedItems(lb);
            if (list is not INotifyCollectionChanged notifying)
                return;

            NotifyCollectionChangedEventHandler handler = (_, __) =>
            {
                if (!GetEnable(lb))
                    return;

                if (!lb.Dispatcher.CheckAccess())
                {
                    lb.Dispatcher.Invoke(() => ApplyBoundSelectionToListBox(lb));
                    return;
                }

                ApplyBoundSelectionToListBox(lb);
            };

            notifying.CollectionChanged += handler;
            SetBoundCollection(lb, notifying);
            SetBoundCollectionHandler(lb, handler);
        }

        private static void UnsubscribeFromBoundCollection(System.Windows.Controls.ListBox lb)
        {
            var previous = GetBoundCollection(lb);
            var handler = GetBoundCollectionHandler(lb);

            if (previous is not null && handler is not null)
                previous.CollectionChanged -= handler;

            SetBoundCollection(lb, null);
            SetBoundCollectionHandler(lb, null);
        }

        private static void ApplyBoundSelectionToListBox(System.Windows.Controls.ListBox lb)
        {
            if (GetIsSyncing(lb))
                return;

            var source = GetSelectedItems(lb);
            if (source is null)
                return;

            try
            {
                SetIsSyncing(lb, true);

                lb.SelectedItems.Clear();

                foreach (var item in source)
                    lb.SelectedItems.Add(item);
            }
            finally
            {
                SetIsSyncing(lb, false);
            }
        }

        private static void CopySelectionToBoundList(System.Windows.Controls.ListBox lb)
        {
            if (!GetEnable(lb))
                return;

            var target = GetSelectedItems(lb);
            if (target is null)
                return;

            try
            {
                SetIsSyncing(lb, true);

                target.Clear();

                foreach (var item in lb.SelectedItems)
                    target.Add(item);
            }
            finally
            {
                SetIsSyncing(lb, false);
            }
        }
    }
}
