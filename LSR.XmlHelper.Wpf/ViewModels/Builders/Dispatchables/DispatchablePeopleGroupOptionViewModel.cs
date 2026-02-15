namespace LSR.XmlHelper.Wpf.ViewModels.Builders
{
    public sealed class DispatchablePeopleGroupOptionViewModel
    {
        public DispatchablePeopleGroupOptionViewModel(string id, int count)
        {
            Id = id;
            Count = count;
        }

        public string Id { get; }

        public int Count { get; }

        public string DisplayText => $"{Id} ({Count} peds)";
    }
}
