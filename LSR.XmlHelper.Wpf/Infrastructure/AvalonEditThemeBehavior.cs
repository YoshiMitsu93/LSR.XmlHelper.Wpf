using ICSharpCode.AvalonEdit;
using System.Windows;
using System.Windows.Media;

namespace LSR.XmlHelper.Wpf.Infrastructure
{
    public static class AvalonEditThemeBehavior
    {
        public static readonly DependencyProperty TextAreaBackgroundProperty =
            DependencyProperty.RegisterAttached(
                "TextAreaBackground",
                typeof(System.Windows.Media.Brush),
                typeof(AvalonEditThemeBehavior),
                new PropertyMetadata(null, OnTextAreaBackgroundChanged));

        public static void SetTextAreaBackground(DependencyObject element, System.Windows.Media.Brush value) =>
            element.SetValue(TextAreaBackgroundProperty, value);

        public static System.Windows.Media.Brush GetTextAreaBackground(DependencyObject element) =>
            (System.Windows.Media.Brush)element.GetValue(TextAreaBackgroundProperty);

        public static readonly DependencyProperty TextAreaForegroundProperty =
            DependencyProperty.RegisterAttached(
                "TextAreaForeground",
                typeof(System.Windows.Media.Brush),
                typeof(AvalonEditThemeBehavior),
                new PropertyMetadata(null, OnTextAreaForegroundChanged));

        public static void SetTextAreaForeground(DependencyObject element, System.Windows.Media.Brush value) =>
            element.SetValue(TextAreaForegroundProperty, value);

        public static System.Windows.Media.Brush GetTextAreaForeground(DependencyObject element) =>
            (System.Windows.Media.Brush)element.GetValue(TextAreaForegroundProperty);

        private static void OnTextAreaBackgroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not TextEditor editor)
                return;

            if (e.NewValue is not System.Windows.Media.Brush brush)
                return;

            editor.Background = brush;

            if (editor.TextArea != null)
                editor.TextArea.Background = brush;
        }

        private static void OnTextAreaForegroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not TextEditor editor)
                return;

            if (e.NewValue is not System.Windows.Media.Brush brush)
                return;

            editor.Foreground = brush;

            if (editor.TextArea != null)
                editor.TextArea.Foreground = brush;
        }
    }
}
