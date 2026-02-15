namespace LSR.XmlHelper.Wpf.ViewModels.Builders
{
    public sealed class IssuableWeaponsGroupOptionViewModel
    {
        public IssuableWeaponsGroupOptionViewModel(string id, string name)
        {
            Id = id;
            Name = name;
        }

        public string Id { get; }

        public string Name { get; }

        public string DisplayText => $"{Name} ({Id})";
    }
}
