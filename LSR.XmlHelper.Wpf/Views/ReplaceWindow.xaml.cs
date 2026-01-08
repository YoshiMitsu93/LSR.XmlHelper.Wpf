using ICSharpCode.AvalonEdit;
using System;
using System.Windows;

namespace LSR.XmlHelper.Wpf.Views
{
    public partial class ReplaceWindow : Window
    {
        private readonly TextEditor _editor;
        private int _lastSearchStart;

        public ReplaceWindow(TextEditor editor)
        {
            InitializeComponent();
            _editor = editor;
            _lastSearchStart = -1;
            Loaded += ReplaceWindow_Loaded;
        }

        private void ReplaceWindow_Loaded(object sender, RoutedEventArgs e)
        {
            FindWhatTextBox.Focus();
            FindWhatTextBox.SelectAll();
        }

        private void FindNext_Click(object sender, RoutedEventArgs e)
        {
            FindNext();
        }

        private void Replace_Click(object sender, RoutedEventArgs e)
        {
            if (TryReplaceCurrentSelection())
                FindNext();
            else
                FindNext();
        }

        private void ReplaceAll_Click(object sender, RoutedEventArgs e)
        {
            var find = FindWhatTextBox.Text ?? string.Empty;
            if (string.IsNullOrEmpty(find))
                return;

            var replace = ReplaceWithTextBox.Text ?? string.Empty;
            var comparison = MatchCaseCheckBox.IsChecked == true ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

            var doc = _editor.Document;
            if (doc is null)
                return;

            var text = doc.Text;
            var count = 0;
            var index = 0;

            doc.BeginUpdate();
            try
            {
                while (true)
                {
                    var hit = text.IndexOf(find, index, comparison);
                    if (hit < 0)
                        break;

                    doc.Replace(hit, find.Length, replace);

                    text = doc.Text;
                    index = hit + replace.Length;
                    count++;
                }
            }
            finally
            {
                doc.EndUpdate();
            }

            System.Windows.MessageBox.Show($"Replaced {count} occurrence(s).", "Replace All", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void FindNext()
        {
            var find = FindWhatTextBox.Text ?? string.Empty;
            if (string.IsNullOrEmpty(find))
                return;

            var comparison = MatchCaseCheckBox.IsChecked == true ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

            var doc = _editor.Document;
            if (doc is null)
                return;

            var text = doc.Text;

            var start = _editor.SelectionStart + _editor.SelectionLength;
            if (start < 0)
                start = 0;

            if (start > text.Length)
                start = text.Length;

            var hit = text.IndexOf(find, start, comparison);

            if (hit < 0 && WrapAroundCheckBox.IsChecked == true)
                hit = text.IndexOf(find, 0, comparison);

            if (hit < 0)
            {
                System.Windows.MessageBox.Show("No matches found.", "Replace", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            _lastSearchStart = hit;
            _editor.Select(hit, find.Length);
            _editor.TextArea.Caret.Offset = hit + find.Length;
            _editor.TextArea.Caret.BringCaretToView();
            _editor.Focus();
        }

        private bool TryReplaceCurrentSelection()
        {
            var find = FindWhatTextBox.Text ?? string.Empty;
            if (string.IsNullOrEmpty(find))
                return false;

            var replace = ReplaceWithTextBox.Text ?? string.Empty;

            var selectionText = _editor.SelectedText ?? string.Empty;
            var comparison = MatchCaseCheckBox.IsChecked == true ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

            if (!string.Equals(selectionText, find, comparison))
                return false;

            var doc = _editor.Document;
            if (doc is null)
                return false;

            doc.Replace(_editor.SelectionStart, _editor.SelectionLength, replace);
            _editor.Select(_editor.SelectionStart, replace.Length);
            return true;
        }
    }
}
