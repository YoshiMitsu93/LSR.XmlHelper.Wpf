using LSR.XmlHelper.Wpf.Infrastructure;

namespace LSR.XmlHelper.Wpf.ViewModels.Builders
{
    public sealed class PossibleVehicleSpawnViewModel : ObservableObject
    {
        private string _denName = "";
        private double _x;
        private double _y;
        private double _z;
        private double _heading;
        private int _percentage;
        private string _taskRequirements = "";
        private int _minHourSpawn;
        private int _maxHourSpawn;
        private int _minWantedLevelSpawn;
        private int _maxWantedLevelSpawn;
        private string _requiredVehicleGroup = "";
        private bool _forceVehicleGroup;
        private bool _allowAirVehicle;
        private bool _allowBoat;

        public System.Xml.Linq.XElement? SourceElement { get; init; }

        public string DenName
        {
            get => _denName;
            set => SetProperty(ref _denName, value);
        }

        public double X
        {
            get => _x;
            set => SetProperty(ref _x, value);
        }

        public double Y
        {
            get => _y;
            set => SetProperty(ref _y, value);
        }

        public double Z
        {
            get => _z;
            set => SetProperty(ref _z, value);
        }

        public double Heading
        {
            get => _heading;
            set => SetProperty(ref _heading, value);
        }

        public int Percentage
        {
            get => _percentage;
            set => SetProperty(ref _percentage, value);
        }

        public string TaskRequirements
        {
            get => _taskRequirements;
            set => SetProperty(ref _taskRequirements, value);
        }

        public int MinHourSpawn
        {
            get => _minHourSpawn;
            set => SetProperty(ref _minHourSpawn, value);
        }

        public int MaxHourSpawn
        {
            get => _maxHourSpawn;
            set => SetProperty(ref _maxHourSpawn, value);
        }

        public int MinWantedLevelSpawn
        {
            get => _minWantedLevelSpawn;
            set => SetProperty(ref _minWantedLevelSpawn, value);
        }

        public int MaxWantedLevelSpawn
        {
            get => _maxWantedLevelSpawn;
            set => SetProperty(ref _maxWantedLevelSpawn, value);
        }

        public string RequiredVehicleGroup
        {
            get => _requiredVehicleGroup;
            set => SetProperty(ref _requiredVehicleGroup, value);
        }

        public bool ForceVehicleGroup
        {
            get => _forceVehicleGroup;
            set => SetProperty(ref _forceVehicleGroup, value);
        }

        public bool AllowAirVehicle
        {
            get => _allowAirVehicle;
            set => SetProperty(ref _allowAirVehicle, value);
        }

        public bool AllowBoat
        {
            get => _allowBoat;
            set => SetProperty(ref _allowBoat, value);
        }
    }
}
