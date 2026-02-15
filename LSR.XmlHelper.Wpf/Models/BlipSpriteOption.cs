namespace LSR.XmlHelper.Wpf.Models
{
    public sealed class BlipSpriteOption
    {
        public BlipSpriteOption(string value, string displayText)
        {
            Value = value;
            DisplayText = displayText;
        }

        public string Value { get; }
        public string DisplayText { get; }
    }
}
