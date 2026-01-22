using System;
using System.Collections.Generic;

namespace LSR.XmlHelper.Wpf.Services
{
    public sealed class AppearanceSettings
    {
        public AppearanceProfileSettings Dark { get; set; } = AppearanceProfileSettings.CreateDarkDefaults();
        public AppearanceProfileSettings Light { get; set; } = AppearanceProfileSettings.CreateLightDefaults();
        public int[] ColorPickerCustomColors { get; set; } = new int[16];
        public string ActiveProfileName { get; set; } = "";
        public List<NamedAppearanceProfile> Profiles { get; set; } = new List<NamedAppearanceProfile>();

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
        public string EditorText { get; set; } = "#FFD4D4D4";
        public string EditorBackground { get; set; } = "#FF1E1E1E";
        public string EditorXmlSyntaxForeground { get; set; } = "";
        public string EditorScopeShadingColor { get; set; } = "";
        public string EditorIndentGuidesColor { get; set; } = "";
        public string EditorRegionHighlightColor { get; set; } = "";
        public string AppearanceWindowBackground { get; set; } = "#FF1E1E1E";
        public string AppearanceWindowText { get; set; } = "#FFD4D4D4";
        public string AppearanceWindowControlBackground { get; set; } = "#FF1E1E1E";
        public string AppearanceWindowControlBorder { get; set; } = "#FF555555";
        public string AppearanceWindowHeaderText { get; set; } = "#FFD4D4D4";
        public string AppearanceWindowHeaderBackground { get; set; } = "#FF1E1E1E";
        public string AppearanceWindowTabBackground { get; set; } = "#FF2F2F2F";
        public string AppearanceWindowTabHoverBackground { get; set; } = "#FF3A3A3A";
        public string AppearanceWindowTabSelectedBackground { get; set; } = "#FF1E1E1E";
        public string AppearanceWindowButtonBackground { get; set; } = "#FF2F2F2F";
        public string AppearanceWindowButtonHoverBackground { get; set; } = "#FF3A3A3A";
        public string SharedConfigPacksWindowBackground { get; set; } = "#FF1E1E1E";
        public string SharedConfigPacksWindowText { get; set; } = "#FFD4D4D4";
        public string SharedConfigPacksWindowControlBackground { get; set; } = "#FF1E1E1E";
        public string SharedConfigPacksWindowControlBorder { get; set; } = "#FF555555";
        public string SharedConfigPacksWindowHeaderText { get; set; } = "#FFD4D4D4";
        public string SharedConfigPacksWindowHeaderBackground { get; set; } = "#FF1E1E1E";
        public string SharedConfigPacksWindowTabBackground { get; set; } = "#FF2F2F2F";
        public string SharedConfigPacksWindowTabHoverBackground { get; set; } = "#FF3A3A3A";
        public string SharedConfigPacksWindowTabSelectedBackground { get; set; } = "#FF1E1E1E";
        public string SharedConfigPacksWindowButtonBackground { get; set; } = "#FF2F2F2F";
        public string SharedConfigPacksWindowButtonHoverBackground { get; set; } = "#FF3A3A3A";
        public string SharedConfigPacksWindowEditsHoverBackground { get; set; } = "#FF3A3A3A";
        public string SharedConfigPacksWindowCheckBoxBackground { get; set; } = "#FF1E1E1E";
        public string SharedConfigPacksWindowCheckBoxTick { get; set; } = "#FFD4D4D4";
        public string CompareXmlWindowBackground { get; set; } = "#FF1E1E1E";
        public string CompareXmlWindowText { get; set; } = "#FFD4D4D4";
        public string CompareXmlWindowControlBackground { get; set; } = "#FF1E1E1E";
        public string CompareXmlWindowControlBorder { get; set; } = "#FF555555";
        public string CompareXmlWindowHeaderText { get; set; } = "#FFD4D4D4";
        public string CompareXmlWindowHeaderBackground { get; set; } = "#FF1E1E1E";
        public string CompareXmlWindowButtonBackground { get; set; } = "#FF2F2F2F";
        public string CompareXmlWindowButtonHoverBackground { get; set; } = "#FF3A3A3A";
        public string CompareXmlWindowEditsHoverBackground { get; set; } = "#FF3A3A3A";
        public string CompareXmlWindowCheckBoxBackground { get; set; } = "#FF1E1E1E";
        public string CompareXmlWindowCheckBoxTick { get; set; } = "#FFD4D4D4";
        public string BackupBrowserWindowBackground { get; set; } = "#FF1E1E1E";
        public string BackupBrowserWindowText { get; set; } = "#FFD4D4D4";
        public string BackupBrowserWindowControlBackground { get; set; } = "#FF1E1E1E";
        public string BackupBrowserWindowControlBorder { get; set; } = "#FF555555";
        public string BackupBrowserWindowHeaderText { get; set; } = "#FFD4D4D4";
        public string BackupBrowserWindowHeaderBackground { get; set; } = "#FF1E1E1E";
        public string BackupBrowserWindowListHoverBackground { get; set; } = "#FF3A3A3A";
        public string BackupBrowserWindowListSelectedBackground { get; set; } = "#FF3A3A3A";
        public string BackupBrowserWindowButtonBackground { get; set; } = "#FF2F2F2F";
        public string BackupBrowserWindowButtonHoverBackground { get; set; } = "#FF3A3A3A";
        public string BackupBrowserWindowXmlFilterHoverBackground { get; set; } = "#FF3A3A3A";
        public string BackupBrowserWindowXmlFilterSelectedBackground { get; set; } = "#FF3A3A3A";

