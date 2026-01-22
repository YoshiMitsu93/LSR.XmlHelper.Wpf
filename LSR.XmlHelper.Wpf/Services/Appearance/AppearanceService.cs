using LSR.XmlHelper.Wpf.Infrastructure;
using System;
using System.Windows;
using System.Windows.Media;

using WpfBrush = System.Windows.Media.Brush;
using WpfBrushes = System.Windows.Media.Brushes;
using WpfFontFamily = System.Windows.Media.FontFamily;
using WpfFontStyle = System.Windows.FontStyle;

namespace LSR.XmlHelper.Wpf.Services
{
    public sealed class AppearanceService : ObservableObject
    {
        private readonly AppearanceSettings _settings;
        private bool _isDarkMode;
        private bool _isFriendlyView;

        public AppearanceService(AppearanceSettings settings, bool isDarkMode, bool isFriendlyView)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _isDarkMode = isDarkMode;
            _isFriendlyView = isFriendlyView;
        }

        public AppearanceSettings Settings => _settings;

        public bool IsDarkMode
        {
            get => _isDarkMode;
            set
            {
                if (!SetProperty(ref _isDarkMode, value))
                    return;

                RaiseAllAppearanceChanged();
            }
        }

        public bool IsFriendlyView
        {
            get => _isFriendlyView;
            set
            {
                if (!SetProperty(ref _isFriendlyView, value))
                    return;

                RaiseAllAppearanceChanged();
            }
        }

        private AppearanceProfileSettings Active => _settings.GetActiveProfile(_isDarkMode);

