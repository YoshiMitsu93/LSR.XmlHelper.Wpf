using System.Windows.Input;

namespace LSR.XmlHelper.Wpf.Infrastructure.Commands
{
    public static class RawOutlineCommands
    {
        public static readonly RoutedUICommand DuplicateEntry = new RoutedUICommand("Duplicate entry", "DuplicateEntry", typeof(RawOutlineCommands));
        public static readonly RoutedUICommand DeleteEntry = new RoutedUICommand("Delete entry", "DeleteEntry", typeof(RawOutlineCommands));
    }
}
