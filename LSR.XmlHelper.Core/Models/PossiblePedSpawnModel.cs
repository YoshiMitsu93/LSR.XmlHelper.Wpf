namespace LSR.XmlHelper.Core.Models
{
    public sealed class PossiblePedSpawnModel
    {
        public string DenName { get; set; } = "";
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
        public bool LongGunAlwaysEquipped { get; set; }

        public System.Xml.Linq.XElement? SourceElement { get; set; }
    }
}
