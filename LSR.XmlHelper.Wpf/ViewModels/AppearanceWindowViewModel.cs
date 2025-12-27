using LSR.XmlHelper.Wpf.Infrastructure;
using LSR.XmlHelper.Wpf.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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

            RaisePreview();
            ApplyPreviewIfEditingCurrentTheme();
        }

        public event EventHandler<bool>? CloseRequested;

        public List<string> FontFamilies { get; }

        public RelayCommandOfT<string> PickColorCommand { get; }

        public RelayCommand ApplyCommand { get; }
        public RelayCommand OkCommand { get; }
        public RelayCommand CancelCommand { get; }

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

        private string _menuText = "#FFD4D4D4";
        private string _menuBackground = "#FF1E1E1E";
        private string _topButtonText = "#FFD4D4D4";
        private string _topButtonBackground = "#FF2F2F2F";

        private string _treeText = "#FFD4D4D4";
        private string _treeBackground = "#FF1E1E1E";
        private string _treeItemHoverBackground = "#FF252525";
        private string _treeItemSelectedBackground = "#FF2F2F2F";

        private string _gridText = "#FFD4D4D4";
        private string _gridBackground = "#FF1E1E1E";
        private string _gridBorder = "#FF555555";
        private string _gridLines = "#FF555555";
        private string _gridRowHoverBackground = "#FF252525";
        private string _gridHeaderText = "#FFD4D4D4";
        private string _gridRowSelectedBackground = "#FF2F2F2F";
        private string _gridCellSelectedBackground = "#FF2F2F2F";
        private string _gridCellSelectedText = "#FFD4D4D4";
        private string _pane3GroupHeaderText = "#FFD4D4D4";

        private string _fieldColumnText = "#FFD4D4D4";
        private string _valueColumnText = "#FFD4D4D4";
        private string _valueColumnBackground = "#00000000";
        private string _headerText = "#FFD4D4D4";
        private string _selectorBackground = "#FF1E1E1E";
        private string _pane2ComboText = "#FFD4D4D4";
        private string _pane2ComboBackground = "#FF1E1E1E";
        private string _pane2DropdownText = "#FFD4D4D4";
        private string _pane2DropdownBackground = "#FF1E1E1E";


        public string UiFontFamily { get => _uiFontFamily; set { if (SetProperty(ref _uiFontFamily, value)) OnEdited(); } }
        public string UiFontSize { get => _uiFontSize; set { if (SetProperty(ref _uiFontSize, value)) OnEdited(); } }

        public string EditorFontFamily { get => _editorFontFamily; set { if (SetProperty(ref _editorFontFamily, value)) OnEdited(); } }
        public string EditorFontSize { get => _editorFontSize; set { if (SetProperty(ref _editorFontSize, value)) OnEdited(); } }

        public string Text { get => _text; set { if (SetProperty(ref _text, value)) OnEdited(); } }
        public string Background { get => _background; set { if (SetProperty(ref _background, value)) OnEdited(); } }

        public string EditorText { get => _editorText; set { if (SetProperty(ref _editorText, value)) OnEdited(); } }
        public string EditorBackground { get => _editorBackground; set { if (SetProperty(ref _editorBackground, value)) OnEdited(); } }

        public string MenuText { get => _menuText; set { if (SetProperty(ref _menuText, value)) OnEdited(); } }
        public string MenuBackground { get => _menuBackground; set { if (SetProperty(ref _menuBackground, value)) OnEdited(); } }
        public string TopButtonText { get => _topButtonText; set { if (SetProperty(ref _topButtonText, value)) OnEdited(); } }
        public string TopButtonBackground { get => _topButtonBackground; set { if (SetProperty(ref _topButtonBackground, value)) OnEdited(); } }

        public string TreeText { get => _treeText; set { if (SetProperty(ref _treeText, value)) OnEdited(); } }
        public string TreeBackground { get => _treeBackground; set { if (SetProperty(ref _treeBackground, value)) OnEdited(); } }
        public string TreeItemHoverBackground { get => _treeItemHoverBackground; set { if (SetProperty(ref _treeItemHoverBackground, value)) OnEdited(); } }
        public string TreeItemSelectedBackground { get => _treeItemSelectedBackground; set { if (SetProperty(ref _treeItemSelectedBackground, value)) OnEdited(); } }

        public string GridText { get => _gridText; set { if (SetProperty(ref _gridText, value)) OnEdited(); } }
        public string GridBackground { get => _gridBackground; set { if (SetProperty(ref _gridBackground, value)) OnEdited(); } }
        public string GridBorder { get => _gridBorder; set { if (SetProperty(ref _gridBorder, value)) OnEdited(); } }
        public string GridLines { get => _gridLines; set { if (SetProperty(ref _gridLines, value)) OnEdited(); } }
        public string GridRowHoverBackground { get => _gridRowHoverBackground; set { if (SetProperty(ref _gridRowHoverBackground, value)) OnEdited(); } }
        public string GridHeaderText { get => _gridHeaderText; set { if (SetProperty(ref _gridHeaderText, value)) OnEdited(); } }
        public string GridRowSelectedBackground { get => _gridRowSelectedBackground; set { if (SetProperty(ref _gridRowSelectedBackground, value)) OnEdited(); } }
        public string GridCellSelectedBackground { get => _gridCellSelectedBackground; set { if (SetProperty(ref _gridCellSelectedBackground, value)) OnEdited(); } }
        public string GridCellSelectedText { get => _gridCellSelectedText; set { if (SetProperty(ref _gridCellSelectedText, value)) OnEdited(); } }

        public string FieldColumnText { get => _fieldColumnText; set { if (SetProperty(ref _fieldColumnText, value)) OnEdited(); } }
        public string ValueColumnText { get => _valueColumnText; set { if (SetProperty(ref _valueColumnText, value)) OnEdited(); } }
        public string ValueColumnBackground { get => _valueColumnBackground; set { if (SetProperty(ref _valueColumnBackground, value)) OnEdited(); } }
        public string HeaderText { get => _headerText; set { if (SetProperty(ref _headerText, value)) OnEdited(); } }
        public string SelectorBackground { get => _selectorBackground; set { if (SetProperty(ref _selectorBackground, value)) OnEdited(); } }

        public string Pane2ComboText { get => _pane2ComboText; set { if (SetProperty(ref _pane2ComboText, value)) OnEdited(); } }
        public string Pane2ComboBackground { get => _pane2ComboBackground; set { if (SetProperty(ref _pane2ComboBackground, value)) OnEdited(); } }
        public string Pane2DropdownText { get => _pane2DropdownText; set { if (SetProperty(ref _pane2DropdownText, value)) OnEdited(); } }
        public string Pane2DropdownBackground { get => _pane2DropdownBackground; set { if (SetProperty(ref _pane2DropdownBackground, value)) OnEdited(); } }
        public WpfBrush PreviewTextBrush => TryParseBrush(Text);
        public WpfBrush PreviewBackgroundBrush => TryParseBrush(Background);

        public WpfBrush PreviewEditorTextBrush => TryParseBrush(EditorText);
        public WpfBrush PreviewEditorBackgroundBrush => TryParseBrush(EditorBackground);

        public WpfBrush PreviewMenuTextBrush => TryParseBrush(MenuText);
        public WpfBrush PreviewMenuBackgroundBrush => TryParseBrush(MenuBackground);
        public WpfBrush PreviewTopButtonTextBrush => TryParseBrush(TopButtonText);
        public WpfBrush PreviewTopButtonBackgroundBrush => TryParseBrush(TopButtonBackground);

        public WpfBrush PreviewTreeTextBrush => TryParseBrush(TreeText);
        public WpfBrush PreviewTreeBackgroundBrush => TryParseBrush(TreeBackground);
        public WpfBrush PreviewTreeHoverBrush => TryParseBrush(TreeItemHoverBackground);
        public WpfBrush PreviewTreeSelectedBrush => TryParseBrush(TreeItemSelectedBackground);

        public WpfBrush PreviewGridTextBrush => TryParseBrush(GridText);
        public WpfBrush PreviewGridBackgroundBrush => TryParseBrush(GridBackground);
        public WpfBrush PreviewGridBorderBrush => TryParseBrush(GridBorder);
        public WpfBrush PreviewGridLinesBrush => TryParseBrush(GridLines);
        public WpfBrush PreviewGridRowHoverBrush => TryParseBrush(GridRowHoverBackground);
        public WpfBrush PreviewGridHeaderTextBrush => TryParseBrush(GridHeaderText);
        public WpfBrush PreviewGridRowSelectedBackgroundBrush => TryParseBrush(GridRowSelectedBackground);
        public WpfBrush PreviewGridCellSelectedBackgroundBrush => TryParseBrush(GridCellSelectedBackground);
        public WpfBrush PreviewGridCellSelectedTextBrush => TryParseBrush(GridCellSelectedText);

        public WpfBrush PreviewFieldColumnTextBrush => TryParseBrush(FieldColumnText);
        public WpfBrush PreviewValueColumnTextBrush => TryParseBrush(ValueColumnText);
        public WpfBrush PreviewValueColumnBackgroundBrush => TryParseBrush(ValueColumnBackground);
        public WpfBrush PreviewHeaderTextBrush => TryParseBrush(HeaderText);
        public WpfBrush PreviewSelectorBackgroundBrush => TryParseBrush(SelectorBackground);

        public WpfBrush PreviewPane2ComboTextBrush => TryParseBrush(Pane2ComboText);
        public WpfBrush PreviewPane2ComboBackgroundBrush => TryParseBrush(Pane2ComboBackground);
        public WpfBrush PreviewPane2DropdownTextBrush => TryParseBrush(Pane2DropdownText);
        public WpfBrush PreviewPane2DropdownBackgroundBrush => TryParseBrush(Pane2DropdownBackground);

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

            using var dlg = new ColorDialog
            {
                FullOpen = true,
                Color = initial
            };

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

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

                _editorText = p.EditorText;
                _editorBackground = p.EditorBackground;

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
                }
                else
                {
                    _treeText = p.RawTreeText;
                    _treeBackground = p.RawTreeBackground;
                    _treeItemHoverBackground = p.RawTreeItemHoverBackground;
                    _treeItemSelectedBackground = p.RawTreeItemSelectedBackground;
                }

                _gridText = p.GridText;
                _gridBackground = p.GridBackground;
                _gridBorder = p.GridBorder;
                _gridLines = p.GridLines;
                _gridRowHoverBackground = p.GridRowHoverBackground;
                _gridHeaderText = p.GridHeaderText;
                _gridRowSelectedBackground = p.GridRowSelectedBackground;
                _gridCellSelectedBackground = p.GridCellSelectedBackground;
                _gridCellSelectedText = p.GridCellSelectedText;

                _fieldColumnText = p.FieldColumnText;
                _valueColumnText = p.ValueColumnText;
                _valueColumnBackground = p.ValueColumnBackground;
                _headerText = p.HeaderText;
                _selectorBackground = p.SelectorBackground;

                _pane2ComboText = p.Pane2ComboText;
                _pane2ComboBackground = p.Pane2ComboBackground;
                _pane2DropdownText = p.Pane2DropdownText;
                _pane2DropdownBackground = p.Pane2DropdownBackground;

                OnPropertyChanged(nameof(UiFontFamily));
                OnPropertyChanged(nameof(UiFontSize));

                OnPropertyChanged(nameof(EditorFontFamily));
                OnPropertyChanged(nameof(EditorFontSize));

                OnPropertyChanged(nameof(Text));
                OnPropertyChanged(nameof(Background));

                OnPropertyChanged(nameof(EditorText));
                OnPropertyChanged(nameof(EditorBackground));

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
                OnPropertyChanged(nameof(GridCellSelectedText));

                OnPropertyChanged(nameof(FieldColumnText));
                OnPropertyChanged(nameof(ValueColumnText));
                OnPropertyChanged(nameof(ValueColumnBackground));
                OnPropertyChanged(nameof(HeaderText));
                OnPropertyChanged(nameof(SelectorBackground));

                OnPropertyChanged(nameof(Pane2ComboText));
                OnPropertyChanged(nameof(Pane2ComboBackground));
                OnPropertyChanged(nameof(Pane2DropdownText));
                OnPropertyChanged(nameof(Pane2DropdownBackground));

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

            p.EditorText = NormalizeColor(EditorText, p.EditorText);
            p.EditorBackground = NormalizeColor(EditorBackground, p.EditorBackground);

            p.MenuText = NormalizeColor(MenuText, p.MenuText);
            p.MenuBackground = NormalizeColor(MenuBackground, p.MenuBackground);
            p.TopButtonText = NormalizeColor(TopButtonText, p.TopButtonText);
            p.TopButtonBackground = NormalizeColor(TopButtonBackground, p.TopButtonBackground);

            p.Pane2ComboText = NormalizeColor(Pane2ComboText, p.Pane2ComboText);
            p.Pane2ComboBackground = NormalizeColor(Pane2ComboBackground, p.Pane2ComboBackground);
            p.Pane2DropdownText = NormalizeColor(Pane2DropdownText, p.Pane2DropdownText);
            p.Pane2DropdownBackground = NormalizeColor(Pane2DropdownBackground, p.Pane2DropdownBackground);


            if (_isEditingFriendlyView)
            {
                p.FriendlyTreeText = NormalizeColor(TreeText, p.FriendlyTreeText);
                p.FriendlyTreeBackground = NormalizeColor(TreeBackground, p.FriendlyTreeBackground);
                p.FriendlyTreeItemHoverBackground = NormalizeColor(TreeItemHoverBackground, p.FriendlyTreeItemHoverBackground);
                p.FriendlyTreeItemSelectedBackground = NormalizeColor(TreeItemSelectedBackground, p.FriendlyTreeItemSelectedBackground);
            }
            else
            {
                p.RawTreeText = NormalizeColor(TreeText, p.RawTreeText);
                p.RawTreeBackground = NormalizeColor(TreeBackground, p.RawTreeBackground);
                p.RawTreeItemHoverBackground = NormalizeColor(TreeItemHoverBackground, p.RawTreeItemHoverBackground);
                p.RawTreeItemSelectedBackground = NormalizeColor(TreeItemSelectedBackground, p.RawTreeItemSelectedBackground);
            }

            p.GridText = NormalizeColor(GridText, p.GridText);
            p.GridBackground = NormalizeColor(GridBackground, p.GridBackground);
            p.GridBorder = NormalizeColor(GridBorder, p.GridBorder);
            p.GridLines = NormalizeColor(GridLines, p.GridLines);
            p.GridRowHoverBackground = NormalizeColor(GridRowHoverBackground, p.GridRowHoverBackground);
            p.GridHeaderText = NormalizeColor(GridHeaderText, p.GridHeaderText);
            p.GridRowSelectedBackground = NormalizeColor(GridRowSelectedBackground, p.GridRowSelectedBackground);
            p.GridCellSelectedBackground = NormalizeColor(GridCellSelectedBackground, p.GridCellSelectedBackground);
            p.GridCellSelectedText = NormalizeColor(GridCellSelectedText, p.GridCellSelectedText);

            p.FieldColumnText = NormalizeColor(FieldColumnText, p.FieldColumnText);
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

            OnPropertyChanged(nameof(PreviewTextBrush));
            OnPropertyChanged(nameof(PreviewBackgroundBrush));

            OnPropertyChanged(nameof(PreviewEditorTextBrush));
            OnPropertyChanged(nameof(PreviewEditorBackgroundBrush));

            OnPropertyChanged(nameof(PreviewMenuTextBrush));
            OnPropertyChanged(nameof(PreviewMenuBackgroundBrush));
            OnPropertyChanged(nameof(PreviewTopButtonTextBrush));
            OnPropertyChanged(nameof(PreviewTopButtonBackgroundBrush));

            OnPropertyChanged(nameof(PreviewTreeTextBrush));
            OnPropertyChanged(nameof(PreviewTreeBackgroundBrush));
            OnPropertyChanged(nameof(PreviewTreeHoverBrush));
            OnPropertyChanged(nameof(PreviewTreeSelectedBrush));

            OnPropertyChanged(nameof(PreviewGridTextBrush));
            OnPropertyChanged(nameof(PreviewGridBackgroundBrush));
            OnPropertyChanged(nameof(PreviewGridBorderBrush));
            OnPropertyChanged(nameof(PreviewGridLinesBrush));
            OnPropertyChanged(nameof(PreviewGridRowHoverBrush));
            OnPropertyChanged(nameof(PreviewGridHeaderTextBrush));
            OnPropertyChanged(nameof(PreviewGridRowSelectedBackgroundBrush));
            OnPropertyChanged(nameof(PreviewGridCellSelectedBackgroundBrush));
            OnPropertyChanged(nameof(PreviewGridCellSelectedTextBrush));

            OnPropertyChanged(nameof(PreviewFieldColumnTextBrush));
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

                nameof(MenuText) => MenuText,
                nameof(MenuBackground) => MenuBackground,
                nameof(TopButtonText) => TopButtonText,
                nameof(TopButtonBackground) => TopButtonBackground,

                nameof(TreeText) => TreeText,
                nameof(TreeBackground) => TreeBackground,
                nameof(TreeItemHoverBackground) => TreeItemHoverBackground,
                nameof(TreeItemSelectedBackground) => TreeItemSelectedBackground,

                nameof(GridBackground) => GridBackground,
                nameof(GridText) => GridText,
                nameof(GridBorder) => GridBorder,
                nameof(GridLines) => GridLines,
                nameof(GridRowHoverBackground) => GridRowHoverBackground,
                nameof(GridHeaderText) => GridHeaderText,
                nameof(GridRowSelectedBackground) => GridRowSelectedBackground,
                nameof(GridCellSelectedBackground) => GridCellSelectedBackground,
                nameof(GridCellSelectedText) => GridCellSelectedText,

                nameof(FieldColumnText) => FieldColumnText,
                nameof(ValueColumnText) => ValueColumnText,
                nameof(ValueColumnBackground) => ValueColumnBackground,
                nameof(HeaderText) => HeaderText,
                nameof(SelectorBackground) => SelectorBackground,
                nameof(Pane2ComboText) => Pane2ComboText,
