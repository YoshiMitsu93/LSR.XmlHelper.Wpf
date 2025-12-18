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

            ParseName(name, out var section, out var item, out var field);
            Section = section;
            Item = item;
            Field = field;
        }

        public string Name { get; }

        public string Section { get; }

        public string Item { get; }

        public string Field { get; }

        public string Value
        {
            get => _value;
            set => SetProperty(ref _value, value);
        }

        private static void ParseName(string name, out string section, out string item, out string field)
        {
            section = "General";
            item = "";
            field = name ?? "";

            if (string.IsNullOrWhiteSpace(name))
            {
                field = "";
                return;
            }

            var parts = name.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 0)
            {
                field = name;
                return;
            }

            if (parts.Length == 1)
            {
                section = "General";
                item = "";
                field = parts[0];
                return;
            }

            section = parts[0];

            if (parts.Length == 2)
            {
                item = "";
                field = parts[1];
                return;
            }

            item = parts[1];
            field = string.Join("/", parts, 2, parts.Length - 2);
        }
    }
}
