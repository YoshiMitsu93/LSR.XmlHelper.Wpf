using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Rendering;
using System;
using System.Windows;

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

        private static readonly DependencyProperty TextViewBackgroundRendererProperty =
            DependencyProperty.RegisterAttached(
                "TextViewBackgroundRenderer",
                typeof(SolidTextViewBackgroundRenderer),
                typeof(AvalonEditThemeBehavior),
                new PropertyMetadata(null));

        private static void OnTextAreaBackgroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not TextEditor editor)
                return;

            if (e.NewValue is not System.Windows.Media.Brush brush)
                return;

            editor.Background = brush;

            ApplyWhenReady(editor, te =>
            {
                if (te.TextArea is null)
                    return;

                te.TextArea.Background = brush;

                var textView = te.TextArea.TextView;
                if (textView is null)
                    return;

                var renderer = (SolidTextViewBackgroundRenderer?)editor.GetValue(TextViewBackgroundRendererProperty);
                if (renderer is null)
                {
                    renderer = new SolidTextViewBackgroundRenderer(brush);
                    editor.SetValue(TextViewBackgroundRendererProperty, renderer);
                }
                else
                {
                    renderer.SetBrush(brush);
                }

                if (!textView.BackgroundRenderers.Contains(renderer))
                {
                    if (textView.BackgroundRenderers is System.Collections.Generic.IList<IBackgroundRenderer> list)
                        list.Insert(0, renderer);
                    else
                        textView.BackgroundRenderers.Add(renderer);
                }

                textView.InvalidateVisual();
            });
        }

        private static void OnTextAreaForegroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not TextEditor editor)
                return;

            if (e.NewValue is not System.Windows.Media.Brush brush)
                return;

            editor.Foreground = brush;

            ApplyWhenReady(editor, te =>
            {
                if (te.TextArea is null)
                    return;

                te.TextArea.Foreground = brush;
            });
        }

        private static void ApplyWhenReady(TextEditor editor, Action<TextEditor> apply)
        {
            if (editor.IsLoaded)
            {
                apply(editor);
                return;
            }

            RoutedEventHandler? handler = null;
            handler = (_, _) =>
            {
                editor.Loaded -= handler;
                apply(editor);
            };

            editor.Loaded += handler;
        }

        private sealed class SolidTextViewBackgroundRenderer : IBackgroundRenderer
        {
            private System.Windows.Media.Brush _brush;

            public SolidTextViewBackgroundRenderer(System.Windows.Media.Brush brush)
            {
                _brush = brush;
            }

            public KnownLayer Layer => KnownLayer.Background;

            public void SetBrush(System.Windows.Media.Brush brush)
            {
                _brush = brush;
            }

            public void Draw(TextView textView, System.Windows.Media.DrawingContext drawingContext)
            {
                var rect = new System.Windows.Rect(new System.Windows.Point(0, 0), textView.RenderSize);
                drawingContext.DrawRectangle(_brush, null, rect);
            }
        }
    }
}
