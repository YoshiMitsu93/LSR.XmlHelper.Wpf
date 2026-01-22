using LSR.XmlHelper.Wpf.Infrastructure;

namespace LSR.XmlHelper.Wpf.ViewModels
{
    public sealed class BreadcrumbSegmentViewModel : ObservableObject
    {
        private string _title = "";
        private int _offset;

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public int Offset
        {
            get => _offset;
            set => SetProperty(ref _offset, value);
        }
    }
}
