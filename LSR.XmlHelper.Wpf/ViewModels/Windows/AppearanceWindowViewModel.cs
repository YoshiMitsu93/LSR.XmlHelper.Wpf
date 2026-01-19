using LSR.XmlHelper.Wpf.Infrastructure;
using LSR.XmlHelper.Wpf.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using System.Windows.Media;

using WpfBrush = System.Windows.Media.Brush;
using WpfBrushes = System.Windows.Media.Brushes;
using WpfColor = System.Windows.Media.Color;
using WpfColorConverter = System.Windows.Media.ColorConverter;

namespace LSR.XmlHelper.Wpf.ViewModels
{
    public sealed class AppearanceWindowViewModel : ObservableObject
    {
        private readonly AppSettingsService _settingsService;
        private readonly AppSettings _appSettings;
        private readonly AppearanceService _appearance;

        private readonly AppearanceSettings _originalFromSettings;
        private readonly AppearanceSettings _workingCopy;

        private readonly bool _originalIsDarkMode;
        private readonly bool _originalIsFriendlyView;

        private bool _isEditingDarkMode;
        private bool _isEditingFriendlyView;

        private bool _isDirty;
        private bool _suppressPreview;
        private bool _isViewReady;
        private readonly ObservableCollection<NamedAppearanceProfile> _appearanceProfiles;
        private NamedAppearanceProfile? _selectedAppearanceProfile;
        private string _profileNameInput = "";

        public AppearanceWindowViewModel(
            AppSettingsService settingsService,
            AppSettings appSettings,
            AppearanceService appearance,
            bool isCurrentDarkMode,
            bool isCurrentFriendlyView)
        {
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
            _appSettings = appSettings ?? throw new ArgumentNullException(nameof(appSettings));
            _appearance = appearance ?? throw new ArgumentNullException(nameof(appearance));

            _originalFromSettings = CloneAppearance(_appSettings.Appearance);
            _workingCopy = CloneAppearance(_appSettings.Appearance);
            _appearanceProfiles = new ObservableCollection<NamedAppearanceProfile>(_workingCopy.Profiles);
            _selectedAppearanceProfile = _appearanceProfiles.FirstOrDefault(p => string.Equals(p.Name, _workingCopy.ActiveProfileName, StringComparison.OrdinalIgnoreCase));
            _profileNameInput = _workingCopy.ActiveProfileName;

            FontFamilies = Fonts.SystemFontFamilies
                .Select(f => f.Source)
                .OrderBy(s => s, StringComparer.OrdinalIgnoreCase)
                .ToList();

            _originalIsDarkMode = _appearance.IsDarkMode;
            _originalIsFriendlyView = _appearance.IsFriendlyView;

            _isEditingDarkMode = _originalIsDarkMode;
            _isEditingFriendlyView = _originalIsFriendlyView;
            _isEditingFriendlyView = _appearance.IsFriendlyView;

            LoadFromProfile(GetEditingProfile());

            PickColorCommand = new RelayCommandOfT<string>(PickColor);
            ApplyCommand = new RelayCommand(Apply);
            OkCommand = new RelayCommand(Ok);
            CancelCommand = new RelayCommand(Cancel);
            ResetTabToDefaultsCommand = new RelayCommandOfT<string>(ResetTabToDefaults);
            ResetAllToDefaultsCommand = new RelayCommand(ResetAllToDefaults);
            SaveAppearanceProfileCommand = new RelayCommand(SaveAppearanceProfile, CanSaveAppearanceProfile);
            ApplyAppearanceProfileCommand = new RelayCommand(ApplySelectedAppearanceProfile, CanApplySelectedAppearanceProfile);
            DeleteAppearanceProfileCommand = new RelayCommand(DeleteSelectedAppearanceProfile, CanDeleteSelectedAppearanceProfile);

            RaisePreview();
            ApplyPreviewIfEditingCurrentTheme();
        }

        public event EventHandler<bool>? CloseRequested;

        public List<string> FontFamilies { get; }

        public RelayCommandOfT<string> PickColorCommand { get; }

        public RelayCommand ApplyCommand { get; }
        public RelayCommand OkCommand { get; }
        public RelayCommand CancelCommand { get; }
        public RelayCommandOfT<string> ResetTabToDefaultsCommand { get; }
        public RelayCommand ResetAllToDefaultsCommand { get; }

        public RelayCommand SaveAppearanceProfileCommand { get; }
        public RelayCommand ApplyAppearanceProfileCommand { get; }
        public RelayCommand DeleteAppearanceProfileCommand { get; }

        public ObservableCollection<NamedAppearanceProfile> AppearanceProfiles => _appearanceProfiles;

        public NamedAppearanceProfile? SelectedAppearanceProfile
        {
            get => _selectedAppearanceProfile;
            set
            {
                if (!SetProperty(ref _selectedAppearanceProfile, value))
                    return;

                System.Windows.Input.CommandManager.InvalidateRequerySuggested();
            }
        }

        public string ProfileNameInput
        {
            get => _profileNameInput;
            set
            {
                if (!SetProperty(ref _profileNameInput, value))
                    return;

                System.Windows.Input.CommandManager.InvalidateRequerySuggested();
            }
        }

        public bool IsDirty
        {
            get => _isDirty;
            private set => SetProperty(ref _isDirty, value);
        }

        public bool IsEditingDarkMode
        {
            get => _isEditingDarkMode;
            set
            {
                if (!_isViewReady)
                    return;

                if (!SetProperty(ref _isEditingDarkMode, value))
                    return;

                OnPropertyChanged(nameof(IsEditingLightMode));
                SwitchEditingProfile();
            }
        }

        public bool IsEditingLightMode
        {
            get => !_isEditingDarkMode;
            set
            {
                if (value)
                    IsEditingDarkMode = false;
            }
        }

        public bool IsEditingFriendlyView
        {
            get => _isEditingFriendlyView;
            set
            {
                if (!_isViewReady)
                    return;

                if (!SetProperty(ref _isEditingFriendlyView, value))
                    return;

                OnPropertyChanged(nameof(IsEditingRawXml));
                SwitchEditingProfile();
            }
        }

        public bool IsEditingRawXml
        {
            get => !_isEditingFriendlyView;
            set
            {
                if (value)
                    IsEditingFriendlyView = false;
            }
        }

        private string _uiFontFamily = "Segoe UI";
        private string _uiFontSize = "12";
        private string _editorFontFamily = "Consolas";
        private string _editorFontSize = "13";

        private string _text = "#FFD4D4D4";
        private string _background = "#FF1E1E1E";

        private string _editorText = "#FFD4D4D4";
        private string _editorBackground = "#FF1E1E1E";
        private string _editorXmlSyntaxForeground = "";

        private string _menuText = "#FFD4D4D4";
        private string _menuBackground = "#FF1E1E1E";
        private string _topButtonText = "#FFD4D4D4";
        private string _topButtonBackground = "#FF2F2F2F";

        private string _treeText = "#FFD4D4D4";
        private string _treeBackground = "#FF1E1E1E";
        private string _treeItemHoverBackground = "#FF252525";
        private string _treeItemSelectedBackground = "#FF2F2F2F";
        private string _pane1TreeItemHoverBackground = "#FF252525";
        private string _pane1TreeItemSelectedBackground = "#FF2F2F2F";


        private string _gridText = "#FFD4D4D4";
        private string _gridBackground = "#FF1E1E1E";
        private string _gridBorder = "#FF555555";
        private string _gridLines = "#FF555555";
        private string _gridRowHoverBackground = "#FF252525";
        private string _gridHeaderText = "#FFD4D4D4";
        private string _gridRowSelectedBackground = "#FF2F2F2F";
        private string _gridCellSelectedBackground = "#FF2F2F2F";
        private string _searchMatchBackground = "#FF665C00";
        private string _searchMatchText = "#FFFFFFFF";


        private string _fieldColumnText = "#FFD4D4D4";
        private string _fieldColumnBackground = "#00000000";
        private string _valueColumnText = "#FFD4D4D4";
        private string _valueColumnBackground = "#00000000";
        private string _headerText = "#FFD4D4D4";
        private string _selectorBackground = "#FF1E1E1E";
        private string _pane2ComboText = "#FFD4D4D4";
        private string _pane2ComboBackground = "#FF1E1E1E";
        private string _pane2DropdownText = "#FFD4D4D4";
        private string _pane2DropdownBackground = "#FF1E1E1E";
        private string _pane2ItemHoverBackground = "#FF252525";
        private string _pane2ItemSelectedBackground = "#FF2F2F2F";

        private string _appearanceWindowBackground = "#FF1E1E1E";
        private string _appearanceWindowText = "#FFD4D4D4";
        private string _appearanceWindowControlBackground = "#FF1E1E1E";
        private string _appearanceWindowControlBorder = "#FF555555";
        private string _appearanceWindowHeaderText = "#FFD4D4D4";
        private string _appearanceWindowHeaderBackground = "#FF1E1E1E";
        private string _appearanceWindowTabBackground = "#FF2F2F2F";
        private string _appearanceWindowTabHoverBackground = "#FF3A3A3A";
        private string _appearanceWindowTabSelectedBackground = "#FF1E1E1E";
        private string _appearanceWindowButtonBackground = "#FF2F2F2F";
        private string _appearanceWindowButtonHoverBackground = "#FF3A3A3A";

        private string _sharedConfigPacksWindowBackground = "#FF1E1E1E";
        private string _sharedConfigPacksWindowText = "#FFD4D4D4";
        private string _sharedConfigPacksWindowControlBackground = "#FF1E1E1E";
        private string _sharedConfigPacksWindowControlBorder = "#FF555555";
        private string _sharedConfigPacksWindowHeaderText = "#FFD4D4D4";
        private string _sharedConfigPacksWindowHeaderBackground = "#FF1E1E1E";
        private string _sharedConfigPacksWindowTabBackground = "#FF2F2F2F";
        private string _sharedConfigPacksWindowTabHoverBackground = "#FF3A3A3A";
        private string _sharedConfigPacksWindowTabSelectedBackground = "#FF1E1E1E";
        private string _sharedConfigPacksWindowButtonBackground = "#FF2F2F2F";
        private string _sharedConfigPacksWindowButtonHoverBackground = "#FF3A3A3A";
        private string _sharedConfigPacksWindowEditsHoverBackground = "#FF3A3A3A";
        private string _sharedConfigPacksWindowCheckBoxBackground = "#FF1E1E1E";
        private string _sharedConfigPacksWindowCheckBoxTick = "#FFD4D4D4";

        private string _compareXmlWindowBackground = "#FF1E1E1E";
        private string _compareXmlWindowText = "#FFD4D4D4";
        private string _compareXmlWindowControlBackground = "#FF1E1E1E";
        private string _compareXmlWindowControlBorder = "#FF555555";
        private string _compareXmlWindowHeaderText = "#FFD4D4D4";
        private string _compareXmlWindowHeaderBackground = "#FF1E1E1E";
        private string _compareXmlWindowButtonBackground = "#FF2F2F2F";
        private string _compareXmlWindowButtonHoverBackground = "#FF3A3A3A";
        private string _compareXmlWindowEditsHoverBackground = "#FF3A3A3A";
        private string _compareXmlWindowCheckBoxBackground = "#FF1E1E1E";
        private string _compareXmlWindowCheckBoxTick = "#FFD4D4D4";

        private string _backupBrowserWindowBackground = "#FF1E1E1E";
        private string _backupBrowserWindowText = "#FFD4D4D4";
        private string _backupBrowserWindowControlBackground = "#FF1E1E1E";
        private string _backupBrowserWindowControlBorder = "#FF555555";
        private string _backupBrowserWindowHeaderText = "#FFD4D4D4";
        private string _backupBrowserWindowHeaderBackground = "#FF1E1E1E";
        private string _backupBrowserWindowListHoverBackground = "#FF3A3A3A";
        private string _backupBrowserWindowListSelectedBackground = "#FF3A3A3A";
        private string _backupBrowserWindowButtonBackground = "#FF2F2F2F";
        private string _backupBrowserWindowButtonHoverBackground = "#FF3A3A3A";
        private string _backupBrowserWindowXmlFilterHoverBackground = "#FF3A3A3A";
        private string _backupBrowserWindowXmlFilterSelectedBackground = "#FF3A3A3A";

        private string _savedEditsWindowBackground = "#FF1E1E1E";
        private string _savedEditsWindowText = "#FFD4D4D4";
        private string _savedEditsWindowControlBackground = "#FF1E1E1E";
        private string _savedEditsWindowControlBorder = "#FF555555";
        private string _savedEditsWindowTabBackground = "#FF2F2F2F";
        private string _savedEditsWindowTabHoverBackground = "#FF3A3A3A";
        private string _savedEditsWindowTabSelectedBackground = "#FF1E1E1E";
        private string _savedEditsWindowButtonBackground = "#FF2F2F2F";
        private string _savedEditsWindowButtonHoverBackground = "#FF3A3A3A";
        private string _savedEditsWindowCheckBoxBackground = "#FF1E1E1E";
        private string _savedEditsWindowCheckBoxTick = "#FFD4D4D4";
        private string _savedEditsWindowGridText = "#FFD4D4D4";
        private string _savedEditsWindowGridBackground = "#FF1E1E1E";
        private string _savedEditsWindowGridBorder = "#FF555555";
        private string _savedEditsWindowGridLines = "#FF555555";
        private string _savedEditsWindowGridHeaderBackground = "#FF1E1E1E";
        private string _savedEditsWindowGridHeaderText = "#FFD4D4D4";
        private string _savedEditsWindowGridRowHoverBackground = "#FF252525";
        private string _savedEditsWindowGridRowSelectedBackground = "#FF2F2F2F";
        private string _savedEditsWindowGridCellSelectedBackground = "#FF2F2F2F";
        private string _savedEditsWindowGridCellSelectedText = "#FFFFFFFF";

        private string _settingsInfoWindowBackground = "#FF1E1E1E";
        private string _settingsInfoWindowText = "#FFD4D4D4";
        private string _settingsInfoWindowControlBackground = "#FF1E1E1E";
        private string _settingsInfoWindowControlBorder = "#FF555555";
        private string _settingsInfoWindowHeaderText = "#FFD4D4D4";
        private string _settingsInfoWindowHeaderBackground = "#FF1E1E1E";
        private string _settingsInfoWindowButtonBackground = "#FF2F2F2F";
        private string _settingsInfoWindowButtonHoverBackground = "#FF3A3A3A";
        private string _settingsInfoWindowCheckBoxBackground = "#FF1E1E1E";
        private string _settingsInfoWindowCheckBoxTick = "#FFD4D4D4";

        private string _documentationWindowBackground = "#FF1E1E1E";
        private string _documentationWindowText = "#FFD4D4D4";
        private string _documentationWindowControlBackground = "#FF1E1E1E";
        private string _documentationWindowControlBorder = "#FF555555";
        private string _documentationWindowHeaderText = "#FFD4D4D4";
        private string _documentationWindowHeaderBackground = "#FF1E1E1E";
        private string _documentationWindowListHoverBackground = "#FF252525";
        private string _documentationWindowListSelectedBackground = "#FF2F2F2F";
        private string _documentationWindowButtonBackground = "#FF2F2F2F";
        private string _documentationWindowButtonHoverBackground = "#FF3A3A3A";

        private string _xmlGuidesWindowBackground = "#FF1E1E1E";
        private string _xmlGuidesWindowText = "#FFD4D4D4";
        private string _xmlGuidesWindowControlBackground = "#FF1E1E1E";
        private string _xmlGuidesWindowControlBorder = "#FF555555";
        private string _xmlGuidesWindowButtonBackground = "#FF2F2F2F";
        private string _xmlGuidesWindowButtonText = "#FFD4D4D4";
        private string _xmlGuidesWindowButtonHoverBackground = "#FF3A3A3A";
        private string _xmlGuidesWindowButtonHoverText = "#FFFFFFFF";
        private string _xmlGuidesWindowButtonSelectedBackground = "#FF0080C0";
        private string _xmlGuidesWindowButtonSelectedText = "#FFFFFFFF";
        private string _xmlGuidesWindowGuidesListBackground = "#FF1E1E1E";
        private string _xmlGuidesWindowGuidesListText = "#FFD4D4D4";
        private string _xmlGuidesWindowGuidesListItemHoverBackground = "#FF252525";
        private string _xmlGuidesWindowGuidesListItemHoverText = "#FFFFFFFF";
        private string _xmlGuidesWindowGuidesListItemSelectedBackground = "#FF2F2F2F";
        private string _xmlGuidesWindowGuidesListItemSelectedText = "#FFFFFFFF";
        private string _xmlGuidesWindowFontPickerText = "#FFD4D4D4";

        public string UiFontFamily { get => _uiFontFamily; set { if (SetProperty(ref _uiFontFamily, value)) OnEdited(); } }
        public string UiFontSize { get => _uiFontSize; set { if (SetProperty(ref _uiFontSize, value)) OnEdited(); } }

        public string EditorFontFamily { get => _editorFontFamily; set { if (SetProperty(ref _editorFontFamily, value)) OnEdited(); } }
        public string EditorFontSize { get => _editorFontSize; set { if (SetProperty(ref _editorFontSize, value)) OnEdited(); } }

        public string Text { get => _text; set { if (SetProperty(ref _text, value)) OnEdited(); } }
        public string Background { get => _background; set { if (SetProperty(ref _background, value)) OnEdited(); } }

        public string EditorText { get => _editorText; set { if (SetProperty(ref _editorText, value)) OnEdited(); } }
        public string EditorBackground { get => _editorBackground; set { if (SetProperty(ref _editorBackground, value)) OnEdited(); } }
        public string EditorXmlSyntaxForeground { get => _editorXmlSyntaxForeground; set { if (SetProperty(ref _editorXmlSyntaxForeground, value)) OnEdited(); } }

        public string MenuText { get => _menuText; set { if (SetProperty(ref _menuText, value)) OnEdited(); } }
        public string MenuBackground { get => _menuBackground; set { if (SetProperty(ref _menuBackground, value)) OnEdited(); } }
        public string TopButtonText { get => _topButtonText; set { if (SetProperty(ref _topButtonText, value)) OnEdited(); } }
        public string TopButtonBackground { get => _topButtonBackground; set { if (SetProperty(ref _topButtonBackground, value)) OnEdited(); } }

        public string TreeText { get => _treeText; set { if (SetProperty(ref _treeText, value)) OnEdited(); } }
        public string TreeBackground { get => _treeBackground; set { if (SetProperty(ref _treeBackground, value)) OnEdited(); } }
        public string TreeItemHoverBackground { get => _treeItemHoverBackground; set { if (SetProperty(ref _treeItemHoverBackground, value)) OnEdited(); } }
        public string TreeItemSelectedBackground { get => _treeItemSelectedBackground; set { if (SetProperty(ref _treeItemSelectedBackground, value)) OnEdited(); } }
        public string Pane1TreeItemHoverBackground { get => _pane1TreeItemHoverBackground; set { if (SetProperty(ref _pane1TreeItemHoverBackground, value)) OnEdited(); } }
        public string Pane1TreeItemSelectedBackground { get => _pane1TreeItemSelectedBackground; set { if (SetProperty(ref _pane1TreeItemSelectedBackground, value)) OnEdited(); } }

        public string GridText { get => _gridText; set { if (SetProperty(ref _gridText, value)) OnEdited(); } }
        public string GridBackground { get => _gridBackground; set { if (SetProperty(ref _gridBackground, value)) OnEdited(); } }
        public string GridBorder { get => _gridBorder; set { if (SetProperty(ref _gridBorder, value)) OnEdited(); } }
        public string GridLines { get => _gridLines; set { if (SetProperty(ref _gridLines, value)) OnEdited(); } }
        public string GridRowHoverBackground { get => _gridRowHoverBackground; set { if (SetProperty(ref _gridRowHoverBackground, value)) OnEdited(); } }
        public string GridHeaderText { get => _gridHeaderText; set { if (SetProperty(ref _gridHeaderText, value)) OnEdited(); } }
        public string GridRowSelectedBackground { get => _gridRowSelectedBackground; set { if (SetProperty(ref _gridRowSelectedBackground, value)) OnEdited(); } }
        public string GridCellSelectedBackground { get => _gridCellSelectedBackground; set { if (SetProperty(ref _gridCellSelectedBackground, value)) OnEdited(); } }
        public string SearchMatchBackground { get => _searchMatchBackground; set { if (SetProperty(ref _searchMatchBackground, value)) OnEdited(); } }
        public string SearchMatchText { get => _searchMatchText; set { if (SetProperty(ref _searchMatchText, value)) OnEdited(); } }

        public string FieldColumnText { get => _fieldColumnText; set { if (SetProperty(ref _fieldColumnText, value)) OnEdited(); } }
        public string FieldColumnBackground { get => _fieldColumnBackground; set { if (SetProperty(ref _fieldColumnBackground, value)) OnEdited(); } }
        public string ValueColumnText { get => _valueColumnText; set { if (SetProperty(ref _valueColumnText, value)) OnEdited(); } }
        public string ValueColumnBackground { get => _valueColumnBackground; set { if (SetProperty(ref _valueColumnBackground, value)) OnEdited(); } }
        public string HeaderText { get => _headerText; set { if (SetProperty(ref _headerText, value)) OnEdited(); } }
        public string SelectorBackground { get => _selectorBackground; set { if (SetProperty(ref _selectorBackground, value)) OnEdited(); } }

