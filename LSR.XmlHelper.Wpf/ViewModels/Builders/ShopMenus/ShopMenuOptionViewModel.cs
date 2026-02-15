namespace LSR.XmlHelper.Wpf.ViewModels.Builders
{
    public sealed class ShopMenuOptionViewModel
    {
        public ShopMenuOptionViewModel(string id, string name)
        {
            Id = id;
            Name = name;
        }

        public string Id { get; }

        public string Name { get; }

        public string DisplayText => $"{Name} ({Id})";
    }
}
