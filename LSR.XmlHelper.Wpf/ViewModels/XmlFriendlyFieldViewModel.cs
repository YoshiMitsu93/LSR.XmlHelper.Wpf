using LSR.XmlHelper.Wpf.Infrastructure;
using System;

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

        public string GroupHeader
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Name))
                    return "General";

                var idx = Name.IndexOf('/');
                if (idx <= 0)
                    return "General";

                return Name.Substring(0, idx);
            }
        }

        public string Value
        {
            get => _value;
            set => SetProperty(ref _value, value);
        }
    }
}
