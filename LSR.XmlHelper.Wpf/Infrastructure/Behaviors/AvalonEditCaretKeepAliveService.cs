using System;
using System.Reflection;
using System.Windows.Threading;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Editing;

namespace LSR.XmlHelper.Wpf.Infrastructure.Behaviors
{
    public sealed class AvalonEditCaretKeepAliveService : IDisposable
    {
        private readonly TextEditor _editor;
        private readonly DispatcherTimer _timer;

        public AvalonEditCaretKeepAliveService(TextEditor editor)
        {
            _editor = editor;

            _timer = new DispatcherTimer(DispatcherPriority.Input)
            {
                Interval = TimeSpan.FromMilliseconds(200)
            };

            _timer.Tick += TimerOnTick;
        }

        public void Start()
        {
            _timer.Start();
        }

        public void Stop()
        {
            _timer.Stop();
        }

        private void TimerOnTick(object? sender, EventArgs e)
        {
            if (_editor.TextArea is null)
                return;

            if (!_editor.TextArea.IsKeyboardFocusWithin)
                return;

            var caret = _editor.TextArea.Caret;
            if (caret is null)
                return;

            var isVisible = TryGetCaretIsVisible(caret);
            if (isVisible is null)
                return;

            if (isVisible.Value)
                return;

            caret.Show();
        }

        private static bool? TryGetCaretIsVisible(Caret caret)
        {
            var t = caret.GetType();

            var prop = t.GetProperty("IsVisible", BindingFlags.Public | BindingFlags.Instance);
            if (prop is not null && prop.PropertyType == typeof(bool))
            {
                try
                {
                    var value = prop.GetValue(caret);
                    if (value is bool b)
                        return b;
                    return null;
                }
                catch
                {
                    return null;
                }
            }

            return null;
        }

        public void Dispose()
        {
            Stop();
            _timer.Tick -= TimerOnTick;
        }
    }
}
