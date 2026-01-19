using System;

namespace LSR.XmlHelper.Wpf.Services
{
    public sealed class NamedAppearanceProfile
    {
        public string Name { get; set; } = "";
        public DateTimeOffset CreatedUtc { get; set; } = DateTimeOffset.UtcNow;
        public AppearanceProfileSettings Dark { get; set; } = AppearanceProfileSettings.CreateDarkDefaults();
        public AppearanceProfileSettings Light { get; set; } = AppearanceProfileSettings.CreateLightDefaults();
    }
}
