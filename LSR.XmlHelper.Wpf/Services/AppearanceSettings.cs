using LSR.XmlHelper.Wpf.Services;
using System;

namespace LSR.XmlHelper.Wpf.Services
{
    public sealed class AppearanceSettings
    {
        public AppearanceProfileSettings Dark { get; set; } = AppearanceProfileSettings.CreateDarkDefaults();
        public AppearanceProfileSettings Light { get; set; } = AppearanceProfileSettings.CreateLightDefaults();

        public AppearanceProfileSettings GetActiveProfile(bool isDarkMode) => isDarkMode ? Dark : Light;
    }

    public sealed class AppearanceProfileSettings
    {
        public string UiFontFamily { get; set; } = "Segoe UI";
        public double UiFontSize { get; set; } = 12;
        public bool UiFontBold { get; set; }
        public bool UiFontItalic { get; set; }

        public string EditorFontFamily { get; set; } = "Consolas";
        public double EditorFontSize { get; set; } = 13;
        public bool EditorFontBold { get; set; }
        public bool EditorFontItalic { get; set; }

        public string Text { get; set; } = "#FFD4D4D4";
        public string Background { get; set; } = "#FF1E1E1E";

        public string TreeText { get; set; } = "#FFD4D4D4";
        public string TreeBackground { get; set; } = "#FF1E1E1E";
        public string TreeItemHoverBackground { get; set; } = "#FF252525";
        public string TreeItemSelectedBackground { get; set; } = "#FF2F2F2F";

        public string GridText { get; set; } = "#FFD4D4D4";
        public string GridBackground { get; set; } = "#FF1E1E1E";
        public string GridBorder { get; set; } = "#FF555555";

        public string GridHeaderBackground { get; set; } = "#FF1E1E1E";
        public string GridHeaderText { get; set; } = "#FFD4D4D4";

        public string GridRowHoverBackground { get; set; } = "#FF252525";
        public string GridRowSelectedBackground { get; set; } = "#FF2F2F2F";

        public string GridCellSelectedBackground { get; set; } = "#FF2F2F2F";
        public string GridCellSelectedText { get; set; } = "#FFFFFFFF";

        public string FieldColumnText { get; set; } = "#FFD4D4D4";
        public string ValueColumnText { get; set; } = "#FFD4D4D4";
        public string HeaderText { get; set; } = "#FFD4D4D4";

        public static AppearanceProfileSettings CreateDarkDefaults()
        {
            return new AppearanceProfileSettings
            {
                UiFontFamily = "Segoe UI",
                UiFontSize = 12,

                EditorFontFamily = "Consolas",
                EditorFontSize = 13,

                Text = "#FFD4D4D4",
                Background = "#FF1E1E1E",

                TreeText = "#FFD4D4D4",
                TreeBackground = "#FF1E1E1E",
                TreeItemHoverBackground = "#FF252525",
                TreeItemSelectedBackground = "#FF2F2F2F",

                GridText = "#FFD4D4D4",
                GridBackground = "#FF1E1E1E",
                GridBorder = "#FF555555",
                GridHeaderBackground = "#FF1E1E1E",
                GridHeaderText = "#FFD4D4D4",

                GridRowHoverBackground = "#FF252525",
                GridRowSelectedBackground = "#FF2F2F2F",

                GridCellSelectedBackground = "#FF2F2F2F",
                GridCellSelectedText = "#FFFFFFFF",

                FieldColumnText = "#FFD4D4D4",
                ValueColumnText = "#FFD4D4D4",
                HeaderText = "#FFD4D4D4"
            };
        }

        public static AppearanceProfileSettings CreateLightDefaults()
        {
            return new AppearanceProfileSettings
            {
                UiFontFamily = "Segoe UI",
                UiFontSize = 12,

                EditorFontFamily = "Consolas",
                EditorFontSize = 13,

                Text = "#FF000000",
                Background = "#FFFFFFFF",

                TreeText = "#FF000000",
                TreeBackground = "#FFFFFFFF",
                TreeItemHoverBackground = "#FFEAEAEA",
                TreeItemSelectedBackground = "#FFCCE4FF",

                GridText = "#FF000000",
                GridBackground = "#FFFFFFFF",
                GridBorder = "#FF555555",
                GridHeaderBackground = "#FFF5F5F5",
                GridHeaderText = "#FF000000",

                GridRowHoverBackground = "#FFEAEAEA",
                GridRowSelectedBackground = "#FFCCE4FF",

                GridCellSelectedBackground = "#FFCCE4FF",
                GridCellSelectedText = "#FF000000",

                FieldColumnText = "#FF000000",
                ValueColumnText = "#FF000000",
                HeaderText = "#FF000000"
            };
        }
    }
}
