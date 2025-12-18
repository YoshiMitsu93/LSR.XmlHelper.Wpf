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

        public string Title => GetDisplayTitle(Collection.Title);

        public ObservableCollection<XmlFriendlyEntryViewModel> Entries { get; }

        private static string GetDisplayTitle(string titlePath)
        {
            if (string.IsNullOrWhiteSpace(titlePath))
                return titlePath;

            var parts = titlePath.Split('/').Where(p => !string.IsNullOrWhiteSpace(p)).ToArray();
            if (parts.Length == 0)
                return titlePath;

            var last = parts[^1];

            if (string.Equals(last, "CellphoneIDLookup", System.StringComparison.OrdinalIgnoreCase) && parts.Length >= 2)
                return parts[^2];

            return last;
        }
    }
}