        public string SavedEditsWindowBackground { get; set; } = "#FF1E1E1E";
        public string SavedEditsWindowText { get; set; } = "#FFD4D4D4";
        public string SavedEditsWindowControlBackground { get; set; } = "#FF1E1E1E";
        public string SavedEditsWindowControlBorder { get; set; } = "#FF555555";
        public string SavedEditsWindowTabBackground { get; set; } = "#FF2F2F2F";
        public string SavedEditsWindowTabHoverBackground { get; set; } = "#FF3A3A3A";
        public string SavedEditsWindowTabSelectedBackground { get; set; } = "#FF1E1E1E";
        public string SavedEditsWindowButtonBackground { get; set; } = "#FF2F2F2F";
        public string SavedEditsWindowButtonHoverBackground { get; set; } = "#FF3A3A3A";
        public string SavedEditsWindowCheckBoxBackground { get; set; } = "#FF1E1E1E";
        public string SavedEditsWindowCheckBoxTick { get; set; } = "#FFD4D4D4";
        public string SavedEditsWindowGridText { get; set; } = "#FFD4D4D4";
        public string SavedEditsWindowGridBackground { get; set; } = "#FF1E1E1E";
        public string SavedEditsWindowGridBorder { get; set; } = "#FF555555";
        public string SavedEditsWindowGridLines { get; set; } = "#FF555555";
        public string SavedEditsWindowGridHeaderBackground { get; set; } = "#FF1E1E1E";
        public string SavedEditsWindowGridHeaderText { get; set; } = "#FFD4D4D4";
        public string SavedEditsWindowGridRowHoverBackground { get; set; } = "#FF252525";
        public string SavedEditsWindowGridRowSelectedBackground { get; set; } = "#FF2F2F2F";
        public string SavedEditsWindowGridCellSelectedBackground { get; set; } = "#FF2F2F2F";
        public string SavedEditsWindowGridCellSelectedText { get; set; } = "#FFFFFFFF";

        public string SettingsInfoWindowBackground { get; set; } = "#FF1E1E1E";
        public string SettingsInfoWindowText { get; set; } = "#FFD4D4D4";
        public string SettingsInfoWindowControlBackground { get; set; } = "#FF1E1E1E";
        public string SettingsInfoWindowControlBorder { get; set; } = "#FF555555";
        public string SettingsInfoWindowHeaderText { get; set; } = "#FFD4D4D4";
        public string SettingsInfoWindowHeaderBackground { get; set; } = "#FF1E1E1E";
        public string SettingsInfoWindowButtonBackground { get; set; } = "#FF2F2F2F";
        public string SettingsInfoWindowButtonHoverBackground { get; set; } = "#FF3A3A3A";
        public string SettingsInfoWindowCheckBoxBackground { get; set; } = "#FF1E1E1E";
        public string SettingsInfoWindowCheckBoxTick { get; set; } = "#FFD4D4D4";

        public string DocumentationWindowBackground { get; set; } = "#FF1E1E1E";
        public string DocumentationWindowText { get; set; } = "#FFD4D4D4";
        public string DocumentationWindowControlBackground { get; set; } = "#FF1E1E1E";
        public string DocumentationWindowControlBorder { get; set; } = "#FF555555";
        public string DocumentationWindowHeaderText { get; set; } = "#FFD4D4D4";
        public string DocumentationWindowHeaderBackground { get; set; } = "#FF1E1E1E";
        public string DocumentationWindowListHoverBackground { get; set; } = "#FF252525";
        public string DocumentationWindowListSelectedBackground { get; set; } = "#FF2F2F2F";
        public string DocumentationWindowButtonBackground { get; set; } = "#FF2F2F2F";
        public string DocumentationWindowButtonHoverBackground { get; set; } = "#FF3A3A3A";

