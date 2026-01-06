using LSR.XmlHelper.Wpf.Infrastructure;

namespace LSR.XmlHelper.Wpf.ViewModels
{
    public sealed class XmlFriendlyLookupItemViewModel : ObservableObject
    {
        private readonly XmlFriendlyFieldViewModel _field;

        public XmlFriendlyLookupItemViewModel(string item, string field, XmlFriendlyFieldViewModel backingField)
        {
            Item = item;
            Field = field;
            _field = backingField;

            _field.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(XmlFriendlyFieldViewModel.IsSearchMatch))
                    OnPropertyChanged(nameof(IsSearchMatch));
            };
        }

        public string Item { get; }

        public string Field { get; }

        public string FullName => _field.Name;
        public bool IsSearchMatch => _field.IsSearchMatch;

        public string Value
        {
            get => _field.Value;
            set
            {
                if (_field.Value == value)
                    return;

                _field.Value = value;
                OnPropertyChanged(nameof(Value));
            }
        }
    }
}
