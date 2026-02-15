namespace LSR.XmlHelper.Wpf.ViewModels.Builders
{
    public sealed class DispatchableVehicleModelOptionViewModel
    {
        public DispatchableVehicleModelOptionViewModel(string modelName, int count)
        {
            ModelName = modelName;
            Count = count;
        }

        public string ModelName { get; }

        public int Count { get; }

        public string DisplayText => ModelName;

        public override string ToString()
        {
            return ModelName;
        }
    }
}