        public string XmlGuidesWindowBackground { get; set; } = "#FF1E1E1E";
        public string XmlGuidesWindowText { get; set; } = "#FFD4D4D4";
        public string XmlGuidesWindowControlBackground { get; set; } = "#FF1E1E1E";
        public string XmlGuidesWindowControlBorder { get; set; } = "#FF555555";
        public string XmlGuidesWindowButtonBackground { get; set; } = "#FF2F2F2F";
        public string XmlGuidesWindowButtonText { get; set; } = "#FFD4D4D4";
        public string XmlGuidesWindowButtonHoverBackground { get; set; } = "#FF3A3A3A";
        public string XmlGuidesWindowButtonHoverText { get; set; } = "#FFFFFFFF";
        public string XmlGuidesWindowGuidesListBackground { get; set; } = "#FF1E1E1E";
        public string XmlGuidesWindowGuidesListText { get; set; } = "#FFD4D4D4";
        public string XmlGuidesWindowGuidesListItemHoverBackground { get; set; } = "#FF252525";
        public string XmlGuidesWindowGuidesListItemHoverText { get; set; } = "#FFFFFFFF";
        public string XmlGuidesWindowGuidesListItemSelectedBackground { get; set; } = "#FF2F2F2F";
        public string XmlGuidesWindowGuidesListItemSelectedText { get; set; } = "#FFFFFFFF";
        public string XmlGuidesWindowFontPickerText { get; set; } = "#FFD4D4D4";

        public string MenuBackground { get; set; } = "#FF1E1E1E";
         public string TopBarText { get; set; } = "#FFD4D4D4";
        public string MenuText { get; set; } = "#FFD4D4D4";
        public string TopButtonText { get; set; } = "#FFD4D4D4";
        public string TopButtonBackground { get; set; } = "#FF2F2F2F";
        public string TreeText { get; set; } = "#FFD4D4D4";
        public string TreeBackground { get; set; } = "#FF1E1E1E";
        public string TreeItemHoverBackground { get; set; } = "#FF0080C0";
        public string TreeItemSelectedBackground { get; set; } = "#FF0080C0";

        public string Pane1TreeItemHoverBackground { get; set; } = "#FF0080C0";
        public string Pane1TreeItemSelectedBackground { get; set; } = "#FF0080C0";

        private string? _rawTreeText;
        private string? _rawTreeBackground;
        private string? _rawTreeItemHoverBackground;
        private string? _rawTreeItemSelectedBackground;

        private string? _rawPane1TreeItemHoverBackground;
        private string? _rawPane1TreeItemSelectedBackground;

        private string? _friendlyTreeText;
        private string? _friendlyTreeBackground;
        private string? _friendlyTreeItemHoverBackground;
        private string? _friendlyTreeItemSelectedBackground;

        private string? _friendlyPane1TreeItemHoverBackground;
        private string? _friendlyPane1TreeItemSelectedBackground;

        public string RawTreeText
        {
            get => string.IsNullOrWhiteSpace(_rawTreeText) ? TreeText : _rawTreeText;
            set => _rawTreeText = value;
        }

        public string RawTreeBackground
        {
            get => string.IsNullOrWhiteSpace(_rawTreeBackground) ? TreeBackground : _rawTreeBackground;
            set => _rawTreeBackground = value;
        }

        public string RawTreeItemHoverBackground
        {
            get => string.IsNullOrWhiteSpace(_rawTreeItemHoverBackground) ? TreeItemHoverBackground : _rawTreeItemHoverBackground;
            set => _rawTreeItemHoverBackground = value;
        }

        public string RawTreeItemSelectedBackground
        {
            get => string.IsNullOrWhiteSpace(_rawTreeItemSelectedBackground) ? TreeItemSelectedBackground : _rawTreeItemSelectedBackground;
            set => _rawTreeItemSelectedBackground = value;
        }

        public string RawPane1TreeItemHoverBackground
        {
            get => string.IsNullOrWhiteSpace(_rawPane1TreeItemHoverBackground) ? Pane1TreeItemHoverBackground : _rawPane1TreeItemHoverBackground;
            set => _rawPane1TreeItemHoverBackground = value;
        }

