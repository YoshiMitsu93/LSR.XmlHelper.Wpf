using LSR.XmlHelper.Wpf.Infrastructure;
using LSR.XmlHelper.Wpf.Services.EditHistory;

namespace LSR.XmlHelper.Wpf.ViewModels
{
    public sealed class SavedEditRowViewModel : ObservableObject
    {
        private bool _isSelected;
        private string _status = "";
        private string? _currentValue;

        public SavedEditRowViewModel(EditHistoryItem item)
        {
            Item = item;
        }

        public EditHistoryItem Item { get; }

        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        public string Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        public string? CurrentValue
        {
            get => _currentValue;
            set => SetProperty(ref _currentValue, value);
        }
    }
}
