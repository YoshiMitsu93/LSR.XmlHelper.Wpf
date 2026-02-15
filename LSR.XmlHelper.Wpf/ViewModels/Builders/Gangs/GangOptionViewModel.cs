namespace LSR.XmlHelper.Wpf.ViewModels.Builders
{
    public sealed class GangOptionViewModel
    {
        public GangOptionViewModel(string id, string fullName)
        {
            Id = id;
            FullName = fullName;
        }

        public string Id { get; }

        public string FullName { get; }

        public string DisplayText => $"{FullName} ({Id})";
    }
}
