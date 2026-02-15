using LSR.XmlHelper.Wpf.Infrastructure;

namespace LSR.XmlHelper.Wpf.ViewModels.Builders
{
    public sealed class GangBuilderTaskViewModel : ObservableObject
    {
        private bool _isEnabled;
        private bool _isRequired;
        private bool _isComplete;
        private string _title;
        private string _details;
        private string _status;

        public GangBuilderTaskViewModel(string title, string details)
        {
            _title = title;
            _details = details;
            _status = "Not started";
            _isEnabled = true;
            _isRequired = true;
        }

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public string Details
        {
            get => _details;
            set => SetProperty(ref _details, value);
        }

        public string Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        public bool IsEnabled
        {
            get => _isEnabled;
            set => SetProperty(ref _isEnabled, value);
        }

        public bool IsRequired
        {
            get => _isRequired;
            set => SetProperty(ref _isRequired, value);
        }

        public bool IsComplete
        {
            get => _isComplete;
            set => SetProperty(ref _isComplete, value);
        }
    }
}
