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
            try
            {
                DialogResult = dialogResult;
            }
            catch (InvalidOperationException)
            {
            }

            Close();
        }
        private void BackupBrowserWindow_OnPreviewDragOver(object sender, System.Windows.DragEventArgs e)
        {
            if (e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop))
                e.Effects = System.Windows.DragDropEffects.Copy;
            else
                e.Effects = System.Windows.DragDropEffects.None;

            e.Handled = true;
        }

        private void BackupBrowserWindow_OnDrop(object sender, System.Windows.DragEventArgs e)
        {
            if (DataContext is not ViewModels.BackupBrowserWindowViewModel vm)
                return;

            if (!e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop))
                return;

            if (e.Data.GetData(System.Windows.DataFormats.FileDrop) is not string[] files)
                return;

            vm.AddDroppedFiles(files);
        }
    }
}