        public WpfFontFamily UiFontFamily => new WpfFontFamily(Active.UiFontFamily);
        public double UiFontSize => Active.UiFontSize;
        public FontWeight UiFontWeight => Active.UiFontBold ? FontWeights.Bold : FontWeights.Normal;
        public WpfFontStyle UiFontStyle => Active.UiFontItalic ? FontStyles.Italic : FontStyles.Normal;
        public WpfFontFamily EditorFontFamily => new WpfFontFamily(Active.EditorFontFamily);
        public double EditorFontSize => Active.EditorFontSize;
        public FontWeight EditorFontWeight => Active.EditorFontBold ? FontWeights.Bold : FontWeights.Normal;
        public WpfFontStyle EditorFontStyle => Active.EditorFontItalic ? FontStyles.Italic : FontStyles.Normal;
        public WpfBrush TextBrush => CreateFrozenBrush(Active.Text);
        public WpfBrush BackgroundBrush => CreateFrozenBrush(Active.Background);
        public WpfBrush AppearanceWindowBackgroundBrush => CreateFrozenBrush(Active.AppearanceWindowBackground);
        public WpfBrush AppearanceWindowTextBrush => CreateFrozenBrush(Active.AppearanceWindowText);
        public WpfBrush AppearanceWindowControlBackgroundBrush => CreateFrozenBrush(Active.AppearanceWindowControlBackground);
        public WpfBrush AppearanceWindowControlBorderBrush => CreateFrozenBrush(Active.AppearanceWindowControlBorder);
        public WpfBrush AppearanceWindowHeaderTextBrush => CreateFrozenBrush(Active.AppearanceWindowHeaderText);
        public WpfBrush AppearanceWindowHeaderBackgroundBrush => CreateFrozenBrush(Active.AppearanceWindowHeaderBackground);
        public WpfBrush AppearanceWindowTabBackgroundBrush => CreateFrozenBrush(Active.AppearanceWindowTabBackground);
        public WpfBrush AppearanceWindowTabHoverBackgroundBrush => CreateFrozenBrush(Active.AppearanceWindowTabHoverBackground);
        public WpfBrush AppearanceWindowTabSelectedBackgroundBrush => CreateFrozenBrush(Active.AppearanceWindowTabSelectedBackground);
        public WpfBrush AppearanceWindowButtonBackgroundBrush => CreateFrozenBrush(Active.AppearanceWindowButtonBackground);
        public WpfBrush AppearanceWindowButtonHoverBackgroundBrush => CreateFrozenBrush(Active.AppearanceWindowButtonHoverBackground);
        public WpfBrush SharedConfigPacksWindowBackgroundBrush => CreateFrozenBrush(Active.SharedConfigPacksWindowBackground);
        public WpfBrush SharedConfigPacksWindowTextBrush => CreateFrozenBrush(Active.SharedConfigPacksWindowText);
        public WpfBrush SharedConfigPacksWindowControlBackgroundBrush => CreateFrozenBrush(Active.SharedConfigPacksWindowControlBackground);
        public WpfBrush SharedConfigPacksWindowControlBorderBrush => CreateFrozenBrush(Active.SharedConfigPacksWindowControlBorder);
        public WpfBrush SharedConfigPacksWindowHeaderTextBrush => CreateFrozenBrush(Active.SharedConfigPacksWindowHeaderText);
        public WpfBrush SharedConfigPacksWindowHeaderBackgroundBrush => CreateFrozenBrush(Active.SharedConfigPacksWindowHeaderBackground);
        public WpfBrush SharedConfigPacksWindowTabBackgroundBrush => CreateFrozenBrush(Active.SharedConfigPacksWindowTabBackground);
        public WpfBrush SharedConfigPacksWindowTabHoverBackgroundBrush => CreateFrozenBrush(Active.SharedConfigPacksWindowTabHoverBackground);
        public WpfBrush SharedConfigPacksWindowTabSelectedBackgroundBrush => CreateFrozenBrush(Active.SharedConfigPacksWindowTabSelectedBackground);
        public WpfBrush SharedConfigPacksWindowButtonBackgroundBrush => CreateFrozenBrush(Active.SharedConfigPacksWindowButtonBackground);
        public WpfBrush SharedConfigPacksWindowButtonHoverBackgroundBrush => CreateFrozenBrush(Active.SharedConfigPacksWindowButtonHoverBackground);
        public WpfBrush SharedConfigPacksWindowEditsHoverBackgroundBrush => CreateFrozenBrush(Active.SharedConfigPacksWindowEditsHoverBackground);
        public WpfBrush SharedConfigPacksWindowCheckBoxBackgroundBrush => CreateFrozenBrush(Active.SharedConfigPacksWindowCheckBoxBackground);
        public WpfBrush SharedConfigPacksWindowCheckBoxTickBrush => CreateFrozenBrush(Active.SharedConfigPacksWindowCheckBoxTick);
        public WpfBrush CompareXmlWindowBackgroundBrush => CreateFrozenBrush(Active.CompareXmlWindowBackground);
        public WpfBrush CompareXmlWindowTextBrush => CreateFrozenBrush(Active.CompareXmlWindowText);
        public WpfBrush CompareXmlWindowControlBackgroundBrush => CreateFrozenBrush(Active.CompareXmlWindowControlBackground);
        public WpfBrush CompareXmlWindowControlBorderBrush => CreateFrozenBrush(Active.CompareXmlWindowControlBorder);
        public WpfBrush CompareXmlWindowHeaderTextBrush => CreateFrozenBrush(Active.CompareXmlWindowHeaderText);
        public WpfBrush CompareXmlWindowHeaderBackgroundBrush => CreateFrozenBrush(Active.CompareXmlWindowHeaderBackground);
        public WpfBrush CompareXmlWindowButtonBackgroundBrush => CreateFrozenBrush(Active.CompareXmlWindowButtonBackground);
        public WpfBrush CompareXmlWindowButtonHoverBackgroundBrush => CreateFrozenBrush(Active.CompareXmlWindowButtonHoverBackground);
        public WpfBrush CompareXmlWindowEditsHoverBackgroundBrush => CreateFrozenBrush(Active.CompareXmlWindowEditsHoverBackground);
        public WpfBrush CompareXmlWindowCheckBoxBackgroundBrush => CreateFrozenBrush(Active.CompareXmlWindowCheckBoxBackground);
        public WpfBrush CompareXmlWindowCheckBoxTickBrush => CreateFrozenBrush(Active.CompareXmlWindowCheckBoxTick);
        public WpfBrush BackupBrowserWindowBackgroundBrush => CreateFrozenBrush(Active.BackupBrowserWindowBackground);
        public WpfBrush BackupBrowserWindowTextBrush => CreateFrozenBrush(Active.BackupBrowserWindowText);
        public WpfBrush BackupBrowserWindowControlBackgroundBrush => CreateFrozenBrush(Active.BackupBrowserWindowControlBackground);
        public WpfBrush BackupBrowserWindowControlBorderBrush => CreateFrozenBrush(Active.BackupBrowserWindowControlBorder);
        public WpfBrush BackupBrowserWindowHeaderTextBrush => CreateFrozenBrush(Active.BackupBrowserWindowHeaderText);
        public WpfBrush BackupBrowserWindowHeaderBackgroundBrush => CreateFrozenBrush(Active.BackupBrowserWindowHeaderBackground);
        public WpfBrush BackupBrowserWindowListHoverBackgroundBrush => CreateFrozenBrush(Active.BackupBrowserWindowListHoverBackground);
        public WpfBrush BackupBrowserWindowListSelectedBackgroundBrush => CreateFrozenBrush(Active.BackupBrowserWindowListSelectedBackground);
        public WpfBrush BackupBrowserWindowButtonBackgroundBrush => CreateFrozenBrush(Active.BackupBrowserWindowButtonBackground);
        public WpfBrush BackupBrowserWindowButtonHoverBackgroundBrush => CreateFrozenBrush(Active.BackupBrowserWindowButtonHoverBackground);
        public WpfBrush BackupBrowserWindowXmlFilterHoverBackgroundBrush => CreateFrozenBrush(Active.BackupBrowserWindowXmlFilterHoverBackground);
        public WpfBrush BackupBrowserWindowXmlFilterSelectedBackgroundBrush => CreateFrozenBrush(Active.BackupBrowserWindowXmlFilterSelectedBackground);
        public WpfBrush SavedEditsWindowBackgroundBrush => CreateFrozenBrush(Active.SavedEditsWindowBackground);
        public WpfBrush SavedEditsWindowTextBrush => CreateFrozenBrush(Active.SavedEditsWindowText);
        public WpfBrush SavedEditsWindowControlBackgroundBrush => CreateFrozenBrush(Active.SavedEditsWindowControlBackground);
        public WpfBrush SavedEditsWindowControlBorderBrush => CreateFrozenBrush(Active.SavedEditsWindowControlBorder);
        public WpfBrush SavedEditsWindowTabBackgroundBrush => CreateFrozenBrush(Active.SavedEditsWindowTabBackground);
        public WpfBrush SavedEditsWindowTabHoverBackgroundBrush => CreateFrozenBrush(Active.SavedEditsWindowTabHoverBackground);
        public WpfBrush SavedEditsWindowTabSelectedBackgroundBrush => CreateFrozenBrush(Active.SavedEditsWindowTabSelectedBackground);
        public WpfBrush SavedEditsWindowButtonBackgroundBrush => CreateFrozenBrush(Active.SavedEditsWindowButtonBackground);
        public WpfBrush SavedEditsWindowButtonHoverBackgroundBrush => CreateFrozenBrush(Active.SavedEditsWindowButtonHoverBackground);
        public WpfBrush SavedEditsWindowCheckBoxBackgroundBrush => CreateFrozenBrush(Active.SavedEditsWindowCheckBoxBackground);
        public WpfBrush SavedEditsWindowCheckBoxTickBrush => CreateFrozenBrush(Active.SavedEditsWindowCheckBoxTick);
        public WpfBrush SavedEditsWindowGridTextBrush => CreateFrozenBrush(Active.SavedEditsWindowGridText);
        public WpfBrush SavedEditsWindowGridBackgroundBrush => CreateFrozenBrush(Active.SavedEditsWindowGridBackground);
        public WpfBrush SavedEditsWindowGridBorderBrush => CreateFrozenBrush(Active.SavedEditsWindowGridBorder);
        public WpfBrush SavedEditsWindowGridLinesBrush => CreateFrozenBrush(Active.SavedEditsWindowGridLines);
        public WpfBrush SavedEditsWindowGridHeaderBackgroundBrush => CreateFrozenBrush(Active.SavedEditsWindowGridHeaderBackground);
        public WpfBrush SavedEditsWindowGridHeaderTextBrush => CreateFrozenBrush(Active.SavedEditsWindowGridHeaderText);
        public WpfBrush SavedEditsWindowGridRowHoverBackgroundBrush => CreateFrozenBrush(Active.SavedEditsWindowGridRowHoverBackground);
        public WpfBrush SavedEditsWindowGridRowSelectedBackgroundBrush => CreateFrozenBrush(Active.SavedEditsWindowGridRowSelectedBackground);
        public WpfBrush SavedEditsWindowGridCellSelectedBackgroundBrush => CreateFrozenBrush(Active.SavedEditsWindowGridCellSelectedBackground);
        public WpfBrush SavedEditsWindowGridCellSelectedTextBrush => CreateFrozenBrush(Active.SavedEditsWindowGridCellSelectedText);
        public WpfBrush EditorTextBrush => CreateFrozenBrush(Active.EditorText);
        public WpfBrush SettingsInfoWindowBackgroundBrush => CreateFrozenBrush(Active.SettingsInfoWindowBackground);
        public WpfBrush SettingsInfoWindowTextBrush => CreateFrozenBrush(Active.SettingsInfoWindowText);
        public WpfBrush SettingsInfoWindowControlBackgroundBrush => CreateFrozenBrush(Active.SettingsInfoWindowControlBackground);
        public WpfBrush SettingsInfoWindowControlBorderBrush => CreateFrozenBrush(Active.SettingsInfoWindowControlBorder);
        public WpfBrush SettingsInfoWindowHeaderTextBrush => CreateFrozenBrush(Active.SettingsInfoWindowHeaderText);
        public WpfBrush SettingsInfoWindowHeaderBackgroundBrush => CreateFrozenBrush(Active.SettingsInfoWindowHeaderBackground);
        public WpfBrush SettingsInfoWindowButtonBackgroundBrush => CreateFrozenBrush(Active.SettingsInfoWindowButtonBackground);
        public WpfBrush SettingsInfoWindowButtonHoverBackgroundBrush => CreateFrozenBrush(Active.SettingsInfoWindowButtonHoverBackground);
        public WpfBrush SettingsInfoWindowCheckBoxBackgroundBrush => CreateFrozenBrush(Active.SettingsInfoWindowCheckBoxBackground);
        public WpfBrush SettingsInfoWindowCheckBoxTickBrush => CreateFrozenBrush(Active.SettingsInfoWindowCheckBoxTick);
        public WpfBrush DocumentationWindowBackgroundBrush => CreateFrozenBrush(Active.DocumentationWindowBackground);
        public WpfBrush DocumentationWindowTextBrush => CreateFrozenBrush(Active.DocumentationWindowText);
        public WpfBrush DocumentationWindowControlBackgroundBrush => CreateFrozenBrush(Active.DocumentationWindowControlBackground);
        public WpfBrush DocumentationWindowControlBorderBrush => CreateFrozenBrush(Active.DocumentationWindowControlBorder);
        public WpfBrush DocumentationWindowHeaderTextBrush => CreateFrozenBrush(Active.DocumentationWindowHeaderText);
        public WpfBrush DocumentationWindowHeaderBackgroundBrush => CreateFrozenBrush(Active.DocumentationWindowHeaderBackground);
        public WpfBrush DocumentationWindowListHoverBackgroundBrush => CreateFrozenBrush(Active.DocumentationWindowListHoverBackground);
        public WpfBrush DocumentationWindowListSelectedBackgroundBrush => CreateFrozenBrush(Active.DocumentationWindowListSelectedBackground);
        public WpfBrush DocumentationWindowButtonBackgroundBrush => CreateFrozenBrush(Active.DocumentationWindowButtonBackground);
        public WpfBrush DocumentationWindowButtonHoverBackgroundBrush => CreateFrozenBrush(Active.DocumentationWindowButtonHoverBackground);
        public WpfBrush XmlGuidesWindowBackgroundBrush => CreateFrozenBrush(Active.XmlGuidesWindowBackground);
        public WpfBrush XmlGuidesWindowTextBrush => CreateFrozenBrush(Active.XmlGuidesWindowText);
        public WpfBrush XmlGuidesWindowControlBackgroundBrush => CreateFrozenBrush(Active.XmlGuidesWindowControlBackground);
        public WpfBrush XmlGuidesWindowControlBorderBrush => CreateFrozenBrush(Active.XmlGuidesWindowControlBorder);
        public WpfBrush XmlGuidesWindowButtonBackgroundBrush => CreateFrozenBrush(Active.XmlGuidesWindowButtonBackground);
        public WpfBrush XmlGuidesWindowButtonTextBrush => CreateFrozenBrush(Active.XmlGuidesWindowButtonText);
        public WpfBrush XmlGuidesWindowButtonHoverBackgroundBrush => CreateFrozenBrush(Active.XmlGuidesWindowButtonHoverBackground);
        public WpfBrush XmlGuidesWindowButtonHoverTextBrush => CreateFrozenBrush(Active.XmlGuidesWindowButtonHoverText);
        public WpfBrush XmlGuidesWindowGuidesListBackgroundBrush => CreateFrozenBrush(Active.XmlGuidesWindowGuidesListBackground);
        public WpfBrush XmlGuidesWindowGuidesListTextBrush => CreateFrozenBrush(Active.XmlGuidesWindowGuidesListText);
        public WpfBrush XmlGuidesWindowGuidesListItemHoverBackgroundBrush => CreateFrozenBrush(Active.XmlGuidesWindowGuidesListItemHoverBackground);
        public WpfBrush XmlGuidesWindowGuidesListItemHoverTextBrush => CreateFrozenBrush(Active.XmlGuidesWindowGuidesListItemHoverText);
        public WpfBrush XmlGuidesWindowGuidesListItemSelectedBackgroundBrush => CreateFrozenBrush(Active.XmlGuidesWindowGuidesListItemSelectedBackground);
        public WpfBrush XmlGuidesWindowGuidesListItemSelectedTextBrush => CreateFrozenBrush(Active.XmlGuidesWindowGuidesListItemSelectedText);
        public WpfBrush XmlGuidesWindowFontPickerTextBrush => CreateFrozenBrush(Active.XmlGuidesWindowFontPickerText);
        public WpfBrush EditorBackgroundBrush => CreateFrozenBrush(Active.EditorBackground);
        public string EditorXmlSyntaxForeground => Active.EditorXmlSyntaxForeground;
        public string EditorScopeShadingColor => Active.EditorScopeShadingColor;
        public string EditorIndentGuidesColor => Active.EditorIndentGuidesColor;
        public string EditorRegionHighlightColor => Active.EditorRegionHighlightColor;
        public WpfBrush MenuBackgroundBrush => CreateFrozenBrush(Active.MenuBackground);
        public WpfBrush MenuTextBrush => CreateFrozenBrush(Active.MenuText);
        public WpfBrush TreeTextBrush => CreateFrozenBrush(_isFriendlyView ? Active.FriendlyTreeText : Active.RawTreeText);
        public WpfBrush TreeBackgroundBrush => CreateFrozenBrush(_isFriendlyView ? Active.FriendlyTreeBackground : Active.RawTreeBackground);
        public WpfBrush TreeItemHoverBackgroundBrush => CreateFrozenBrush(_isFriendlyView ? Active.FriendlyTreeItemHoverBackground : Active.RawTreeItemHoverBackground);
        public WpfBrush TreeItemSelectedBackgroundBrush => CreateFrozenBrush(_isFriendlyView ? Active.FriendlyTreeItemSelectedBackground : Active.RawTreeItemSelectedBackground);
        public WpfBrush Pane1TreeItemHoverBackgroundBrush => CreateFrozenBrush(_isFriendlyView ? Active.FriendlyPane1TreeItemHoverBackground : Active.RawPane1TreeItemHoverBackground);
        public WpfBrush Pane1TreeItemSelectedBackgroundBrush => CreateFrozenBrush(_isFriendlyView ? Active.FriendlyPane1TreeItemSelectedBackground : Active.RawPane1TreeItemSelectedBackground);
        public WpfBrush GridTextBrush => CreateFrozenBrush(Active.GridText);
        public WpfBrush GridBackgroundBrush => CreateFrozenBrush(Active.GridBackground);
        public WpfBrush GridBorderBrush => CreateFrozenBrush(Active.GridBorder);
        public WpfBrush GridLinesBrush => CreateFrozenBrush(Active.GridLines);
        public WpfBrush GridHeaderBackgroundBrush => CreateFrozenBrush(Active.GridHeaderBackground);
        public WpfBrush GridHeaderTextBrush => CreateFrozenBrush(Active.GridHeaderText);
        public WpfBrush GridRowHoverBackgroundBrush => CreateFrozenBrush(Active.GridRowHoverBackground);
        public WpfBrush GridRowSelectedBackgroundBrush => CreateFrozenBrush(Active.GridRowSelectedBackground);
        public WpfBrush GridCellSelectedBackgroundBrush => CreateFrozenBrush(Active.GridCellSelectedBackground);
        public WpfBrush SearchMatchBackgroundBrush => CreateFrozenBrush(Active.SearchMatchBackground);
        public WpfBrush SearchMatchTextBrush => CreateFrozenBrush(Active.SearchMatchText);
        public WpfBrush FieldColumnTextBrush => CreateFrozenBrush(Active.FieldColumnText);
        public WpfBrush FieldColumnBackgroundBrush => CreateFrozenBrush(Active.FieldColumnBackground);
        public WpfBrush ValueColumnTextBrush => CreateFrozenBrush(Active.ValueColumnText);
        public WpfBrush ValueColumnBackgroundBrush => CreateFrozenBrush(Active.ValueColumnBackground);
        public WpfBrush HeaderTextBrush => CreateFrozenBrush(Active.HeaderText);
        public WpfBrush SelectorBackgroundBrush => CreateFrozenBrush(Active.SelectorBackground);
        public WpfBrush Pane2ComboTextBrush => CreateFrozenBrush(Active.Pane2ComboText);
        public WpfBrush Pane2ComboBackgroundBrush => CreateFrozenBrush(Active.Pane2ComboBackground);
        public WpfBrush Pane2DropdownTextBrush => CreateFrozenBrush(Active.Pane2DropdownText);
        public WpfBrush Pane2DropdownBackgroundBrush => CreateFrozenBrush(Active.Pane2DropdownBackground);
        public WpfBrush Pane2ItemHoverBackgroundBrush => CreateFrozenBrush(Active.Pane2ItemHoverBackground);
        public WpfBrush Pane2ItemSelectedBackgroundBrush => CreateFrozenBrush(Active.Pane2ItemSelectedBackground);
        public WpfBrush TopButtonTextBrush => CreateFrozenBrush(Active.TopButtonText);
        public WpfBrush TopButtonBackgroundBrush => CreateFrozenBrush(Active.TopButtonBackground);