nameof(Pane2ComboBackground) => Pane2ComboBackground,
nameof(Pane2DropdownText) => Pane2DropdownText,
nameof(Pane2DropdownBackground) => Pane2DropdownBackground,

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

                case nameof(MenuText): MenuText = hex; break;
                case nameof(MenuBackground): MenuBackground = hex; break;
                case nameof(TopButtonText): TopButtonText = hex; break;
                case nameof(TopButtonBackground): TopButtonBackground = hex; break;

                case nameof(TreeText): TreeText = hex; break;
                case nameof(TreeBackground): TreeBackground = hex; break;
                case nameof(TreeItemHoverBackground): TreeItemHoverBackground = hex; break;
                case nameof(TreeItemSelectedBackground): TreeItemSelectedBackground = hex; break;

                case nameof(Pane2ComboText): Pane2ComboText = hex; break;
                case nameof(Pane2ComboBackground): Pane2ComboBackground = hex; break;
                case nameof(Pane2DropdownText): Pane2DropdownText = hex; break;
                case nameof(Pane2DropdownBackground): Pane2DropdownBackground = hex; break;

                case nameof(GridBackground): GridBackground = hex; break;
                case nameof(GridText): GridText = hex; break;
                case nameof(GridBorder): GridBorder = hex; break;
                case nameof(GridLines): GridLines = hex; break;
                case nameof(GridRowHoverBackground): GridRowHoverBackground = hex; break;
                case nameof(GridHeaderText): GridHeaderText = hex; break;
                case nameof(GridRowSelectedBackground): GridRowSelectedBackground = hex; break;
                case nameof(GridCellSelectedBackground): GridCellSelectedBackground = hex; break;
                case nameof(GridCellSelectedText): GridCellSelectedText = hex; break;

                case nameof(FieldColumnText): FieldColumnText = hex; break;
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
            return new AppearanceSettings
            {
                Dark = CloneProfile(source.Dark),
                Light = CloneProfile(source.Light)
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

                EditorText = p.EditorText,
                EditorBackground = p.EditorBackground,

                MenuText = p.MenuText,
                MenuBackground = p.MenuBackground,
                TopBarText = p.TopBarText,
                TopButtonText = p.TopButtonText,
                TopButtonBackground = p.TopButtonBackground,

                TreeText = p.TreeText,
                TreeBackground = p.TreeBackground,
                TreeItemHoverBackground = p.TreeItemHoverBackground,
                TreeItemSelectedBackground = p.TreeItemSelectedBackground,

                RawTreeText = p.RawTreeText,
                RawTreeBackground = p.RawTreeBackground,
                RawTreeItemHoverBackground = p.RawTreeItemHoverBackground,
                RawTreeItemSelectedBackground = p.RawTreeItemSelectedBackground,

                FriendlyTreeText = p.FriendlyTreeText,
                FriendlyTreeBackground = p.FriendlyTreeBackground,
                FriendlyTreeItemHoverBackground = p.FriendlyTreeItemHoverBackground,
                FriendlyTreeItemSelectedBackground = p.FriendlyTreeItemSelectedBackground,

                GridText = p.GridText,
                GridBackground = p.GridBackground,
                GridBorder = p.GridBorder,
                GridLines = p.GridLines,
                GridHeaderBackground = p.GridHeaderBackground,
                GridHeaderText = p.GridHeaderText,
                GridRowHoverBackground = p.GridRowHoverBackground,
                GridRowSelectedBackground = p.GridRowSelectedBackground,
                GridCellSelectedBackground = p.GridCellSelectedBackground,
                GridCellSelectedText = p.GridCellSelectedText,

                FieldColumnText = p.FieldColumnText,
                ValueColumnText = p.ValueColumnText,
                ValueColumnBackground = p.ValueColumnBackground,
                HeaderText = p.HeaderText,
                SelectorBackground = p.SelectorBackground,

                Pane2ComboText = p.Pane2ComboText,
                Pane2ComboBackground = p.Pane2ComboBackground,
                Pane2DropdownText = p.Pane2DropdownText,
                Pane2DropdownBackground = p.Pane2DropdownBackground
            };
        }
    }
}

