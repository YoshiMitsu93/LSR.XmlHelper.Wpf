using LSR.XmlHelper.Core.Services;
using System.Collections.ObjectModel;
using System.Linq;

namespace LSR.XmlHelper.Wpf.ViewModels
{
    public sealed class XmlFriendlyEntryViewModel
    {
        public XmlFriendlyEntryViewModel(XmlFriendlyEntry entry)
        {
            Entry = entry;

            Display = entry.Fields.TryGetValue("ModItemName", out var n)
                ? n.Value
                : entry.Fields.TryGetValue("Name", out var name)
                    ? name.Value
                    : entry.Fields.TryGetValue("ID", out var id)
                        ? id.Value
                        : "<no name>";

            ChildCollections = new ObservableCollection<XmlFriendlyChildCollectionViewModel>(
                entry.ChildCollections.Select(c => new XmlFriendlyChildCollectionViewModel(c)));
        }

        public XmlFriendlyEntry Entry { get; }

        public string Display { get; }

        public ObservableCollection<XmlFriendlyChildCollectionViewModel> ChildCollections { get; }
    }
}