        public string RawPane1TreeItemSelectedBackground
        {
            get => string.IsNullOrWhiteSpace(_rawPane1TreeItemSelectedBackground) ? Pane1TreeItemSelectedBackground : _rawPane1TreeItemSelectedBackground;
            set => _rawPane1TreeItemSelectedBackground = value;
        }

        public string FriendlyTreeText
        {
            get => string.IsNullOrWhiteSpace(_friendlyTreeText) ? TreeText : _friendlyTreeText;
            set => _friendlyTreeText = value;
        }

        public string FriendlyTreeBackground
        {
            get => string.IsNullOrWhiteSpace(_friendlyTreeBackground) ? TreeBackground : _friendlyTreeBackground;
            set => _friendlyTreeBackground = value;
        }

        public string FriendlyTreeItemHoverBackground
        {
            get => string.IsNullOrWhiteSpace(_friendlyTreeItemHoverBackground) ? TreeItemHoverBackground : _friendlyTreeItemHoverBackground;
            set => _friendlyTreeItemHoverBackground = value;
        }

        public string FriendlyTreeItemSelectedBackground
        {
            get => string.IsNullOrWhiteSpace(_friendlyTreeItemSelectedBackground) ? TreeItemSelectedBackground : _friendlyTreeItemSelectedBackground;
            set => _friendlyTreeItemSelectedBackground = value;
        }

        public string FriendlyPane1TreeItemHoverBackground
        {
            get => string.IsNullOrWhiteSpace(_friendlyPane1TreeItemHoverBackground) ? Pane1TreeItemHoverBackground : _friendlyPane1TreeItemHoverBackground;
            set => _friendlyPane1TreeItemHoverBackground = value;
        }

        public string FriendlyPane1TreeItemSelectedBackground
        {
            get => string.IsNullOrWhiteSpace(_friendlyPane1TreeItemSelectedBackground) ? Pane1TreeItemSelectedBackground : _friendlyPane1TreeItemSelectedBackground;
            set => _friendlyPane1TreeItemSelectedBackground = value;
        }

        public string GridText { get; set; } = "#FFD4D4D4";
        public string GridBackground { get; set; } = "#FF1E1E1E";
        public string GridBorder { get; set; } = "#FF555555";
        public string GridLines { get; set; } = "#FF555555";
        public string GridHeaderBackground { get; set; } = "#FF1E1E1E";
        public string GridHeaderText { get; set; } = "#FFD4D4D4";
        public string GridRowHoverBackground { get; set; } = "#FF252525";
        public string GridRowSelectedBackground { get; set; } = "#FF2F2F2F";
        public string GridCellSelectedBackground { get; set; } = "#FF2F2F2F";
        public string GridCellSelectedText { get; set; } = "#FFFFFFFF";
        public string SearchMatchBackground { get; set; } = "#FFFFF2CC";
        public string SearchMatchText { get; set; } = "#FF000000";
        public string FieldColumnText { get; set; } = "#FFD4D4D4";
        public string FieldColumnBackground { get; set; } = "#00000000";
        public string ValueColumnText { get; set; } = "#FFD4D4D4";
        public string ValueColumnBackground { get; set; } = "#00000000";
        public string HeaderText { get; set; } = "#FFD4D4D4";
        public string SelectorBackground { get; set; } = "#FF1E1E1E";
        public string Pane2ComboText { get; set; } = "#FFD4D4D4";
        public string Pane2ComboBackground { get; set; } = "#FF1E1E1E";
        public string Pane2DropdownText { get; set; } = "#FFD4D4D4";
        public string Pane2DropdownBackground { get; set; } = "#FF1E1E1E";
        public string Pane2ItemHoverBackground { get; set; } = "#FF252525";
        public string Pane2ItemSelectedBackground { get; set; } = "#FF2F2F2F";


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
                AppearanceWindowBackground = "#FF2F2F2F",
                AppearanceWindowText = "#FFFFFFFF",
                AppearanceWindowControlBackground = "#FF808080",
                AppearanceWindowControlBorder = "#FF2F2F2F",
                AppearanceWindowHeaderText = "#FF0080C0",
                AppearanceWindowHeaderBackground = "#FF1E1E1E",
                AppearanceWindowTabBackground = "#FF2F2F2F",
                AppearanceWindowTabHoverBackground = "#FF0080C0",
                AppearanceWindowTabSelectedBackground = "#FF0080C0",
                AppearanceWindowButtonBackground = "#FF2F2F2F",
                AppearanceWindowButtonHoverBackground = "#FF0080C0",