        public string Pane2ComboText { get => _pane2ComboText; set { if (SetProperty(ref _pane2ComboText, value)) OnEdited(); } }
        public string Pane2ComboBackground { get => _pane2ComboBackground; set { if (SetProperty(ref _pane2ComboBackground, value)) OnEdited(); } }
        public string Pane2DropdownText { get => _pane2DropdownText; set { if (SetProperty(ref _pane2DropdownText, value)) OnEdited(); } }
        public string Pane2DropdownBackground { get => _pane2DropdownBackground; set { if (SetProperty(ref _pane2DropdownBackground, value)) OnEdited(); } }
        public string Pane2ItemHoverBackground { get => _pane2ItemHoverBackground; set { if (SetProperty(ref _pane2ItemHoverBackground, value)) OnEdited(); } }
        public string Pane2ItemSelectedBackground { get => _pane2ItemSelectedBackground; set { if (SetProperty(ref _pane2ItemSelectedBackground, value)) OnEdited(); } }
        public string AppearanceWindowBackground { get => _appearanceWindowBackground; set { if (SetProperty(ref _appearanceWindowBackground, value)) OnEdited(); } }
        public string AppearanceWindowText { get => _appearanceWindowText; set { if (SetProperty(ref _appearanceWindowText, value)) OnEdited(); } }
        public string AppearanceWindowControlBackground { get => _appearanceWindowControlBackground; set { if (SetProperty(ref _appearanceWindowControlBackground, value)) OnEdited(); } }
        public string AppearanceWindowControlBorder { get => _appearanceWindowControlBorder; set { if (SetProperty(ref _appearanceWindowControlBorder, value)) OnEdited(); } }
        public string AppearanceWindowHeaderText { get => _appearanceWindowHeaderText; set { if (SetProperty(ref _appearanceWindowHeaderText, value)) OnEdited(); } }
        public string AppearanceWindowHeaderBackground { get => _appearanceWindowHeaderBackground; set { if (SetProperty(ref _appearanceWindowHeaderBackground, value)) OnEdited(); } }
        public string AppearanceWindowTabBackground { get => _appearanceWindowTabBackground; set { if (SetProperty(ref _appearanceWindowTabBackground, value)) OnEdited(); } }
        public string AppearanceWindowTabHoverBackground { get => _appearanceWindowTabHoverBackground; set { if (SetProperty(ref _appearanceWindowTabHoverBackground, value)) OnEdited(); } }
        public string AppearanceWindowTabSelectedBackground { get => _appearanceWindowTabSelectedBackground; set { if (SetProperty(ref _appearanceWindowTabSelectedBackground, value)) OnEdited(); } }
        public string AppearanceWindowButtonBackground { get => _appearanceWindowButtonBackground; set { if (SetProperty(ref _appearanceWindowButtonBackground, value)) OnEdited(); } }
        public string AppearanceWindowButtonHoverBackground { get => _appearanceWindowButtonHoverBackground; set { if (SetProperty(ref _appearanceWindowButtonHoverBackground, value)) OnEdited(); } }
        public string SharedConfigPacksWindowBackground { get => _sharedConfigPacksWindowBackground; set { if (SetProperty(ref _sharedConfigPacksWindowBackground, value)) OnEdited(); } }
        public string SharedConfigPacksWindowText { get => _sharedConfigPacksWindowText; set { if (SetProperty(ref _sharedConfigPacksWindowText, value)) OnEdited(); } }
        public string SharedConfigPacksWindowControlBackground { get => _sharedConfigPacksWindowControlBackground; set { if (SetProperty(ref _sharedConfigPacksWindowControlBackground, value)) OnEdited(); } }
        public string SharedConfigPacksWindowControlBorder { get => _sharedConfigPacksWindowControlBorder; set { if (SetProperty(ref _sharedConfigPacksWindowControlBorder, value)) OnEdited(); } }
        public string SharedConfigPacksWindowHeaderText { get => _sharedConfigPacksWindowHeaderText; set { if (SetProperty(ref _sharedConfigPacksWindowHeaderText, value)) OnEdited(); } }
        public string SharedConfigPacksWindowHeaderBackground { get => _sharedConfigPacksWindowHeaderBackground; set { if (SetProperty(ref _sharedConfigPacksWindowHeaderBackground, value)) OnEdited(); } }
        public string SharedConfigPacksWindowTabBackground { get => _sharedConfigPacksWindowTabBackground; set { if (SetProperty(ref _sharedConfigPacksWindowTabBackground, value)) OnEdited(); } }
        public string SharedConfigPacksWindowTabHoverBackground { get => _sharedConfigPacksWindowTabHoverBackground; set { if (SetProperty(ref _sharedConfigPacksWindowTabHoverBackground, value)) OnEdited(); } }
        public string SharedConfigPacksWindowTabSelectedBackground { get => _sharedConfigPacksWindowTabSelectedBackground; set { if (SetProperty(ref _sharedConfigPacksWindowTabSelectedBackground, value)) OnEdited(); } }
        public string SharedConfigPacksWindowButtonBackground { get => _sharedConfigPacksWindowButtonBackground; set { if (SetProperty(ref _sharedConfigPacksWindowButtonBackground, value)) OnEdited(); } }
        public string SharedConfigPacksWindowButtonHoverBackground { get => _sharedConfigPacksWindowButtonHoverBackground; set { if (SetProperty(ref _sharedConfigPacksWindowButtonHoverBackground, value)) OnEdited(); } }
        public string SharedConfigPacksWindowEditsHoverBackground { get => _sharedConfigPacksWindowEditsHoverBackground; set { if (SetProperty(ref _sharedConfigPacksWindowEditsHoverBackground, value)) OnEdited(); } }
        public string SharedConfigPacksWindowCheckBoxBackground { get => _sharedConfigPacksWindowCheckBoxBackground; set { if (SetProperty(ref _sharedConfigPacksWindowCheckBoxBackground, value)) OnEdited(); } }
        public string SharedConfigPacksWindowCheckBoxTick { get => _sharedConfigPacksWindowCheckBoxTick; set { if (SetProperty(ref _sharedConfigPacksWindowCheckBoxTick, value)) OnEdited(); } }
        public string CompareXmlWindowBackground { get => _compareXmlWindowBackground; set { if (SetProperty(ref _compareXmlWindowBackground, value)) OnEdited(); } }
        public string CompareXmlWindowText { get => _compareXmlWindowText; set { if (SetProperty(ref _compareXmlWindowText, value)) OnEdited(); } }
        public string CompareXmlWindowControlBackground { get => _compareXmlWindowControlBackground; set { if (SetProperty(ref _compareXmlWindowControlBackground, value)) OnEdited(); } }
        public string CompareXmlWindowControlBorder { get => _compareXmlWindowControlBorder; set { if (SetProperty(ref _compareXmlWindowControlBorder, value)) OnEdited(); } }
        public string CompareXmlWindowHeaderText { get => _compareXmlWindowHeaderText; set { if (SetProperty(ref _compareXmlWindowHeaderText, value)) OnEdited(); } }
        public string CompareXmlWindowHeaderBackground { get => _compareXmlWindowHeaderBackground; set { if (SetProperty(ref _compareXmlWindowHeaderBackground, value)) OnEdited(); } }
        public string CompareXmlWindowButtonBackground { get => _compareXmlWindowButtonBackground; set { if (SetProperty(ref _compareXmlWindowButtonBackground, value)) OnEdited(); } }
        public string CompareXmlWindowButtonHoverBackground { get => _compareXmlWindowButtonHoverBackground; set { if (SetProperty(ref _compareXmlWindowButtonHoverBackground, value)) OnEdited(); } }
        public string CompareXmlWindowEditsHoverBackground { get => _compareXmlWindowEditsHoverBackground; set { if (SetProperty(ref _compareXmlWindowEditsHoverBackground, value)) OnEdited(); } }
        public string CompareXmlWindowCheckBoxBackground { get => _compareXmlWindowCheckBoxBackground; set { if (SetProperty(ref _compareXmlWindowCheckBoxBackground, value)) OnEdited(); } }
        public string CompareXmlWindowCheckBoxTick { get => _compareXmlWindowCheckBoxTick; set { if (SetProperty(ref _compareXmlWindowCheckBoxTick, value)) OnEdited(); } }

        public string BackupBrowserWindowBackground { get => _backupBrowserWindowBackground; set { if (SetProperty(ref _backupBrowserWindowBackground, value)) OnEdited(); } }
        public string BackupBrowserWindowText { get => _backupBrowserWindowText; set { if (SetProperty(ref _backupBrowserWindowText, value)) OnEdited(); } }
        public string BackupBrowserWindowControlBackground { get => _backupBrowserWindowControlBackground; set { if (SetProperty(ref _backupBrowserWindowControlBackground, value)) OnEdited(); } }
        public string BackupBrowserWindowControlBorder { get => _backupBrowserWindowControlBorder; set { if (SetProperty(ref _backupBrowserWindowControlBorder, value)) OnEdited(); } }
        public string BackupBrowserWindowHeaderText { get => _backupBrowserWindowHeaderText; set { if (SetProperty(ref _backupBrowserWindowHeaderText, value)) OnEdited(); } }
        public string BackupBrowserWindowHeaderBackground { get => _backupBrowserWindowHeaderBackground; set { if (SetProperty(ref _backupBrowserWindowHeaderBackground, value)) OnEdited(); } }
        public string BackupBrowserWindowListHoverBackground { get => _backupBrowserWindowListHoverBackground; set { if (SetProperty(ref _backupBrowserWindowListHoverBackground, value)) OnEdited(); } }
        public string BackupBrowserWindowListSelectedBackground { get => _backupBrowserWindowListSelectedBackground; set { if (SetProperty(ref _backupBrowserWindowListSelectedBackground, value)) OnEdited(); } }
        public string BackupBrowserWindowButtonBackground { get => _backupBrowserWindowButtonBackground; set { if (SetProperty(ref _backupBrowserWindowButtonBackground, value)) OnEdited(); } }
        public string BackupBrowserWindowButtonHoverBackground { get => _backupBrowserWindowButtonHoverBackground; set { if (SetProperty(ref _backupBrowserWindowButtonHoverBackground, value)) OnEdited(); } }
        public string BackupBrowserWindowXmlFilterHoverBackground { get => _backupBrowserWindowXmlFilterHoverBackground; set { if (SetProperty(ref _backupBrowserWindowXmlFilterHoverBackground, value)) OnEdited(); } }
        public string BackupBrowserWindowXmlFilterSelectedBackground { get => _backupBrowserWindowXmlFilterSelectedBackground; set { if (SetProperty(ref _backupBrowserWindowXmlFilterSelectedBackground, value)) OnEdited(); } }

        public string SavedEditsWindowBackground { get => _savedEditsWindowBackground; set { if (SetProperty(ref _savedEditsWindowBackground, value)) OnEdited(); } }
        public string SavedEditsWindowText { get => _savedEditsWindowText; set { if (SetProperty(ref _savedEditsWindowText, value)) OnEdited(); } }
        public string SavedEditsWindowControlBackground { get => _savedEditsWindowControlBackground; set { if (SetProperty(ref _savedEditsWindowControlBackground, value)) OnEdited(); } }
        public string SavedEditsWindowControlBorder { get => _savedEditsWindowControlBorder; set { if (SetProperty(ref _savedEditsWindowControlBorder, value)) OnEdited(); } }
        public string SavedEditsWindowTabBackground { get => _savedEditsWindowTabBackground; set { if (SetProperty(ref _savedEditsWindowTabBackground, value)) OnEdited(); } }
        public string SavedEditsWindowTabHoverBackground { get => _savedEditsWindowTabHoverBackground; set { if (SetProperty(ref _savedEditsWindowTabHoverBackground, value)) OnEdited(); } }
        public string SavedEditsWindowTabSelectedBackground { get => _savedEditsWindowTabSelectedBackground; set { if (SetProperty(ref _savedEditsWindowTabSelectedBackground, value)) OnEdited(); } }
        public string SavedEditsWindowButtonBackground { get => _savedEditsWindowButtonBackground; set { if (SetProperty(ref _savedEditsWindowButtonBackground, value)) OnEdited(); } }
        public string SavedEditsWindowButtonHoverBackground { get => _savedEditsWindowButtonHoverBackground; set { if (SetProperty(ref _savedEditsWindowButtonHoverBackground, value)) OnEdited(); } }
        public string SavedEditsWindowCheckBoxBackground { get => _savedEditsWindowCheckBoxBackground; set { if (SetProperty(ref _savedEditsWindowCheckBoxBackground, value)) OnEdited(); } }
        public string SavedEditsWindowCheckBoxTick { get => _savedEditsWindowCheckBoxTick; set { if (SetProperty(ref _savedEditsWindowCheckBoxTick, value)) OnEdited(); } }
        public string SavedEditsWindowGridText { get => _savedEditsWindowGridText; set { if (SetProperty(ref _savedEditsWindowGridText, value)) OnEdited(); } }
        public string SavedEditsWindowGridBackground { get => _savedEditsWindowGridBackground; set { if (SetProperty(ref _savedEditsWindowGridBackground, value)) OnEdited(); } }
        public string SavedEditsWindowGridBorder { get => _savedEditsWindowGridBorder; set { if (SetProperty(ref _savedEditsWindowGridBorder, value)) OnEdited(); } }
        public string SavedEditsWindowGridLines { get => _savedEditsWindowGridLines; set { if (SetProperty(ref _savedEditsWindowGridLines, value)) OnEdited(); } }
        public string SavedEditsWindowGridHeaderBackground { get => _savedEditsWindowGridHeaderBackground; set { if (SetProperty(ref _savedEditsWindowGridHeaderBackground, value)) OnEdited(); } }
        public string SavedEditsWindowGridHeaderText { get => _savedEditsWindowGridHeaderText; set { if (SetProperty(ref _savedEditsWindowGridHeaderText, value)) OnEdited(); } }
        public string SavedEditsWindowGridRowHoverBackground { get => _savedEditsWindowGridRowHoverBackground; set { if (SetProperty(ref _savedEditsWindowGridRowHoverBackground, value)) OnEdited(); } }
        public string SavedEditsWindowGridRowSelectedBackground { get => _savedEditsWindowGridRowSelectedBackground; set { if (SetProperty(ref _savedEditsWindowGridRowSelectedBackground, value)) OnEdited(); } }
        public string SavedEditsWindowGridCellSelectedBackground { get => _savedEditsWindowGridCellSelectedBackground; set { if (SetProperty(ref _savedEditsWindowGridCellSelectedBackground, value)) OnEdited(); } }
        public string SavedEditsWindowGridCellSelectedText { get => _savedEditsWindowGridCellSelectedText; set { if (SetProperty(ref _savedEditsWindowGridCellSelectedText, value)) OnEdited(); } }

        public string SettingsInfoWindowBackground { get => _settingsInfoWindowBackground; set { if (SetProperty(ref _settingsInfoWindowBackground, value)) OnEdited(); } }
        public string SettingsInfoWindowText { get => _settingsInfoWindowText; set { if (SetProperty(ref _settingsInfoWindowText, value)) OnEdited(); } }
        public string SettingsInfoWindowControlBackground { get => _settingsInfoWindowControlBackground; set { if (SetProperty(ref _settingsInfoWindowControlBackground, value)) OnEdited(); } }
        public string SettingsInfoWindowControlBorder { get => _settingsInfoWindowControlBorder; set { if (SetProperty(ref _settingsInfoWindowControlBorder, value)) OnEdited(); } }
        public string SettingsInfoWindowHeaderText { get => _settingsInfoWindowHeaderText; set { if (SetProperty(ref _settingsInfoWindowHeaderText, value)) OnEdited(); } }
        public string SettingsInfoWindowHeaderBackground { get => _settingsInfoWindowHeaderBackground; set { if (SetProperty(ref _settingsInfoWindowHeaderBackground, value)) OnEdited(); } }
        public string SettingsInfoWindowButtonBackground { get => _settingsInfoWindowButtonBackground; set { if (SetProperty(ref _settingsInfoWindowButtonBackground, value)) OnEdited(); } }
        public string SettingsInfoWindowButtonHoverBackground { get => _settingsInfoWindowButtonHoverBackground; set { if (SetProperty(ref _settingsInfoWindowButtonHoverBackground, value)) OnEdited(); } }
        public string SettingsInfoWindowCheckBoxBackground { get => _settingsInfoWindowCheckBoxBackground; set { if (SetProperty(ref _settingsInfoWindowCheckBoxBackground, value)) OnEdited(); } }
        public string SettingsInfoWindowCheckBoxTick { get => _settingsInfoWindowCheckBoxTick; set { if (SetProperty(ref _settingsInfoWindowCheckBoxTick, value)) OnEdited(); } }

        public string DocumentationWindowBackground { get => _documentationWindowBackground; set { if (SetProperty(ref _documentationWindowBackground, value)) OnEdited(); } }
        public string DocumentationWindowText { get => _documentationWindowText; set { if (SetProperty(ref _documentationWindowText, value)) OnEdited(); } }
        public string DocumentationWindowControlBackground { get => _documentationWindowControlBackground; set { if (SetProperty(ref _documentationWindowControlBackground, value)) OnEdited(); } }
        public string DocumentationWindowControlBorder { get => _documentationWindowControlBorder; set { if (SetProperty(ref _documentationWindowControlBorder, value)) OnEdited(); } }
        public string DocumentationWindowHeaderText { get => _documentationWindowHeaderText; set { if (SetProperty(ref _documentationWindowHeaderText, value)) OnEdited(); } }
        public string DocumentationWindowHeaderBackground { get => _documentationWindowHeaderBackground; set { if (SetProperty(ref _documentationWindowHeaderBackground, value)) OnEdited(); } }
        public string DocumentationWindowListHoverBackground { get => _documentationWindowListHoverBackground; set { if (SetProperty(ref _documentationWindowListHoverBackground, value)) OnEdited(); } }
        public string DocumentationWindowListSelectedBackground { get => _documentationWindowListSelectedBackground; set { if (SetProperty(ref _documentationWindowListSelectedBackground, value)) OnEdited(); } }
        public string DocumentationWindowButtonBackground { get => _documentationWindowButtonBackground; set { if (SetProperty(ref _documentationWindowButtonBackground, value)) OnEdited(); } }
        public string DocumentationWindowButtonHoverBackground { get => _documentationWindowButtonHoverBackground; set { if (SetProperty(ref _documentationWindowButtonHoverBackground, value)) OnEdited(); } }
        public string XmlGuidesWindowBackground { get => _xmlGuidesWindowBackground; set { if (SetProperty(ref _xmlGuidesWindowBackground, value)) OnEdited(); } }
        public string XmlGuidesWindowText { get => _xmlGuidesWindowText; set { if (SetProperty(ref _xmlGuidesWindowText, value)) OnEdited(); } }
        public string XmlGuidesWindowControlBackground { get => _xmlGuidesWindowControlBackground; set { if (SetProperty(ref _xmlGuidesWindowControlBackground, value)) OnEdited(); } }
        public string XmlGuidesWindowControlBorder { get => _xmlGuidesWindowControlBorder; set { if (SetProperty(ref _xmlGuidesWindowControlBorder, value)) OnEdited(); } }
        public string XmlGuidesWindowButtonBackground { get => _xmlGuidesWindowButtonBackground; set { if (SetProperty(ref _xmlGuidesWindowButtonBackground, value)) OnEdited(); } }
        public string XmlGuidesWindowButtonText { get => _xmlGuidesWindowButtonText; set { if (SetProperty(ref _xmlGuidesWindowButtonText, value)) OnEdited(); } }
        public string XmlGuidesWindowButtonHoverBackground { get => _xmlGuidesWindowButtonHoverBackground; set { if (SetProperty(ref _xmlGuidesWindowButtonHoverBackground, value)) OnEdited(); } }
        public string XmlGuidesWindowButtonHoverText { get => _xmlGuidesWindowButtonHoverText; set { if (SetProperty(ref _xmlGuidesWindowButtonHoverText, value)) OnEdited(); } }
        public string XmlGuidesWindowButtonSelectedBackground { get => _xmlGuidesWindowButtonSelectedBackground; set { if (SetProperty(ref _xmlGuidesWindowButtonSelectedBackground, value)) OnEdited(); } }
        public string XmlGuidesWindowButtonSelectedText { get => _xmlGuidesWindowButtonSelectedText; set { if (SetProperty(ref _xmlGuidesWindowButtonSelectedText, value)) OnEdited(); } }
        public string XmlGuidesWindowGuidesListBackground { get => _xmlGuidesWindowGuidesListBackground; set { if (SetProperty(ref _xmlGuidesWindowGuidesListBackground, value)) OnEdited(); } }
        public string XmlGuidesWindowGuidesListText { get => _xmlGuidesWindowGuidesListText; set { if (SetProperty(ref _xmlGuidesWindowGuidesListText, value)) OnEdited(); } }
        public string XmlGuidesWindowGuidesListItemHoverBackground { get => _xmlGuidesWindowGuidesListItemHoverBackground; set { if (SetProperty(ref _xmlGuidesWindowGuidesListItemHoverBackground, value)) OnEdited(); } }
        public string XmlGuidesWindowGuidesListItemHoverText { get => _xmlGuidesWindowGuidesListItemHoverText; set { if (SetProperty(ref _xmlGuidesWindowGuidesListItemHoverText, value)) OnEdited(); } }
        public string XmlGuidesWindowGuidesListItemSelectedBackground { get => _xmlGuidesWindowGuidesListItemSelectedBackground; set { if (SetProperty(ref _xmlGuidesWindowGuidesListItemSelectedBackground, value)) OnEdited(); } }
        public string XmlGuidesWindowGuidesListItemSelectedText { get => _xmlGuidesWindowGuidesListItemSelectedText; set { if (SetProperty(ref _xmlGuidesWindowGuidesListItemSelectedText, value)) OnEdited(); } }
        public string XmlGuidesWindowFontPickerText { get => _xmlGuidesWindowFontPickerText; set { if (SetProperty(ref _xmlGuidesWindowFontPickerText, value)) OnEdited(); } }

