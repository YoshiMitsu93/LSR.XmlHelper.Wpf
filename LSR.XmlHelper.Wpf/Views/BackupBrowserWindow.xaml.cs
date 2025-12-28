using System;
using System.Windows;

namespace LSR.XmlHelper.Wpf.Views
{
    public partial class BackupBrowserWindow : Window
    {
        public BackupBrowserWindow()
        {
            InitializeComponent();
        }

        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);

            if (DataContext is not ViewModels.BackupBrowserWindowViewModel vm)
                return;

            vm.CloseRequested += VmOnCloseRequested;
        }

        protected override void OnClosed(EventArgs e)
        {
            if (DataContext is ViewModels.BackupBrowserWindowViewModel vm)
                vm.CloseRequested -= VmOnCloseRequested;

            base.OnClosed(e);
        }

        private void VmOnCloseRequested(object? sender, bool dialogResult)
        {
            DialogResult = dialogResult;
            Close();
        }
    }
}
