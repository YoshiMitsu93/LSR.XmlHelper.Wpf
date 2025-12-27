using System;
using System.ComponentModel;
using System.Windows;

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
        }

        protected override void OnClosed(EventArgs e)
        {
            if (DataContext is ViewModels.AppearanceWindowViewModel vm)
                vm.CloseRequested -= VmOnCloseRequested;

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

        private void VmOnCloseRequested(object? sender, bool dialogResult)
        {
            DialogResult = dialogResult;
            Close();
        }
    }
}
