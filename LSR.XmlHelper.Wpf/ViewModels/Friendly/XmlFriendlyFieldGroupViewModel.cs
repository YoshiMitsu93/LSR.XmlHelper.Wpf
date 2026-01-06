using LSR.XmlHelper.Wpf.Infrastructure;
using System.Collections.ObjectModel;

namespace LSR.XmlHelper.Wpf.ViewModels
{
    public sealed class XmlFriendlyFieldGroupViewModel : ObservableObject
    {
        private bool _isExpanded = true;
        private XmlFriendlyFieldViewModel? _selectedField;
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
        public XmlFriendlyFieldViewModel? SelectedField
        {
            get => _selectedField;
            set => SetProperty(ref _selectedField, value);
        }
    }
}