                SharedConfigPacksWindowBackground = "#FF2F2F2F",
                SharedConfigPacksWindowText = "#FFFFFFFF",
                SharedConfigPacksWindowControlBackground = "#FF2F2F2F",
                SharedConfigPacksWindowControlBorder = "#FFFFFFFF",
                SharedConfigPacksWindowHeaderText = "#FFFFFFFF",
                SharedConfigPacksWindowHeaderBackground = "#FF2F2F2F",
                SharedConfigPacksWindowTabBackground = "#FF2F2F2F",
                SharedConfigPacksWindowTabHoverBackground = "#FF0080C0",
                SharedConfigPacksWindowTabSelectedBackground = "#FF0080C0",
                SharedConfigPacksWindowButtonBackground = "#FF2F2F2F",
                SharedConfigPacksWindowButtonHoverBackground = "#FF0080C0",
                SharedConfigPacksWindowEditsHoverBackground = "#FF0080C0",
                SharedConfigPacksWindowCheckBoxBackground = "#FFFFFFFF",
                SharedConfigPacksWindowCheckBoxTick = "#FF000000",

                CompareXmlWindowBackground = "#FF2F2F2F",
                CompareXmlWindowText = "#FF000000",
                CompareXmlWindowControlBackground = "#FFA1A1A1",
                CompareXmlWindowControlBorder = "#FFC0C0C0",
                CompareXmlWindowHeaderText = "#FFFFFFFF",
                CompareXmlWindowHeaderBackground = "#FF2F2F2F",
                CompareXmlWindowButtonBackground = "#FF808080",
                CompareXmlWindowButtonHoverBackground = "#FF0080C0",
                CompareXmlWindowEditsHoverBackground = "#FF0080C0",
                CompareXmlWindowCheckBoxBackground = "#FFFFFFFF",
                CompareXmlWindowCheckBoxTick = "#FF000000",

                BackupBrowserWindowBackground = "#FF1E1E1E",
                BackupBrowserWindowText = "#FFFFFFFF",
                BackupBrowserWindowControlBackground = "#FF1E1E1E",
                BackupBrowserWindowControlBorder = "#FF555555",
                BackupBrowserWindowHeaderText = "#FFFFFFFF",
                BackupBrowserWindowHeaderBackground = "#FF1E1E1E",
                BackupBrowserWindowListHoverBackground = "#FF0080C0",
                BackupBrowserWindowListSelectedBackground = "#FF0080C0",
                BackupBrowserWindowButtonBackground = "#FF2F2F2F",
                BackupBrowserWindowButtonHoverBackground = "#FF0080C0",
                BackupBrowserWindowXmlFilterHoverBackground = "#FF0080C0",
                BackupBrowserWindowXmlFilterSelectedBackground = "#FF0080C0",
                SavedEditsWindowBackground = "#FF2F2F2F",
                SavedEditsWindowText = "#FF000000",
                SavedEditsWindowControlBackground = "#FFC0C0C0",
                SavedEditsWindowControlBorder = "#FF555555",
                SavedEditsWindowTabBackground = "#FFC0C0C0",
                SavedEditsWindowTabHoverBackground = "#FF0080C0",
                SavedEditsWindowTabSelectedBackground = "#FFFFFFFF",
                SavedEditsWindowButtonBackground = "#FFC0C0C0",
                SavedEditsWindowButtonHoverBackground = "#FF0080C0",
                SavedEditsWindowCheckBoxBackground = "#FFFFFFFF",
                SavedEditsWindowCheckBoxTick = "#FF000000",
                SavedEditsWindowGridText = "#FFFFFFFF",
                SavedEditsWindowGridBackground = "#FF2F2F2F",
                SavedEditsWindowGridBorder = "#FF000000",
                SavedEditsWindowGridLines = "#FFC0C0C0",
                SavedEditsWindowGridHeaderBackground = "#FF2F2F2F",
                SavedEditsWindowGridHeaderText = "#FFFFFFFF",
                SavedEditsWindowGridRowHoverBackground = "#FF0080C0",
                SavedEditsWindowGridRowSelectedBackground = "#FFCCE4FF",
                SavedEditsWindowGridCellSelectedBackground = "#FF0080C0",
                SavedEditsWindowGridCellSelectedText = "#FFFFFFFF",

                SettingsInfoWindowBackground = "#FF1E1E1E",
                SettingsInfoWindowText = "#FFD4D4D4",
                SettingsInfoWindowControlBackground = "#FF1E1E1E",
                SettingsInfoWindowControlBorder = "#FF555555",
                SettingsInfoWindowHeaderText = "#FFD4D4D4",
                SettingsInfoWindowHeaderBackground = "#FF1E1E1E",
                SettingsInfoWindowButtonBackground = "#FF555555",
                SettingsInfoWindowButtonHoverBackground = "#FF0080C0",
                SettingsInfoWindowCheckBoxBackground = "#FF1E1E1E",
                SettingsInfoWindowCheckBoxTick = "#FFD4D4D4",

