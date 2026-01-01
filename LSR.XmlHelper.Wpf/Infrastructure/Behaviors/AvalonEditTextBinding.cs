using ICSharpCode.AvalonEdit;
using System.Windows;

namespace LSR.XmlHelper.Wpf.Infrastructure
{
    public static class AvalonEditTextBinding
    {
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.RegisterAttached(
                "Text",
                typeof(string),
                typeof(AvalonEditTextBinding),
                new FrameworkPropertyMetadata(
                    "",
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    OnTextPropertyChanged));

        private static readonly DependencyProperty IsUpdatingProperty =
            DependencyProperty.RegisterAttached(
                "IsUpdating",
                typeof(bool),
                typeof(AvalonEditTextBinding),
                new PropertyMetadata(false));

        public static string GetText(DependencyObject obj) => (string)obj.GetValue(TextProperty);
        public static void SetText(DependencyObject obj, string value) => obj.SetValue(TextProperty, value);

        private static bool GetIsUpdating(DependencyObject obj) => (bool)obj.GetValue(IsUpdatingProperty);
        private static void SetIsUpdating(DependencyObject obj, bool value) => obj.SetValue(IsUpdatingProperty, value);

        private static void OnTextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not TextEditor editor)
                return;

            editor.TextChanged -= EditorOnTextChanged;

            var newText = e.NewValue as string ?? "";

            if (!GetIsUpdating(editor))
            {
                if (editor.Text != newText)
                    editor.Text = newText;
            }

            editor.TextChanged += EditorOnTextChanged;
        }

        private static void EditorOnTextChanged(object? sender, System.EventArgs e)
        {
            if (sender is not TextEditor editor)
                return;

            SetIsUpdating(editor, true);
            SetText(editor, editor.Text);
            SetIsUpdating(editor, false);
        }
    }
}
