using System.Windows.Input;

namespace LSR.XmlHelper.Wpf.Infrastructure.Commands
{
    public static class RawXmlQuickActionsCommands
    {
        public static readonly RoutedUICommand DuplicateEntry = new RoutedUICommand("Duplicate entry", "DuplicateEntry", typeof(RawXmlQuickActionsCommands));
        public static readonly RoutedUICommand DeleteEntry = new RoutedUICommand("Delete entry", "DeleteEntry", typeof(RawXmlQuickActionsCommands));
        public static readonly RoutedUICommand DuplicateLine = new RoutedUICommand("Duplicate line", "DuplicateLine", typeof(RawXmlQuickActionsCommands));
        public static readonly RoutedUICommand CollapseElement = new RoutedUICommand("Collapse element", "CollapseElement", typeof(RawXmlQuickActionsCommands));
        public static readonly RoutedUICommand ExpandElement = new RoutedUICommand("Expand element", "ExpandElement", typeof(RawXmlQuickActionsCommands));
        public static readonly RoutedUICommand CollapseAll = new RoutedUICommand("Collapse all", "CollapseAll", typeof(RawXmlQuickActionsCommands));
        public static readonly RoutedUICommand ExpandAll = new RoutedUICommand("Expand all", "ExpandAll", typeof(RawXmlQuickActionsCommands));
    }
}
