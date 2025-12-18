using LSR.XmlHelper.Core.Models;
using System.Collections.ObjectModel;
using System.Linq;

namespace LSR.XmlHelper.Wpf.ViewModels
{
    public sealed class XmlFriendlyChildCollectionViewModel
    {
        public XmlFriendlyChildCollectionViewModel(XmlFriendlyChildCollection model)
        {
            Name = model.Name;
            Items = new ObservableCollection<XmlFriendlyChildItemViewModel>(
                model.Items.Select(i => new XmlFriendlyChildItemViewModel(i)));
        }

        public string Name { get; }

        public ObservableCollection<XmlFriendlyChildItemViewModel> Items { get; }
    }
}
