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
        public WpfBrush EditorTextBrush => CreateFrozenBrush(Active.EditorText);
        public WpfBrush EditorBackgroundBrush => CreateFrozenBrush(Active.EditorBackground);
        public WpfBrush MenuBackgroundBrush => CreateFrozenBrush(Active.MenuBackground);
        public WpfBrush MenuTextBrush => CreateFrozenBrush(Active.MenuText);
        public WpfBrush TreeTextBrush => CreateFrozenBrush(_isFriendlyView ? Active.FriendlyTreeText : Active.RawTreeText);
        public WpfBrush TreeBackgroundBrush => CreateFrozenBrush(_isFriendlyView ? Active.FriendlyTreeBackground : Active.RawTreeBackground);
        public WpfBrush TreeItemHoverBackgroundBrush => CreateFrozenBrush(_isFriendlyView ? Active.FriendlyTreeItemHoverBackground : Active.RawTreeItemHoverBackground);
        public WpfBrush TreeItemSelectedBackgroundBrush => CreateFrozenBrush(_isFriendlyView ? Active.FriendlyTreeItemSelectedBackground : Active.RawTreeItemSelectedBackground);
        public WpfBrush GridTextBrush => CreateFrozenBrush(Active.GridText);
        public WpfBrush GridBackgroundBrush => CreateFrozenBrush(Active.GridBackground);
        public WpfBrush GridBorderBrush => CreateFrozenBrush(Active.GridBorder);
        public WpfBrush GridLinesBrush => CreateFrozenBrush(Active.GridLines);
        public WpfBrush GridHeaderBackgroundBrush => CreateFrozenBrush(Active.GridHeaderBackground);
        public WpfBrush GridHeaderTextBrush => CreateFrozenBrush(Active.GridHeaderText);
        public WpfBrush GridRowHoverBackgroundBrush => CreateFrozenBrush(Active.GridRowHoverBackground);
        public WpfBrush GridRowSelectedBackgroundBrush => CreateFrozenBrush(Active.GridRowSelectedBackground);
        public WpfBrush GridCellSelectedBackgroundBrush => CreateFrozenBrush(Active.GridCellSelectedBackground);
        public WpfBrush GridCellSelectedTextBrush => CreateFrozenBrush(Active.GridCellSelectedText);
        public WpfBrush FieldColumnTextBrush => CreateFrozenBrush(Active.FieldColumnText);
        public WpfBrush ValueColumnTextBrush => CreateFrozenBrush(Active.ValueColumnText);
        public WpfBrush ValueColumnBackgroundBrush => CreateFrozenBrush(Active.ValueColumnBackground);
        public WpfBrush HeaderTextBrush => CreateFrozenBrush(Active.HeaderText);
        public WpfBrush SelectorBackgroundBrush => CreateFrozenBrush(Active.SelectorBackground);
        public WpfBrush Pane2ComboTextBrush => CreateFrozenBrush(Active.Pane2ComboText);
        public WpfBrush Pane2ComboBackgroundBrush => CreateFrozenBrush(Active.Pane2ComboBackground);
        public WpfBrush Pane2DropdownTextBrush => CreateFrozenBrush(Active.Pane2DropdownText);
        public WpfBrush Pane2DropdownBackgroundBrush => CreateFrozenBrush(Active.Pane2DropdownBackground);
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

            OnPropertyChanged(nameof(EditorTextBrush));
            OnPropertyChanged(nameof(EditorBackgroundBrush));

            OnPropertyChanged(nameof(MenuBackgroundBrush));
            OnPropertyChanged(nameof(MenuTextBrush));

            OnPropertyChanged(nameof(TreeTextBrush));
            OnPropertyChanged(nameof(TreeBackgroundBrush));
            OnPropertyChanged(nameof(TreeItemHoverBackgroundBrush));
            OnPropertyChanged(nameof(TreeItemSelectedBackgroundBrush));

            OnPropertyChanged(nameof(GridTextBrush));
            OnPropertyChanged(nameof(GridBackgroundBrush));
            OnPropertyChanged(nameof(GridBorderBrush));
            OnPropertyChanged(nameof(GridLinesBrush));
            OnPropertyChanged(nameof(GridHeaderBackgroundBrush));
            OnPropertyChanged(nameof(GridHeaderTextBrush));

            OnPropertyChanged(nameof(GridRowHoverBackgroundBrush));
            OnPropertyChanged(nameof(GridRowSelectedBackgroundBrush));
            OnPropertyChanged(nameof(GridCellSelectedBackgroundBrush));
            OnPropertyChanged(nameof(GridCellSelectedTextBrush));

            OnPropertyChanged(nameof(FieldColumnTextBrush));
            OnPropertyChanged(nameof(ValueColumnTextBrush));
            OnPropertyChanged(nameof(ValueColumnBackgroundBrush));
            OnPropertyChanged(nameof(HeaderTextBrush));
            OnPropertyChanged(nameof(SelectorBackgroundBrush));
            OnPropertyChanged(nameof(Pane2ComboTextBrush));
            OnPropertyChanged(nameof(Pane2ComboBackgroundBrush));
            OnPropertyChanged(nameof(Pane2DropdownTextBrush));
            OnPropertyChanged(nameof(Pane2DropdownBackgroundBrush));
            OnPropertyChanged(nameof(TopButtonTextBrush));
            OnPropertyChanged(nameof(TopButtonBackgroundBrush));
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
