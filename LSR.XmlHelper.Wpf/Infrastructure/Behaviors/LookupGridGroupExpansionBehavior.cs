using LSR.XmlHelper.Wpf.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;

namespace LSR.XmlHelper.Wpf.Infrastructure
{
    public static class LookupGridGroupExpansionBehavior
    {
        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.RegisterAttached(
                "IsEnabled",
                typeof(bool),
                typeof(LookupGridGroupExpansionBehavior),
                new PropertyMetadata(false, OnIsEnabledChanged));

        private static readonly DependencyProperty SubscriptionProperty =
            DependencyProperty.RegisterAttached(
                "Subscription",
                typeof(Action<string>),
                typeof(LookupGridGroupExpansionBehavior),
                new PropertyMetadata(null));

        public static bool GetIsEnabled(DependencyObject obj) => (bool)obj.GetValue(IsEnabledProperty);

        public static void SetIsEnabled(DependencyObject obj, bool value) => obj.SetValue(IsEnabledProperty, value);

        private static Action<string>? GetSubscription(DependencyObject obj) => (Action<string>?)obj.GetValue(SubscriptionProperty);

        private static void SetSubscription(DependencyObject obj, Action<string>? value) => obj.SetValue(SubscriptionProperty, value);

        private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not Expander expander)
                return;

            if (e.OldValue is bool oldValue && oldValue)
            {
                expander.Loaded -= Expander_Loaded;
                expander.Expanded -= Expander_Expanded;
                expander.Collapsed -= Expander_Collapsed;
                expander.Unloaded -= Expander_Unloaded;
            }

            if (e.NewValue is bool newValue && newValue)
            {
                expander.Loaded += Expander_Loaded;
                expander.Expanded += Expander_Expanded;
                expander.Collapsed += Expander_Collapsed;
                expander.Unloaded += Expander_Unloaded;
            }
        }

        private static void Expander_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is not Expander expander)
                return;

            var vm = expander.Tag as MainWindowViewModel;
            var groupName = expander.Header?.ToString();

            if (vm is null || string.IsNullOrWhiteSpace(groupName))
                return;

            if (vm.TryGetLookupGridGroupIsExpanded(groupName, out var expanded))
                expander.IsExpanded = expanded;

            var existing = GetSubscription(expander);
            if (existing is not null)
                vm.LookupGridGroupExpandRequested -= existing;

            Action<string> handler = requested =>
            {
                if (!string.Equals(requested, groupName, StringComparison.OrdinalIgnoreCase))
                    return;

                expander.Dispatcher.BeginInvoke(new Action(() =>
                {
                    expander.IsExpanded = true;
                    expander.BringIntoView();
                }));

                expander.Dispatcher.BeginInvoke(
                    new Action(vm.RequestFriendlyLookupScroll),
                    System.Windows.Threading.DispatcherPriority.Loaded);
            };

            SetSubscription(expander, handler);
            vm.LookupGridGroupExpandRequested += handler;
        }
        private static void Expander_Expanded(object sender, RoutedEventArgs e)
        {
            SetState(sender, true);
        }

        private static void Expander_Collapsed(object sender, RoutedEventArgs e)
        {
            SetState(sender, false);
        }

        private static void Expander_Unloaded(object sender, RoutedEventArgs e)
        {
            if (sender is not Expander expander)
                return;

            var vm = expander.Tag as MainWindowViewModel;
            if (vm is null)
                return;

            var handler = GetSubscription(expander);
            if (handler is null)
                return;

            vm.LookupGridGroupExpandRequested -= handler;
            SetSubscription(expander, null);
        }

        private static void SetState(object sender, bool isExpanded)
        {
            if (sender is not Expander expander)
                return;

            var vm = expander.Tag as MainWindowViewModel;
            var groupName = expander.Header?.ToString();

            if (vm is null || string.IsNullOrWhiteSpace(groupName))
                return;

            vm.SetLookupGridGroupIsExpanded(groupName, isExpanded);
        }
    }
}
