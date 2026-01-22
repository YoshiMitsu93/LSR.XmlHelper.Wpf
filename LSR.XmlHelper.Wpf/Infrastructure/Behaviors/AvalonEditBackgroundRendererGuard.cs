using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Rendering;
using System;
using System.Collections.Generic;

namespace LSR.XmlHelper.Wpf.Infrastructure.Behaviors
{
    public sealed class AvalonEditBackgroundRendererGuard : IDisposable
    {
        private readonly TextEditor _editor;
        private readonly IReadOnlyList<IBackgroundRenderer> _renderers;
        private bool _isDisposed;

        public AvalonEditBackgroundRendererGuard(TextEditor editor, params IBackgroundRenderer[] renderers)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _renderers = renderers ?? Array.Empty<IBackgroundRenderer>();

            _editor.LayoutUpdated += EditorOnLayoutUpdated;

            EnsureRenderers();
        }

        private void EditorOnLayoutUpdated(object? sender, EventArgs e)
        {
            EnsureRenderers();
        }

        public void Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;

            _editor.LayoutUpdated -= EditorOnLayoutUpdated;
        }

        private void EnsureRenderers()
        {
            if (_isDisposed)
                return;

            if (_editor.TextArea is null)
                return;

            var textView = _editor.TextArea.TextView;
            if (textView is null)
                return;

            var collection = textView.BackgroundRenderers;

            foreach (var renderer in _renderers)
            {
                if (renderer is null)
                    continue;

                if (!collection.Contains(renderer))
                    collection.Add(renderer);
            }

            textView.InvalidateVisual();
        }
    }
}
