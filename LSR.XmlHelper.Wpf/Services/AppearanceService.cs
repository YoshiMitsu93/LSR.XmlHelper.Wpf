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

        public AppearanceService(AppearanceSettings settings, bool isDarkMode)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _isDarkMode = isDarkMode;
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

        public WpfBrush TreeTextBrush => CreateFrozenBrush(Active.TreeText);
        public WpfBrush TreeBackgroundBrush => CreateFrozenBrush(Active.TreeBackground);
        public WpfBrush TreeItemHoverBackgroundBrush => CreateFrozenBrush(Active.TreeItemHoverBackground);
        public WpfBrush TreeItemSelectedBackgroundBrush => CreateFrozenBrush(Active.TreeItemSelectedBackground);

        public WpfBrush GridTextBrush => CreateFrozenBrush(Active.GridText);
        public WpfBrush GridBackgroundBrush => CreateFrozenBrush(Active.GridBackground);
        public WpfBrush GridBorderBrush => CreateFrozenBrush(Active.GridBorder);

        public WpfBrush GridHeaderBackgroundBrush => CreateFrozenBrush(Active.GridHeaderBackground);
        public WpfBrush GridHeaderTextBrush => CreateFrozenBrush(Active.GridHeaderText);

        public WpfBrush GridRowHoverBackgroundBrush => CreateFrozenBrush(Active.GridRowHoverBackground);
        public WpfBrush GridRowSelectedBackgroundBrush => CreateFrozenBrush(Active.GridRowSelectedBackground);

        public WpfBrush GridCellSelectedBackgroundBrush => CreateFrozenBrush(Active.GridCellSelectedBackground);
        public WpfBrush GridCellSelectedTextBrush => CreateFrozenBrush(Active.GridCellSelectedText);

        public WpfBrush FieldColumnTextBrush => CreateFrozenBrush(Active.FieldColumnText);
        public WpfBrush ValueColumnTextBrush => CreateFrozenBrush(Active.ValueColumnText);
        public WpfBrush HeaderTextBrush => CreateFrozenBrush(Active.HeaderText);

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

            OnPropertyChanged(nameof(TreeTextBrush));
            OnPropertyChanged(nameof(TreeBackgroundBrush));
            OnPropertyChanged(nameof(TreeItemHoverBackgroundBrush));
            OnPropertyChanged(nameof(TreeItemSelectedBackgroundBrush));

            OnPropertyChanged(nameof(GridTextBrush));
            OnPropertyChanged(nameof(GridBackgroundBrush));
            OnPropertyChanged(nameof(GridBorderBrush));
            OnPropertyChanged(nameof(GridHeaderBackgroundBrush));
            OnPropertyChanged(nameof(GridHeaderTextBrush));

            OnPropertyChanged(nameof(GridRowHoverBackgroundBrush));
            OnPropertyChanged(nameof(GridRowSelectedBackgroundBrush));
            OnPropertyChanged(nameof(GridCellSelectedBackgroundBrush));
            OnPropertyChanged(nameof(GridCellSelectedTextBrush));

            OnPropertyChanged(nameof(FieldColumnTextBrush));
            OnPropertyChanged(nameof(ValueColumnTextBrush));
            OnPropertyChanged(nameof(HeaderTextBrush));
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