        public WpfBrush PreviewTextBrush => TryParseBrush(Text);
        public WpfBrush PreviewBackgroundBrush => TryParseBrush(Background);
        public System.Windows.Media.FontFamily PreviewUiFontFamily => new System.Windows.Media.FontFamily(UiFontFamily ?? "Segoe UI");
        public double PreviewUiFontSize => TryParseDouble(UiFontSize, 12);
        public WpfBrush PreviewAppearanceWindowBackgroundBrush => TryParseBrush(AppearanceWindowBackground);
        public WpfBrush PreviewAppearanceWindowTextBrush => TryParseBrush(AppearanceWindowText);
        public WpfBrush PreviewAppearanceWindowControlBackgroundBrush => TryParseBrush(AppearanceWindowControlBackground);
        public WpfBrush PreviewAppearanceWindowControlBorderBrush => TryParseBrush(AppearanceWindowControlBorder);
        public WpfBrush PreviewAppearanceWindowHeaderTextBrush => TryParseBrush(AppearanceWindowHeaderText);
        public WpfBrush PreviewAppearanceWindowHeaderBackgroundBrush => TryParseBrush(AppearanceWindowHeaderBackground);
        public WpfBrush PreviewAppearanceWindowTabBackgroundBrush => TryParseBrush(AppearanceWindowTabBackground);
        public WpfBrush PreviewAppearanceWindowTabHoverBackgroundBrush => TryParseBrush(AppearanceWindowTabHoverBackground);
        public WpfBrush PreviewAppearanceWindowTabSelectedBackgroundBrush => TryParseBrush(AppearanceWindowTabSelectedBackground);
        public WpfBrush PreviewAppearanceWindowButtonBackgroundBrush => TryParseBrush(AppearanceWindowButtonBackground);
        public WpfBrush PreviewAppearanceWindowButtonHoverBackgroundBrush => TryParseBrush(AppearanceWindowButtonHoverBackground);
        public WpfBrush PreviewSharedConfigPacksWindowBackgroundBrush => TryParseBrush(SharedConfigPacksWindowBackground);
        public WpfBrush PreviewSharedConfigPacksWindowTextBrush => TryParseBrush(SharedConfigPacksWindowText);
        public WpfBrush PreviewSharedConfigPacksWindowControlBackgroundBrush => TryParseBrush(SharedConfigPacksWindowControlBackground);
        public WpfBrush PreviewSharedConfigPacksWindowControlBorderBrush => TryParseBrush(SharedConfigPacksWindowControlBorder);
        public WpfBrush PreviewSharedConfigPacksWindowHeaderTextBrush => TryParseBrush(SharedConfigPacksWindowHeaderText);
        public WpfBrush PreviewSharedConfigPacksWindowHeaderBackgroundBrush => TryParseBrush(SharedConfigPacksWindowHeaderBackground);
        public WpfBrush PreviewSharedConfigPacksWindowTabBackgroundBrush => TryParseBrush(SharedConfigPacksWindowTabBackground);
        public WpfBrush PreviewSharedConfigPacksWindowTabHoverBackgroundBrush => TryParseBrush(SharedConfigPacksWindowTabHoverBackground);
        public WpfBrush PreviewSharedConfigPacksWindowTabSelectedBackgroundBrush => TryParseBrush(SharedConfigPacksWindowTabSelectedBackground);
        public WpfBrush PreviewSharedConfigPacksWindowButtonBackgroundBrush => TryParseBrush(SharedConfigPacksWindowButtonBackground);
        public WpfBrush PreviewSharedConfigPacksWindowButtonHoverBackgroundBrush => TryParseBrush(SharedConfigPacksWindowButtonHoverBackground);
        public WpfBrush PreviewSharedConfigPacksWindowEditsHoverBackgroundBrush => TryParseBrush(SharedConfigPacksWindowEditsHoverBackground);
        public WpfBrush PreviewSharedConfigPacksWindowCheckBoxBackgroundBrush => TryParseBrush(SharedConfigPacksWindowCheckBoxBackground);
        public WpfBrush PreviewSharedConfigPacksWindowCheckBoxTickBrush => TryParseBrush(SharedConfigPacksWindowCheckBoxTick);
        public WpfBrush PreviewCompareXmlWindowBackgroundBrush => TryParseBrush(CompareXmlWindowBackground);
        public WpfBrush PreviewCompareXmlWindowTextBrush => TryParseBrush(CompareXmlWindowText);
        public WpfBrush PreviewCompareXmlWindowControlBackgroundBrush => TryParseBrush(CompareXmlWindowControlBackground);
        public WpfBrush PreviewCompareXmlWindowControlBorderBrush => TryParseBrush(CompareXmlWindowControlBorder);
        public WpfBrush PreviewCompareXmlWindowHeaderTextBrush => TryParseBrush(CompareXmlWindowHeaderText);
        public WpfBrush PreviewCompareXmlWindowHeaderBackgroundBrush => TryParseBrush(CompareXmlWindowHeaderBackground);
        public WpfBrush PreviewCompareXmlWindowButtonBackgroundBrush => TryParseBrush(CompareXmlWindowButtonBackground);
        public WpfBrush PreviewCompareXmlWindowButtonHoverBackgroundBrush => TryParseBrush(CompareXmlWindowButtonHoverBackground);
        public WpfBrush PreviewCompareXmlWindowEditsHoverBackgroundBrush => TryParseBrush(CompareXmlWindowEditsHoverBackground);
        public WpfBrush PreviewCompareXmlWindowCheckBoxBackgroundBrush => TryParseBrush(CompareXmlWindowCheckBoxBackground);
        public WpfBrush PreviewCompareXmlWindowCheckBoxTickBrush => TryParseBrush(CompareXmlWindowCheckBoxTick);
        public WpfBrush PreviewBackupBrowserWindowBackgroundBrush => TryParseBrush(BackupBrowserWindowBackground);
        public WpfBrush PreviewBackupBrowserWindowTextBrush => TryParseBrush(BackupBrowserWindowText);
        public WpfBrush PreviewBackupBrowserWindowControlBackgroundBrush => TryParseBrush(BackupBrowserWindowControlBackground);
        public WpfBrush PreviewBackupBrowserWindowControlBorderBrush => TryParseBrush(BackupBrowserWindowControlBorder);
        public WpfBrush PreviewBackupBrowserWindowHeaderTextBrush => TryParseBrush(BackupBrowserWindowHeaderText);
        public WpfBrush PreviewBackupBrowserWindowHeaderBackgroundBrush => TryParseBrush(BackupBrowserWindowHeaderBackground);
        public WpfBrush PreviewBackupBrowserWindowListHoverBackgroundBrush => TryParseBrush(BackupBrowserWindowListHoverBackground);
        public WpfBrush PreviewBackupBrowserWindowListSelectedBackgroundBrush => TryParseBrush(BackupBrowserWindowListSelectedBackground);
        public WpfBrush PreviewBackupBrowserWindowButtonBackgroundBrush => TryParseBrush(BackupBrowserWindowButtonBackground);
        public WpfBrush PreviewBackupBrowserWindowButtonHoverBackgroundBrush => TryParseBrush(BackupBrowserWindowButtonHoverBackground);
        public WpfBrush PreviewBackupBrowserWindowXmlFilterHoverBackgroundBrush => TryParseBrush(BackupBrowserWindowXmlFilterHoverBackground);
        public WpfBrush PreviewBackupBrowserWindowXmlFilterSelectedBackgroundBrush => TryParseBrush(BackupBrowserWindowXmlFilterSelectedBackground);
        public WpfBrush PreviewSavedEditsWindowBackgroundBrush => TryParseBrush(SavedEditsWindowBackground);
        public WpfBrush PreviewSavedEditsWindowTextBrush => TryParseBrush(SavedEditsWindowText);
        public WpfBrush PreviewSavedEditsWindowControlBackgroundBrush => TryParseBrush(SavedEditsWindowControlBackground);
        public WpfBrush PreviewSavedEditsWindowControlBorderBrush => TryParseBrush(SavedEditsWindowControlBorder);
        public WpfBrush PreviewSavedEditsWindowTabBackgroundBrush => TryParseBrush(SavedEditsWindowTabBackground);
        public WpfBrush PreviewSavedEditsWindowTabHoverBackgroundBrush => TryParseBrush(SavedEditsWindowTabHoverBackground);
        public WpfBrush PreviewSavedEditsWindowTabSelectedBackgroundBrush => TryParseBrush(SavedEditsWindowTabSelectedBackground);
        public WpfBrush PreviewSavedEditsWindowButtonBackgroundBrush => TryParseBrush(SavedEditsWindowButtonBackground);
        public WpfBrush PreviewSavedEditsWindowButtonHoverBackgroundBrush => TryParseBrush(SavedEditsWindowButtonHoverBackground);
        public WpfBrush PreviewSavedEditsWindowCheckBoxBackgroundBrush => TryParseBrush(SavedEditsWindowCheckBoxBackground);
        public WpfBrush PreviewSavedEditsWindowCheckBoxTickBrush => TryParseBrush(SavedEditsWindowCheckBoxTick);
        public WpfBrush PreviewSavedEditsWindowGridTextBrush => TryParseBrush(SavedEditsWindowGridText);
        public WpfBrush PreviewSavedEditsWindowGridBackgroundBrush => TryParseBrush(SavedEditsWindowGridBackground);
        public WpfBrush PreviewSavedEditsWindowGridBorderBrush => TryParseBrush(SavedEditsWindowGridBorder);
        public WpfBrush PreviewSavedEditsWindowGridLinesBrush => TryParseBrush(SavedEditsWindowGridLines);
        public WpfBrush PreviewSavedEditsWindowGridHeaderBackgroundBrush => TryParseBrush(SavedEditsWindowGridHeaderBackground);
        public WpfBrush PreviewSavedEditsWindowGridHeaderTextBrush => TryParseBrush(SavedEditsWindowGridHeaderText);
        public WpfBrush PreviewSavedEditsWindowGridRowHoverBackgroundBrush => TryParseBrush(SavedEditsWindowGridRowHoverBackground);
        public WpfBrush PreviewSavedEditsWindowGridRowSelectedBackgroundBrush => TryParseBrush(SavedEditsWindowGridRowSelectedBackground);
        public WpfBrush PreviewSavedEditsWindowGridCellSelectedBackgroundBrush => TryParseBrush(SavedEditsWindowGridCellSelectedBackground);
        public WpfBrush PreviewSavedEditsWindowGridCellSelectedTextBrush => TryParseBrush(SavedEditsWindowGridCellSelectedText);
        public WpfBrush PreviewSettingsInfoWindowBackgroundBrush => TryParseBrush(SettingsInfoWindowBackground);
        public WpfBrush PreviewSettingsInfoWindowTextBrush => TryParseBrush(SettingsInfoWindowText);
        public WpfBrush PreviewSettingsInfoWindowControlBackgroundBrush => TryParseBrush(SettingsInfoWindowControlBackground);
        public WpfBrush PreviewSettingsInfoWindowControlBorderBrush => TryParseBrush(SettingsInfoWindowControlBorder);
        public WpfBrush PreviewSettingsInfoWindowHeaderTextBrush => TryParseBrush(SettingsInfoWindowHeaderText);
        public WpfBrush PreviewSettingsInfoWindowHeaderBackgroundBrush => TryParseBrush(SettingsInfoWindowHeaderBackground);
        public WpfBrush PreviewSettingsInfoWindowButtonBackgroundBrush => TryParseBrush(SettingsInfoWindowButtonBackground);
        public WpfBrush PreviewSettingsInfoWindowButtonHoverBackgroundBrush => TryParseBrush(SettingsInfoWindowButtonHoverBackground);
        public WpfBrush PreviewSettingsInfoWindowCheckBoxBackgroundBrush => TryParseBrush(SettingsInfoWindowCheckBoxBackground);
        public WpfBrush PreviewSettingsInfoWindowCheckBoxTickBrush => TryParseBrush(SettingsInfoWindowCheckBoxTick);
        public WpfBrush PreviewDocumentationWindowBackgroundBrush => TryParseBrush(DocumentationWindowBackground);
        public WpfBrush PreviewDocumentationWindowTextBrush => TryParseBrush(DocumentationWindowText);
        public WpfBrush PreviewDocumentationWindowControlBackgroundBrush => TryParseBrush(DocumentationWindowControlBackground);
        public WpfBrush PreviewDocumentationWindowControlBorderBrush => TryParseBrush(DocumentationWindowControlBorder);
        public WpfBrush PreviewDocumentationWindowHeaderTextBrush => TryParseBrush(DocumentationWindowHeaderText);
        public WpfBrush PreviewDocumentationWindowHeaderBackgroundBrush => TryParseBrush(DocumentationWindowHeaderBackground);
        public WpfBrush PreviewDocumentationWindowListHoverBackgroundBrush => TryParseBrush(DocumentationWindowListHoverBackground);
        public WpfBrush PreviewDocumentationWindowListSelectedBackgroundBrush => TryParseBrush(DocumentationWindowListSelectedBackground);
        public WpfBrush PreviewDocumentationWindowButtonBackgroundBrush => TryParseBrush(DocumentationWindowButtonBackground);
        public WpfBrush PreviewDocumentationWindowButtonHoverBackgroundBrush => TryParseBrush(DocumentationWindowButtonHoverBackground);
        public WpfBrush PreviewXmlGuidesWindowBackgroundBrush => TryParseBrush(XmlGuidesWindowBackground);
        public WpfBrush PreviewXmlGuidesWindowTextBrush => TryParseBrush(XmlGuidesWindowText);
        public WpfBrush PreviewXmlGuidesWindowControlBackgroundBrush => TryParseBrush(XmlGuidesWindowControlBackground);
        public WpfBrush PreviewXmlGuidesWindowControlBorderBrush => TryParseBrush(XmlGuidesWindowControlBorder);
        public WpfBrush PreviewXmlGuidesWindowButtonBackgroundBrush => TryParseBrush(XmlGuidesWindowButtonBackground);
        public WpfBrush PreviewXmlGuidesWindowButtonTextBrush => TryParseBrush(XmlGuidesWindowButtonText);
        public WpfBrush PreviewXmlGuidesWindowButtonHoverBackgroundBrush => TryParseBrush(XmlGuidesWindowButtonHoverBackground);
        public WpfBrush PreviewXmlGuidesWindowButtonHoverTextBrush => TryParseBrush(XmlGuidesWindowButtonHoverText);
        public WpfBrush PreviewXmlGuidesWindowButtonSelectedBackgroundBrush => TryParseBrush(XmlGuidesWindowButtonSelectedBackground);
        public WpfBrush PreviewXmlGuidesWindowButtonSelectedTextBrush => TryParseBrush(XmlGuidesWindowButtonSelectedText);
        public WpfBrush PreviewXmlGuidesWindowGuidesListBackgroundBrush => TryParseBrush(XmlGuidesWindowGuidesListBackground);
        public WpfBrush PreviewXmlGuidesWindowGuidesListTextBrush => TryParseBrush(XmlGuidesWindowGuidesListText);
        public WpfBrush PreviewXmlGuidesWindowGuidesListItemHoverBackgroundBrush => TryParseBrush(XmlGuidesWindowGuidesListItemHoverBackground);
        public WpfBrush PreviewXmlGuidesWindowGuidesListItemHoverTextBrush => TryParseBrush(XmlGuidesWindowGuidesListItemHoverText);
        public WpfBrush PreviewXmlGuidesWindowGuidesListItemSelectedBackgroundBrush => TryParseBrush(XmlGuidesWindowGuidesListItemSelectedBackground);
        public WpfBrush PreviewXmlGuidesWindowGuidesListItemSelectedTextBrush => TryParseBrush(XmlGuidesWindowGuidesListItemSelectedText);
        public WpfBrush PreviewXmlGuidesWindowFontPickerTextBrush => TryParseBrush(XmlGuidesWindowFontPickerText);
        public WpfBrush PreviewEditorTextBrush => TryParseBrush(EditorText);
        public WpfBrush PreviewEditorBackgroundBrush => TryParseBrush(EditorBackground);
        public WpfBrush PreviewEditorXmlSyntaxForegroundBrush => TryParseBrush(EditorXmlSyntaxForeground);
        public WpfBrush PreviewMenuTextBrush => TryParseBrush(MenuText);
        public WpfBrush PreviewMenuBackgroundBrush => TryParseBrush(MenuBackground);
        public WpfBrush PreviewTopButtonTextBrush => TryParseBrush(TopButtonText);
        public WpfBrush PreviewTopButtonBackgroundBrush => TryParseBrush(TopButtonBackground);
        public WpfBrush PreviewTreeTextBrush => TryParseBrush(TreeText);
        public WpfBrush PreviewTreeBackgroundBrush => TryParseBrush(TreeBackground);
        public WpfBrush PreviewTreeHoverBrush => TryParseBrush(TreeItemHoverBackground);
        public WpfBrush PreviewTreeSelectedBrush => TryParseBrush(TreeItemSelectedBackground);
        public WpfBrush PreviewPane1TreeHoverBrush => TryParseBrush(Pane1TreeItemHoverBackground);
        public WpfBrush PreviewPane1TreeSelectedBrush => TryParseBrush(Pane1TreeItemSelectedBackground);
        public WpfBrush PreviewGridTextBrush => TryParseBrush(GridText);
        public WpfBrush PreviewGridBackgroundBrush => TryParseBrush(GridBackground);
        public WpfBrush PreviewGridBorderBrush => TryParseBrush(GridBorder);
        public WpfBrush PreviewGridLinesBrush => TryParseBrush(GridLines);
        public WpfBrush PreviewGridRowHoverBrush => TryParseBrush(GridRowHoverBackground);
        public WpfBrush PreviewGridHeaderTextBrush => TryParseBrush(GridHeaderText);
        public WpfBrush PreviewGridRowSelectedBackgroundBrush => TryParseBrush(GridRowSelectedBackground);
        public WpfBrush PreviewGridCellSelectedBackgroundBrush => TryParseBrush(GridCellSelectedBackground);

        public WpfBrush PreviewSearchMatchBackgroundBrush => TryParseBrush(SearchMatchBackground);
        public WpfBrush PreviewSearchMatchTextBrush => TryParseBrush(SearchMatchText);
        public WpfBrush PreviewFieldColumnTextBrush => TryParseBrush(FieldColumnText);
        public WpfBrush PreviewFieldColumnBackgroundBrush => TryParseBrush(FieldColumnBackground);
        public WpfBrush PreviewValueColumnTextBrush => TryParseBrush(ValueColumnText);
        public WpfBrush PreviewValueColumnBackgroundBrush => TryParseBrush(ValueColumnBackground);
        public WpfBrush PreviewHeaderTextBrush => TryParseBrush(HeaderText);
        public WpfBrush PreviewSelectorBackgroundBrush => TryParseBrush(SelectorBackground);
        public WpfBrush PreviewPane2ComboTextBrush => TryParseBrush(Pane2ComboText);
        public WpfBrush PreviewPane2ComboBackgroundBrush => TryParseBrush(Pane2ComboBackground);
        public WpfBrush PreviewPane2DropdownTextBrush => TryParseBrush(Pane2DropdownText);
        public WpfBrush PreviewPane2DropdownBackgroundBrush => TryParseBrush(Pane2DropdownBackground);
        public WpfBrush PreviewPane2ItemHoverBackgroundBrush => TryParseBrush(Pane2ItemHoverBackground);
        public WpfBrush PreviewPane2ItemSelectedBackgroundBrush => TryParseBrush(Pane2ItemSelectedBackground);

        public void OnViewReady()
        {
            if (_isViewReady)
                return;

            _isViewReady = true;

            OnPropertyChanged(nameof(IsEditingDarkMode));
            OnPropertyChanged(nameof(IsEditingLightMode));
            OnPropertyChanged(nameof(IsEditingFriendlyView));
            OnPropertyChanged(nameof(IsEditingRawXml));

            ApplyPreviewIfEditingCurrentTheme();
        }

        public void RevertPreview()
        {
            _appearance.IsDarkMode = _originalIsDarkMode;
            _appearance.IsFriendlyView = _originalIsFriendlyView;
            _appearance.ReplaceSettings(CloneAppearance(_originalFromSettings));
        }
        public bool TryCommit()
        {
            var profile = GetEditingProfile();
            WriteToProfile(profile);

            _appSettings.Appearance = CloneAppearance(_workingCopy);
            _appearance.ReplaceSettings(CloneAppearance(_workingCopy));

            try
            {
                _settingsService.Save(_appSettings);
            }
            catch
            {
            }

            IsDirty = false;
            return true;
        }

        private void PickColor(string? key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return;

            var currentHex = GetColorByKey(key);
            var initial = TryParseDrawingColor(currentHex, out var c) ? c : System.Drawing.Color.White;

            var customColors = _appSettings.Appearance.ColorPickerCustomColors;
            if (customColors is null || customColors.Length != 16)
                customColors = _appSettings.Appearance.ColorPickerCustomColors = new int[16];

            using var dlg = new ColorDialog
            {
                AllowFullOpen = true,
                FullOpen = false,
                Color = initial,
                CustomColors = customColors
            };

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            var savedCustomColors = dlg.CustomColors;
            if (savedCustomColors is not null && savedCustomColors.Length == 16)
            {
                _appSettings.Appearance.ColorPickerCustomColors = savedCustomColors;
                try
                {
                    _settingsService.Save(_appSettings);
                }
                catch
                {
                }
            }

            var hex = $"#{dlg.Color.A:X2}{dlg.Color.R:X2}{dlg.Color.G:X2}{dlg.Color.B:X2}";
            SetColorByKey(key, hex);
        }
        private void Apply()
        {
            var profile = GetEditingProfile();
            WriteToProfile(profile);

            _appSettings.Appearance = CloneAppearance(_workingCopy);
            _appearance.ReplaceSettings(CloneAppearance(_workingCopy));

            IsDirty = false;
        }

        private void Ok()
        {
            TryCommit();
            CloseRequested?.Invoke(this, true);
        }

        private void Cancel()
        {
            CloseRequested?.Invoke(this, false);
        }

