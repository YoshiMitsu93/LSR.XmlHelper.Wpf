using LSR.XmlHelper.Wpf.Infrastructure;

namespace LSR.XmlHelper.Wpf.ViewModels.Builders
{
    public sealed class EmbeddedDealerMenuOptionViewModel : ObservableObject
    {
        public int MenuIndex { get; }

        public string DisplayText { get; }

        public EmbeddedDealerMenuOptionViewModel(int menuIndex, string displayText)
        {
            MenuIndex = menuIndex;
            DisplayText = displayText;
        }
    }
}
