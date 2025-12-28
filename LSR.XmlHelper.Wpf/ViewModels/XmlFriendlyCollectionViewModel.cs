using LSR.XmlHelper.Core.Services;
using LSR.XmlHelper.Wpf.Infrastructure;
using System.Collections.Generic;
using System.Linq;

namespace LSR.XmlHelper.Wpf.ViewModels
{
    public sealed class XmlFriendlyCollectionViewModel
    {
        private LazyVmList<XmlFriendlyEntry, XmlFriendlyEntryViewModel>? _entries;

        public XmlFriendlyCollectionViewModel(XmlFriendlyCollection collection)
        {
            Collection = collection;
        }

        public XmlFriendlyCollection Collection { get; }

        public string Title => GetDisplayTitle(Collection.Title);

        public IReadOnlyList<XmlFriendlyEntryViewModel> Entries =>
            _entries ??= new LazyVmList<XmlFriendlyEntry, XmlFriendlyEntryViewModel>(
                Collection.Entries,
                e => new XmlFriendlyEntryViewModel(e));

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
