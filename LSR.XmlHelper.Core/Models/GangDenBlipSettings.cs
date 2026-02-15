namespace LSR.XmlHelper.Core.Models
{
    public sealed class GangDenBlipSettings
    {
        public bool IsBlipEnabled { get; set; }
        public string MapIcon { get; set; } = "";
        public string MapIconColorString { get; set; } = "";
        public string MapIconScale { get; set; } = "";
        public string MapIconRadius { get; set; } = "";
        public string MapOpenIconAlpha { get; set; } = "";
        public string MapClosedIconAlpha { get; set; } = "";
    }
}
