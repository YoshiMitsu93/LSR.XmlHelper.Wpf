using System.Collections.ObjectModel;

namespace LSR.XmlHelper.Wpf.ViewModels
{
    public abstract class XmlExplorerNode
    {
        protected XmlExplorerNode(string name)
        {
            Name = name;
            Children = new ObservableCollection<XmlExplorerNode>();
        }

        public string Name { get; }
        public ObservableCollection<XmlExplorerNode> Children { get; }
        public virtual bool IsFile => false;
        public virtual string? FullPath => null;

        public override string ToString() => Name;
    }
}
