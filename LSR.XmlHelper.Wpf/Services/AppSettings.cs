namespace LSR.XmlHelper.Wpf.Services
{
    public sealed class AppSettings
    {
        public string? LastFolder { get; set; }
        public string ViewMode { get; set; } = "Flat";
        public bool IncludeSubfolders { get; set; } = false;
    }
}
