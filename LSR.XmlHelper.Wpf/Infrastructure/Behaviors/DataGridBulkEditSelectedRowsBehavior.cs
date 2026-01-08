using LSR.XmlHelper.Wpf.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace LSR.XmlHelper.Wpf.Infrastructure
{
    public static class DataGridBulkEditSelectedRowsBehavior
    {
        public static readonly DependencyProperty EnableProperty =
            DependencyProperty.RegisterAttached(
                "Enable",
                typeof(bool),
                typeof(DataGridBulkEditSelectedRowsBehavior),
                new PropertyMetadata(false, OnEnableChanged));

        private static readonly DependencyProperty SessionProperty =
            DependencyProperty.RegisterAttached(
                "Session",
                typeof(EditSession),
                typeof(DataGridBulkEditSelectedRowsBehavior),
                new PropertyMetadata(null));

        public static void SetEnable(DependencyObject element, bool value) => element.SetValue(EnableProperty, value);

        public static bool GetEnable(DependencyObject element) => (bool)element.GetValue(EnableProperty);

        private static void OnEnableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not DataGrid dg)
                return;

            if (e.NewValue is true)
            {
                dg.PreparingCellForEdit += Dg_PreparingCellForEdit;
                dg.CellEditEnding += Dg_CellEditEnding;
            }
            else
            {
                dg.PreparingCellForEdit -= Dg_PreparingCellForEdit;
                dg.CellEditEnding -= Dg_CellEditEnding;
            }
        }

        private static void Dg_PreparingCellForEdit(object? sender, DataGridPreparingCellForEditEventArgs e)
        {
            if (sender is not DataGrid dg)
                return;

            if (dg.SelectedItems is null || dg.SelectedItems.Count <= 1)
                return;

            if (e.Column is not DataGridBoundColumn boundColumn)
                return;

            if (boundColumn.Binding is not System.Windows.Data.Binding binding)
                return;

            if (binding.Path is null || binding.Path.Path != "Value")
                return;

            if (e.EditingElement is not System.Windows.Controls.TextBox tb)
                return;

            var selectedFields = dg.SelectedItems.OfType<XmlFriendlyFieldViewModel>().ToList();
            if (selectedFields.Count <= 1)
                return;

            var originalValues = new Dictionary<XmlFriendlyFieldViewModel, string>();
            foreach (var f in selectedFields)
                originalValues[f] = f.Value;

            var session = new EditSession(dg, tb, originalValues);
            dg.SetValue(SessionProperty, session);

            tb.TextChanged += Tb_TextChanged;
            tb.PreviewKeyDown += Tb_PreviewKeyDown;
        }

        private static void Tb_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is not System.Windows.Controls.TextBox tb)
                return;

            if (TryGetSession(tb, out var session) == false)
                return;

            var newValue = tb.Text;

            foreach (var kvp in session.OriginalValues)
            {
                if (ReferenceEquals(kvp.Key, session.CurrentEditedRow))
                    continue;

                kvp.Key.Value = newValue;
            }
        }

        private static void Tb_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key != System.Windows.Input.Key.Escape)
                return;

            if (sender is not System.Windows.Controls.TextBox tb)
                return;

            if (TryGetSession(tb, out var session) == false)
                return;

            foreach (var kvp in session.OriginalValues)
                kvp.Key.Value = kvp.Value;

            EndSession(session);
        }

        private static void Dg_CellEditEnding(object? sender, DataGridCellEditEndingEventArgs e)
        {
            if (sender is not DataGrid dg)
                return;

            var session = dg.GetValue(SessionProperty) as EditSession;
            if (session is null)
                return;

            if (e.EditAction != DataGridEditAction.Commit)
            {
                foreach (var kvp in session.OriginalValues)
                    kvp.Key.Value = kvp.Value;
            }

            EndSession(session);
        }

        private static bool TryGetSession(System.Windows.Controls.TextBox tb, out EditSession session)
        {
            session = null!;

            var dg = FindAncestor<DataGrid>(tb);
            if (dg is null)
                return false;

            session = dg.GetValue(SessionProperty) as EditSession;
            if (session is null)
                return false;

            if (ReferenceEquals(session.Editor, tb) == false)
                return false;

            session.CurrentEditedRow = dg.CurrentItem as XmlFriendlyFieldViewModel;
            return true;
        }

        private static void EndSession(EditSession session)
        {
            session.Editor.TextChanged -= Tb_TextChanged;
            session.Editor.PreviewKeyDown -= Tb_PreviewKeyDown;
            session.Grid.SetValue(SessionProperty, null);
        }

        private static T? FindAncestor<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject? current = child;

            while (current is not null)
            {
                if (current is T found)
                    return found;

                current = System.Windows.Media.VisualTreeHelper.GetParent(current);
            }

            return null;
        }

        private sealed class EditSession
        {
            public EditSession(DataGrid grid, System.Windows.Controls.TextBox editor, Dictionary<XmlFriendlyFieldViewModel, string> originalValues)
            {
                Grid = grid;
                Editor = editor;
                OriginalValues = originalValues;
            }

            public DataGrid Grid { get; }

            public System.Windows.Controls.TextBox Editor { get; }

            public Dictionary<XmlFriendlyFieldViewModel, string> OriginalValues { get; }

            public XmlFriendlyFieldViewModel? CurrentEditedRow { get; set; }
        }
    }
}
