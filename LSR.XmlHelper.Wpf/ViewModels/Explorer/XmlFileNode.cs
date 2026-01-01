namespace LSR.XmlHelper.Wpf.ViewModels
{
    public sealed class XmlFileNode : XmlExplorerNode
    {
        public XmlFileNode(string name, string fullPath) : base(name)
        {
            FullPath = fullPath;
        }

        public override bool IsFile => true;
        public override string FullPath { get; }
    }
}
