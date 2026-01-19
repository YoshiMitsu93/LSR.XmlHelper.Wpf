using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;

namespace LSR.XmlHelper.Wpf.Infrastructure
{
    public static class DataGridUpdateSourceOnCommitBehavior
    {
        public static readonly DependencyProperty EnableProperty =
            DependencyProperty.RegisterAttached(
                "Enable",
                typeof(bool),
                typeof(DataGridUpdateSourceOnCommitBehavior),
                new PropertyMetadata(false, OnEnableChanged));

        public static bool GetEnable(DependencyObject obj)
        {
            return (bool)obj.GetValue(EnableProperty);
        }

        public static void SetEnable(DependencyObject obj, bool value)
        {
            obj.SetValue(EnableProperty, value);
        }

        private static void OnEnableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not DataGrid dg)
                return;

            if (e.NewValue is true)
                dg.CellEditEnding += Dg_CellEditEnding;
            else
                dg.CellEditEnding -= Dg_CellEditEnding;
        }

        private static void Dg_CellEditEnding(object? sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction != DataGridEditAction.Commit)
                return;

            if (e.EditingElement is not System.Windows.Controls.TextBox tb)
                return;

            tb.Dispatcher.BeginInvoke(
                new Action(() =>
                {
                    var be = BindingOperations.GetBindingExpression(tb, System.Windows.Controls.TextBox.TextProperty);
                    be?.UpdateSource();
                }),
                DispatcherPriority.Background);
        }
    }
}