        private static readonly Dictionary<string, string[]> TabDefaultProperties = new(StringComparer.Ordinal)
        {
            ["Fonts"] = new[]
    {
        nameof(AppearanceProfileSettings.UiFontFamily),
        nameof(AppearanceProfileSettings.UiFontSize),
        nameof(AppearanceProfileSettings.EditorFontFamily),
        nameof(AppearanceProfileSettings.EditorFontSize)
    },
            ["Appearance Window"] = new[]
    {
        nameof(AppearanceProfileSettings.AppearanceWindowBackground),
        nameof(AppearanceProfileSettings.AppearanceWindowText),
        nameof(AppearanceProfileSettings.AppearanceWindowControlBackground),
        nameof(AppearanceProfileSettings.AppearanceWindowControlBorder),
        nameof(AppearanceProfileSettings.AppearanceWindowHeaderText),
        nameof(AppearanceProfileSettings.AppearanceWindowHeaderBackground),
        nameof(AppearanceProfileSettings.AppearanceWindowTabBackground),
        nameof(AppearanceProfileSettings.AppearanceWindowTabHoverBackground),
        nameof(AppearanceProfileSettings.AppearanceWindowTabSelectedBackground),
        nameof(AppearanceProfileSettings.AppearanceWindowButtonBackground),
        nameof(AppearanceProfileSettings.AppearanceWindowButtonHoverBackground)
    },
            ["Shared Config Packs"] = new[]
    {
        nameof(AppearanceProfileSettings.SharedConfigPacksWindowBackground),
        nameof(AppearanceProfileSettings.SharedConfigPacksWindowText),
        nameof(AppearanceProfileSettings.SharedConfigPacksWindowControlBackground),
        nameof(AppearanceProfileSettings.SharedConfigPacksWindowControlBorder),
        nameof(AppearanceProfileSettings.SharedConfigPacksWindowHeaderText),
        nameof(AppearanceProfileSettings.SharedConfigPacksWindowHeaderBackground),
        nameof(AppearanceProfileSettings.SharedConfigPacksWindowTabBackground),
        nameof(AppearanceProfileSettings.SharedConfigPacksWindowTabHoverBackground),
        nameof(AppearanceProfileSettings.SharedConfigPacksWindowTabSelectedBackground),
        nameof(AppearanceProfileSettings.SharedConfigPacksWindowButtonBackground),
        nameof(AppearanceProfileSettings.SharedConfigPacksWindowButtonHoverBackground),
        nameof(AppearanceProfileSettings.SharedConfigPacksWindowEditsHoverBackground),
        nameof(AppearanceProfileSettings.SharedConfigPacksWindowCheckBoxBackground),
        nameof(AppearanceProfileSettings.SharedConfigPacksWindowCheckBoxTick)
    },
            ["Compare XML"] = new[]
    {
        nameof(AppearanceProfileSettings.CompareXmlWindowBackground),
        nameof(AppearanceProfileSettings.CompareXmlWindowText),
        nameof(AppearanceProfileSettings.CompareXmlWindowControlBackground),
        nameof(AppearanceProfileSettings.CompareXmlWindowControlBorder),
        nameof(AppearanceProfileSettings.CompareXmlWindowHeaderText),
        nameof(AppearanceProfileSettings.CompareXmlWindowHeaderBackground),
        nameof(AppearanceProfileSettings.CompareXmlWindowButtonBackground),
        nameof(AppearanceProfileSettings.CompareXmlWindowButtonHoverBackground),
        nameof(AppearanceProfileSettings.CompareXmlWindowEditsHoverBackground),
        nameof(AppearanceProfileSettings.CompareXmlWindowCheckBoxBackground),
        nameof(AppearanceProfileSettings.CompareXmlWindowCheckBoxTick)
    },
            ["Backup Browser"] = new[]
    {
        nameof(AppearanceProfileSettings.BackupBrowserWindowBackground),
        nameof(AppearanceProfileSettings.BackupBrowserWindowText),
        nameof(AppearanceProfileSettings.BackupBrowserWindowControlBackground),
        nameof(AppearanceProfileSettings.BackupBrowserWindowControlBorder),
        nameof(AppearanceProfileSettings.BackupBrowserWindowHeaderText),
        nameof(AppearanceProfileSettings.BackupBrowserWindowHeaderBackground),
        nameof(AppearanceProfileSettings.BackupBrowserWindowListHoverBackground),
        nameof(AppearanceProfileSettings.BackupBrowserWindowListSelectedBackground),
        nameof(AppearanceProfileSettings.BackupBrowserWindowButtonBackground),
        nameof(AppearanceProfileSettings.BackupBrowserWindowButtonHoverBackground),
        nameof(AppearanceProfileSettings.BackupBrowserWindowXmlFilterHoverBackground),
        nameof(AppearanceProfileSettings.BackupBrowserWindowXmlFilterSelectedBackground)
    },
            ["Settings & Info"] = new[]
    {
        nameof(AppearanceProfileSettings.SettingsInfoWindowBackground),
        nameof(AppearanceProfileSettings.SettingsInfoWindowText),
        nameof(AppearanceProfileSettings.SettingsInfoWindowControlBackground),
        nameof(AppearanceProfileSettings.SettingsInfoWindowControlBorder),
        nameof(AppearanceProfileSettings.SettingsInfoWindowHeaderText),
        nameof(AppearanceProfileSettings.SettingsInfoWindowHeaderBackground),
        nameof(AppearanceProfileSettings.SettingsInfoWindowButtonBackground),
        nameof(AppearanceProfileSettings.SettingsInfoWindowButtonHoverBackground),
        nameof(AppearanceProfileSettings.SettingsInfoWindowCheckBoxBackground),
        nameof(AppearanceProfileSettings.SettingsInfoWindowCheckBoxTick)
    },
            ["Documentation"] = new[]
    {
        nameof(AppearanceProfileSettings.DocumentationWindowBackground),
        nameof(AppearanceProfileSettings.DocumentationWindowText),
        nameof(AppearanceProfileSettings.DocumentationWindowControlBackground),
        nameof(AppearanceProfileSettings.DocumentationWindowControlBorder),
        nameof(AppearanceProfileSettings.DocumentationWindowHeaderText),
        nameof(AppearanceProfileSettings.DocumentationWindowHeaderBackground),
        nameof(AppearanceProfileSettings.DocumentationWindowListHoverBackground),
        nameof(AppearanceProfileSettings.DocumentationWindowListSelectedBackground),
        nameof(AppearanceProfileSettings.DocumentationWindowButtonBackground),
        nameof(AppearanceProfileSettings.DocumentationWindowButtonHoverBackground)
    },
            ["LSR XML Guides"] = new[]
    {
        nameof(AppearanceProfileSettings.XmlGuidesWindowBackground),
        nameof(AppearanceProfileSettings.XmlGuidesWindowText),
        nameof(AppearanceProfileSettings.XmlGuidesWindowControlBackground),
        nameof(AppearanceProfileSettings.XmlGuidesWindowControlBorder)
    },
            ["Editor"] = new[]
    {
        nameof(AppearanceProfileSettings.EditorText),
        nameof(AppearanceProfileSettings.EditorBackground),
        nameof(AppearanceProfileSettings.EditorXmlSyntaxForeground)
    },
            ["Saved Edits"] = new[]
    {
        nameof(AppearanceProfileSettings.SavedEditsWindowBackground),
        nameof(AppearanceProfileSettings.SavedEditsWindowText),
        nameof(AppearanceProfileSettings.SavedEditsWindowControlBackground),
        nameof(AppearanceProfileSettings.SavedEditsWindowControlBorder),
        nameof(AppearanceProfileSettings.SavedEditsWindowTabBackground),
        nameof(AppearanceProfileSettings.SavedEditsWindowTabHoverBackground),
        nameof(AppearanceProfileSettings.SavedEditsWindowTabSelectedBackground),
        nameof(AppearanceProfileSettings.SavedEditsWindowButtonBackground),
        nameof(AppearanceProfileSettings.SavedEditsWindowButtonHoverBackground),
        nameof(AppearanceProfileSettings.SavedEditsWindowCheckBoxBackground),
        nameof(AppearanceProfileSettings.SavedEditsWindowCheckBoxTick),
        nameof(AppearanceProfileSettings.SavedEditsWindowGridText),
        nameof(AppearanceProfileSettings.SavedEditsWindowGridBackground),
        nameof(AppearanceProfileSettings.SavedEditsWindowGridBorder),
        nameof(AppearanceProfileSettings.SavedEditsWindowGridLines),
        nameof(AppearanceProfileSettings.SavedEditsWindowGridHeaderBackground),
        nameof(AppearanceProfileSettings.SavedEditsWindowGridHeaderText),
        nameof(AppearanceProfileSettings.SavedEditsWindowGridRowHoverBackground),
        nameof(AppearanceProfileSettings.SavedEditsWindowGridRowSelectedBackground),
        nameof(AppearanceProfileSettings.SavedEditsWindowGridCellSelectedBackground),
        nameof(AppearanceProfileSettings.SavedEditsWindowGridCellSelectedText)
    },
            ["Top Menu Bar"] = new[]
    {
        nameof(AppearanceProfileSettings.MenuText),
        nameof(AppearanceProfileSettings.MenuBackground),
        nameof(AppearanceProfileSettings.TreeItemHoverBackground),
        nameof(AppearanceProfileSettings.TreeItemSelectedBackground),
        nameof(AppearanceProfileSettings.XmlGuidesWindowButtonBackground),
        nameof(AppearanceProfileSettings.XmlGuidesWindowButtonText),
        nameof(AppearanceProfileSettings.XmlGuidesWindowButtonHoverBackground),
        nameof(AppearanceProfileSettings.XmlGuidesWindowButtonHoverText),
        nameof(AppearanceProfileSettings.XmlGuidesWindowGuidesListBackground),
        nameof(AppearanceProfileSettings.XmlGuidesWindowGuidesListText),
        nameof(AppearanceProfileSettings.XmlGuidesWindowGuidesListItemHoverBackground),
        nameof(AppearanceProfileSettings.XmlGuidesWindowGuidesListItemHoverText),
        nameof(AppearanceProfileSettings.XmlGuidesWindowGuidesListItemSelectedBackground),
        nameof(AppearanceProfileSettings.XmlGuidesWindowGuidesListItemSelectedText),
        nameof(AppearanceProfileSettings.XmlGuidesWindowFontPickerText)
    },
            ["Top Bar Controls"] = new[]
    {
        nameof(AppearanceProfileSettings.TopButtonText),
        nameof(AppearanceProfileSettings.TopButtonBackground),
        nameof(AppearanceProfileSettings.GridText)
    },
            ["Pane 1"] = new[]
    {
        nameof(AppearanceProfileSettings.TreeText),
        nameof(AppearanceProfileSettings.TreeBackground),
        nameof(AppearanceProfileSettings.Pane1TreeItemHoverBackground),
        nameof(AppearanceProfileSettings.Pane1TreeItemSelectedBackground)
    },
            ["Pane 2"] = new[]
    {
        nameof(AppearanceProfileSettings.HeaderText),
        nameof(AppearanceProfileSettings.SelectorBackground),
        nameof(AppearanceProfileSettings.Pane2ComboText),
        nameof(AppearanceProfileSettings.Pane2ComboBackground),
        nameof(AppearanceProfileSettings.Pane2DropdownText),
        nameof(AppearanceProfileSettings.Pane2DropdownBackground),
        nameof(AppearanceProfileSettings.Pane2ItemHoverBackground),
        nameof(AppearanceProfileSettings.Pane2ItemSelectedBackground)
    },
            ["Pane 3"] = new[]
    {
        nameof(AppearanceProfileSettings.GridBorder),
        nameof(AppearanceProfileSettings.GridLines),
        nameof(AppearanceProfileSettings.GridBackground),
        nameof(AppearanceProfileSettings.GridRowHoverBackground),
        nameof(AppearanceProfileSettings.GridHeaderText),
        nameof(AppearanceProfileSettings.GridRowSelectedBackground),
        nameof(AppearanceProfileSettings.GridCellSelectedBackground),
        nameof(AppearanceProfileSettings.FieldColumnText),
        nameof(AppearanceProfileSettings.FieldColumnBackground),
        nameof(AppearanceProfileSettings.ValueColumnText),
        nameof(AppearanceProfileSettings.ValueColumnBackground),
        nameof(AppearanceProfileSettings.SearchMatchBackground),
        nameof(AppearanceProfileSettings.SearchMatchText)
    }
        };

        private bool ConfirmReset(string message, string caption)
        {
            var result = System.Windows.MessageBox.Show(
                message,
                caption,
                System.Windows.MessageBoxButton.YesNo,
                System.Windows.MessageBoxImage.Warning);

            return result == System.Windows.MessageBoxResult.Yes;
        }

        private void ResetTabToDefaults(string? tabHeader)
        {
            if (string.IsNullOrWhiteSpace(tabHeader))
                return;

            if (!TabDefaultProperties.TryGetValue(tabHeader, out var properties))
                return;

            var ok = ConfirmReset(
                $"Reset \"{tabHeader}\" tab to defaults?\n\nThis will overwrite any appearance changes in this tab.",
                "Reset tab");

            if (!ok)
                return;

            ResetPropertiesToDefaults(properties);
        }

        private void ResetAllToDefaults()
        {
            var ok = ConfirmReset(
                "Reset all tabs to defaults?\n\nThis will overwrite any appearance changes in every tab.",
                "Reset all");

            if (!ok)
                return;

            var defaults = IsEditingDarkMode
                ? AppearanceProfileSettings.CreateDarkDefaults()
                : AppearanceProfileSettings.CreateLightDefaults();

            LoadFromProfile(defaults);

            IsDirty = true;

            var profile = GetEditingProfile();
            WriteToProfile(profile);

            RaisePreview();
            ApplyPreviewIfEditingCurrentTheme();
        }

        private bool CanSaveAppearanceProfile() => !string.IsNullOrWhiteSpace(ProfileNameInput);

        private bool CanApplySelectedAppearanceProfile() => SelectedAppearanceProfile is not null;

        private bool CanDeleteSelectedAppearanceProfile() => SelectedAppearanceProfile is not null;

        private void SaveAppearanceProfile()
        {
            var name = (ProfileNameInput ?? "").Trim();
            if (string.IsNullOrWhiteSpace(name))
                return;

            var existing = _workingCopy.Profiles.FirstOrDefault(p => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase));
            if (existing is not null)
            {
                var ok = ConfirmReset(
                    $"Overwrite profile \"{existing.Name}\"?\n\nThis will replace the saved profile contents.",
                    "Save profile");

                if (!ok)
                    return;

                var existingIndex = _workingCopy.Profiles.IndexOf(existing);
                if (existingIndex >= 0)
                    _workingCopy.Profiles.RemoveAt(existingIndex);

                _appearanceProfiles.Remove(existing);
            }

            var snapshot = new NamedAppearanceProfile
            {
                Name = name,
                Dark = CloneProfile(_workingCopy.Dark),
                Light = CloneProfile(_workingCopy.Light)
            };

            _workingCopy.Profiles.Add(snapshot);
            _appearanceProfiles.Add(snapshot);

            _workingCopy.ActiveProfileName = name;
            SelectedAppearanceProfile = snapshot;

            IsDirty = true;
        }

        private void ApplySelectedAppearanceProfile()
        {
            var profile = SelectedAppearanceProfile;
            if (profile is null)
                return;

            var ok = ConfirmReset(
                $"Load profile \"{profile.Name}\"?\n\nThis will overwrite the appearance values currently shown in this window.",
                "Load profile");

            if (!ok)
                return;

            _workingCopy.Dark = CloneProfile(profile.Dark);
            _workingCopy.Light = CloneProfile(profile.Light);
            _workingCopy.ActiveProfileName = profile.Name;

            LoadFromProfile(GetEditingProfile());

            IsDirty = true;

            RaisePreview();
            ApplyPreviewIfEditingCurrentTheme();
        }

        private void DeleteSelectedAppearanceProfile()
        {
            var profile = SelectedAppearanceProfile;
            if (profile is null)
                return;

            var ok = ConfirmReset(
                $"Delete profile \"{profile.Name}\"?\n\nThis cannot be undone.",
                "Delete profile");

            if (!ok)
                return;

            _workingCopy.Profiles.Remove(profile);
            _appearanceProfiles.Remove(profile);

            if (string.Equals(_workingCopy.ActiveProfileName, profile.Name, StringComparison.OrdinalIgnoreCase))
                _workingCopy.ActiveProfileName = "";

            SelectedAppearanceProfile = null;

            IsDirty = true;
        }

        private void ResetPropertiesToDefaults(IEnumerable<string> propertyNames)
        {
            var defaults = IsEditingDarkMode
                ? AppearanceProfileSettings.CreateDarkDefaults()
                : AppearanceProfileSettings.CreateLightDefaults();

            _suppressPreview = true;
            try
            {
                foreach (var name in propertyNames)
                    ApplyDefaultToProperty(defaults, name);
            }
            finally
            {
                _suppressPreview = false;
            }

            foreach (var name in propertyNames)
                OnPropertyChanged(name);

            IsDirty = true;

            var profile = GetEditingProfile();
            WriteToProfile(profile);

            RaisePreview();
            ApplyPreviewIfEditingCurrentTheme();
        }

        private void ApplyDefaultToProperty(AppearanceProfileSettings defaults, string propertyName)
        {
            var profileProperty = typeof(AppearanceProfileSettings).GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public);
            if (profileProperty is null)
                return;

            var raw = profileProperty.GetValue(defaults);
            var text = raw switch
            {
                null => string.Empty,
                double d => d.ToString(CultureInfo.InvariantCulture),
                int i => i.ToString(CultureInfo.InvariantCulture),
                bool b => b.ToString(),
                _ => raw.ToString() ?? string.Empty
            };

            var fieldName = "_" + char.ToLowerInvariant(propertyName[0]) + propertyName.Substring(1);
            var field = GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            if (field is null)
                return;

