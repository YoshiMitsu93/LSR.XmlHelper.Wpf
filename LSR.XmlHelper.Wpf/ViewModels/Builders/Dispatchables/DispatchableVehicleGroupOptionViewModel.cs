namespace LSR.XmlHelper.Wpf.ViewModels.Builders
{
    public sealed class DispatchableVehicleGroupOptionViewModel
    {
        public DispatchableVehicleGroupOptionViewModel(string id, int count)
        {
            Id = id;
            Count = count;
            DisplayName = id;
        }

        public DispatchableVehicleGroupOptionViewModel(string id, int count, string displayName)
        {
            Id = id;
            Count = count;
            DisplayName = string.IsNullOrWhiteSpace(displayName) ? id : displayName;
        }

        public string Id { get; }

        public int Count { get; }

        public string DisplayName { get; }

        public string DisplayText => $"{DisplayName} ({Count} vehicles)";

        public override string ToString()
        {
            return Id;
        }
    }
}
