using ICSharpCode.AvalonEdit;
using System;
using System.Reflection;
using System.Windows.Threading;

namespace LSR.XmlHelper.Wpf.Infrastructure.Behaviors
{
    public sealed class AvalonEditCaretVisibilityService : IDisposable
    {
        private readonly TextEditor _editor;
        private readonly DispatcherTimer _timer;
        private bool _isDisposed;

        public AvalonEditCaretVisibilityService(TextEditor editor)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));

            TryDisableBlinkingIfSupported();

            _timer = new DispatcherTimer(DispatcherPriority.Input)
            {
                Interval = TimeSpan.FromMilliseconds(250)
            };

            _timer.Tick += TimerOnTick;
            _timer.Start();
        }

        public void Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;

            _timer.Stop();
            _timer.Tick -= TimerOnTick;
        }

        private void TimerOnTick(object? sender, EventArgs e)
        {
            if (_isDisposed)
                return;

            var caret = _editor.TextArea?.Caret;
            if (caret is null)
                return;

            var show = caret.GetType().GetMethod("Show", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (show is not null)
            {
                show.Invoke(caret, null);
                return;
            }

            var isVisible = caret.GetType().GetProperty("IsVisible", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (isVisible is not null && isVisible.PropertyType == typeof(bool) && isVisible.CanWrite)
            {
                isVisible.SetValue(caret, true);
                return;
            }
        }

        private void TryDisableBlinkingIfSupported()
        {
            var caret = _editor.TextArea?.Caret;
            if (caret is null)
                return;

            var caretType = caret.GetType();

            var blinkModeProp = caretType.GetProperty("BlinkMode", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (blinkModeProp is not null && blinkModeProp.CanWrite)
            {
                var enumType = blinkModeProp.PropertyType;
                if (enumType.IsEnum)
                {
                    var solid = Enum.Parse(enumType, "Solid", true);
                    blinkModeProp.SetValue(caret, solid);
                    return;
                }
            }

            var blinkIntervalProp = caretType.GetProperty("BlinkInterval", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (blinkIntervalProp is not null && blinkIntervalProp.CanWrite && blinkIntervalProp.PropertyType == typeof(TimeSpan))
            {
                blinkIntervalProp.SetValue(caret, TimeSpan.FromDays(1));
                return;
            }
        }
    }
}