                DocumentationWindowBackground = "#FF2F2F2F",
                DocumentationWindowText = "#FFFFFFFF",
                DocumentationWindowControlBackground = "#FF2F2F2F",
                DocumentationWindowControlBorder = "#FFC0C0C0",
                DocumentationWindowHeaderText = "#FFFFFFFF",
                DocumentationWindowHeaderBackground = "#FF2F2F2F",
                DocumentationWindowListHoverBackground = "#FF0080C0",
                DocumentationWindowListSelectedBackground = "#FF0080C0",
                DocumentationWindowButtonBackground = "#FF555555",
                DocumentationWindowButtonHoverBackground = "#FF0080C0",

                XmlGuidesWindowBackground = "#FF1E1E1E",
                XmlGuidesWindowText = "#FFFFFFFF",
                XmlGuidesWindowControlBackground = "#FF1E1E1E",
                XmlGuidesWindowControlBorder = "#FF555555",
                XmlGuidesWindowButtonBackground = "#FF2F2F2F",
                XmlGuidesWindowButtonText = "#FFFFFFFF",
                XmlGuidesWindowButtonHoverBackground = "#FF0080C0",
                XmlGuidesWindowButtonHoverText = "#FFFFFFFF",
                XmlGuidesWindowGuidesListBackground = "#FF1E1E1E",
                XmlGuidesWindowGuidesListText = "#FFFFFFFF",
                XmlGuidesWindowGuidesListItemHoverBackground = "#FF0080C0",
                XmlGuidesWindowGuidesListItemHoverText = "#FFFFFFFF",
                XmlGuidesWindowGuidesListItemSelectedBackground = "#FF0080C0",
                XmlGuidesWindowGuidesListItemSelectedText = "#FFFFFFFF",
                XmlGuidesWindowFontPickerText = "#FF000000",

                EditorText = "#FFFFFF00",
                EditorBackground = "#FF000000",
                EditorXmlSyntaxForeground = "#FFC0C0C0",
                EditorScopeShadingColor = "#FF0080C0",
                EditorRegionHighlightColor = "#FFFFFFFF",

                MenuBackground = "#FF1E1E1E",
                MenuText = "#FFFFFFFF",
                TopButtonText = "#FFFFFFFF",
                TopButtonBackground = "#FF2F2F2F",

                TreeText = "#FFD4D4D4",
                TreeBackground = "#FF1E1E1E",
                TreeItemHoverBackground = "#FF0080C0",
                TreeItemSelectedBackground = "#FF0080C0",
                FriendlyTreeItemHoverBackground = "#FF0080C0",
                FriendlyTreeItemSelectedBackground = "#FF0080C0",

                GridText = "#FFFFFFFF",
                GridBackground = "#FF1E1E1E",
                GridBorder = "#FFFFFFFF",
                GridLines = "#FFFFFFFF",
                GridHeaderBackground = "#FF1E1E1E",
                GridHeaderText = "#FFFFFFFF",

                GridRowHoverBackground = "#FF0080C0",
                GridRowSelectedBackground = "#FF0080C0",

                GridCellSelectedBackground = "#FF0080C0",
                GridCellSelectedText = "#FFFFFFFF",
                SearchMatchBackground = "#FF0080C0",
                SearchMatchText = "#FF000000",
                Pane2ItemHoverBackground = "#FF0080C0",
                Pane2ItemSelectedBackground = "#FF0080C0",
                Pane2ComboText = "#FFFFFFFF",
                Pane2DropdownText = "#FFFFFFFF",                

                FieldColumnText = "#FFFFFFFF",
                FieldColumnBackground = "#FF252525",
                ValueColumnText = "#FFFFFF00",
                ValueColumnBackground = "#FF252525",
                HeaderText = "#FFFFFFFF",
                SelectorBackground = "#FF1E1E1E"
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
                AppearanceWindowBackground = "#FFFFFFFF",
                AppearanceWindowText = "#FF000000",
                AppearanceWindowControlBackground = "#FFFFFFFF",
                AppearanceWindowControlBorder = "#FF000000",
                AppearanceWindowHeaderText = "#FF000000",
                AppearanceWindowHeaderBackground = "#FFFFFFFF",
                AppearanceWindowTabBackground = "#FFC0C0C0",
                AppearanceWindowTabHoverBackground = "#FFCCE4FF",
                AppearanceWindowTabSelectedBackground = "#FFCCE4FF",
                AppearanceWindowButtonBackground = "#FFC0C0C0",
                AppearanceWindowButtonHoverBackground = "#FFCCE4FF",

