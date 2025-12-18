using LSR.XmlHelper.Core.Models;
using LSR.XmlHelper.Wpf.Infrastructure;

namespace LSR.XmlHelper.Wpf.ViewModels
{
    public sealed class XmlFriendlyChildItemViewModel : ObservableObject
    {
        private readonly XmlFriendlyChildItem _model;
        private string _id;
        private string _name;

        public XmlFriendlyChildItemViewModel(XmlFriendlyChildItem model)
        {
            _model = model;
            _id = _model.GetValue("ID") ?? string.Empty;
            _name = _model.GetValue("Name") ?? string.Empty;
        }

        public string Id
        {
            get => _id;
            set
            {
                if (SetProperty(ref _id, value))
                {
                    _model.TrySetValue("ID", value, out _);
                }
            }
        }

        public string Name
        {
            get => _name;
            set
            {
                if (SetProperty(ref _name, value))
                {
                    _model.TrySetValue("Name", value, out _);
                }
            }
        }
    }
}
