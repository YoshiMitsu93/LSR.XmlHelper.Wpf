namespace LSR.XmlHelper.Wpf.ViewModels.Builders
{
    public sealed class DispatchableVehicleVariantOptionViewModel
    {
        public DispatchableVehicleVariantOptionViewModel(string variantKey, string displayText)
        {
            VariantKey = variantKey;
            DisplayText = displayText;
        }

        public string VariantKey { get; }

        public string DisplayText { get; }

        public override string ToString()
        {
            return DisplayText;
        }
    }
}
