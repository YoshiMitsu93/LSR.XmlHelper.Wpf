using System.Xml.Linq;

namespace LSR.XmlHelper.Core.Models
{
    public sealed class PossibleVehicleSpawnModel
    {
        public string? DenName { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public double Heading { get; set; }
        public int Percentage { get; set; }
        public string TaskRequirements { get; set; } = "";
        public int MinHourSpawn { get; set; }
        public int MaxHourSpawn { get; set; }
        public int MinWantedLevelSpawn { get; set; }
        public int MaxWantedLevelSpawn { get; set; }
        public string RequiredVehicleGroup { get; set; } = "";
        public bool ForceVehicleGroup { get; set; }
        public bool AllowAirVehicle { get; set; }
        public bool AllowBoat { get; set; }

        public XElement? SourceElement { get; set; }
    }
}
