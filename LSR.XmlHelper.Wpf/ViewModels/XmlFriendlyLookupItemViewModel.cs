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
        }

        public string Item { get; }

        public string Field { get; }

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
