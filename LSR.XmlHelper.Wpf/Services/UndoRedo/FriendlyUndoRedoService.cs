using System.Collections.Generic;

namespace LSR.XmlHelper.Wpf.Services.UndoRedo
{
    public sealed class FriendlyUndoRedoService
    {
        private readonly List<string> _undo = new List<string>();
        private readonly List<string> _redo = new List<string>();
        private readonly int _max;

        public FriendlyUndoRedoService(int max = 50)
        {
            _max = max <= 0 ? 50 : max;
        }

        public bool CanUndo => _undo.Count > 0;
        public bool CanRedo => _redo.Count > 0;

        public void Clear()
        {
            _undo.Clear();
            _redo.Clear();
        }

        public void Record(string previous)
        {
            if (previous is null)
                return;

            _undo.Add(previous);
            if (_undo.Count > _max)
                _undo.RemoveAt(0);

            _redo.Clear();
        }

        public bool TryUndo(string current, out string previous)
        {
            previous = current;
            if (!CanUndo)
                return false;

            var idx = _undo.Count - 1;
            previous = _undo[idx];
            _undo.RemoveAt(idx);

            _redo.Add(current);
            if (_redo.Count > _max)
                _redo.RemoveAt(0);

            return true;
        }

        public bool TryRedo(string current, out string next)
        {
            next = current;
            if (!CanRedo)
                return false;

            var idx = _redo.Count - 1;
            next = _redo[idx];
            _redo.RemoveAt(idx);

            _undo.Add(current);
            if (_undo.Count > _max)
                _undo.RemoveAt(0);

            return true;
        }
    }
}