            field.SetValue(this, text);
        }

        private void SwitchEditingProfile()
        {
            var profile = GetEditingProfile();
            LoadFromProfile(profile);

            ApplyPreviewIfEditingCurrentTheme();
        }

        private void OnEdited()
        {
            IsDirty = true;

            RaisePreview();

            if (_suppressPreview)
                return;

            var profile = GetEditingProfile();
            WriteToProfile(profile);

            ApplyPreviewIfEditingCurrentTheme();
        }

        private void ApplyPreviewIfEditingCurrentTheme()
        {
            _appearance.IsDarkMode = IsEditingDarkMode;
            _appearance.IsFriendlyView = IsEditingFriendlyView;

            _appearance.ReplaceSettings(CloneAppearance(_workingCopy));
        }

        private AppearanceProfileSettings GetEditingProfile()
        {
            return _workingCopy.GetActiveProfile(IsEditingDarkMode);
        }

        void LoadFromProfile(AppearanceProfileSettings p)
        {
            _suppressPreview = true;
            try
            {
                _uiFontFamily = p.UiFontFamily;
                _uiFontSize = p.UiFontSize.ToString(CultureInfo.InvariantCulture);

                _editorFontFamily = p.EditorFontFamily;
                _editorFontSize = p.EditorFontSize.ToString(CultureInfo.InvariantCulture);

                _text = p.Text;
                _background = p.Background;
                _appearanceWindowBackground = p.AppearanceWindowBackground;
                _appearanceWindowText = p.AppearanceWindowText;
                _appearanceWindowControlBackground = p.AppearanceWindowControlBackground;
                _appearanceWindowControlBorder = p.AppearanceWindowControlBorder;
                _appearanceWindowHeaderText = p.AppearanceWindowHeaderText;
                _appearanceWindowHeaderBackground = p.AppearanceWindowHeaderBackground;
                _appearanceWindowTabBackground = p.AppearanceWindowTabBackground;
                _appearanceWindowTabHoverBackground = p.AppearanceWindowTabHoverBackground;
                _appearanceWindowTabSelectedBackground = p.AppearanceWindowTabSelectedBackground;
                _appearanceWindowButtonBackground = p.AppearanceWindowButtonBackground;
                _appearanceWindowButtonHoverBackground = p.AppearanceWindowButtonHoverBackground;

                _sharedConfigPacksWindowBackground = p.SharedConfigPacksWindowBackground;
                _sharedConfigPacksWindowText = p.SharedConfigPacksWindowText;
                _sharedConfigPacksWindowControlBackground = p.SharedConfigPacksWindowControlBackground;
                _sharedConfigPacksWindowControlBorder = p.SharedConfigPacksWindowControlBorder;
                _sharedConfigPacksWindowHeaderText = p.SharedConfigPacksWindowHeaderText;
                _sharedConfigPacksWindowHeaderBackground = p.SharedConfigPacksWindowHeaderBackground;
                _sharedConfigPacksWindowTabBackground = p.SharedConfigPacksWindowTabBackground;
                _sharedConfigPacksWindowTabHoverBackground = p.SharedConfigPacksWindowTabHoverBackground;
                _sharedConfigPacksWindowTabSelectedBackground = p.SharedConfigPacksWindowTabSelectedBackground;
                _sharedConfigPacksWindowButtonBackground = p.SharedConfigPacksWindowButtonBackground;
                _sharedConfigPacksWindowButtonHoverBackground = p.SharedConfigPacksWindowButtonHoverBackground;
                _sharedConfigPacksWindowEditsHoverBackground = p.SharedConfigPacksWindowEditsHoverBackground;
                _sharedConfigPacksWindowCheckBoxBackground = p.SharedConfigPacksWindowCheckBoxBackground;
                _sharedConfigPacksWindowCheckBoxTick = p.SharedConfigPacksWindowCheckBoxTick;

                _compareXmlWindowBackground = p.CompareXmlWindowBackground;
                _compareXmlWindowText = p.CompareXmlWindowText;
                _compareXmlWindowControlBackground = p.CompareXmlWindowControlBackground;
                _compareXmlWindowControlBorder = p.CompareXmlWindowControlBorder;
                _compareXmlWindowHeaderText = p.CompareXmlWindowHeaderText;
                _compareXmlWindowHeaderBackground = p.CompareXmlWindowHeaderBackground;
                _compareXmlWindowButtonBackground = p.CompareXmlWindowButtonBackground;
                _compareXmlWindowButtonHoverBackground = p.CompareXmlWindowButtonHoverBackground;
                _compareXmlWindowEditsHoverBackground = p.CompareXmlWindowEditsHoverBackground;
                _compareXmlWindowCheckBoxBackground = p.CompareXmlWindowCheckBoxBackground;
                _compareXmlWindowCheckBoxTick = p.CompareXmlWindowCheckBoxTick;

                _backupBrowserWindowBackground = p.BackupBrowserWindowBackground;
                _backupBrowserWindowText = p.BackupBrowserWindowText;
                _backupBrowserWindowControlBackground = p.BackupBrowserWindowControlBackground;
                _backupBrowserWindowControlBorder = p.BackupBrowserWindowControlBorder;
                _backupBrowserWindowHeaderText = p.BackupBrowserWindowHeaderText;
                _backupBrowserWindowHeaderBackground = p.BackupBrowserWindowHeaderBackground;
                _backupBrowserWindowListHoverBackground = p.BackupBrowserWindowListHoverBackground;
                _backupBrowserWindowListSelectedBackground = p.BackupBrowserWindowListSelectedBackground;
                _backupBrowserWindowButtonBackground = p.BackupBrowserWindowButtonBackground;
                _backupBrowserWindowButtonHoverBackground = p.BackupBrowserWindowButtonHoverBackground;
                _backupBrowserWindowXmlFilterHoverBackground = p.BackupBrowserWindowXmlFilterHoverBackground;
                _backupBrowserWindowXmlFilterSelectedBackground = p.BackupBrowserWindowXmlFilterSelectedBackground;

                _settingsInfoWindowBackground = p.SettingsInfoWindowBackground;
                _settingsInfoWindowText = p.SettingsInfoWindowText;
                _settingsInfoWindowControlBackground = p.SettingsInfoWindowControlBackground;
                _settingsInfoWindowControlBorder = p.SettingsInfoWindowControlBorder;
                _settingsInfoWindowHeaderText = p.SettingsInfoWindowHeaderText;
                _settingsInfoWindowHeaderBackground = p.SettingsInfoWindowHeaderBackground;
                _settingsInfoWindowButtonBackground = p.SettingsInfoWindowButtonBackground;
                _settingsInfoWindowButtonHoverBackground = p.SettingsInfoWindowButtonHoverBackground;
                _settingsInfoWindowCheckBoxBackground = p.SettingsInfoWindowCheckBoxBackground;
                _settingsInfoWindowCheckBoxTick = p.SettingsInfoWindowCheckBoxTick;

                _documentationWindowBackground = p.DocumentationWindowBackground;
                _documentationWindowText = p.DocumentationWindowText;
                _documentationWindowControlBackground = p.DocumentationWindowControlBackground;
                _documentationWindowControlBorder = p.DocumentationWindowControlBorder;
                _documentationWindowHeaderText = p.DocumentationWindowHeaderText;
                _documentationWindowHeaderBackground = p.DocumentationWindowHeaderBackground;
                _documentationWindowListHoverBackground = p.DocumentationWindowListHoverBackground;
                _documentationWindowListSelectedBackground = p.DocumentationWindowListSelectedBackground;
                _documentationWindowButtonBackground = p.DocumentationWindowButtonBackground;
                _documentationWindowButtonHoverBackground = p.DocumentationWindowButtonHoverBackground;

                _xmlGuidesWindowBackground = p.XmlGuidesWindowBackground;
                _xmlGuidesWindowText = p.XmlGuidesWindowText;
                _xmlGuidesWindowControlBackground = p.XmlGuidesWindowControlBackground;
                _xmlGuidesWindowControlBorder = p.XmlGuidesWindowControlBorder;
                _xmlGuidesWindowButtonBackground = p.XmlGuidesWindowButtonBackground;
                _xmlGuidesWindowButtonText = p.XmlGuidesWindowButtonText;
                _xmlGuidesWindowButtonHoverBackground = p.XmlGuidesWindowButtonHoverBackground;
                _xmlGuidesWindowButtonHoverText = p.XmlGuidesWindowButtonHoverText;
                _xmlGuidesWindowGuidesListBackground = p.XmlGuidesWindowGuidesListBackground;
                _xmlGuidesWindowGuidesListText = p.XmlGuidesWindowGuidesListText;
                _xmlGuidesWindowGuidesListItemHoverBackground = p.XmlGuidesWindowGuidesListItemHoverBackground;
                _xmlGuidesWindowGuidesListItemHoverText = p.XmlGuidesWindowGuidesListItemHoverText;
                _xmlGuidesWindowGuidesListItemSelectedBackground = p.XmlGuidesWindowGuidesListItemSelectedBackground;
                _xmlGuidesWindowGuidesListItemSelectedText = p.XmlGuidesWindowGuidesListItemSelectedText;
                _xmlGuidesWindowFontPickerText = p.XmlGuidesWindowFontPickerText;

                _savedEditsWindowBackground = p.SavedEditsWindowBackground;
                _savedEditsWindowText = p.SavedEditsWindowText;
                _savedEditsWindowControlBackground = p.SavedEditsWindowControlBackground;
                _savedEditsWindowControlBorder = p.SavedEditsWindowControlBorder;
                _savedEditsWindowTabBackground = p.SavedEditsWindowTabBackground;
                _savedEditsWindowTabHoverBackground = p.SavedEditsWindowTabHoverBackground;
                _savedEditsWindowTabSelectedBackground = p.SavedEditsWindowTabSelectedBackground;
                _savedEditsWindowButtonBackground = p.SavedEditsWindowButtonBackground;
                _savedEditsWindowButtonHoverBackground = p.SavedEditsWindowButtonHoverBackground;
                _savedEditsWindowCheckBoxBackground = p.SavedEditsWindowCheckBoxBackground;
                _savedEditsWindowCheckBoxTick = p.SavedEditsWindowCheckBoxTick;
                _savedEditsWindowGridText = p.SavedEditsWindowGridText;
                _savedEditsWindowGridBackground = p.SavedEditsWindowGridBackground;
                _savedEditsWindowGridBorder = p.SavedEditsWindowGridBorder;
                _savedEditsWindowGridLines = p.SavedEditsWindowGridLines;
                _savedEditsWindowGridHeaderBackground = p.SavedEditsWindowGridHeaderBackground;
                _savedEditsWindowGridHeaderText = p.SavedEditsWindowGridHeaderText;
                _savedEditsWindowGridRowHoverBackground = p.SavedEditsWindowGridRowHoverBackground;
                _savedEditsWindowGridRowSelectedBackground = p.SavedEditsWindowGridRowSelectedBackground;
                _savedEditsWindowGridCellSelectedBackground = p.SavedEditsWindowGridCellSelectedBackground;
                _savedEditsWindowGridCellSelectedText = p.SavedEditsWindowGridCellSelectedText;

                _editorText = p.EditorText;
                _editorBackground = p.EditorBackground;
                _editorXmlSyntaxForeground = p.EditorXmlSyntaxForeground;

                _menuText = p.MenuText;
                _menuBackground = p.MenuBackground;
                _topButtonText = p.TopButtonText;
                _topButtonBackground = p.TopButtonBackground;

                if (_isEditingFriendlyView)
                {
                    _treeText = p.FriendlyTreeText;
                    _treeBackground = p.FriendlyTreeBackground;
                    _treeItemHoverBackground = p.FriendlyTreeItemHoverBackground;
                    _treeItemSelectedBackground = p.FriendlyTreeItemSelectedBackground;
                    _pane1TreeItemHoverBackground = p.FriendlyPane1TreeItemHoverBackground;
                    _pane1TreeItemSelectedBackground = p.FriendlyPane1TreeItemSelectedBackground;
                }
                else
                {
                    _treeText = p.RawTreeText;
                    _treeBackground = p.RawTreeBackground;
                    _treeItemHoverBackground = p.RawTreeItemHoverBackground;
                    _treeItemSelectedBackground = p.RawTreeItemSelectedBackground;
                    _pane1TreeItemHoverBackground = p.RawPane1TreeItemHoverBackground;
                    _pane1TreeItemSelectedBackground = p.RawPane1TreeItemSelectedBackground;
                }

                _gridText = p.GridText;
                _gridBackground = p.GridBackground;
                _gridBorder = p.GridBorder;
                _gridLines = p.GridLines;
                _gridRowHoverBackground = p.GridRowHoverBackground;
                _gridHeaderText = p.GridHeaderText;
                _gridRowSelectedBackground = p.GridRowSelectedBackground;
                _gridCellSelectedBackground = p.GridCellSelectedBackground;
                _searchMatchBackground = p.SearchMatchBackground;
                _searchMatchText = p.SearchMatchText;

                _fieldColumnText = p.FieldColumnText;
                _fieldColumnBackground = p.FieldColumnBackground;
                _valueColumnText = p.ValueColumnText;
                _valueColumnBackground = p.ValueColumnBackground;
                _headerText = p.HeaderText;
                _selectorBackground = p.SelectorBackground;

                _pane2ComboText = p.Pane2ComboText;
                _pane2ComboBackground = p.Pane2ComboBackground;
                _pane2DropdownText = p.Pane2DropdownText;
                _pane2DropdownBackground = p.Pane2DropdownBackground;
                _pane2ItemHoverBackground = p.Pane2ItemHoverBackground;
                _pane2ItemSelectedBackground = p.Pane2ItemSelectedBackground;

                OnPropertyChanged(nameof(UiFontFamily));
                OnPropertyChanged(nameof(UiFontSize));
                OnPropertyChanged(nameof(PreviewUiFontFamily));
                OnPropertyChanged(nameof(PreviewUiFontSize));

                OnPropertyChanged(nameof(AppearanceWindowBackground));
                OnPropertyChanged(nameof(AppearanceWindowText));
                OnPropertyChanged(nameof(AppearanceWindowControlBackground));
                OnPropertyChanged(nameof(AppearanceWindowControlBorder));
                OnPropertyChanged(nameof(AppearanceWindowHeaderText));
                OnPropertyChanged(nameof(AppearanceWindowHeaderBackground));
                OnPropertyChanged(nameof(AppearanceWindowTabBackground));
                OnPropertyChanged(nameof(AppearanceWindowTabHoverBackground));
                OnPropertyChanged(nameof(AppearanceWindowTabSelectedBackground));
                OnPropertyChanged(nameof(AppearanceWindowButtonBackground));
                OnPropertyChanged(nameof(AppearanceWindowButtonHoverBackground));

                OnPropertyChanged(nameof(SharedConfigPacksWindowBackground));
                OnPropertyChanged(nameof(SharedConfigPacksWindowText));
                OnPropertyChanged(nameof(SharedConfigPacksWindowControlBackground));
                OnPropertyChanged(nameof(SharedConfigPacksWindowControlBorder));
                OnPropertyChanged(nameof(SharedConfigPacksWindowHeaderText));
                OnPropertyChanged(nameof(SharedConfigPacksWindowHeaderBackground));
                OnPropertyChanged(nameof(SharedConfigPacksWindowTabBackground));
                OnPropertyChanged(nameof(SharedConfigPacksWindowTabHoverBackground));
                OnPropertyChanged(nameof(SharedConfigPacksWindowTabSelectedBackground));
                OnPropertyChanged(nameof(SharedConfigPacksWindowButtonBackground));
                OnPropertyChanged(nameof(SharedConfigPacksWindowButtonHoverBackground));
                OnPropertyChanged(nameof(SharedConfigPacksWindowEditsHoverBackground));
                OnPropertyChanged(nameof(SharedConfigPacksWindowCheckBoxBackground));
                OnPropertyChanged(nameof(SharedConfigPacksWindowCheckBoxTick));

                OnPropertyChanged(nameof(CompareXmlWindowBackground));
                OnPropertyChanged(nameof(CompareXmlWindowText));
                OnPropertyChanged(nameof(CompareXmlWindowControlBackground));
                OnPropertyChanged(nameof(CompareXmlWindowControlBorder));
                OnPropertyChanged(nameof(CompareXmlWindowHeaderText));
                OnPropertyChanged(nameof(CompareXmlWindowHeaderBackground));
                OnPropertyChanged(nameof(CompareXmlWindowButtonBackground));
                OnPropertyChanged(nameof(CompareXmlWindowButtonHoverBackground));
                OnPropertyChanged(nameof(CompareXmlWindowEditsHoverBackground));
                OnPropertyChanged(nameof(CompareXmlWindowCheckBoxBackground));
                OnPropertyChanged(nameof(CompareXmlWindowCheckBoxTick));

                OnPropertyChanged(nameof(BackupBrowserWindowBackground));
                OnPropertyChanged(nameof(BackupBrowserWindowText));
                OnPropertyChanged(nameof(BackupBrowserWindowControlBackground));
                OnPropertyChanged(nameof(BackupBrowserWindowControlBorder));
                OnPropertyChanged(nameof(BackupBrowserWindowHeaderText));
                OnPropertyChanged(nameof(BackupBrowserWindowHeaderBackground));
                OnPropertyChanged(nameof(BackupBrowserWindowListHoverBackground));
                OnPropertyChanged(nameof(BackupBrowserWindowListSelectedBackground));
                OnPropertyChanged(nameof(BackupBrowserWindowButtonBackground));
                OnPropertyChanged(nameof(BackupBrowserWindowButtonHoverBackground));

                OnPropertyChanged(nameof(PreviewAppearanceWindowBackgroundBrush));
                OnPropertyChanged(nameof(PreviewAppearanceWindowTextBrush));
                OnPropertyChanged(nameof(PreviewAppearanceWindowControlBackgroundBrush));
                OnPropertyChanged(nameof(PreviewAppearanceWindowControlBorderBrush));
                OnPropertyChanged(nameof(PreviewAppearanceWindowHeaderTextBrush));
                OnPropertyChanged(nameof(PreviewAppearanceWindowHeaderBackgroundBrush));
                OnPropertyChanged(nameof(PreviewAppearanceWindowTabBackgroundBrush));
                OnPropertyChanged(nameof(PreviewAppearanceWindowTabHoverBackgroundBrush));
                OnPropertyChanged(nameof(PreviewAppearanceWindowTabSelectedBackgroundBrush));
                OnPropertyChanged(nameof(PreviewAppearanceWindowButtonBackgroundBrush));
                OnPropertyChanged(nameof(PreviewAppearanceWindowButtonHoverBackgroundBrush));

                OnPropertyChanged(nameof(PreviewSharedConfigPacksWindowBackgroundBrush));
                OnPropertyChanged(nameof(PreviewSharedConfigPacksWindowTextBrush));
                OnPropertyChanged(nameof(PreviewSharedConfigPacksWindowControlBackgroundBrush));
                OnPropertyChanged(nameof(PreviewSharedConfigPacksWindowControlBorderBrush));
                OnPropertyChanged(nameof(PreviewSharedConfigPacksWindowHeaderTextBrush));
                OnPropertyChanged(nameof(PreviewSharedConfigPacksWindowHeaderBackgroundBrush));
                OnPropertyChanged(nameof(PreviewSharedConfigPacksWindowTabBackgroundBrush));
                OnPropertyChanged(nameof(PreviewSharedConfigPacksWindowTabHoverBackgroundBrush));
                OnPropertyChanged(nameof(PreviewSharedConfigPacksWindowTabSelectedBackgroundBrush));
                OnPropertyChanged(nameof(PreviewSharedConfigPacksWindowButtonBackgroundBrush));
                OnPropertyChanged(nameof(PreviewSharedConfigPacksWindowButtonHoverBackgroundBrush));
                OnPropertyChanged(nameof(PreviewSharedConfigPacksWindowEditsHoverBackgroundBrush));
                OnPropertyChanged(nameof(PreviewSharedConfigPacksWindowCheckBoxBackgroundBrush));
                OnPropertyChanged(nameof(PreviewSharedConfigPacksWindowCheckBoxTickBrush));
                OnPropertyChanged(nameof(PreviewSharedConfigPacksWindowCheckBoxTickBrush));
                OnPropertyChanged(nameof(PreviewSharedConfigPacksWindowCheckBoxTickBrush));

                OnPropertyChanged(nameof(PreviewCompareXmlWindowBackgroundBrush));
                OnPropertyChanged(nameof(PreviewCompareXmlWindowTextBrush));
                OnPropertyChanged(nameof(PreviewCompareXmlWindowControlBackgroundBrush));
                OnPropertyChanged(nameof(PreviewCompareXmlWindowControlBorderBrush));
                OnPropertyChanged(nameof(PreviewCompareXmlWindowHeaderTextBrush));
                OnPropertyChanged(nameof(PreviewCompareXmlWindowHeaderBackgroundBrush));
                OnPropertyChanged(nameof(PreviewCompareXmlWindowButtonBackgroundBrush));
                OnPropertyChanged(nameof(PreviewCompareXmlWindowButtonHoverBackgroundBrush));
                OnPropertyChanged(nameof(PreviewCompareXmlWindowEditsHoverBackgroundBrush));
                OnPropertyChanged(nameof(PreviewCompareXmlWindowCheckBoxBackgroundBrush));
                OnPropertyChanged(nameof(PreviewCompareXmlWindowCheckBoxTickBrush));

                OnPropertyChanged(nameof(PreviewBackupBrowserWindowBackgroundBrush));
                OnPropertyChanged(nameof(PreviewBackupBrowserWindowTextBrush));
                OnPropertyChanged(nameof(PreviewBackupBrowserWindowControlBackgroundBrush));
                OnPropertyChanged(nameof(PreviewBackupBrowserWindowControlBorderBrush));
                OnPropertyChanged(nameof(PreviewBackupBrowserWindowHeaderTextBrush));
                OnPropertyChanged(nameof(PreviewBackupBrowserWindowHeaderBackgroundBrush));
                OnPropertyChanged(nameof(PreviewBackupBrowserWindowListHoverBackgroundBrush));
                OnPropertyChanged(nameof(PreviewBackupBrowserWindowListSelectedBackgroundBrush));
                OnPropertyChanged(nameof(PreviewBackupBrowserWindowButtonBackgroundBrush));
                OnPropertyChanged(nameof(PreviewBackupBrowserWindowButtonHoverBackgroundBrush));
                OnPropertyChanged(nameof(PreviewBackupBrowserWindowXmlFilterHoverBackgroundBrush));
                OnPropertyChanged(nameof(PreviewBackupBrowserWindowXmlFilterSelectedBackgroundBrush));

                OnPropertyChanged(nameof(PreviewSettingsInfoWindowBackgroundBrush));
                OnPropertyChanged(nameof(PreviewSettingsInfoWindowTextBrush));
                OnPropertyChanged(nameof(PreviewSettingsInfoWindowControlBackgroundBrush));
                OnPropertyChanged(nameof(PreviewSettingsInfoWindowControlBorderBrush));
                OnPropertyChanged(nameof(PreviewSettingsInfoWindowHeaderTextBrush));
                OnPropertyChanged(nameof(PreviewSettingsInfoWindowHeaderBackgroundBrush));
                OnPropertyChanged(nameof(PreviewSettingsInfoWindowButtonBackgroundBrush));
                OnPropertyChanged(nameof(PreviewSettingsInfoWindowButtonHoverBackgroundBrush));
                OnPropertyChanged(nameof(PreviewSettingsInfoWindowCheckBoxBackgroundBrush));
                OnPropertyChanged(nameof(PreviewSettingsInfoWindowCheckBoxTickBrush));

                OnPropertyChanged(nameof(PreviewDocumentationWindowBackgroundBrush));
                OnPropertyChanged(nameof(PreviewDocumentationWindowTextBrush));
                OnPropertyChanged(nameof(PreviewDocumentationWindowControlBackgroundBrush));
                OnPropertyChanged(nameof(PreviewDocumentationWindowControlBorderBrush));
                OnPropertyChanged(nameof(PreviewDocumentationWindowHeaderTextBrush));
                OnPropertyChanged(nameof(PreviewDocumentationWindowHeaderBackgroundBrush));
                OnPropertyChanged(nameof(PreviewDocumentationWindowListHoverBackgroundBrush));
                OnPropertyChanged(nameof(PreviewDocumentationWindowListSelectedBackgroundBrush));
                OnPropertyChanged(nameof(PreviewDocumentationWindowButtonBackgroundBrush));
                OnPropertyChanged(nameof(PreviewDocumentationWindowButtonHoverBackgroundBrush));

                OnPropertyChanged(nameof(PreviewXmlGuidesWindowBackgroundBrush));
                OnPropertyChanged(nameof(PreviewXmlGuidesWindowTextBrush));
                OnPropertyChanged(nameof(PreviewXmlGuidesWindowControlBackgroundBrush));
                OnPropertyChanged(nameof(PreviewXmlGuidesWindowControlBorderBrush));
                OnPropertyChanged(nameof(PreviewXmlGuidesWindowButtonBackgroundBrush));
                OnPropertyChanged(nameof(PreviewXmlGuidesWindowButtonTextBrush));
                OnPropertyChanged(nameof(PreviewXmlGuidesWindowButtonHoverBackgroundBrush));
                OnPropertyChanged(nameof(PreviewXmlGuidesWindowButtonHoverTextBrush));
                OnPropertyChanged(nameof(PreviewXmlGuidesWindowButtonSelectedBackgroundBrush));
                OnPropertyChanged(nameof(PreviewXmlGuidesWindowButtonSelectedTextBrush));
                OnPropertyChanged(nameof(PreviewXmlGuidesWindowGuidesListBackgroundBrush));
                OnPropertyChanged(nameof(PreviewXmlGuidesWindowGuidesListTextBrush));
                OnPropertyChanged(nameof(PreviewXmlGuidesWindowGuidesListItemHoverBackgroundBrush));
                OnPropertyChanged(nameof(PreviewXmlGuidesWindowGuidesListItemHoverTextBrush));
                OnPropertyChanged(nameof(PreviewXmlGuidesWindowGuidesListItemSelectedBackgroundBrush));
                OnPropertyChanged(nameof(PreviewXmlGuidesWindowGuidesListItemSelectedTextBrush));
                OnPropertyChanged(nameof(PreviewXmlGuidesWindowFontPickerTextBrush));
                OnPropertyChanged(nameof(EditorFontFamily));
                OnPropertyChanged(nameof(EditorFontSize));

                OnPropertyChanged(nameof(Text));
                OnPropertyChanged(nameof(Background));

                OnPropertyChanged(nameof(EditorText));
                OnPropertyChanged(nameof(EditorBackground));
                OnPropertyChanged(nameof(EditorXmlSyntaxForeground));

                OnPropertyChanged(nameof(MenuText));
                OnPropertyChanged(nameof(MenuBackground));
                OnPropertyChanged(nameof(TopButtonText));
                OnPropertyChanged(nameof(TopButtonBackground));

                OnPropertyChanged(nameof(TreeText));
                OnPropertyChanged(nameof(TreeBackground));
                OnPropertyChanged(nameof(TreeItemHoverBackground));
                OnPropertyChanged(nameof(TreeItemSelectedBackground));

                OnPropertyChanged(nameof(GridText));
                OnPropertyChanged(nameof(GridBackground));
                OnPropertyChanged(nameof(GridBorder));
                OnPropertyChanged(nameof(GridLines));
                OnPropertyChanged(nameof(GridRowHoverBackground));
                OnPropertyChanged(nameof(GridHeaderText));
                OnPropertyChanged(nameof(GridRowSelectedBackground));
                OnPropertyChanged(nameof(GridCellSelectedBackground));

                OnPropertyChanged(nameof(FieldColumnText));
                OnPropertyChanged(nameof(FieldColumnBackground));
                OnPropertyChanged(nameof(ValueColumnText));
                OnPropertyChanged(nameof(ValueColumnBackground));
                OnPropertyChanged(nameof(HeaderText));
                OnPropertyChanged(nameof(SelectorBackground));

                OnPropertyChanged(nameof(Pane2ComboText));
                OnPropertyChanged(nameof(Pane2ComboBackground));
                OnPropertyChanged(nameof(Pane2DropdownText));
                OnPropertyChanged(nameof(Pane2DropdownBackground));
                OnPropertyChanged(nameof(Pane2ItemHoverBackground));
                OnPropertyChanged(nameof(Pane2ItemSelectedBackground));
                OnPropertyChanged(null);
                RaisePreview();
            }
            finally
            {
                _suppressPreview = false;
            }
        }

        void WriteToProfile(AppearanceProfileSettings p)
        {
            p.UiFontFamily = UiFontFamily ?? "Segoe UI";
            p.UiFontSize = TryParseDouble(UiFontSize, 12);

            p.EditorFontFamily = EditorFontFamily ?? "Consolas";
            p.EditorFontSize = TryParseDouble(EditorFontSize, 13);

            p.Text = NormalizeColor(Text, p.Text);
            p.Background = NormalizeColor(Background, p.Background);
            p.AppearanceWindowBackground = NormalizeColor(AppearanceWindowBackground, p.AppearanceWindowBackground);
            p.AppearanceWindowText = NormalizeColor(AppearanceWindowText, p.AppearanceWindowText);
            p.AppearanceWindowControlBackground = NormalizeColor(AppearanceWindowControlBackground, p.AppearanceWindowControlBackground);
            p.AppearanceWindowControlBorder = NormalizeColor(AppearanceWindowControlBorder, p.AppearanceWindowControlBorder);
            p.AppearanceWindowHeaderText = NormalizeColor(AppearanceWindowHeaderText, p.AppearanceWindowHeaderText);
            p.AppearanceWindowHeaderBackground = NormalizeColor(AppearanceWindowHeaderBackground, p.AppearanceWindowHeaderBackground);
            p.AppearanceWindowTabBackground = NormalizeColor(AppearanceWindowTabBackground, p.AppearanceWindowTabBackground);
            p.AppearanceWindowTabHoverBackground = NormalizeColor(AppearanceWindowTabHoverBackground, p.AppearanceWindowTabHoverBackground);
            p.AppearanceWindowTabSelectedBackground = NormalizeColor(AppearanceWindowTabSelectedBackground, p.AppearanceWindowTabSelectedBackground);
            p.AppearanceWindowButtonBackground = NormalizeColor(AppearanceWindowButtonBackground, p.AppearanceWindowButtonBackground);
            p.AppearanceWindowButtonHoverBackground = NormalizeColor(AppearanceWindowButtonHoverBackground, p.AppearanceWindowButtonHoverBackground);

            p.SharedConfigPacksWindowBackground = NormalizeColor(SharedConfigPacksWindowBackground, p.SharedConfigPacksWindowBackground);
            p.SharedConfigPacksWindowText = NormalizeColor(SharedConfigPacksWindowText, p.SharedConfigPacksWindowText);
            p.SharedConfigPacksWindowControlBackground = NormalizeColor(SharedConfigPacksWindowControlBackground, p.SharedConfigPacksWindowControlBackground);
            p.SharedConfigPacksWindowControlBorder = NormalizeColor(SharedConfigPacksWindowControlBorder, p.SharedConfigPacksWindowControlBorder);
            p.SharedConfigPacksWindowHeaderText = NormalizeColor(SharedConfigPacksWindowHeaderText, p.SharedConfigPacksWindowHeaderText);
            p.SharedConfigPacksWindowHeaderBackground = NormalizeColor(SharedConfigPacksWindowHeaderBackground, p.SharedConfigPacksWindowHeaderBackground);
            p.SharedConfigPacksWindowTabBackground = NormalizeColor(SharedConfigPacksWindowTabBackground, p.SharedConfigPacksWindowTabBackground);
            p.SharedConfigPacksWindowTabHoverBackground = NormalizeColor(SharedConfigPacksWindowTabHoverBackground, p.SharedConfigPacksWindowTabHoverBackground);
            p.SharedConfigPacksWindowTabSelectedBackground = NormalizeColor(SharedConfigPacksWindowTabSelectedBackground, p.SharedConfigPacksWindowTabSelectedBackground);
            p.SharedConfigPacksWindowButtonBackground = NormalizeColor(SharedConfigPacksWindowButtonBackground, p.SharedConfigPacksWindowButtonBackground);
            p.SharedConfigPacksWindowButtonHoverBackground = NormalizeColor(SharedConfigPacksWindowButtonHoverBackground, p.SharedConfigPacksWindowButtonHoverBackground);
            p.SharedConfigPacksWindowEditsHoverBackground = NormalizeColor(SharedConfigPacksWindowEditsHoverBackground, p.SharedConfigPacksWindowEditsHoverBackground);
            p.SharedConfigPacksWindowCheckBoxBackground = NormalizeColor(SharedConfigPacksWindowCheckBoxBackground, p.SharedConfigPacksWindowCheckBoxBackground);
            p.SharedConfigPacksWindowCheckBoxTick = NormalizeColor(SharedConfigPacksWindowCheckBoxTick, p.SharedConfigPacksWindowCheckBoxTick);

            p.CompareXmlWindowBackground = NormalizeColor(CompareXmlWindowBackground, p.CompareXmlWindowBackground);
            p.CompareXmlWindowText = NormalizeColor(CompareXmlWindowText, p.CompareXmlWindowText);
            p.CompareXmlWindowControlBackground = NormalizeColor(CompareXmlWindowControlBackground, p.CompareXmlWindowControlBackground);
            p.CompareXmlWindowControlBorder = NormalizeColor(CompareXmlWindowControlBorder, p.CompareXmlWindowControlBorder);
            p.CompareXmlWindowHeaderText = NormalizeColor(CompareXmlWindowHeaderText, p.CompareXmlWindowHeaderText);
            p.CompareXmlWindowHeaderBackground = NormalizeColor(CompareXmlWindowHeaderBackground, p.CompareXmlWindowHeaderBackground);
            p.CompareXmlWindowButtonBackground = NormalizeColor(CompareXmlWindowButtonBackground, p.CompareXmlWindowButtonBackground);
            p.CompareXmlWindowButtonHoverBackground = NormalizeColor(CompareXmlWindowButtonHoverBackground, p.CompareXmlWindowButtonHoverBackground);
            p.CompareXmlWindowEditsHoverBackground = NormalizeColor(CompareXmlWindowEditsHoverBackground, p.CompareXmlWindowEditsHoverBackground);
            p.CompareXmlWindowCheckBoxBackground = NormalizeColor(CompareXmlWindowCheckBoxBackground, p.CompareXmlWindowCheckBoxBackground);
            p.CompareXmlWindowCheckBoxTick = NormalizeColor(CompareXmlWindowCheckBoxTick, p.CompareXmlWindowCheckBoxTick);

            p.BackupBrowserWindowBackground = NormalizeColor(BackupBrowserWindowBackground, p.BackupBrowserWindowBackground);
            p.BackupBrowserWindowText = NormalizeColor(BackupBrowserWindowText, p.BackupBrowserWindowText);
            p.BackupBrowserWindowControlBackground = NormalizeColor(BackupBrowserWindowControlBackground, p.BackupBrowserWindowControlBackground);
            p.BackupBrowserWindowControlBorder = NormalizeColor(BackupBrowserWindowControlBorder, p.BackupBrowserWindowControlBorder);
            p.BackupBrowserWindowHeaderText = NormalizeColor(BackupBrowserWindowHeaderText, p.BackupBrowserWindowHeaderText);
            p.BackupBrowserWindowHeaderBackground = NormalizeColor(BackupBrowserWindowHeaderBackground, p.BackupBrowserWindowHeaderBackground);
            p.BackupBrowserWindowListHoverBackground = NormalizeColor(BackupBrowserWindowListHoverBackground, p.BackupBrowserWindowListHoverBackground);
            p.BackupBrowserWindowListSelectedBackground = NormalizeColor(BackupBrowserWindowListSelectedBackground, p.BackupBrowserWindowListSelectedBackground);
            p.BackupBrowserWindowButtonBackground = NormalizeColor(BackupBrowserWindowButtonBackground, p.BackupBrowserWindowButtonBackground);
            p.BackupBrowserWindowButtonHoverBackground = NormalizeColor(BackupBrowserWindowButtonHoverBackground, p.BackupBrowserWindowButtonHoverBackground);
            p.BackupBrowserWindowXmlFilterHoverBackground = NormalizeColor(BackupBrowserWindowXmlFilterHoverBackground, p.BackupBrowserWindowXmlFilterHoverBackground);
            p.BackupBrowserWindowXmlFilterSelectedBackground = NormalizeColor(BackupBrowserWindowXmlFilterSelectedBackground, p.BackupBrowserWindowXmlFilterSelectedBackground);

            p.SavedEditsWindowBackground = NormalizeColor(SavedEditsWindowBackground, p.SavedEditsWindowBackground);
            p.SavedEditsWindowText = NormalizeColor(SavedEditsWindowText, p.SavedEditsWindowText);
            p.SavedEditsWindowControlBackground = NormalizeColor(SavedEditsWindowControlBackground, p.SavedEditsWindowControlBackground);
            p.SavedEditsWindowControlBorder = NormalizeColor(SavedEditsWindowControlBorder, p.SavedEditsWindowControlBorder);
            p.SavedEditsWindowTabBackground = NormalizeColor(SavedEditsWindowTabBackground, p.SavedEditsWindowTabBackground);
            p.SavedEditsWindowTabHoverBackground = NormalizeColor(SavedEditsWindowTabHoverBackground, p.SavedEditsWindowTabHoverBackground);
            p.SavedEditsWindowTabSelectedBackground = NormalizeColor(SavedEditsWindowTabSelectedBackground, p.SavedEditsWindowTabSelectedBackground);
            p.SavedEditsWindowButtonBackground = NormalizeColor(SavedEditsWindowButtonBackground, p.SavedEditsWindowButtonBackground);
            p.SavedEditsWindowButtonHoverBackground = NormalizeColor(SavedEditsWindowButtonHoverBackground, p.SavedEditsWindowButtonHoverBackground);
            p.SavedEditsWindowCheckBoxBackground = NormalizeColor(SavedEditsWindowCheckBoxBackground, p.SavedEditsWindowCheckBoxBackground);
            p.SavedEditsWindowCheckBoxTick = NormalizeColor(SavedEditsWindowCheckBoxTick, p.SavedEditsWindowCheckBoxTick);
            p.SavedEditsWindowGridText = NormalizeColor(SavedEditsWindowGridText, p.SavedEditsWindowGridText);
            p.SavedEditsWindowGridBackground = NormalizeColor(SavedEditsWindowGridBackground, p.SavedEditsWindowGridBackground);
            p.SavedEditsWindowGridBorder = NormalizeColor(SavedEditsWindowGridBorder, p.SavedEditsWindowGridBorder);
            p.SavedEditsWindowGridLines = NormalizeColor(SavedEditsWindowGridLines, p.SavedEditsWindowGridLines);
            p.SavedEditsWindowGridHeaderBackground = NormalizeColor(SavedEditsWindowGridHeaderBackground, p.SavedEditsWindowGridHeaderBackground);
            p.SavedEditsWindowGridHeaderText = NormalizeColor(SavedEditsWindowGridHeaderText, p.SavedEditsWindowGridHeaderText);
            p.SavedEditsWindowGridRowHoverBackground = NormalizeColor(SavedEditsWindowGridRowHoverBackground, p.SavedEditsWindowGridRowHoverBackground);
            p.SavedEditsWindowGridRowSelectedBackground = NormalizeColor(SavedEditsWindowGridRowSelectedBackground, p.SavedEditsWindowGridRowSelectedBackground);
            p.SavedEditsWindowGridCellSelectedBackground = NormalizeColor(SavedEditsWindowGridCellSelectedBackground, p.SavedEditsWindowGridCellSelectedBackground);
            p.SavedEditsWindowGridCellSelectedText = NormalizeColor(SavedEditsWindowGridCellSelectedText, p.SavedEditsWindowGridCellSelectedText);

            p.SettingsInfoWindowBackground = NormalizeColor(SettingsInfoWindowBackground, p.SettingsInfoWindowBackground);
            p.SettingsInfoWindowText = NormalizeColor(SettingsInfoWindowText, p.SettingsInfoWindowText);
            p.SettingsInfoWindowControlBackground = NormalizeColor(SettingsInfoWindowControlBackground, p.SettingsInfoWindowControlBackground);
            p.SettingsInfoWindowControlBorder = NormalizeColor(SettingsInfoWindowControlBorder, p.SettingsInfoWindowControlBorder);
            p.SettingsInfoWindowHeaderText = NormalizeColor(SettingsInfoWindowHeaderText, p.SettingsInfoWindowHeaderText);
            p.SettingsInfoWindowHeaderBackground = NormalizeColor(SettingsInfoWindowHeaderBackground, p.SettingsInfoWindowHeaderBackground);
            p.SettingsInfoWindowButtonBackground = NormalizeColor(SettingsInfoWindowButtonBackground, p.SettingsInfoWindowButtonBackground);
            p.SettingsInfoWindowButtonHoverBackground = NormalizeColor(SettingsInfoWindowButtonHoverBackground, p.SettingsInfoWindowButtonHoverBackground);
            p.SettingsInfoWindowCheckBoxBackground = NormalizeColor(SettingsInfoWindowCheckBoxBackground, p.SettingsInfoWindowCheckBoxBackground);
            p.SettingsInfoWindowCheckBoxTick = NormalizeColor(SettingsInfoWindowCheckBoxTick, p.SettingsInfoWindowCheckBoxTick);

            p.DocumentationWindowBackground = NormalizeColor(DocumentationWindowBackground, p.DocumentationWindowBackground);
            p.DocumentationWindowText = NormalizeColor(DocumentationWindowText, p.DocumentationWindowText);
            p.DocumentationWindowControlBackground = NormalizeColor(DocumentationWindowControlBackground, p.DocumentationWindowControlBackground);
            p.DocumentationWindowControlBorder = NormalizeColor(DocumentationWindowControlBorder, p.DocumentationWindowControlBorder);
            p.DocumentationWindowHeaderText = NormalizeColor(DocumentationWindowHeaderText, p.DocumentationWindowHeaderText);
            p.DocumentationWindowHeaderBackground = NormalizeColor(DocumentationWindowHeaderBackground, p.DocumentationWindowHeaderBackground);
            p.DocumentationWindowListHoverBackground = NormalizeColor(DocumentationWindowListHoverBackground, p.DocumentationWindowListHoverBackground);
            p.DocumentationWindowListSelectedBackground = NormalizeColor(DocumentationWindowListSelectedBackground, p.DocumentationWindowListSelectedBackground);
            p.DocumentationWindowButtonBackground = NormalizeColor(DocumentationWindowButtonBackground, p.DocumentationWindowButtonBackground);
            p.DocumentationWindowButtonHoverBackground = NormalizeColor(DocumentationWindowButtonHoverBackground, p.DocumentationWindowButtonHoverBackground);

            p.XmlGuidesWindowBackground = NormalizeColor(XmlGuidesWindowBackground, p.XmlGuidesWindowBackground);
            p.XmlGuidesWindowText = NormalizeColor(XmlGuidesWindowText, p.XmlGuidesWindowText);
            p.XmlGuidesWindowControlBackground = NormalizeColor(XmlGuidesWindowControlBackground, p.XmlGuidesWindowControlBackground);
            p.XmlGuidesWindowControlBorder = NormalizeColor(XmlGuidesWindowControlBorder, p.XmlGuidesWindowControlBorder);
            p.XmlGuidesWindowButtonBackground = NormalizeColor(XmlGuidesWindowButtonBackground, p.XmlGuidesWindowButtonBackground);
            p.XmlGuidesWindowButtonText = NormalizeColor(XmlGuidesWindowButtonText, p.XmlGuidesWindowButtonText);
            p.XmlGuidesWindowButtonHoverBackground = NormalizeColor(XmlGuidesWindowButtonHoverBackground, p.XmlGuidesWindowButtonHoverBackground);
            p.XmlGuidesWindowButtonHoverText = NormalizeColor(XmlGuidesWindowButtonHoverText, p.XmlGuidesWindowButtonHoverText);
            p.XmlGuidesWindowGuidesListBackground = NormalizeColor(XmlGuidesWindowGuidesListBackground, p.XmlGuidesWindowGuidesListBackground);
            p.XmlGuidesWindowGuidesListText = NormalizeColor(XmlGuidesWindowGuidesListText, p.XmlGuidesWindowGuidesListText);
            p.XmlGuidesWindowGuidesListItemHoverBackground = NormalizeColor(XmlGuidesWindowGuidesListItemHoverBackground, p.XmlGuidesWindowGuidesListItemHoverBackground);
            p.XmlGuidesWindowGuidesListItemHoverText = NormalizeColor(XmlGuidesWindowGuidesListItemHoverText, p.XmlGuidesWindowGuidesListItemHoverText);
            p.XmlGuidesWindowGuidesListItemSelectedBackground = NormalizeColor(XmlGuidesWindowGuidesListItemSelectedBackground, p.XmlGuidesWindowGuidesListItemSelectedBackground);
            p.XmlGuidesWindowGuidesListItemSelectedText = NormalizeColor(XmlGuidesWindowGuidesListItemSelectedText, p.XmlGuidesWindowGuidesListItemSelectedText);
            p.XmlGuidesWindowFontPickerText = NormalizeColor(XmlGuidesWindowFontPickerText, p.XmlGuidesWindowFontPickerText);

            p.EditorText = NormalizeColor(EditorText, p.EditorText);
            p.EditorBackground = NormalizeColor(EditorBackground, p.EditorBackground);
            p.EditorXmlSyntaxForeground = NormalizeColor(EditorXmlSyntaxForeground, p.EditorXmlSyntaxForeground);

            p.MenuText = NormalizeColor(MenuText, p.MenuText);
            p.MenuBackground = NormalizeColor(MenuBackground, p.MenuBackground);
            p.TopButtonText = NormalizeColor(TopButtonText, p.TopButtonText);
            p.TopButtonBackground = NormalizeColor(TopButtonBackground, p.TopButtonBackground);

            p.Pane2ComboText = NormalizeColor(Pane2ComboText, p.Pane2ComboText);
            p.Pane2ComboBackground = NormalizeColor(Pane2ComboBackground, p.Pane2ComboBackground);
            p.Pane2DropdownText = NormalizeColor(Pane2DropdownText, p.Pane2DropdownText);
            p.Pane2DropdownBackground = NormalizeColor(Pane2DropdownBackground, p.Pane2DropdownBackground);
            p.Pane2ItemHoverBackground = NormalizeColor(Pane2ItemHoverBackground, p.Pane2ItemHoverBackground);
            p.Pane2ItemSelectedBackground = NormalizeColor(Pane2ItemSelectedBackground, p.Pane2ItemSelectedBackground);


            if (_isEditingFriendlyView)
            {
                p.FriendlyTreeText = NormalizeColor(TreeText, p.FriendlyTreeText);
                p.FriendlyTreeBackground = NormalizeColor(TreeBackground, p.FriendlyTreeBackground);
                p.FriendlyTreeItemHoverBackground = NormalizeColor(TreeItemHoverBackground, p.FriendlyTreeItemHoverBackground);
                p.FriendlyTreeItemSelectedBackground = NormalizeColor(TreeItemSelectedBackground, p.FriendlyTreeItemSelectedBackground);
                p.FriendlyPane1TreeItemHoverBackground = NormalizeColor(Pane1TreeItemHoverBackground, p.FriendlyPane1TreeItemHoverBackground);
                p.FriendlyPane1TreeItemSelectedBackground = NormalizeColor(Pane1TreeItemSelectedBackground, p.FriendlyPane1TreeItemSelectedBackground);
            }
            else
            {
                p.RawTreeText = NormalizeColor(TreeText, p.RawTreeText);
                p.RawTreeBackground = NormalizeColor(TreeBackground, p.RawTreeBackground);
                p.RawTreeItemHoverBackground = NormalizeColor(TreeItemHoverBackground, p.RawTreeItemHoverBackground);
                p.RawTreeItemSelectedBackground = NormalizeColor(TreeItemSelectedBackground, p.RawTreeItemSelectedBackground);
                p.RawPane1TreeItemHoverBackground = NormalizeColor(Pane1TreeItemHoverBackground, p.RawPane1TreeItemHoverBackground);
                p.RawPane1TreeItemSelectedBackground = NormalizeColor(Pane1TreeItemSelectedBackground, p.RawPane1TreeItemSelectedBackground);
            }

            p.GridText = NormalizeColor(GridText, p.GridText);
            p.GridBackground = NormalizeColor(GridBackground, p.GridBackground);
            p.GridBorder = NormalizeColor(GridBorder, p.GridBorder);
            p.GridLines = NormalizeColor(GridLines, p.GridLines);
            p.GridRowHoverBackground = NormalizeColor(GridRowHoverBackground, p.GridRowHoverBackground);
            p.GridHeaderText = NormalizeColor(GridHeaderText, p.GridHeaderText);
            p.GridRowSelectedBackground = NormalizeColor(GridRowSelectedBackground, p.GridRowSelectedBackground);
            p.GridCellSelectedBackground = NormalizeColor(GridCellSelectedBackground, p.GridCellSelectedBackground);
            p.SearchMatchBackground = NormalizeColor(SearchMatchBackground, p.SearchMatchBackground);
            p.SearchMatchText = NormalizeColor(SearchMatchText, p.SearchMatchText);


            p.FieldColumnText = NormalizeColor(FieldColumnText, p.FieldColumnText);
            p.FieldColumnBackground = NormalizeColor(FieldColumnBackground, p.FieldColumnBackground);
            p.ValueColumnText = NormalizeColor(ValueColumnText, p.ValueColumnText);
            p.ValueColumnBackground = NormalizeColor(ValueColumnBackground, p.ValueColumnBackground);
            p.HeaderText = NormalizeColor(HeaderText, p.HeaderText);
            p.SelectorBackground = NormalizeColor(SelectorBackground, p.SelectorBackground);
        }

        private void RaisePreview()
        {
            OnPropertyChanged(nameof(PreviewPane2ComboTextBrush));
            OnPropertyChanged(nameof(PreviewPane2ComboBackgroundBrush));
            OnPropertyChanged(nameof(PreviewPane2DropdownTextBrush));
            OnPropertyChanged(nameof(PreviewPane2DropdownBackgroundBrush));
            OnPropertyChanged(nameof(PreviewPane2ItemHoverBackgroundBrush));
            OnPropertyChanged(nameof(PreviewPane2ItemSelectedBackgroundBrush));

            OnPropertyChanged(nameof(PreviewTextBrush));
            OnPropertyChanged(nameof(PreviewBackgroundBrush));
            OnPropertyChanged(nameof(PreviewUiFontFamily));
            OnPropertyChanged(nameof(PreviewUiFontSize));

            OnPropertyChanged(nameof(PreviewAppearanceWindowBackgroundBrush));
            OnPropertyChanged(nameof(PreviewAppearanceWindowTextBrush));
            OnPropertyChanged(nameof(PreviewAppearanceWindowControlBackgroundBrush));
            OnPropertyChanged(nameof(PreviewAppearanceWindowControlBorderBrush));
            OnPropertyChanged(nameof(PreviewAppearanceWindowHeaderTextBrush));
            OnPropertyChanged(nameof(PreviewAppearanceWindowHeaderBackgroundBrush));
            OnPropertyChanged(nameof(PreviewAppearanceWindowTabBackgroundBrush));
            OnPropertyChanged(nameof(PreviewAppearanceWindowTabHoverBackgroundBrush));
            OnPropertyChanged(nameof(PreviewAppearanceWindowTabSelectedBackgroundBrush));
            OnPropertyChanged(nameof(PreviewAppearanceWindowButtonBackgroundBrush));
            OnPropertyChanged(nameof(PreviewAppearanceWindowButtonHoverBackgroundBrush));

            OnPropertyChanged(nameof(PreviewSharedConfigPacksWindowBackgroundBrush));
            OnPropertyChanged(nameof(PreviewSharedConfigPacksWindowTextBrush));
            OnPropertyChanged(nameof(PreviewSharedConfigPacksWindowControlBackgroundBrush));
            OnPropertyChanged(nameof(PreviewSharedConfigPacksWindowControlBorderBrush));
            OnPropertyChanged(nameof(PreviewSharedConfigPacksWindowHeaderTextBrush));
            OnPropertyChanged(nameof(PreviewSharedConfigPacksWindowHeaderBackgroundBrush));
            OnPropertyChanged(nameof(PreviewSharedConfigPacksWindowTabBackgroundBrush));
            OnPropertyChanged(nameof(PreviewSharedConfigPacksWindowTabHoverBackgroundBrush));
            OnPropertyChanged(nameof(PreviewSharedConfigPacksWindowTabSelectedBackgroundBrush));
            OnPropertyChanged(nameof(PreviewSharedConfigPacksWindowButtonBackgroundBrush));
            OnPropertyChanged(nameof(PreviewSharedConfigPacksWindowButtonHoverBackgroundBrush));
            OnPropertyChanged(nameof(PreviewSharedConfigPacksWindowEditsHoverBackgroundBrush));
            OnPropertyChanged(nameof(PreviewSharedConfigPacksWindowCheckBoxBackgroundBrush));
            OnPropertyChanged(nameof(PreviewSharedConfigPacksWindowCheckBoxTickBrush));

            OnPropertyChanged(nameof(PreviewCompareXmlWindowBackgroundBrush));
            OnPropertyChanged(nameof(PreviewCompareXmlWindowTextBrush));
            OnPropertyChanged(nameof(PreviewCompareXmlWindowControlBackgroundBrush));
            OnPropertyChanged(nameof(PreviewCompareXmlWindowControlBorderBrush));
            OnPropertyChanged(nameof(PreviewCompareXmlWindowHeaderTextBrush));
            OnPropertyChanged(nameof(PreviewCompareXmlWindowHeaderBackgroundBrush));
            OnPropertyChanged(nameof(PreviewCompareXmlWindowButtonBackgroundBrush));
            OnPropertyChanged(nameof(PreviewCompareXmlWindowButtonHoverBackgroundBrush));
            OnPropertyChanged(nameof(PreviewCompareXmlWindowEditsHoverBackgroundBrush));
            OnPropertyChanged(nameof(PreviewCompareXmlWindowCheckBoxBackgroundBrush));
            OnPropertyChanged(nameof(PreviewCompareXmlWindowCheckBoxTickBrush));

            OnPropertyChanged(nameof(PreviewBackupBrowserWindowBackgroundBrush));
            OnPropertyChanged(nameof(PreviewBackupBrowserWindowTextBrush));
            OnPropertyChanged(nameof(PreviewBackupBrowserWindowControlBackgroundBrush));
            OnPropertyChanged(nameof(PreviewBackupBrowserWindowControlBorderBrush));
            OnPropertyChanged(nameof(PreviewBackupBrowserWindowHeaderTextBrush));
            OnPropertyChanged(nameof(PreviewBackupBrowserWindowHeaderBackgroundBrush));
            OnPropertyChanged(nameof(PreviewBackupBrowserWindowListHoverBackgroundBrush));
            OnPropertyChanged(nameof(PreviewBackupBrowserWindowListSelectedBackgroundBrush));
            OnPropertyChanged(nameof(PreviewBackupBrowserWindowButtonBackgroundBrush));
            OnPropertyChanged(nameof(PreviewBackupBrowserWindowButtonHoverBackgroundBrush));
            OnPropertyChanged(nameof(PreviewBackupBrowserWindowXmlFilterHoverBackgroundBrush));
            OnPropertyChanged(nameof(PreviewBackupBrowserWindowXmlFilterSelectedBackgroundBrush));

            OnPropertyChanged(nameof(PreviewSettingsInfoWindowBackgroundBrush));
            OnPropertyChanged(nameof(PreviewSettingsInfoWindowTextBrush));
            OnPropertyChanged(nameof(PreviewSettingsInfoWindowControlBackgroundBrush));
            OnPropertyChanged(nameof(PreviewSettingsInfoWindowControlBorderBrush));
            OnPropertyChanged(nameof(PreviewSettingsInfoWindowHeaderTextBrush));
            OnPropertyChanged(nameof(PreviewSettingsInfoWindowHeaderBackgroundBrush));
            OnPropertyChanged(nameof(PreviewSettingsInfoWindowButtonBackgroundBrush));
            OnPropertyChanged(nameof(PreviewSettingsInfoWindowButtonHoverBackgroundBrush));
            OnPropertyChanged(nameof(PreviewSettingsInfoWindowCheckBoxBackgroundBrush));
            OnPropertyChanged(nameof(PreviewSettingsInfoWindowCheckBoxTickBrush));

            OnPropertyChanged(nameof(PreviewDocumentationWindowBackgroundBrush));
            OnPropertyChanged(nameof(PreviewDocumentationWindowTextBrush));
            OnPropertyChanged(nameof(PreviewDocumentationWindowControlBackgroundBrush));
            OnPropertyChanged(nameof(PreviewDocumentationWindowControlBorderBrush));
            OnPropertyChanged(nameof(PreviewDocumentationWindowHeaderTextBrush));
            OnPropertyChanged(nameof(PreviewDocumentationWindowHeaderBackgroundBrush));
            OnPropertyChanged(nameof(PreviewDocumentationWindowListHoverBackgroundBrush));
            OnPropertyChanged(nameof(PreviewDocumentationWindowListSelectedBackgroundBrush));
            OnPropertyChanged(nameof(PreviewDocumentationWindowButtonBackgroundBrush));
            OnPropertyChanged(nameof(PreviewDocumentationWindowButtonHoverBackgroundBrush));

            OnPropertyChanged(nameof(PreviewXmlGuidesWindowBackgroundBrush));
            OnPropertyChanged(nameof(PreviewXmlGuidesWindowTextBrush));
            OnPropertyChanged(nameof(PreviewXmlGuidesWindowControlBackgroundBrush));
            OnPropertyChanged(nameof(PreviewXmlGuidesWindowControlBorderBrush));
            OnPropertyChanged(nameof(PreviewXmlGuidesWindowButtonBackgroundBrush));
            OnPropertyChanged(nameof(PreviewXmlGuidesWindowButtonTextBrush));
            OnPropertyChanged(nameof(PreviewXmlGuidesWindowButtonHoverBackgroundBrush));
            OnPropertyChanged(nameof(PreviewXmlGuidesWindowButtonHoverTextBrush));
            OnPropertyChanged(nameof(PreviewXmlGuidesWindowButtonSelectedBackgroundBrush));
            OnPropertyChanged(nameof(PreviewXmlGuidesWindowButtonSelectedTextBrush));
            OnPropertyChanged(nameof(PreviewXmlGuidesWindowGuidesListBackgroundBrush));
            OnPropertyChanged(nameof(PreviewXmlGuidesWindowGuidesListTextBrush));
            OnPropertyChanged(nameof(PreviewXmlGuidesWindowGuidesListItemHoverBackgroundBrush));
            OnPropertyChanged(nameof(PreviewXmlGuidesWindowGuidesListItemHoverTextBrush));
            OnPropertyChanged(nameof(PreviewXmlGuidesWindowGuidesListItemSelectedBackgroundBrush));
            OnPropertyChanged(nameof(PreviewXmlGuidesWindowGuidesListItemSelectedTextBrush));
            OnPropertyChanged(nameof(PreviewXmlGuidesWindowFontPickerTextBrush));

            OnPropertyChanged(nameof(PreviewSavedEditsWindowBackgroundBrush));
            OnPropertyChanged(nameof(PreviewSavedEditsWindowTextBrush));
            OnPropertyChanged(nameof(PreviewSavedEditsWindowControlBackgroundBrush));
            OnPropertyChanged(nameof(PreviewSavedEditsWindowControlBorderBrush));
            OnPropertyChanged(nameof(PreviewSavedEditsWindowTabBackgroundBrush));
            OnPropertyChanged(nameof(PreviewSavedEditsWindowTabHoverBackgroundBrush));
            OnPropertyChanged(nameof(PreviewSavedEditsWindowTabSelectedBackgroundBrush));
            OnPropertyChanged(nameof(PreviewSavedEditsWindowButtonBackgroundBrush));
            OnPropertyChanged(nameof(PreviewSavedEditsWindowButtonHoverBackgroundBrush));
            OnPropertyChanged(nameof(PreviewSavedEditsWindowCheckBoxBackgroundBrush));
            OnPropertyChanged(nameof(PreviewSavedEditsWindowCheckBoxTickBrush));
            OnPropertyChanged(nameof(PreviewSavedEditsWindowGridTextBrush));
            OnPropertyChanged(nameof(PreviewSavedEditsWindowGridBackgroundBrush));
            OnPropertyChanged(nameof(PreviewSavedEditsWindowGridBorderBrush));
            OnPropertyChanged(nameof(PreviewSavedEditsWindowGridLinesBrush));
            OnPropertyChanged(nameof(PreviewSavedEditsWindowGridHeaderBackgroundBrush));
            OnPropertyChanged(nameof(PreviewSavedEditsWindowGridHeaderTextBrush));
            OnPropertyChanged(nameof(PreviewSavedEditsWindowGridRowHoverBackgroundBrush));
            OnPropertyChanged(nameof(PreviewSavedEditsWindowGridRowSelectedBackgroundBrush));
            OnPropertyChanged(nameof(PreviewSavedEditsWindowGridCellSelectedBackgroundBrush));
            OnPropertyChanged(nameof(PreviewSavedEditsWindowGridCellSelectedTextBrush));

            OnPropertyChanged(nameof(PreviewEditorTextBrush));
            OnPropertyChanged(nameof(PreviewEditorBackgroundBrush));
            OnPropertyChanged(nameof(PreviewEditorXmlSyntaxForegroundBrush));

            OnPropertyChanged(nameof(PreviewMenuTextBrush));
            OnPropertyChanged(nameof(PreviewMenuBackgroundBrush));
            OnPropertyChanged(nameof(PreviewTopButtonTextBrush));
            OnPropertyChanged(nameof(PreviewTopButtonBackgroundBrush));

            OnPropertyChanged(nameof(PreviewTreeTextBrush));
            OnPropertyChanged(nameof(PreviewTreeBackgroundBrush));
            OnPropertyChanged(nameof(PreviewTreeHoverBrush));
            OnPropertyChanged(nameof(PreviewTreeSelectedBrush));
            OnPropertyChanged(nameof(PreviewPane1TreeHoverBrush));
            OnPropertyChanged(nameof(PreviewPane1TreeSelectedBrush));

            OnPropertyChanged(nameof(PreviewGridTextBrush));
            OnPropertyChanged(nameof(PreviewGridBackgroundBrush));
            OnPropertyChanged(nameof(PreviewGridBorderBrush));
            OnPropertyChanged(nameof(PreviewGridLinesBrush));
            OnPropertyChanged(nameof(PreviewGridRowHoverBrush));
            OnPropertyChanged(nameof(PreviewGridHeaderTextBrush));
            OnPropertyChanged(nameof(PreviewGridRowSelectedBackgroundBrush));
            OnPropertyChanged(nameof(PreviewGridCellSelectedBackgroundBrush));
            OnPropertyChanged(nameof(PreviewSearchMatchBackgroundBrush));
            OnPropertyChanged(nameof(PreviewSearchMatchTextBrush));


            OnPropertyChanged(nameof(PreviewFieldColumnTextBrush));
            OnPropertyChanged(nameof(PreviewFieldColumnBackgroundBrush));
            OnPropertyChanged(nameof(PreviewValueColumnTextBrush));
            OnPropertyChanged(nameof(PreviewValueColumnBackgroundBrush));
            OnPropertyChanged(nameof(PreviewHeaderTextBrush));
            OnPropertyChanged(nameof(PreviewSelectorBackgroundBrush));
        }

        private string GetColorByKey(string key)
        {
            return key switch
            {
                nameof(Text) => Text,
                nameof(Background) => Background,

                nameof(EditorText) => EditorText,
                nameof(EditorBackground) => EditorBackground,
                nameof(EditorXmlSyntaxForeground) => EditorXmlSyntaxForeground,

                nameof(MenuText) => MenuText,
                nameof(MenuBackground) => MenuBackground,
                nameof(TopButtonText) => TopButtonText,
                nameof(TopButtonBackground) => TopButtonBackground,

                nameof(TreeText) => TreeText,
                nameof(TreeBackground) => TreeBackground,
                nameof(TreeItemHoverBackground) => TreeItemHoverBackground,
                nameof(TreeItemSelectedBackground) => TreeItemSelectedBackground,
                nameof(Pane1TreeItemHoverBackground) => Pane1TreeItemHoverBackground,
                nameof(Pane1TreeItemSelectedBackground) => Pane1TreeItemSelectedBackground,

                nameof(GridText) => GridText,
                nameof(GridBackground) => GridBackground,
                nameof(GridBorder) => GridBorder,
                nameof(GridLines) => GridLines,
                nameof(GridRowHoverBackground) => GridRowHoverBackground,
                nameof(GridHeaderText) => GridHeaderText,
                nameof(GridRowSelectedBackground) => GridRowSelectedBackground,
                nameof(GridCellSelectedBackground) => GridCellSelectedBackground,
                nameof(SearchMatchBackground) => SearchMatchBackground,
                nameof(SearchMatchText) => SearchMatchText,


                nameof(FieldColumnText) => FieldColumnText,
                nameof(FieldColumnBackground) => FieldColumnBackground,
                nameof(ValueColumnText) => ValueColumnText,
                nameof(ValueColumnBackground) => ValueColumnBackground,
                nameof(HeaderText) => HeaderText,
                nameof(SelectorBackground) => SelectorBackground,
                nameof(Pane2ComboText) => Pane2ComboText,
                nameof(Pane2ComboBackground) => Pane2ComboBackground,
                nameof(Pane2DropdownText) => Pane2DropdownText,
                nameof(Pane2DropdownBackground) => Pane2DropdownBackground,
                nameof(Pane2ItemHoverBackground) => Pane2ItemHoverBackground,
                nameof(Pane2ItemSelectedBackground) => Pane2ItemSelectedBackground,

                nameof(AppearanceWindowBackground) => AppearanceWindowBackground,
                nameof(AppearanceWindowText) => AppearanceWindowText,
                nameof(AppearanceWindowControlBackground) => AppearanceWindowControlBackground,
                nameof(AppearanceWindowControlBorder) => AppearanceWindowControlBorder,
                nameof(AppearanceWindowHeaderText) => AppearanceWindowHeaderText,
                nameof(AppearanceWindowHeaderBackground) => AppearanceWindowHeaderBackground,
                nameof(AppearanceWindowTabBackground) => AppearanceWindowTabBackground,
                nameof(AppearanceWindowTabHoverBackground) => AppearanceWindowTabHoverBackground,
                nameof(AppearanceWindowTabSelectedBackground) => AppearanceWindowTabSelectedBackground,
                nameof(AppearanceWindowButtonBackground) => AppearanceWindowButtonBackground,
                nameof(AppearanceWindowButtonHoverBackground) => AppearanceWindowButtonHoverBackground,

                nameof(SharedConfigPacksWindowBackground) => SharedConfigPacksWindowBackground,
                nameof(SharedConfigPacksWindowText) => SharedConfigPacksWindowText,
                nameof(SharedConfigPacksWindowControlBackground) => SharedConfigPacksWindowControlBackground,
                nameof(SharedConfigPacksWindowControlBorder) => SharedConfigPacksWindowControlBorder,
                nameof(SharedConfigPacksWindowHeaderText) => SharedConfigPacksWindowHeaderText,
                nameof(SharedConfigPacksWindowHeaderBackground) => SharedConfigPacksWindowHeaderBackground,
                nameof(SharedConfigPacksWindowTabBackground) => SharedConfigPacksWindowTabBackground,
                nameof(SharedConfigPacksWindowTabHoverBackground) => SharedConfigPacksWindowTabHoverBackground,
                nameof(SharedConfigPacksWindowTabSelectedBackground) => SharedConfigPacksWindowTabSelectedBackground,
                nameof(SharedConfigPacksWindowButtonBackground) => SharedConfigPacksWindowButtonBackground,
                nameof(SharedConfigPacksWindowButtonHoverBackground) => SharedConfigPacksWindowButtonHoverBackground,

                nameof(CompareXmlWindowBackground) => CompareXmlWindowBackground,
                nameof(CompareXmlWindowText) => CompareXmlWindowText,
                nameof(CompareXmlWindowControlBackground) => CompareXmlWindowControlBackground,
                nameof(CompareXmlWindowControlBorder) => CompareXmlWindowControlBorder,
                nameof(CompareXmlWindowHeaderText) => CompareXmlWindowHeaderText,
                nameof(CompareXmlWindowHeaderBackground) => CompareXmlWindowHeaderBackground,
                nameof(CompareXmlWindowButtonBackground) => CompareXmlWindowButtonBackground,
                nameof(CompareXmlWindowButtonHoverBackground) => CompareXmlWindowButtonHoverBackground,
                nameof(CompareXmlWindowEditsHoverBackground) => CompareXmlWindowEditsHoverBackground,
                nameof(CompareXmlWindowCheckBoxBackground) => CompareXmlWindowCheckBoxBackground,
                nameof(CompareXmlWindowCheckBoxTick) => CompareXmlWindowCheckBoxTick,

                nameof(BackupBrowserWindowBackground) => BackupBrowserWindowBackground,
                nameof(BackupBrowserWindowText) => BackupBrowserWindowText,
                nameof(BackupBrowserWindowControlBackground) => BackupBrowserWindowControlBackground,
                nameof(BackupBrowserWindowControlBorder) => BackupBrowserWindowControlBorder,
                nameof(BackupBrowserWindowHeaderText) => BackupBrowserWindowHeaderText,
                nameof(BackupBrowserWindowHeaderBackground) => BackupBrowserWindowHeaderBackground,
                nameof(BackupBrowserWindowListHoverBackground) => BackupBrowserWindowListHoverBackground,
                nameof(BackupBrowserWindowListSelectedBackground) => BackupBrowserWindowListSelectedBackground,
                nameof(BackupBrowserWindowButtonBackground) => BackupBrowserWindowButtonBackground,
                nameof(BackupBrowserWindowButtonHoverBackground) => BackupBrowserWindowButtonHoverBackground,
                nameof(BackupBrowserWindowXmlFilterHoverBackground) => BackupBrowserWindowXmlFilterHoverBackground,
                nameof(BackupBrowserWindowXmlFilterSelectedBackground) => BackupBrowserWindowXmlFilterSelectedBackground,

                nameof(SavedEditsWindowBackground) => SavedEditsWindowBackground,
                nameof(SavedEditsWindowText) => SavedEditsWindowText,
                nameof(SavedEditsWindowControlBackground) => SavedEditsWindowControlBackground,
                nameof(SavedEditsWindowControlBorder) => SavedEditsWindowControlBorder,
                nameof(SavedEditsWindowTabBackground) => SavedEditsWindowTabBackground,
                nameof(SavedEditsWindowTabHoverBackground) => SavedEditsWindowTabHoverBackground,
                nameof(SavedEditsWindowTabSelectedBackground) => SavedEditsWindowTabSelectedBackground,
                nameof(SavedEditsWindowButtonBackground) => SavedEditsWindowButtonBackground,
                nameof(SavedEditsWindowButtonHoverBackground) => SavedEditsWindowButtonHoverBackground,
                nameof(SavedEditsWindowCheckBoxBackground) => SavedEditsWindowCheckBoxBackground,
                nameof(SavedEditsWindowCheckBoxTick) => SavedEditsWindowCheckBoxTick,
                nameof(SavedEditsWindowGridText) => SavedEditsWindowGridText,
                nameof(SavedEditsWindowGridBackground) => SavedEditsWindowGridBackground,
                nameof(SavedEditsWindowGridBorder) => SavedEditsWindowGridBorder,
                nameof(SavedEditsWindowGridLines) => SavedEditsWindowGridLines,
                nameof(SavedEditsWindowGridHeaderBackground) => SavedEditsWindowGridHeaderBackground,
                nameof(SavedEditsWindowGridHeaderText) => SavedEditsWindowGridHeaderText,
                nameof(SavedEditsWindowGridRowHoverBackground) => SavedEditsWindowGridRowHoverBackground,
                nameof(SavedEditsWindowGridRowSelectedBackground) => SavedEditsWindowGridRowSelectedBackground,
                nameof(SavedEditsWindowGridCellSelectedBackground) => SavedEditsWindowGridCellSelectedBackground,
                nameof(SavedEditsWindowGridCellSelectedText) => SavedEditsWindowGridCellSelectedText,

                nameof(SettingsInfoWindowBackground) => SettingsInfoWindowBackground,
                nameof(SettingsInfoWindowText) => SettingsInfoWindowText,
                nameof(SettingsInfoWindowControlBackground) => SettingsInfoWindowControlBackground,
                nameof(SettingsInfoWindowControlBorder) => SettingsInfoWindowControlBorder,
                nameof(SettingsInfoWindowHeaderText) => SettingsInfoWindowHeaderText,
                nameof(SettingsInfoWindowHeaderBackground) => SettingsInfoWindowHeaderBackground,
                nameof(SettingsInfoWindowButtonBackground) => SettingsInfoWindowButtonBackground,
                nameof(SettingsInfoWindowButtonHoverBackground) => SettingsInfoWindowButtonHoverBackground,
                nameof(SettingsInfoWindowCheckBoxBackground) => SettingsInfoWindowCheckBoxBackground,
                nameof(SettingsInfoWindowCheckBoxTick) => SettingsInfoWindowCheckBoxTick,

                nameof(DocumentationWindowBackground) => DocumentationWindowBackground,
                nameof(DocumentationWindowText) => DocumentationWindowText,
                nameof(DocumentationWindowControlBackground) => DocumentationWindowControlBackground,
                nameof(DocumentationWindowControlBorder) => DocumentationWindowControlBorder,
                nameof(DocumentationWindowHeaderText) => DocumentationWindowHeaderText,
                nameof(DocumentationWindowHeaderBackground) => DocumentationWindowHeaderBackground,
                nameof(DocumentationWindowListHoverBackground) => DocumentationWindowListHoverBackground,
                nameof(DocumentationWindowListSelectedBackground) => DocumentationWindowListSelectedBackground,
                nameof(DocumentationWindowButtonBackground) => DocumentationWindowButtonBackground,
                nameof(DocumentationWindowButtonHoverBackground) => DocumentationWindowButtonHoverBackground,

                nameof(XmlGuidesWindowBackground) => XmlGuidesWindowBackground,
                nameof(XmlGuidesWindowText) => XmlGuidesWindowText,
                nameof(XmlGuidesWindowControlBackground) => XmlGuidesWindowControlBackground,
                nameof(XmlGuidesWindowControlBorder) => XmlGuidesWindowControlBorder,
                nameof(XmlGuidesWindowButtonBackground) => XmlGuidesWindowButtonBackground,
                nameof(XmlGuidesWindowButtonText) => XmlGuidesWindowButtonText,
                nameof(XmlGuidesWindowButtonHoverBackground) => XmlGuidesWindowButtonHoverBackground,
                nameof(XmlGuidesWindowButtonHoverText) => XmlGuidesWindowButtonHoverText,
                nameof(XmlGuidesWindowButtonSelectedBackground) => XmlGuidesWindowButtonSelectedBackground,
                nameof(XmlGuidesWindowButtonSelectedText) => XmlGuidesWindowButtonSelectedText,
                nameof(XmlGuidesWindowGuidesListBackground) => XmlGuidesWindowGuidesListBackground,
                nameof(XmlGuidesWindowGuidesListText) => XmlGuidesWindowGuidesListText,
                nameof(XmlGuidesWindowGuidesListItemHoverBackground) => XmlGuidesWindowGuidesListItemHoverBackground,
                nameof(XmlGuidesWindowGuidesListItemHoverText) => XmlGuidesWindowGuidesListItemHoverText,
                nameof(XmlGuidesWindowGuidesListItemSelectedBackground) => XmlGuidesWindowGuidesListItemSelectedBackground,
                nameof(XmlGuidesWindowGuidesListItemSelectedText) => XmlGuidesWindowGuidesListItemSelectedText,
                nameof(XmlGuidesWindowFontPickerText) => XmlGuidesWindowFontPickerText,

                _ => "#FFFFFFFF"
            };
        }

        private void SetColorByKey(string key, string hex)
        {
            switch (key)
            {
                case nameof(Text): Text = hex; break;
                case nameof(Background): Background = hex; break;

                case nameof(EditorText): EditorText = hex; break;
                case nameof(EditorBackground): EditorBackground = hex; break;
                case nameof(EditorXmlSyntaxForeground): EditorXmlSyntaxForeground = hex; break;

                case nameof(MenuText): MenuText = hex; break;
                case nameof(MenuBackground): MenuBackground = hex; break;
                case nameof(TopButtonText): TopButtonText = hex; break;
                case nameof(TopButtonBackground): TopButtonBackground = hex; break;

                case nameof(TreeText): TreeText = hex; break;
                case nameof(TreeBackground): TreeBackground = hex; break;
                case nameof(TreeItemHoverBackground): TreeItemHoverBackground = hex; break;
                case nameof(TreeItemSelectedBackground): TreeItemSelectedBackground = hex; break;
                case nameof(Pane1TreeItemHoverBackground): Pane1TreeItemHoverBackground = hex; break;
                case nameof(Pane1TreeItemSelectedBackground): Pane1TreeItemSelectedBackground = hex; break;

                case nameof(Pane2ComboText): Pane2ComboText = hex; break;
                case nameof(Pane2ComboBackground): Pane2ComboBackground = hex; break;
                case nameof(Pane2DropdownText): Pane2DropdownText = hex; break;
                case nameof(Pane2DropdownBackground): Pane2DropdownBackground = hex; break;
                case nameof(Pane2ItemHoverBackground): Pane2ItemHoverBackground = hex; break;
                case nameof(Pane2ItemSelectedBackground): Pane2ItemSelectedBackground = hex; break;

                case nameof(AppearanceWindowBackground): AppearanceWindowBackground = hex; break;
                case nameof(AppearanceWindowText): AppearanceWindowText = hex; break;
                case nameof(AppearanceWindowControlBackground): AppearanceWindowControlBackground = hex; break;
                case nameof(AppearanceWindowControlBorder): AppearanceWindowControlBorder = hex; break;
                case nameof(AppearanceWindowHeaderText): AppearanceWindowHeaderText = hex; break;
                case nameof(AppearanceWindowHeaderBackground): AppearanceWindowHeaderBackground = hex; break;
                case nameof(AppearanceWindowTabBackground): AppearanceWindowTabBackground = hex; break;
                case nameof(AppearanceWindowTabHoverBackground): AppearanceWindowTabHoverBackground = hex; break;
                case nameof(AppearanceWindowTabSelectedBackground): AppearanceWindowTabSelectedBackground = hex; break;
                case nameof(AppearanceWindowButtonBackground): AppearanceWindowButtonBackground = hex; break;
                case nameof(AppearanceWindowButtonHoverBackground): AppearanceWindowButtonHoverBackground = hex; break;

                case nameof(SharedConfigPacksWindowBackground): SharedConfigPacksWindowBackground = hex; break;
                case nameof(SharedConfigPacksWindowText): SharedConfigPacksWindowText = hex; break;
                case nameof(SharedConfigPacksWindowControlBackground): SharedConfigPacksWindowControlBackground = hex; break;
                case nameof(SharedConfigPacksWindowControlBorder): SharedConfigPacksWindowControlBorder = hex; break;
                case nameof(SharedConfigPacksWindowHeaderText): SharedConfigPacksWindowHeaderText = hex; break;
                case nameof(SharedConfigPacksWindowHeaderBackground): SharedConfigPacksWindowHeaderBackground = hex; break;
                case nameof(SharedConfigPacksWindowTabBackground): SharedConfigPacksWindowTabBackground = hex; break;
                case nameof(SharedConfigPacksWindowTabHoverBackground): SharedConfigPacksWindowTabHoverBackground = hex; break;
                case nameof(SharedConfigPacksWindowTabSelectedBackground): SharedConfigPacksWindowTabSelectedBackground = hex; break;
                case nameof(SharedConfigPacksWindowButtonBackground): SharedConfigPacksWindowButtonBackground = hex; break;
                case nameof(SharedConfigPacksWindowButtonHoverBackground): SharedConfigPacksWindowButtonHoverBackground = hex; break;
                case nameof(SharedConfigPacksWindowEditsHoverBackground): SharedConfigPacksWindowEditsHoverBackground = hex; break;
                case nameof(SharedConfigPacksWindowCheckBoxBackground): SharedConfigPacksWindowCheckBoxBackground = hex; break;
                case nameof(SharedConfigPacksWindowCheckBoxTick): SharedConfigPacksWindowCheckBoxTick = hex; break;

                case nameof(CompareXmlWindowBackground): CompareXmlWindowBackground = hex; break;
                case nameof(CompareXmlWindowText): CompareXmlWindowText = hex; break;
                case nameof(CompareXmlWindowControlBackground): CompareXmlWindowControlBackground = hex; break;
                case nameof(CompareXmlWindowControlBorder): CompareXmlWindowControlBorder = hex; break;
                case nameof(CompareXmlWindowHeaderText): CompareXmlWindowHeaderText = hex; break;
                case nameof(CompareXmlWindowHeaderBackground): CompareXmlWindowHeaderBackground = hex; break;
                case nameof(CompareXmlWindowButtonBackground): CompareXmlWindowButtonBackground = hex; break;
                case nameof(CompareXmlWindowButtonHoverBackground): CompareXmlWindowButtonHoverBackground = hex; break;
                case nameof(CompareXmlWindowEditsHoverBackground): CompareXmlWindowEditsHoverBackground = hex; break;
                case nameof(CompareXmlWindowCheckBoxBackground): CompareXmlWindowCheckBoxBackground = hex; break;
                case nameof(CompareXmlWindowCheckBoxTick): CompareXmlWindowCheckBoxTick = hex; break;

                case nameof(BackupBrowserWindowBackground): BackupBrowserWindowBackground = hex; break;
                case nameof(BackupBrowserWindowText): BackupBrowserWindowText = hex; break;
                case nameof(BackupBrowserWindowControlBackground): BackupBrowserWindowControlBackground = hex; break;
                case nameof(BackupBrowserWindowControlBorder): BackupBrowserWindowControlBorder = hex; break;
                case nameof(BackupBrowserWindowHeaderText): BackupBrowserWindowHeaderText = hex; break;
                case nameof(BackupBrowserWindowHeaderBackground): BackupBrowserWindowHeaderBackground = hex; break;
                case nameof(BackupBrowserWindowListHoverBackground): BackupBrowserWindowListHoverBackground = hex; break;
                case nameof(BackupBrowserWindowListSelectedBackground): BackupBrowserWindowListSelectedBackground = hex; break;
                case nameof(BackupBrowserWindowButtonBackground): BackupBrowserWindowButtonBackground = hex; break;
                case nameof(BackupBrowserWindowButtonHoverBackground): BackupBrowserWindowButtonHoverBackground = hex; break;
                case nameof(BackupBrowserWindowXmlFilterHoverBackground): BackupBrowserWindowXmlFilterHoverBackground = hex; break;
                case nameof(BackupBrowserWindowXmlFilterSelectedBackground): BackupBrowserWindowXmlFilterSelectedBackground = hex; break;

                case nameof(SavedEditsWindowBackground): SavedEditsWindowBackground = hex; break;
                case nameof(SavedEditsWindowText): SavedEditsWindowText = hex; break;
                case nameof(SavedEditsWindowControlBackground): SavedEditsWindowControlBackground = hex; break;
                case nameof(SavedEditsWindowControlBorder): SavedEditsWindowControlBorder = hex; break;
                case nameof(SavedEditsWindowTabBackground): SavedEditsWindowTabBackground = hex; break;
                case nameof(SavedEditsWindowTabHoverBackground): SavedEditsWindowTabHoverBackground = hex; break;
                case nameof(SavedEditsWindowTabSelectedBackground): SavedEditsWindowTabSelectedBackground = hex; break;
                case nameof(SavedEditsWindowButtonBackground): SavedEditsWindowButtonBackground = hex; break;
                case nameof(SavedEditsWindowButtonHoverBackground): SavedEditsWindowButtonHoverBackground = hex; break;
                case nameof(SavedEditsWindowCheckBoxBackground): SavedEditsWindowCheckBoxBackground = hex; break;
                case nameof(SavedEditsWindowCheckBoxTick): SavedEditsWindowCheckBoxTick = hex; break;
                case nameof(SavedEditsWindowGridText): SavedEditsWindowGridText = hex; break;
                case nameof(SavedEditsWindowGridBackground): SavedEditsWindowGridBackground = hex; break;
                case nameof(SavedEditsWindowGridBorder): SavedEditsWindowGridBorder = hex; break;
                case nameof(SavedEditsWindowGridLines): SavedEditsWindowGridLines = hex; break;
                case nameof(SavedEditsWindowGridHeaderBackground): SavedEditsWindowGridHeaderBackground = hex; break;
                case nameof(SavedEditsWindowGridHeaderText): SavedEditsWindowGridHeaderText = hex; break;
                case nameof(SavedEditsWindowGridRowHoverBackground): SavedEditsWindowGridRowHoverBackground = hex; break;
                case nameof(SavedEditsWindowGridRowSelectedBackground): SavedEditsWindowGridRowSelectedBackground = hex; break;
                case nameof(SavedEditsWindowGridCellSelectedBackground): SavedEditsWindowGridCellSelectedBackground = hex; break;
                case nameof(SavedEditsWindowGridCellSelectedText): SavedEditsWindowGridCellSelectedText = hex; break;

                case nameof(SettingsInfoWindowBackground): SettingsInfoWindowBackground = hex; break;
                case nameof(SettingsInfoWindowText): SettingsInfoWindowText = hex; break;
                case nameof(SettingsInfoWindowControlBackground): SettingsInfoWindowControlBackground = hex; break;
                case nameof(SettingsInfoWindowControlBorder): SettingsInfoWindowControlBorder = hex; break;
                case nameof(SettingsInfoWindowHeaderText): SettingsInfoWindowHeaderText = hex; break;
                case nameof(SettingsInfoWindowHeaderBackground): SettingsInfoWindowHeaderBackground = hex; break;
                case nameof(SettingsInfoWindowButtonBackground): SettingsInfoWindowButtonBackground = hex; break;
                case nameof(SettingsInfoWindowButtonHoverBackground): SettingsInfoWindowButtonHoverBackground = hex; break;
                case nameof(SettingsInfoWindowCheckBoxBackground): SettingsInfoWindowCheckBoxBackground = hex; break;
                case nameof(SettingsInfoWindowCheckBoxTick): SettingsInfoWindowCheckBoxTick = hex; break;

                case nameof(DocumentationWindowBackground): DocumentationWindowBackground = hex; break;
                case nameof(DocumentationWindowText): DocumentationWindowText = hex; break;
                case nameof(DocumentationWindowControlBackground): DocumentationWindowControlBackground = hex; break;
                case nameof(DocumentationWindowControlBorder): DocumentationWindowControlBorder = hex; break;
                case nameof(DocumentationWindowHeaderText): DocumentationWindowHeaderText = hex; break;
                case nameof(DocumentationWindowHeaderBackground): DocumentationWindowHeaderBackground = hex; break;
                case nameof(DocumentationWindowListHoverBackground): DocumentationWindowListHoverBackground = hex; break;
                case nameof(DocumentationWindowListSelectedBackground): DocumentationWindowListSelectedBackground = hex; break;
                case nameof(DocumentationWindowButtonBackground): DocumentationWindowButtonBackground = hex; break;
                case nameof(DocumentationWindowButtonHoverBackground): DocumentationWindowButtonHoverBackground = hex; break;

                case nameof(XmlGuidesWindowBackground): XmlGuidesWindowBackground = hex; break;
                case nameof(XmlGuidesWindowText): XmlGuidesWindowText = hex; break;
                case nameof(XmlGuidesWindowControlBackground): XmlGuidesWindowControlBackground = hex; break;
                case nameof(XmlGuidesWindowControlBorder): XmlGuidesWindowControlBorder = hex; break;
                case nameof(XmlGuidesWindowButtonBackground): XmlGuidesWindowButtonBackground = hex; break;
                case nameof(XmlGuidesWindowButtonText): XmlGuidesWindowButtonText = hex; break;
                case nameof(XmlGuidesWindowButtonHoverBackground): XmlGuidesWindowButtonHoverBackground = hex; break;
                case nameof(XmlGuidesWindowButtonHoverText): XmlGuidesWindowButtonHoverText = hex; break;
                case nameof(XmlGuidesWindowButtonSelectedBackground): XmlGuidesWindowButtonSelectedBackground = hex; break;
                case nameof(XmlGuidesWindowButtonSelectedText): XmlGuidesWindowButtonSelectedText = hex; break;
                case nameof(XmlGuidesWindowGuidesListBackground): XmlGuidesWindowGuidesListBackground = hex; break;
                case nameof(XmlGuidesWindowGuidesListText): XmlGuidesWindowGuidesListText = hex; break;
                case nameof(XmlGuidesWindowGuidesListItemHoverBackground): XmlGuidesWindowGuidesListItemHoverBackground = hex; break;
                case nameof(XmlGuidesWindowGuidesListItemHoverText): XmlGuidesWindowGuidesListItemHoverText = hex; break;
                case nameof(XmlGuidesWindowGuidesListItemSelectedBackground): XmlGuidesWindowGuidesListItemSelectedBackground = hex; break;
                case nameof(XmlGuidesWindowGuidesListItemSelectedText): XmlGuidesWindowGuidesListItemSelectedText = hex; break;
                case nameof(XmlGuidesWindowFontPickerText): XmlGuidesWindowFontPickerText = hex; break;

                case nameof(GridText): GridText = hex; break;
                case nameof(GridBackground): GridBackground = hex; break;
                case nameof(GridBorder): GridBorder = hex; break;
                case nameof(GridLines): GridLines = hex; break;
                case nameof(GridRowHoverBackground): GridRowHoverBackground = hex; break;
                case nameof(GridHeaderText): GridHeaderText = hex; break;
                case nameof(GridRowSelectedBackground): GridRowSelectedBackground = hex; break;
                case nameof(GridCellSelectedBackground): GridCellSelectedBackground = hex; break;
                case nameof(SearchMatchBackground): SearchMatchBackground = hex; break;
                case nameof(SearchMatchText): SearchMatchText = hex; break;

                case nameof(FieldColumnText): FieldColumnText = hex; break;
                case nameof(FieldColumnBackground): FieldColumnBackground = hex; break;
                case nameof(ValueColumnText): ValueColumnText = hex; break;
                case nameof(ValueColumnBackground): ValueColumnBackground = hex; break;
                case nameof(HeaderText): HeaderText = hex; break;
                case nameof(SelectorBackground): SelectorBackground = hex; break;
            }
        }

        private static bool TryParseDrawingColor(string? hex, out System.Drawing.Color c)
        {
            c = default;

            if (string.IsNullOrWhiteSpace(hex))
                return false;

            try
            {
                var obj = WpfColorConverter.ConvertFromString(hex);
                if (obj is WpfColor wc)
                {
                    c = System.Drawing.Color.FromArgb(wc.A, wc.R, wc.G, wc.B);
                    return true;
                }
            }
            catch
            {
            }

            return false;
        }

        private static double TryParseDouble(string? s, double fallback)
        {
            if (double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var v))
                return v;

            if (double.TryParse(s, NumberStyles.Float, CultureInfo.CurrentCulture, out v))
                return v;

            return fallback;
        }

        private static string NormalizeColor(string? s, string fallback)
        {
            if (TryParseColor(s, out _))
                return s!;

            return fallback;
        }

        private static WpfBrush TryParseBrush(string? s)
        {
            if (!TryParseColor(s, out var c))
                return WpfBrushes.Transparent;

            var b = new SolidColorBrush(c);
            b.Freeze();
            return b;
        }

        private static bool TryParseColor(string? s, out WpfColor c)
        {
            c = default;

            if (string.IsNullOrWhiteSpace(s))
                return false;

            try
            {
                var obj = WpfColorConverter.ConvertFromString(s);
                if (obj is WpfColor col)
                {
                    c = col;
                    return true;
                }
            }
            catch
            {
            }

            return false;
        }

        private static AppearanceSettings CloneAppearance(AppearanceSettings source)
        {
            var colors = source.ColorPickerCustomColors;
            var safeColors = colors is not null && colors.Length == 16 ? colors.ToArray() : new int[16];

            var safeProfiles = (source.Profiles ?? new List<NamedAppearanceProfile>())
                .Where(p => p is not null)
                .Select(p => new NamedAppearanceProfile
                {
                    Name = p.Name ?? "",
                    CreatedUtc = p.CreatedUtc,
                    Dark = CloneProfile(p.Dark),
                    Light = CloneProfile(p.Light)
                })
                .ToList();

            return new AppearanceSettings
            {
                Dark = CloneProfile(source.Dark),
                Light = CloneProfile(source.Light),
                ColorPickerCustomColors = safeColors,
                ActiveProfileName = source.ActiveProfileName ?? "",
                Profiles = safeProfiles
            };
        }

        private static AppearanceProfileSettings CloneProfile(AppearanceProfileSettings p)
        {
            return new AppearanceProfileSettings
            {
                UiFontFamily = p.UiFontFamily,
                UiFontSize = p.UiFontSize,
                UiFontBold = p.UiFontBold,
                UiFontItalic = p.UiFontItalic,

                EditorFontFamily = p.EditorFontFamily,
                EditorFontSize = p.EditorFontSize,
                EditorFontBold = p.EditorFontBold,
                EditorFontItalic = p.EditorFontItalic,

                Text = p.Text,
                Background = p.Background,
                AppearanceWindowBackground = p.AppearanceWindowBackground,
                AppearanceWindowText = p.AppearanceWindowText,
                AppearanceWindowControlBackground = p.AppearanceWindowControlBackground,
                AppearanceWindowControlBorder = p.AppearanceWindowControlBorder,
                AppearanceWindowHeaderText = p.AppearanceWindowHeaderText,
                AppearanceWindowHeaderBackground = p.AppearanceWindowHeaderBackground,
                AppearanceWindowTabBackground = p.AppearanceWindowTabBackground,
                AppearanceWindowTabHoverBackground = p.AppearanceWindowTabHoverBackground,
                AppearanceWindowTabSelectedBackground = p.AppearanceWindowTabSelectedBackground,
                AppearanceWindowButtonBackground = p.AppearanceWindowButtonBackground,
                AppearanceWindowButtonHoverBackground = p.AppearanceWindowButtonHoverBackground,

                SharedConfigPacksWindowBackground = p.SharedConfigPacksWindowBackground,
                SharedConfigPacksWindowText = p.SharedConfigPacksWindowText,
                SharedConfigPacksWindowControlBackground = p.SharedConfigPacksWindowControlBackground,
                SharedConfigPacksWindowControlBorder = p.SharedConfigPacksWindowControlBorder,
                SharedConfigPacksWindowHeaderText = p.SharedConfigPacksWindowHeaderText,
                SharedConfigPacksWindowHeaderBackground = p.SharedConfigPacksWindowHeaderBackground,
                SharedConfigPacksWindowTabBackground = p.SharedConfigPacksWindowTabBackground,
                SharedConfigPacksWindowTabHoverBackground = p.SharedConfigPacksWindowTabHoverBackground,
                SharedConfigPacksWindowTabSelectedBackground = p.SharedConfigPacksWindowTabSelectedBackground,
                SharedConfigPacksWindowButtonBackground = p.SharedConfigPacksWindowButtonBackground,
                SharedConfigPacksWindowButtonHoverBackground = p.SharedConfigPacksWindowButtonHoverBackground,
                SharedConfigPacksWindowEditsHoverBackground = p.SharedConfigPacksWindowEditsHoverBackground,
                SharedConfigPacksWindowCheckBoxBackground = p.SharedConfigPacksWindowCheckBoxBackground,
                SharedConfigPacksWindowCheckBoxTick = p.SharedConfigPacksWindowCheckBoxTick,

                CompareXmlWindowBackground = p.CompareXmlWindowBackground,
                CompareXmlWindowText = p.CompareXmlWindowText,
                CompareXmlWindowControlBackground = p.CompareXmlWindowControlBackground,
                CompareXmlWindowControlBorder = p.CompareXmlWindowControlBorder,
                CompareXmlWindowHeaderText = p.CompareXmlWindowHeaderText,
                CompareXmlWindowHeaderBackground = p.CompareXmlWindowHeaderBackground,
                CompareXmlWindowButtonBackground = p.CompareXmlWindowButtonBackground,
                CompareXmlWindowButtonHoverBackground = p.CompareXmlWindowButtonHoverBackground,
                CompareXmlWindowEditsHoverBackground = p.CompareXmlWindowEditsHoverBackground,
                CompareXmlWindowCheckBoxBackground = p.CompareXmlWindowCheckBoxBackground,
                CompareXmlWindowCheckBoxTick = p.CompareXmlWindowCheckBoxTick,

                BackupBrowserWindowBackground = p.BackupBrowserWindowBackground,
                BackupBrowserWindowText = p.BackupBrowserWindowText,
                BackupBrowserWindowControlBackground = p.BackupBrowserWindowControlBackground,
                BackupBrowserWindowControlBorder = p.BackupBrowserWindowControlBorder,
                BackupBrowserWindowHeaderText = p.BackupBrowserWindowHeaderText,
                BackupBrowserWindowHeaderBackground = p.BackupBrowserWindowHeaderBackground,
                BackupBrowserWindowListHoverBackground = p.BackupBrowserWindowListHoverBackground,
                BackupBrowserWindowListSelectedBackground = p.BackupBrowserWindowListSelectedBackground,
                BackupBrowserWindowButtonBackground = p.BackupBrowserWindowButtonBackground,
                BackupBrowserWindowButtonHoverBackground = p.BackupBrowserWindowButtonHoverBackground,
                BackupBrowserWindowXmlFilterHoverBackground = p.BackupBrowserWindowXmlFilterHoverBackground,
                BackupBrowserWindowXmlFilterSelectedBackground = p.BackupBrowserWindowXmlFilterSelectedBackground,

                SettingsInfoWindowBackground = p.SettingsInfoWindowBackground,
                SettingsInfoWindowText = p.SettingsInfoWindowText,
                SettingsInfoWindowControlBackground = p.SettingsInfoWindowControlBackground,
                SettingsInfoWindowControlBorder = p.SettingsInfoWindowControlBorder,
                SettingsInfoWindowHeaderText = p.SettingsInfoWindowHeaderText,
                SettingsInfoWindowHeaderBackground = p.SettingsInfoWindowHeaderBackground,
                SettingsInfoWindowButtonBackground = p.SettingsInfoWindowButtonBackground,
                SettingsInfoWindowButtonHoverBackground = p.SettingsInfoWindowButtonHoverBackground,
                SettingsInfoWindowCheckBoxBackground = p.SettingsInfoWindowCheckBoxBackground,
                SettingsInfoWindowCheckBoxTick = p.SettingsInfoWindowCheckBoxTick,

                DocumentationWindowBackground = p.DocumentationWindowBackground,
                DocumentationWindowText = p.DocumentationWindowText,
                DocumentationWindowControlBackground = p.DocumentationWindowControlBackground,
                DocumentationWindowControlBorder = p.DocumentationWindowControlBorder,
                DocumentationWindowHeaderText = p.DocumentationWindowHeaderText,
                DocumentationWindowHeaderBackground = p.DocumentationWindowHeaderBackground,
                DocumentationWindowListHoverBackground = p.DocumentationWindowListHoverBackground,
                DocumentationWindowListSelectedBackground = p.DocumentationWindowListSelectedBackground,
                DocumentationWindowButtonBackground = p.DocumentationWindowButtonBackground,
                DocumentationWindowButtonHoverBackground = p.DocumentationWindowButtonHoverBackground,

                XmlGuidesWindowBackground = p.XmlGuidesWindowBackground,
                XmlGuidesWindowText = p.XmlGuidesWindowText,
                XmlGuidesWindowControlBackground = p.XmlGuidesWindowControlBackground,
                XmlGuidesWindowControlBorder = p.XmlGuidesWindowControlBorder,
                XmlGuidesWindowButtonBackground = p.XmlGuidesWindowButtonBackground,
                XmlGuidesWindowButtonText = p.XmlGuidesWindowButtonText,
                XmlGuidesWindowButtonHoverBackground = p.XmlGuidesWindowButtonHoverBackground,
                XmlGuidesWindowButtonHoverText = p.XmlGuidesWindowButtonHoverText,
                XmlGuidesWindowGuidesListBackground = p.XmlGuidesWindowGuidesListBackground,
                XmlGuidesWindowGuidesListText = p.XmlGuidesWindowGuidesListText,
                XmlGuidesWindowGuidesListItemHoverBackground = p.XmlGuidesWindowGuidesListItemHoverBackground,
                XmlGuidesWindowGuidesListItemHoverText = p.XmlGuidesWindowGuidesListItemHoverText,
                XmlGuidesWindowGuidesListItemSelectedBackground = p.XmlGuidesWindowGuidesListItemSelectedBackground,
                XmlGuidesWindowGuidesListItemSelectedText = p.XmlGuidesWindowGuidesListItemSelectedText,
                XmlGuidesWindowFontPickerText = p.XmlGuidesWindowFontPickerText,

                SavedEditsWindowBackground = p.SavedEditsWindowBackground,
                SavedEditsWindowText = p.SavedEditsWindowText,
                SavedEditsWindowControlBackground = p.SavedEditsWindowControlBackground,
                SavedEditsWindowControlBorder = p.SavedEditsWindowControlBorder,
                SavedEditsWindowTabBackground = p.SavedEditsWindowTabBackground,
                SavedEditsWindowTabHoverBackground = p.SavedEditsWindowTabHoverBackground,
                SavedEditsWindowTabSelectedBackground = p.SavedEditsWindowTabSelectedBackground,
                SavedEditsWindowButtonBackground = p.SavedEditsWindowButtonBackground,
                SavedEditsWindowButtonHoverBackground = p.SavedEditsWindowButtonHoverBackground,
                SavedEditsWindowCheckBoxBackground = p.SavedEditsWindowCheckBoxBackground,
                SavedEditsWindowCheckBoxTick = p.SavedEditsWindowCheckBoxTick,
                SavedEditsWindowGridText = p.SavedEditsWindowGridText,
                SavedEditsWindowGridBackground = p.SavedEditsWindowGridBackground,
                SavedEditsWindowGridBorder = p.SavedEditsWindowGridBorder,
                SavedEditsWindowGridLines = p.SavedEditsWindowGridLines,
                SavedEditsWindowGridHeaderBackground = p.SavedEditsWindowGridHeaderBackground,
                SavedEditsWindowGridHeaderText = p.SavedEditsWindowGridHeaderText,
                SavedEditsWindowGridRowHoverBackground = p.SavedEditsWindowGridRowHoverBackground,
                SavedEditsWindowGridRowSelectedBackground = p.SavedEditsWindowGridRowSelectedBackground,
                SavedEditsWindowGridCellSelectedBackground = p.SavedEditsWindowGridCellSelectedBackground,
                SavedEditsWindowGridCellSelectedText = p.SavedEditsWindowGridCellSelectedText,

                EditorText = p.EditorText,
                EditorBackground = p.EditorBackground,
                EditorXmlSyntaxForeground = p.EditorXmlSyntaxForeground,

                MenuText = p.MenuText,
                MenuBackground = p.MenuBackground,
                TopBarText = p.TopBarText,
                TopButtonText = p.TopButtonText,
                TopButtonBackground = p.TopButtonBackground,

                TreeText = p.TreeText,
                TreeBackground = p.TreeBackground,
                TreeItemHoverBackground = p.TreeItemHoverBackground,
                TreeItemSelectedBackground = p.TreeItemSelectedBackground,
                Pane1TreeItemHoverBackground = p.Pane1TreeItemHoverBackground,
                Pane1TreeItemSelectedBackground = p.Pane1TreeItemSelectedBackground,

                RawTreeText = p.RawTreeText,
                RawTreeBackground = p.RawTreeBackground,
                RawTreeItemHoverBackground = p.RawTreeItemHoverBackground,
                RawTreeItemSelectedBackground = p.RawTreeItemSelectedBackground,
                RawPane1TreeItemHoverBackground = p.RawPane1TreeItemHoverBackground,
                RawPane1TreeItemSelectedBackground = p.RawPane1TreeItemSelectedBackground,

                FriendlyTreeText = p.FriendlyTreeText,
                FriendlyTreeBackground = p.FriendlyTreeBackground,
                FriendlyTreeItemHoverBackground = p.FriendlyTreeItemHoverBackground,
                FriendlyTreeItemSelectedBackground = p.FriendlyTreeItemSelectedBackground,
                FriendlyPane1TreeItemHoverBackground = p.FriendlyPane1TreeItemHoverBackground,
                FriendlyPane1TreeItemSelectedBackground = p.FriendlyPane1TreeItemSelectedBackground,

                GridText = p.GridText,
                GridBackground = p.GridBackground,
                GridBorder = p.GridBorder,
                GridLines = p.GridLines,
                GridHeaderBackground = p.GridHeaderBackground,
                GridHeaderText = p.GridHeaderText,
                GridRowHoverBackground = p.GridRowHoverBackground,
                GridRowSelectedBackground = p.GridRowSelectedBackground,
                GridCellSelectedBackground = p.GridCellSelectedBackground,
                SearchMatchBackground = p.SearchMatchBackground,
                SearchMatchText = p.SearchMatchText,

                FieldColumnText = p.FieldColumnText,
                FieldColumnBackground = p.FieldColumnBackground,
                ValueColumnText = p.ValueColumnText,
                ValueColumnBackground = p.ValueColumnBackground,
                HeaderText = p.HeaderText,
                SelectorBackground = p.SelectorBackground,

                Pane2ComboText = p.Pane2ComboText,
                Pane2ComboBackground = p.Pane2ComboBackground,
                Pane2DropdownText = p.Pane2DropdownText,
                Pane2DropdownBackground = p.Pane2DropdownBackground,
                Pane2ItemHoverBackground = p.Pane2ItemHoverBackground,
                Pane2ItemSelectedBackground = p.Pane2ItemSelectedBackground
            };
        }
    }
}

