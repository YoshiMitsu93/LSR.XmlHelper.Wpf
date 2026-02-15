namespace LSR.XmlHelper.Wpf.ViewModels.Builders
{
    public sealed class BuildOutputFileViewModel
    {
        public BuildOutputFileViewModel(string fileName, string fullPath)
        {
            FileName = fileName;
            FullPath = fullPath;
        }

        public string FileName { get; }
        public string FullPath { get; }
    }
}
