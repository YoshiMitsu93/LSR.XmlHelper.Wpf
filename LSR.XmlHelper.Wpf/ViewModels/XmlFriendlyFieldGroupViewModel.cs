// LSR.XmlHelper.Wpf\ViewModels\XmlFriendlyFieldGroupViewModel.cs
using LSR.XmlHelper.Wpf.Infrastructure;
using System.Collections.ObjectModel;

namespace LSR.XmlHelper.Wpf.ViewModels
{
    public sealed class XmlFriendlyFieldGroupViewModel : ObservableObject
    {
        private bool _isExpanded = true;

        public XmlFriendlyFieldGroupViewModel(string title, ObservableCollection<XmlFriendlyFieldViewModel> fields)
        {
            Title = title;
            Fields = fields;
        }

        public string Title { get; }

        public ObservableCollection<XmlFriendlyFieldViewModel> Fields { get; }

        public bool IsExpanded
        {
            get => _isExpanded;
            set => SetProperty(ref _isExpanded, value);
        }
    }
}