        public void ReplaceSettings(AppearanceSettings newSettings)
        {
            if (newSettings is null)
                throw new ArgumentNullException(nameof(newSettings));

            _settings.Dark = newSettings.Dark;
            _settings.Light = newSettings.Light;

            RaiseAllAppearanceChanged();
        }

        private void RaiseAllAppearanceChanged()
        {
            OnPropertyChanged(nameof(UiFontFamily));
            OnPropertyChanged(nameof(UiFontSize));
            OnPropertyChanged(nameof(UiFontWeight));
            OnPropertyChanged(nameof(UiFontStyle));

            OnPropertyChanged(nameof(EditorFontFamily));
            OnPropertyChanged(nameof(EditorFontSize));
            OnPropertyChanged(nameof(EditorFontWeight));
            OnPropertyChanged(nameof(EditorFontStyle));

            OnPropertyChanged(nameof(TextBrush));
            OnPropertyChanged(nameof(BackgroundBrush));
            OnPropertyChanged(nameof(AppearanceWindowBackgroundBrush));
            OnPropertyChanged(nameof(AppearanceWindowTextBrush));
            OnPropertyChanged(nameof(AppearanceWindowControlBackgroundBrush));
            OnPropertyChanged(nameof(AppearanceWindowControlBorderBrush));
            OnPropertyChanged(nameof(AppearanceWindowHeaderTextBrush));
            OnPropertyChanged(nameof(AppearanceWindowHeaderBackgroundBrush));
            OnPropertyChanged(nameof(AppearanceWindowTabBackgroundBrush));
            OnPropertyChanged(nameof(AppearanceWindowTabHoverBackgroundBrush));
            OnPropertyChanged(nameof(AppearanceWindowTabSelectedBackgroundBrush));
            OnPropertyChanged(nameof(AppearanceWindowButtonBackgroundBrush));
            OnPropertyChanged(nameof(AppearanceWindowButtonHoverBackgroundBrush));

            OnPropertyChanged(nameof(SharedConfigPacksWindowBackgroundBrush));
            OnPropertyChanged(nameof(SharedConfigPacksWindowTextBrush));
            OnPropertyChanged(nameof(SharedConfigPacksWindowControlBackgroundBrush));
            OnPropertyChanged(nameof(SharedConfigPacksWindowControlBorderBrush));
            OnPropertyChanged(nameof(SharedConfigPacksWindowHeaderTextBrush));
            OnPropertyChanged(nameof(SharedConfigPacksWindowHeaderBackgroundBrush));
            OnPropertyChanged(nameof(SharedConfigPacksWindowTabBackgroundBrush));
            OnPropertyChanged(nameof(SharedConfigPacksWindowTabHoverBackgroundBrush));
            OnPropertyChanged(nameof(SharedConfigPacksWindowTabSelectedBackgroundBrush));
            OnPropertyChanged(nameof(SharedConfigPacksWindowButtonBackgroundBrush));
            OnPropertyChanged(nameof(SharedConfigPacksWindowButtonHoverBackgroundBrush));
            OnPropertyChanged(nameof(SharedConfigPacksWindowEditsHoverBackgroundBrush));
            OnPropertyChanged(nameof(SharedConfigPacksWindowCheckBoxBackgroundBrush));
            OnPropertyChanged(nameof(SharedConfigPacksWindowCheckBoxTickBrush));
            OnPropertyChanged(nameof(CompareXmlWindowBackgroundBrush));
            OnPropertyChanged(nameof(CompareXmlWindowTextBrush));
            OnPropertyChanged(nameof(CompareXmlWindowControlBackgroundBrush));
            OnPropertyChanged(nameof(CompareXmlWindowControlBorderBrush));
            OnPropertyChanged(nameof(CompareXmlWindowHeaderTextBrush));
            OnPropertyChanged(nameof(CompareXmlWindowHeaderBackgroundBrush));
            OnPropertyChanged(nameof(CompareXmlWindowButtonBackgroundBrush));
            OnPropertyChanged(nameof(CompareXmlWindowButtonHoverBackgroundBrush));
            OnPropertyChanged(nameof(CompareXmlWindowEditsHoverBackgroundBrush));
            OnPropertyChanged(nameof(CompareXmlWindowCheckBoxBackgroundBrush));
            OnPropertyChanged(nameof(CompareXmlWindowCheckBoxTickBrush));

            OnPropertyChanged(nameof(BackupBrowserWindowBackgroundBrush));
            OnPropertyChanged(nameof(BackupBrowserWindowTextBrush));
            OnPropertyChanged(nameof(BackupBrowserWindowControlBackgroundBrush));
            OnPropertyChanged(nameof(BackupBrowserWindowControlBorderBrush));
            OnPropertyChanged(nameof(BackupBrowserWindowHeaderTextBrush));
            OnPropertyChanged(nameof(BackupBrowserWindowHeaderBackgroundBrush));
            OnPropertyChanged(nameof(BackupBrowserWindowListHoverBackgroundBrush));
            OnPropertyChanged(nameof(BackupBrowserWindowListSelectedBackgroundBrush));
            OnPropertyChanged(nameof(BackupBrowserWindowButtonBackgroundBrush));
            OnPropertyChanged(nameof(BackupBrowserWindowButtonHoverBackgroundBrush));
            OnPropertyChanged(nameof(BackupBrowserWindowXmlFilterHoverBackgroundBrush));
            OnPropertyChanged(nameof(BackupBrowserWindowXmlFilterSelectedBackgroundBrush));

            OnPropertyChanged(nameof(SavedEditsWindowBackgroundBrush));
            OnPropertyChanged(nameof(SavedEditsWindowTextBrush));
            OnPropertyChanged(nameof(SavedEditsWindowControlBackgroundBrush));
            OnPropertyChanged(nameof(SavedEditsWindowControlBorderBrush));
            OnPropertyChanged(nameof(SavedEditsWindowTabBackgroundBrush));
            OnPropertyChanged(nameof(SavedEditsWindowTabHoverBackgroundBrush));
            OnPropertyChanged(nameof(SavedEditsWindowTabSelectedBackgroundBrush));
            OnPropertyChanged(nameof(SavedEditsWindowButtonBackgroundBrush));
            OnPropertyChanged(nameof(SavedEditsWindowButtonHoverBackgroundBrush));
            OnPropertyChanged(nameof(SavedEditsWindowCheckBoxBackgroundBrush));
            OnPropertyChanged(nameof(SavedEditsWindowCheckBoxTickBrush));
            OnPropertyChanged(nameof(SavedEditsWindowGridTextBrush));
            OnPropertyChanged(nameof(SavedEditsWindowGridBackgroundBrush));
            OnPropertyChanged(nameof(SavedEditsWindowGridBorderBrush));
            OnPropertyChanged(nameof(SavedEditsWindowGridLinesBrush));
            OnPropertyChanged(nameof(SavedEditsWindowGridHeaderBackgroundBrush));
            OnPropertyChanged(nameof(SavedEditsWindowGridHeaderTextBrush));
            OnPropertyChanged(nameof(SavedEditsWindowGridRowHoverBackgroundBrush));
            OnPropertyChanged(nameof(SavedEditsWindowGridRowSelectedBackgroundBrush));
            OnPropertyChanged(nameof(SavedEditsWindowGridCellSelectedBackgroundBrush));
            OnPropertyChanged(nameof(SavedEditsWindowGridCellSelectedTextBrush));

            OnPropertyChanged(nameof(SettingsInfoWindowBackgroundBrush));
            OnPropertyChanged(nameof(SettingsInfoWindowTextBrush));
            OnPropertyChanged(nameof(SettingsInfoWindowControlBackgroundBrush));
            OnPropertyChanged(nameof(SettingsInfoWindowControlBorderBrush));
            OnPropertyChanged(nameof(SettingsInfoWindowHeaderTextBrush));
            OnPropertyChanged(nameof(SettingsInfoWindowHeaderBackgroundBrush));
            OnPropertyChanged(nameof(SettingsInfoWindowButtonBackgroundBrush));
            OnPropertyChanged(nameof(SettingsInfoWindowButtonHoverBackgroundBrush));
            OnPropertyChanged(nameof(SettingsInfoWindowCheckBoxBackgroundBrush));
            OnPropertyChanged(nameof(SettingsInfoWindowCheckBoxTickBrush));

            OnPropertyChanged(nameof(DocumentationWindowBackgroundBrush));
            OnPropertyChanged(nameof(DocumentationWindowTextBrush));
            OnPropertyChanged(nameof(DocumentationWindowControlBackgroundBrush));
            OnPropertyChanged(nameof(DocumentationWindowControlBorderBrush));
            OnPropertyChanged(nameof(DocumentationWindowHeaderTextBrush));
            OnPropertyChanged(nameof(DocumentationWindowHeaderBackgroundBrush));
            OnPropertyChanged(nameof(DocumentationWindowListHoverBackgroundBrush));
            OnPropertyChanged(nameof(DocumentationWindowListSelectedBackgroundBrush));
            OnPropertyChanged(nameof(DocumentationWindowButtonBackgroundBrush));
            OnPropertyChanged(nameof(DocumentationWindowButtonHoverBackgroundBrush));

            OnPropertyChanged(nameof(XmlGuidesWindowBackgroundBrush));
            OnPropertyChanged(nameof(XmlGuidesWindowTextBrush));
            OnPropertyChanged(nameof(XmlGuidesWindowControlBackgroundBrush));
            OnPropertyChanged(nameof(XmlGuidesWindowControlBorderBrush));
            OnPropertyChanged(nameof(XmlGuidesWindowButtonBackgroundBrush));
            OnPropertyChanged(nameof(XmlGuidesWindowButtonTextBrush));
            OnPropertyChanged(nameof(XmlGuidesWindowButtonHoverBackgroundBrush));
            OnPropertyChanged(nameof(XmlGuidesWindowButtonHoverTextBrush));
            OnPropertyChanged(nameof(XmlGuidesWindowGuidesListBackgroundBrush));
            OnPropertyChanged(nameof(XmlGuidesWindowGuidesListTextBrush));
            OnPropertyChanged(nameof(XmlGuidesWindowGuidesListItemHoverBackgroundBrush));
            OnPropertyChanged(nameof(XmlGuidesWindowGuidesListItemHoverTextBrush));
            OnPropertyChanged(nameof(XmlGuidesWindowGuidesListItemSelectedBackgroundBrush));
            OnPropertyChanged(nameof(XmlGuidesWindowGuidesListItemSelectedTextBrush));
            OnPropertyChanged(nameof(XmlGuidesWindowFontPickerTextBrush));

            OnPropertyChanged(nameof(EditorTextBrush));
            OnPropertyChanged(nameof(EditorBackgroundBrush));
            OnPropertyChanged(nameof(EditorXmlSyntaxForeground));
            OnPropertyChanged(nameof(EditorScopeShadingColor));
            OnPropertyChanged(nameof(EditorIndentGuidesColor));
            OnPropertyChanged(nameof(EditorRegionHighlightColor));

            OnPropertyChanged(nameof(MenuTextBrush));
            OnPropertyChanged(nameof(MenuBackgroundBrush));

            OnPropertyChanged(nameof(TopButtonTextBrush));
            OnPropertyChanged(nameof(TopButtonBackgroundBrush));

            OnPropertyChanged(nameof(TreeTextBrush));
            OnPropertyChanged(nameof(TreeBackgroundBrush));
            OnPropertyChanged(nameof(TreeItemHoverBackgroundBrush));
            OnPropertyChanged(nameof(TreeItemSelectedBackgroundBrush));
            OnPropertyChanged(nameof(Pane1TreeItemHoverBackgroundBrush));
            OnPropertyChanged(nameof(Pane1TreeItemSelectedBackgroundBrush));

            OnPropertyChanged(nameof(GridTextBrush));
            OnPropertyChanged(nameof(GridBackgroundBrush));
            OnPropertyChanged(nameof(GridBorderBrush));
            OnPropertyChanged(nameof(GridLinesBrush));
            OnPropertyChanged(nameof(GridRowHoverBackgroundBrush));
            OnPropertyChanged(nameof(GridHeaderBackgroundBrush));
            OnPropertyChanged(nameof(GridHeaderTextBrush));
            OnPropertyChanged(nameof(GridRowSelectedBackgroundBrush));
            OnPropertyChanged(nameof(GridCellSelectedBackgroundBrush));
            OnPropertyChanged(nameof(SearchMatchBackgroundBrush));
            OnPropertyChanged(nameof(SearchMatchTextBrush));


            OnPropertyChanged(nameof(FieldColumnTextBrush));
            OnPropertyChanged(nameof(FieldColumnBackgroundBrush));
            OnPropertyChanged(nameof(ValueColumnTextBrush));
            OnPropertyChanged(nameof(ValueColumnBackgroundBrush));

            OnPropertyChanged(nameof(HeaderTextBrush));
            OnPropertyChanged(nameof(SelectorBackgroundBrush));

            OnPropertyChanged(nameof(Pane2ComboTextBrush));
            OnPropertyChanged(nameof(Pane2ComboBackgroundBrush));
            OnPropertyChanged(nameof(Pane2DropdownTextBrush));
            OnPropertyChanged(nameof(Pane2DropdownBackgroundBrush));
            OnPropertyChanged(nameof(Pane2ItemHoverBackgroundBrush));
            OnPropertyChanged(nameof(Pane2ItemSelectedBackgroundBrush));
        }

        private static WpfBrush CreateFrozenBrush(string hex)
        {
            if (string.IsNullOrWhiteSpace(hex))
                hex = "#00000000";

            var obj = new BrushConverter().ConvertFromString(hex);
            var brush = obj as WpfBrush ?? WpfBrushes.Transparent;

            if (brush.CanFreeze)
                brush.Freeze();

            return brush;
        }
    }
}
