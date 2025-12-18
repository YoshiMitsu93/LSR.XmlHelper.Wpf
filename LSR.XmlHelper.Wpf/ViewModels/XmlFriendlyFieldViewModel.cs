using LSR.XmlHelper.Wpf.Infrastructure;

namespace LSR.XmlHelper.Wpf.ViewModels
{
    public sealed class XmlFriendlyFieldViewModel : ObservableObject
    {
        private string _value;

        public XmlFriendlyFieldViewModel(string name, string value)
        {
            Name = name;
            _value = value;
        }

        public string Name { get; }

        public string Value
        {
            get => _value;
            set => SetProperty(ref _value, value);
        }
    }
}
