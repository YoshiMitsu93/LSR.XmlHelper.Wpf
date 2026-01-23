using System.Windows.Input;

namespace LSR.XmlHelper.Wpf.Infrastructure.Commands
{
    public static class EditorUndoRedoCommands
    {
        public static readonly RoutedUICommand Undo = new RoutedUICommand("Undo", "Undo", typeof(EditorUndoRedoCommands));
        public static readonly RoutedUICommand Redo = new RoutedUICommand("Redo", "Redo", typeof(EditorUndoRedoCommands));
    }
}