                SharedConfigPacksWindowBackground = "#FFFFFFFF",
                SharedConfigPacksWindowText = "#FF000000",
                SharedConfigPacksWindowControlBackground = "#FFFFFFFF",
                SharedConfigPacksWindowControlBorder = "#FF000000",
                SharedConfigPacksWindowHeaderText = "#FF000000",
                SharedConfigPacksWindowHeaderBackground = "#FFFFFFFF",
                SharedConfigPacksWindowTabBackground = "#FFC0C0C0",
                SharedConfigPacksWindowTabHoverBackground = "#FFCCE4FF",
                SharedConfigPacksWindowTabSelectedBackground = "#FFCCE4FF",
                SharedConfigPacksWindowButtonBackground = "#FFC0C0C0",
                SharedConfigPacksWindowButtonHoverBackground = "#FFCCE4FF",
                SharedConfigPacksWindowEditsHoverBackground = "#FFCCE4FF",
                SharedConfigPacksWindowCheckBoxBackground = "#FFFFFFFF",
                SharedConfigPacksWindowCheckBoxTick = "#FF000000",

                CompareXmlWindowBackground = "#FFFFFFFF",
                CompareXmlWindowText = "#FF000000",
                CompareXmlWindowControlBackground = "#FFFFFFFF",
                CompareXmlWindowControlBorder = "#FF000000",
                CompareXmlWindowHeaderText = "#FF000000",
                CompareXmlWindowHeaderBackground = "#FFFFFFFF",
                CompareXmlWindowButtonBackground = "#FFFFFFFF",
                CompareXmlWindowButtonHoverBackground = "#FFCCE4FF",
                CompareXmlWindowEditsHoverBackground = "#FFCCE4FF",
                CompareXmlWindowCheckBoxBackground = "#FFFFFFFF",
                CompareXmlWindowCheckBoxTick = "#FF000000",

                BackupBrowserWindowBackground = "#FFFFFFFF",
                BackupBrowserWindowText = "#FF000000",
                BackupBrowserWindowControlBackground = "#FFFFFFFF",
                BackupBrowserWindowControlBorder = "#FF000000",
                BackupBrowserWindowHeaderText = "#FF000000",
                BackupBrowserWindowHeaderBackground = "#FFFFFFFF",
                BackupBrowserWindowListHoverBackground = "#FFCCE4FF",
                BackupBrowserWindowListSelectedBackground = "#FFCCE4FF",
                BackupBrowserWindowButtonBackground = "#FFFFFFFF",
                BackupBrowserWindowButtonHoverBackground = "#FFCCE4FF",
                BackupBrowserWindowXmlFilterHoverBackground = "#FFCCE4FF",
                BackupBrowserWindowXmlFilterSelectedBackground = "#FFCCE4FF",

                SavedEditsWindowBackground = "#FFFFFFFF",
                SavedEditsWindowText = "#FF000000",
                SavedEditsWindowControlBackground = "#FFFFFFFF",
                SavedEditsWindowControlBorder = "#FF000000",
                SavedEditsWindowTabBackground = "#FFC0C0C0",
                SavedEditsWindowTabHoverBackground = "#FFCCE4FF",
                SavedEditsWindowTabSelectedBackground = "#FFCCE4FF",
                SavedEditsWindowButtonBackground = "#FFFFFFFF",
                SavedEditsWindowButtonHoverBackground = "#FFCCE4FF",
                SavedEditsWindowCheckBoxBackground = "#FFFFFFFF",
                SavedEditsWindowCheckBoxTick = "#FF000000",
                SavedEditsWindowGridText = "#FF000000",
                SavedEditsWindowGridBackground = "#FFFFFFFF",
                SavedEditsWindowGridBorder = "#FF000000",
                SavedEditsWindowGridLines = "#FF000000",
                SavedEditsWindowGridHeaderBackground = "#FFFFFFFF",
                SavedEditsWindowGridHeaderText = "#FF000000",
                SavedEditsWindowGridRowHoverBackground = "#FFCCE4FF",
                SavedEditsWindowGridRowSelectedBackground = "#FFCCE4FF",
                SavedEditsWindowGridCellSelectedBackground = "#FFCCE4FF",
                SavedEditsWindowGridCellSelectedText = "#FF000000",

