using LSR.XmlHelper.Wpf.Infrastructure;
using LSR.XmlHelper.Wpf.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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

        private readonly AppearanceSettings _workingCopy;
        private readonly bool _isDarkMode;

        public AppearanceWindowViewModel(AppSettingsService settingsService, AppSettings appSettings, AppearanceService appearance, bool isDarkMode)
        {
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
            _appSettings = appSettings ?? throw new ArgumentNullException(nameof(appSettings));
            _appearance = appearance ?? throw new ArgumentNullException(nameof(appearance));
            _isDarkMode = isDarkMode;

            _workingCopy = CloneAppearance(appSettings.Appearance);

            FontFamilies = Fonts.SystemFontFamilies
                .Select(f => f.Source)
                .OrderBy(s => s, StringComparer.OrdinalIgnoreCase)
                .ToList();

            LoadFromProfile(_workingCopy.GetActiveProfile(_isDarkMode));

            ApplyCommand = new RelayCommand(Apply);
            OkCommand = new RelayCommand(Ok);
            CancelCommand = new RelayCommand(Cancel);
        }

        public event EventHandler<bool>? CloseRequested;

        public List<string> FontFamilies { get; }

        public RelayCommand ApplyCommand { get; }
        public RelayCommand OkCommand { get; }
        public RelayCommand CancelCommand { get; }

        private string _uiFontFamily = "Segoe UI";
        private string _uiFontSize = "12";
        private string _editorFontFamily = "Consolas";
        private string _editorFontSize = "13";

        private string _text = "#FFD4D4D4";
        private string _background = "#FF1E1E1E";

        private string _treeText = "#FFD4D4D4";
        private string _treeBackground = "#FF1E1E1E";
        private string _treeItemHoverBackground = "#FF252525";
        private string _treeItemSelectedBackground = "#FF2F2F2F";

        private string _gridText = "#FFD4D4D4";
        private string _gridBackground = "#FF1E1E1E";
        private string _gridBorder = "#FF555555";
        private string _gridRowHoverBackground = "#FF252525";

        public string UiFontFamily { get => _uiFontFamily; set { if (SetProperty(ref _uiFontFamily, value)) RaisePreview(); } }
        public string UiFontSize { get => _uiFontSize; set { if (SetProperty(ref _uiFontSize, value)) RaisePreview(); } }

        public string EditorFontFamily { get => _editorFontFamily; set { if (SetProperty(ref _editorFontFamily, value)) RaisePreview(); } }
        public string EditorFontSize { get => _editorFontSize; set { if (SetProperty(ref _editorFontSize, value)) RaisePreview(); } }

        public string Text { get => _text; set { if (SetProperty(ref _text, value)) RaisePreview(); } }
        public string Background { get => _background; set { if (SetProperty(ref _background, value)) RaisePreview(); } }

        public string TreeText { get => _treeText; set { if (SetProperty(ref _treeText, value)) RaisePreview(); } }
        public string TreeBackground { get => _treeBackground; set { if (SetProperty(ref _treeBackground, value)) RaisePreview(); } }
        public string TreeItemHoverBackground { get => _treeItemHoverBackground; set { if (SetProperty(ref _treeItemHoverBackground, value)) RaisePreview(); } }
        public string TreeItemSelectedBackground { get => _treeItemSelectedBackground; set { if (SetProperty(ref _treeItemSelectedBackground, value)) RaisePreview(); } }

        public string GridText { get => _gridText; set { if (SetProperty(ref _gridText, value)) RaisePreview(); } }
        public string GridBackground { get => _gridBackground; set { if (SetProperty(ref _gridBackground, value)) RaisePreview(); } }
        public string GridBorder { get => _gridBorder; set { if (SetProperty(ref _gridBorder, value)) RaisePreview(); } }
        public string GridRowHoverBackground { get => _gridRowHoverBackground; set { if (SetProperty(ref _gridRowHoverBackground, value)) RaisePreview(); } }

        public WpfBrush PreviewTextBrush => TryParseBrush(Text);
        public WpfBrush PreviewBackgroundBrush => TryParseBrush(Background);

        public WpfBrush PreviewTreeTextBrush => TryParseBrush(TreeText);
        public WpfBrush PreviewTreeBackgroundBrush => TryParseBrush(TreeBackground);
        public WpfBrush PreviewTreeHoverBrush => TryParseBrush(TreeItemHoverBackground);
        public WpfBrush PreviewTreeSelectedBrush => TryParseBrush(TreeItemSelectedBackground);

        public WpfBrush PreviewGridTextBrush => TryParseBrush(GridText);
        public WpfBrush PreviewGridBackgroundBrush => TryParseBrush(GridBackground);
        public WpfBrush PreviewGridBorderBrush => TryParseBrush(GridBorder);
        public WpfBrush PreviewGridRowHoverBrush => TryParseBrush(GridRowHoverBackground);

        private void RaisePreview()
        {
            OnPropertyChanged(nameof(PreviewTextBrush));
            OnPropertyChanged(nameof(PreviewBackgroundBrush));

            OnPropertyChanged(nameof(PreviewTreeTextBrush));
            OnPropertyChanged(nameof(PreviewTreeBackgroundBrush));
            OnPropertyChanged(nameof(PreviewTreeHoverBrush));
            OnPropertyChanged(nameof(PreviewTreeSelectedBrush));

            OnPropertyChanged(nameof(PreviewGridTextBrush));
            OnPropertyChanged(nameof(PreviewGridBackgroundBrush));
            OnPropertyChanged(nameof(PreviewGridBorderBrush));
            OnPropertyChanged(nameof(PreviewGridRowHoverBrush));
        }

        private void Apply()
        {
            var profile = _workingCopy.GetActiveProfile(_isDarkMode);
            WriteToProfile(profile);

            _appSettings.Appearance = _workingCopy;
            _appearance.ReplaceSettings(_workingCopy);
        }

        private void Ok()
        {
            Apply();

            try
            {
                _settingsService.Save(_appSettings);
            }
            catch
            {
            }

            CloseRequested?.Invoke(this, true);
        }

        private void Cancel()
        {
            CloseRequested?.Invoke(this, false);
        }

        private void LoadFromProfile(AppearanceProfileSettings p)
        {
            _uiFontFamily = p.UiFontFamily;
            _uiFontSize = p.UiFontSize.ToString(CultureInfo.InvariantCulture);

            _editorFontFamily = p.EditorFontFamily;
            _editorFontSize = p.EditorFontSize.ToString(CultureInfo.InvariantCulture);

            _text = p.Text;
            _background = p.Background;

            _treeText = p.TreeText;
            _treeBackground = p.TreeBackground;
            _treeItemHoverBackground = p.TreeItemHoverBackground;
            _treeItemSelectedBackground = p.TreeItemSelectedBackground;

            _gridText = p.GridText;
            _gridBackground = p.GridBackground;
            _gridBorder = p.GridBorder;
            _gridRowHoverBackground = p.GridRowHoverBackground;

            OnPropertyChanged(nameof(UiFontFamily));
            OnPropertyChanged(nameof(UiFontSize));
            OnPropertyChanged(nameof(EditorFontFamily));
            OnPropertyChanged(nameof(EditorFontSize));

            OnPropertyChanged(nameof(Text));
            OnPropertyChanged(nameof(Background));

            OnPropertyChanged(nameof(TreeText));
            OnPropertyChanged(nameof(TreeBackground));
            OnPropertyChanged(nameof(TreeItemHoverBackground));
            OnPropertyChanged(nameof(TreeItemSelectedBackground));

            OnPropertyChanged(nameof(GridText));
            OnPropertyChanged(nameof(GridBackground));
            OnPropertyChanged(nameof(GridBorder));
            OnPropertyChanged(nameof(GridRowHoverBackground));

            RaisePreview();
        }

        private void WriteToProfile(AppearanceProfileSettings p)
        {
            p.UiFontFamily = UiFontFamily ?? "Segoe UI";
            p.UiFontSize = TryParseDouble(UiFontSize, 12);

            p.EditorFontFamily = EditorFontFamily ?? "Consolas";
            p.EditorFontSize = TryParseDouble(EditorFontSize, 13);

            p.Text = NormalizeColor(Text, p.Text);
            p.Background = NormalizeColor(Background, p.Background);

            p.TreeText = NormalizeColor(TreeText, p.TreeText);
            p.TreeBackground = NormalizeColor(TreeBackground, p.TreeBackground);
            p.TreeItemHoverBackground = NormalizeColor(TreeItemHoverBackground, p.TreeItemHoverBackground);
            p.TreeItemSelectedBackground = NormalizeColor(TreeItemSelectedBackground, p.TreeItemSelectedBackground);

            p.GridText = NormalizeColor(GridText, p.GridText);
            p.GridBackground = NormalizeColor(GridBackground, p.GridBackground);
            p.GridBorder = NormalizeColor(GridBorder, p.GridBorder);
            p.GridRowHoverBackground = NormalizeColor(GridRowHoverBackground, p.GridRowHoverBackground);
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

                TreeText = p.TreeText,
                TreeBackground = p.TreeBackground,
                TreeItemHoverBackground = p.TreeItemHoverBackground,
                TreeItemSelectedBackground = p.TreeItemSelectedBackground,

                GridText = p.GridText,
                GridBackground = p.GridBackground,
                GridBorder = p.GridBorder,
                GridHeaderBackground = p.GridHeaderBackground,
                GridHeaderText = p.GridHeaderText,
                GridRowHoverBackground = p.GridRowHoverBackground,
                GridRowSelectedBackground = p.GridRowSelectedBackground,
                GridCellSelectedBackground = p.GridCellSelectedBackground,
                GridCellSelectedText = p.GridCellSelectedText,
                FieldColumnText = p.FieldColumnText,
                ValueColumnText = p.ValueColumnText,
                HeaderText = p.HeaderText
            };
        }
    }
}