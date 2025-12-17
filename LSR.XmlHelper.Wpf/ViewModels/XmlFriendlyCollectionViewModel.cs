using LSR.XmlHelper.Core.Services;
using System.Collections.ObjectModel;
using System.Linq;

namespace LSR.XmlHelper.Wpf.ViewModels
{
    public sealed class XmlFriendlyCollectionViewModel
    {
        public XmlFriendlyCollectionViewModel(XmlFriendlyCollection collection)
        {
            Collection = collection;
            Entries = new ObservableCollection<XmlFriendlyEntryViewModel>(
                collection.Entries.Select(e => new XmlFriendlyEntryViewModel(e)));
        }

        public XmlFriendlyCollection Collection { get; }
        public string Title => Collection.Title;

        public ObservableCollection<XmlFriendlyEntryViewModel> Entries { get; }
    }
}
