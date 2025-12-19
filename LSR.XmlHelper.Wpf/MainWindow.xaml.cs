using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Search;
using LSR.XmlHelper.Core.Services;
using LSR.XmlHelper.Wpf.ViewModels;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace LSR.XmlHelper.Wpf
{
    public partial class MainWindow : Window
    {
        private readonly XmlHelperRootService _helperRoot = new XmlHelperRootService();

        public MainWindow()
        {
            InitializeComponent();

            var editor = FindName("XmlEditor") as TextEditor;
            if (editor is not null)
            {
                editor.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("XML");
                SearchPanel.Install(editor);
            }

            DataContext = new MainWindowViewModel();
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (DataContext is not MainWindowViewModel vm)
                return;

            vm.SelectedTreeNode = e.NewValue as XmlExplorerNode;
        }

        private void FriendlyGrid_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key != System.Windows.Input.Key.Enter)
                return;

            if (sender is not System.Windows.Controls.DataGrid grid)
                return;

            if (grid.CurrentColumn == null)
                return;

            if (grid.CurrentColumn.IsReadOnly || grid.IsReadOnly)
            {
                e.Handled = true;
                return;
            }

            if (grid.CommitEdit(System.Windows.Controls.DataGridEditingUnit.Cell, true))
            {
                grid.CommitEdit(System.Windows.Controls.DataGridEditingUnit.Row, true);
                e.Handled = true;
                return;
            }

            grid.BeginEdit();
            e.Handled = true;
        }

        private void TreeView_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key != System.Windows.Input.Key.Enter)
                return;

            if (DataContext is not MainWindowViewModel vm)
                return;

            if (vm.SelectedXmlFile != null)
            {
                e.Handled = true;
                return;
            }

            e.Handled = true;
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.MessageBox.Show(
                "LSR XML Helper\n\nFriendly view + XML editor.\nBackups are stored under the 'LSR-XML-Helper' folder next to your XML files.",
                "About",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void OpenCurrentXmlFolder_Click(object sender, RoutedEventArgs e)
        {
            var xmlPath = GetCurrentXmlPath();
            if (string.IsNullOrWhiteSpace(xmlPath) || !File.Exists(xmlPath))
            {
                System.Windows.MessageBox.Show("No XML file is currently selected.", "Open Folder", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var dir = Path.GetDirectoryName(xmlPath);
            if (string.IsNullOrWhiteSpace(dir) || !Directory.Exists(dir))
                return;

            OpenInExplorer(dir);
        }

        private void OpenHelperRootFolder_Click(object sender, RoutedEventArgs e)
        {
            var xmlPath = GetCurrentXmlPath();
            if (string.IsNullOrWhiteSpace(xmlPath) || !File.Exists(xmlPath))
            {
                System.Windows.MessageBox.Show("No XML file is currently selected.", "Open Helper Root Folder", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var root = _helperRoot.GetHelperRootForXmlPath(xmlPath);
            if (!Directory.Exists(root))
                Directory.CreateDirectory(root);

            OpenInExplorer(root);
        }

        private void OpenBackupsFolder_Click(object sender, RoutedEventArgs e)
        {
            var xmlPath = GetCurrentXmlPath();
            if (string.IsNullOrWhiteSpace(xmlPath) || !File.Exists(xmlPath))
            {
                System.Windows.MessageBox.Show("No XML file is currently selected.", "Open Backups Folder", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var backups = _helperRoot.GetOrCreateSubfolder(xmlPath, "BackupXMLs");
            OpenInExplorer(backups);
        }

        private string? GetCurrentXmlPath()
        {
            if (DataContext is not MainWindowViewModel vm)
                return null;

            if (vm.SelectedXmlFile is not null && !string.IsNullOrWhiteSpace(vm.SelectedXmlFile.FullPath))
                return vm.SelectedXmlFile.FullPath;

            if (vm.SelectedTreeNode is not null && vm.SelectedTreeNode.IsFile && !string.IsNullOrWhiteSpace(vm.SelectedTreeNode.FullPath))
                return vm.SelectedTreeNode.FullPath;

            return null;
        }

        private static void OpenInExplorer(string path)
        {
            try
            {
                if (Directory.Exists(path))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "explorer.exe",
                        Arguments = $"\"{path}\"",
                        UseShellExecute = true
                    });
                    return;
                }

                if (File.Exists(path))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "explorer.exe",
                        Arguments = $"/select,\"{path}\"",
                        UseShellExecute = true
                    });
                }
            }
            catch
            {
            }
        }
    }
}
