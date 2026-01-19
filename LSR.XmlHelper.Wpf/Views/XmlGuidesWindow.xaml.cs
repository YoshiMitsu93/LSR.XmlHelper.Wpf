using System.Windows;

namespace LSR.XmlHelper.Wpf.Views
{
    public partial class XmlGuidesWindow : Window
    {
        public XmlGuidesWindow()
        {
            InitializeComponent();
        }
        protected override void OnPreviewKeyDown(System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter &&
                (System.Windows.Input.Keyboard.Modifiers & System.Windows.Input.ModifierKeys.Control) == System.Windows.Input.ModifierKeys.Control)
            {
                e.Handled = true;
                return;
            }

            base.OnPreviewKeyDown(e);
        }

        private void BodyTextBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key != System.Windows.Input.Key.V)
                return;

            if ((System.Windows.Input.Keyboard.Modifiers & System.Windows.Input.ModifierKeys.Control) != System.Windows.Input.ModifierKeys.Control)
                return;

            if (DataContext is not LSR.XmlHelper.Wpf.ViewModels.Windows.XmlGuidesWindowViewModel vm)
                return;

            if (!vm.TryPasteClipboardImage(out var token))
                return;

            if (sender is not System.Windows.Controls.TextBox tb)
                return;

            var insert = Environment.NewLine + token + Environment.NewLine;

            var caret = tb.CaretIndex;
            tb.Text = tb.Text.Insert(caret, insert);
            tb.CaretIndex = caret + insert.Length;

            e.Handled = true;
        }

        private void GuideViewer_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            if ((System.Windows.Input.Keyboard.Modifiers & System.Windows.Input.ModifierKeys.Control) != System.Windows.Input.ModifierKeys.Control)
                return;

            if (DataContext is not LSR.XmlHelper.Wpf.ViewModels.Windows.XmlGuidesWindowViewModel vm)
                return;

            vm.GuideZoom = vm.GuideZoom + (e.Delta > 0 ? 10 : -10);
            e.Handled = true;
        }

        private void Bold_Click(object sender, RoutedEventArgs e) => WrapSelection("**");

        private void Italic_Click(object sender, RoutedEventArgs e) => WrapSelection("*");

        private void Underline_Click(object sender, RoutedEventArgs e) => WrapSelection("__");

        private void Bullets_Click(object sender, RoutedEventArgs e)
        {
            if (BodyTextBox is null)
                return;

            var start = BodyTextBox.SelectionStart;
            var len = BodyTextBox.SelectionLength;

            if (len <= 0)
            {
                var insertAt = start;
                var nl = BodyTextBox.Text.LastIndexOf(Environment.NewLine, Math.Max(0, start - 1), StringComparison.Ordinal);
                if (nl >= 0)
                    insertAt = nl + Environment.NewLine.Length;
                else
                    insertAt = 0;

                BodyTextBox.Text = BodyTextBox.Text.Insert(insertAt, "- ");
                BodyTextBox.CaretIndex = start + 2;
                return;
            }

            var selected = BodyTextBox.SelectedText.Replace("\r\n", "\n");
            var parts = selected.Split('\n');

            for (var i = 0; i < parts.Length; i++)
            {
                var p = parts[i];
                if (string.IsNullOrWhiteSpace(p))
                    continue;

                if (!p.StartsWith("- ", StringComparison.Ordinal))
                    parts[i] = "- " + p;
            }

            var replaced = string.Join(Environment.NewLine, parts);
            BodyTextBox.SelectedText = replaced;

            BodyTextBox.SelectionStart = start;
            BodyTextBox.SelectionLength = replaced.Length;
        }

        private void WrapSelection(string token)
        {
            if (BodyTextBox is null)
                return;

            var start = BodyTextBox.SelectionStart;
            var len = BodyTextBox.SelectionLength;

            if (len <= 0)
            {
                BodyTextBox.Text = BodyTextBox.Text.Insert(start, token + token);
                BodyTextBox.CaretIndex = start + token.Length;
                return;
            }

            var selected = BodyTextBox.SelectedText;
            BodyTextBox.SelectedText = token + selected + token;

            BodyTextBox.SelectionStart = start + token.Length;
            BodyTextBox.SelectionLength = selected.Length;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
