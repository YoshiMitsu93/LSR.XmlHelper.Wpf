using LSR.XmlHelper.Wpf.Infrastructure;

namespace LSR.XmlHelper.Wpf.ViewModels.Builders
{
    public sealed class DispatchablePersonFieldViewModel : ObservableObject
    {
        private string _value;

        public DispatchablePersonFieldViewModel(string name, string value, bool isXml)
        {
            Name = name;
            _value = value;
            IsXml = isXml;
            TooltipText = DispatchablePersonFieldTooltipService.GetTooltip(name);
        }

        public string Name { get; }

        public bool IsXml { get; }

        public string TooltipText { get; }

        public string Value
        {
            get => _value;
            set => SetProperty(ref _value, value);
        }
    }
}
