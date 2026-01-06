using System.Windows;
using System.Windows.Controls;

namespace LSR.XmlHelper.Wpf.Infrastructure
{
    public static class BringIntoViewOnExpandBehavior
    {
        public static readonly DependencyProperty EnableProperty =
            DependencyProperty.RegisterAttached(
                "Enable",
                typeof(bool),
                typeof(BringIntoViewOnExpandBehavior),
                new PropertyMetadata(false, OnEnableChanged));

        public static bool GetEnable(DependencyObject obj) => (bool)obj.GetValue(EnableProperty);

        public static void SetEnable(DependencyObject obj, bool value) => obj.SetValue(EnableProperty, value);

        private static void OnEnableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not Expander expander)
                return;

            if (e.NewValue is true)
                expander.Expanded += Expander_Expanded;
            else
                expander.Expanded -= Expander_Expanded;
        }

        private static void Expander_Expanded(object sender, RoutedEventArgs e)
        {
            if (sender is not Expander expander)
                return;

            expander.BringIntoView();
        }
    }
}
