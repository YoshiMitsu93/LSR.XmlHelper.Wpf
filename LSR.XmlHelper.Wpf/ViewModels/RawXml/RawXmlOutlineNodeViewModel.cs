using LSR.XmlHelper.Wpf.Infrastructure;
using System.Collections.ObjectModel;

namespace LSR.XmlHelper.Wpf.ViewModels
{
    public sealed class RawXmlOutlineNodeViewModel : ObservableObject
    {
        private string _title = "";
        private int _offset;
        private readonly ObservableCollection<RawXmlOutlineNodeViewModel> _children = new();

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

        public ObservableCollection<RawXmlOutlineNodeViewModel> Children => _children;
    }
}
