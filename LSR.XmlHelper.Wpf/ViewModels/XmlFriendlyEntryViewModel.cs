using LSR.XmlHelper.Core.Services;

namespace LSR.XmlHelper.Wpf.ViewModels
{
    public sealed class XmlFriendlyEntryViewModel
    {
        public XmlFriendlyEntryViewModel(XmlFriendlyEntry entry)
        {
            Entry = entry;
        }

        public XmlFriendlyEntry Entry { get; }

        public string Display => Entry.Display;
    }
}
