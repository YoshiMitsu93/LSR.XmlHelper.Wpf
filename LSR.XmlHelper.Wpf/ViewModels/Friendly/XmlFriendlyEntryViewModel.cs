using LSR.XmlHelper.Core.Services;
using LSR.XmlHelper.Wpf.Infrastructure;
using System;

namespace LSR.XmlHelper.Wpf.ViewModels
{
    public sealed class XmlFriendlyEntryViewModel : ObservableObject
    {
        private readonly string _displayOverride;

        public XmlFriendlyEntryViewModel(XmlFriendlyEntry entry, string displayOverride = null)
        {
            Entry = entry;
            _displayOverride = displayOverride;
        }

        public XmlFriendlyEntry Entry { get; }

        public string Display => string.IsNullOrWhiteSpace(_displayOverride) ? Entry.Display : _displayOverride;
        public void RefreshDisplay()
        {
            OnPropertyChanged(nameof(Display));
        }
    }
}
