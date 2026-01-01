namespace LSR.XmlHelper.Wpf.ViewModels
{
    public sealed class XmlFileListItem
    {
        public XmlFileListItem(string fullPath, string displayPath)
        {
            FullPath = fullPath;
            DisplayPath = displayPath;
        }

        public string FullPath { get; }
        public string DisplayPath { get; }

        public override string ToString() => DisplayPath;
    }
}