                SettingsInfoWindowBackground = "#FFFFFFFF",
                SettingsInfoWindowText = "#FF000000",
                SettingsInfoWindowControlBackground = "#FFFFFFFF",
                SettingsInfoWindowControlBorder = "#FF000000",
                SettingsInfoWindowHeaderText = "#FF000000",
                SettingsInfoWindowHeaderBackground = "#FFFFFFFF",
                SettingsInfoWindowButtonBackground = "#FFC0C0C0",
                SettingsInfoWindowButtonHoverBackground = "#FFCCE4FF",
                SettingsInfoWindowCheckBoxBackground = "#FFFFFFFF",
                SettingsInfoWindowCheckBoxTick = "#FF000000",

                DocumentationWindowBackground = "#FFFFFFFF",
                DocumentationWindowText = "#FF000000",
                DocumentationWindowControlBackground = "#FFFFFFFF",
                DocumentationWindowControlBorder = "#FF000000",
                DocumentationWindowHeaderText = "#FF000000",
                DocumentationWindowHeaderBackground = "#FFFFFFFF",
                DocumentationWindowListHoverBackground = "#FFCCE4FF",
                DocumentationWindowListSelectedBackground = "#FFCCE4FF",
                DocumentationWindowButtonBackground = "#FFC0C0C0",
                DocumentationWindowButtonHoverBackground = "#FFCCE4FF",

                XmlGuidesWindowBackground = "#FFFFFFFF",
                XmlGuidesWindowText = "#FF000000",
                XmlGuidesWindowControlBackground = "#FFFFFFFF",
                XmlGuidesWindowControlBorder = "#FF000000",
                XmlGuidesWindowButtonBackground = "#FFC0C0C0",
                XmlGuidesWindowButtonText = "#FF000000",
                XmlGuidesWindowButtonHoverBackground = "#FFCCE4FF",
                XmlGuidesWindowButtonHoverText = "#FF000000",
                XmlGuidesWindowGuidesListBackground = "#FFFFFFFF",
                XmlGuidesWindowGuidesListText = "#FF000000",
                XmlGuidesWindowGuidesListItemHoverBackground = "#FFCCE4FF",
                XmlGuidesWindowGuidesListItemHoverText = "#FF000000",
                XmlGuidesWindowGuidesListItemSelectedBackground = "#FFCCE4FF",
                XmlGuidesWindowGuidesListItemSelectedText = "#FF000000",
                XmlGuidesWindowFontPickerText = "#FF000000",

                EditorText = "#FF0000FF",
                EditorBackground = "#FFFFFFFF",
                EditorXmlSyntaxForeground = "#FF000000",
                EditorScopeShadingColor = "#FF008080",
                EditorRegionHighlightColor = "#FF000000",

                MenuBackground = "#FFFFFFFF",
                MenuText = "#FF000000",
                TopButtonText = "#FF000000",
                TopButtonBackground = "#FFFFFFFF",

                TreeText = "#FF000000",
                TreeBackground = "#FFFFFFFF",
                TreeItemHoverBackground = "#FFEAEAEA",
                TreeItemSelectedBackground = "#FFCCE4FF",
                FriendlyTreeItemHoverBackground = "#FFCCE4FF",
                FriendlyPane1TreeItemHoverBackground = "#FFCCE4FF",
                FriendlyPane1TreeItemSelectedBackground = "#FFCCE4FF",

                GridText = "#FF000000",
                GridBackground = "#FFFFFFFF",
                GridBorder = "#FF555555",
                GridHeaderBackground = "#FFF5F5F5",
                GridHeaderText = "#FF000000",

                GridRowHoverBackground = "#FFCCE4FF",
                GridRowSelectedBackground = "#FFCCE4FF",

                GridCellSelectedBackground = "#FFCCE4FF",
                GridCellSelectedText = "#FFFFFFFF",
                SearchMatchBackground = "#FFCCE4FF",
                SearchMatchText = "#FF0000FF",
                Pane2ComboText = "#FF000000",
                Pane2ComboBackground = "#FFFFFFFF",
                Pane2DropdownText = "#FF000000",
                Pane2DropdownBackground = "#FFFFFFFF",
                Pane2ItemHoverBackground = "#FFCCE4FF",
                Pane2ItemSelectedBackground = "#FFCCE4FF",

                FieldColumnText = "#FF000000",
                FieldColumnBackground = "#FFFFFFFF",
                ValueColumnText = "#FFFF0000",
                ValueColumnBackground = "#FFFFFFFF",
                HeaderText = "#FF000000",
                SelectorBackground = "#FFFFFFFF"
            };
        }
    }
}
