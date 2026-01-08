using System.Windows;
using System.Windows.Controls;

namespace LSR.XmlHelper.Wpf.Infrastructure
{
    public static class DataGridForceExtendedSelectionBehavior
    {
        public static readonly DependencyProperty EnableProperty =
            DependencyProperty.RegisterAttached(
                "Enable",
                typeof(bool),
                typeof(DataGridForceExtendedSelectionBehavior),
                new PropertyMetadata(false, OnEnableChanged));

        public static void SetEnable(DependencyObject element, bool value) => element.SetValue(EnableProperty, value);

        public static bool GetEnable(DependencyObject element) => (bool)element.GetValue(EnableProperty);

        private static void OnEnableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not DataGrid dg)
                return;

            if (e.NewValue is true)
            {
                dg.Loaded += Dg_Loaded;
                return;
            }

            dg.Loaded -= Dg_Loaded;
        }

        private static void Dg_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is not DataGrid dg)
                return;

            dg.SelectionMode = DataGridSelectionMode.Extended;
            dg.SelectionUnit = DataGridSelectionUnit.FullRow;
        }
    }
}
