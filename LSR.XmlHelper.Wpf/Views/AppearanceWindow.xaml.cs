using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace LSR.XmlHelper.Wpf.Views
{
    public partial class AppearanceWindow : Window
    {
        public AppearanceWindow()
        {
            InitializeComponent();
        }

        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);

            if (DataContext is not ViewModels.AppearanceWindowViewModel vm)
                return;

            vm.CloseRequested += VmOnCloseRequested;
            vm.OnViewReady();
            vm.PropertyChanged += VmOnPropertyChanged;
            ApplyTabVisibility();
        }

        protected override void OnClosed(EventArgs e)
        {
            if (DataContext is ViewModels.AppearanceWindowViewModel vm)
            {
                vm.CloseRequested -= VmOnCloseRequested;
                vm.PropertyChanged -= VmOnPropertyChanged;
            }

            base.OnClosed(e);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (DataContext is not ViewModels.AppearanceWindowViewModel vm)
            {
                base.OnClosing(e);
                return;
            }

            if (!vm.IsDirty)
            {
                base.OnClosing(e);
                return;
            }

            var result = System.Windows.MessageBox.Show(
                "You have uncommitted appearance changes.\n\nApply changes before closing?",
                "Unapplied changes",
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Cancel)
            {
                e.Cancel = true;
                return;
            }

            if (result == MessageBoxResult.Yes)
            {
                vm.TryCommit();
            }
            else
            {
                vm.RevertPreview();
            }

            base.OnClosing(e);
        }

        private void AppearanceTabs_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded)
                return;

            ApplyTabVisibility();
        }

        private void VmOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ViewModels.AppearanceWindowViewModel.IsEditingFriendlyView))
                ApplyTabVisibility();
        }

        private void ApplyTabVisibility()
        {
            if (DataContext is not ViewModels.AppearanceWindowViewModel vm)
                return;

            var header = (AppearanceTabs.SelectedItem as TabItem)?.Header as string;
            var isFriendly = vm.IsEditingFriendlyView;

            SectionFontsPanel.Visibility = header == "Fonts" ? Visibility.Visible : Visibility.Collapsed;
            SectionAppearanceWindowPanel.Visibility = header == "Appearance Window" ? Visibility.Visible : Visibility.Collapsed;

            SectionSharedConfigPacksPanel.Visibility = header == "Shared Config Packs" ? Visibility.Visible : Visibility.Collapsed;

            SectionCompareXmlPanel.Visibility = header == "Compare XML" ? Visibility.Visible : Visibility.Collapsed;

            SectionBackupBrowserPanel.Visibility = header == "Backup Browser" ? Visibility.Visible : Visibility.Collapsed;

            SectionSettingsInfoPanel.Visibility = header == "Settings & Info" ? Visibility.Visible : Visibility.Collapsed;

            SectionDocumentationPanel.Visibility = header == "Documentation" ? Visibility.Visible : Visibility.Collapsed;

            SectionXmlGuidesPanel.Visibility = header == "LSR XML Guides" ? Visibility.Visible : Visibility.Collapsed;

            SectionSavedEditsPanel.Visibility = header == "Saved Edits" ? Visibility.Visible : Visibility.Collapsed;

            SectionEditorPanel.Visibility = header == "Editor" && !isFriendly ? Visibility.Visible : Visibility.Collapsed;

            SectionTopMenuRawPanel.Visibility = header == "Top Menu Bar" && !isFriendly ? Visibility.Visible : Visibility.Collapsed;
            SectionTopMenuFriendlyPanel.Visibility = header == "Top Menu Bar" && isFriendly ? Visibility.Visible : Visibility.Collapsed;

            SectionTopBarRawPanel.Visibility = header == "Top Bar Controls" && !isFriendly ? Visibility.Visible : Visibility.Collapsed;
            SectionTopBarFriendlyPanel.Visibility = header == "Top Bar Controls" && isFriendly ? Visibility.Visible : Visibility.Collapsed;

            SectionPane1RawPanel.Visibility = header == "Pane 1" && !isFriendly ? Visibility.Visible : Visibility.Collapsed;
            SectionPane1FriendlyPanel.Visibility = header == "Pane 1" && isFriendly ? Visibility.Visible : Visibility.Collapsed;

            SectionPane2Panel.Visibility = header == "Pane 2" && isFriendly ? Visibility.Visible : Visibility.Collapsed;
            SectionPane3Panel.Visibility = header == "Pane 3" && isFriendly ? Visibility.Visible : Visibility.Collapsed;

            Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() => AppearanceScrollViewer.ScrollToTop()));
        }

        private void VmOnCloseRequested(object? sender, bool dialogResult)
        {
            if (System.Windows.Interop.ComponentDispatcher.IsThreadModal)
                DialogResult = dialogResult;

            Close();
        }
    }
}
