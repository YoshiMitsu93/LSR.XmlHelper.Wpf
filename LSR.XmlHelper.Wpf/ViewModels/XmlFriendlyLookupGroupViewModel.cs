using LSR.XmlHelper.Wpf.Infrastructure;
using System.Collections.ObjectModel;

namespace LSR.XmlHelper.Wpf.ViewModels
{
    public sealed class XmlFriendlyLookupGroupViewModel : ObservableObject
    {
        private bool _isExpanded = true;

        public XmlFriendlyLookupGroupViewModel(string title, ObservableCollection<XmlFriendlyLookupItemViewModel> items)
        {
            Title = title;
            Items = items;
        }

        public string Title { get; }

        public ObservableCollection<XmlFriendlyLookupItemViewModel> Items { get; }

        public bool IsExpanded
        {
            get => _isExpanded;
            set => SetProperty(ref _isExpanded, value);
        }
    }
}
