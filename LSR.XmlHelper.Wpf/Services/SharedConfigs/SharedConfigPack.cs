using LSR.XmlHelper.Wpf.Services.EditHistory;
using System;

namespace LSR.XmlHelper.Wpf.Services.SharedConfigs
{
    public sealed class SharedConfigPack
    {
        public int Version { get; set; } = 1;
        public DateTimeOffset CreatedUtc { get; set; } = DateTimeOffset.UtcNow;
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";

        public AppearanceSettings? Appearance { get; set; }
        public EditHistorySettings? EditHistory { get; set; }
    }
}
