using LSR.XmlHelper.Wpf.Infrastructure;

namespace LSR.XmlHelper.Wpf.ViewModels.Dialogs
{
    public sealed class EditSummaryExportOptionsViewModel : ObservableObject
    {
        private string _fileName = "";
        private string _notes = "";

        public string FileName
        {
            get => _fileName;
            set => SetProperty(ref _fileName, value);
        }

        public string Notes
        {
            get => _notes;
            set => SetProperty(ref _notes, value);
        }
    }
}
