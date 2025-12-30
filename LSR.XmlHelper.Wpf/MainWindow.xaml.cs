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

        protected override void OnPreviewKeyDown(System.Windows.Input.KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);

            if (e.Handled)
                return;

            if (e.Key != System.Windows.Input.Key.D)
                return;

            var mods = System.Windows.Input.Keyboard.Modifiers;

            if ((mods & System.Windows.Input.ModifierKeys.Control) != System.Windows.Input.ModifierKeys.Control)
                return;

            if (DataContext is not MainWindowViewModel vm)
                return;

            if (!vm.IsFriendlyView || vm.SelectedFriendlyEntry is null)
                return;

            if ((mods & System.Windows.Input.ModifierKeys.Shift) == System.Windows.Input.ModifierKeys.Shift)
            {
                if (vm.SelectedFriendlyLookupItem is null)
                    return;

                vm.DuplicateSelectedFriendlyLookupItem();
                e.Handled = true;
                return;
            }

            vm.DuplicateSelectedFriendlyEntry();
            e.Handled = true;
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (DataContext is not MainWindowViewModel vm)
                return;

            vm.SelectedTreeNode = e.NewValue as XmlExplorerNode;
        }

        private void LookupGrid_PreviewMouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var dep = e.OriginalSource as System.Windows.DependencyObject;

            while (dep is not null && dep is not System.Windows.Controls.DataGridRow)
                dep = System.Windows.Media.VisualTreeHelper.GetParent(dep);

            if (dep is System.Windows.Controls.DataGridRow row)
                row.IsSelected = true;
        }
        private void LookupGrid_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (sender is not System.Windows.Controls.DataGrid grid)
                return;

            var selected = grid.SelectedItem;

            if (DataContext is MainWindowViewModel vm)
                vm.SelectedFriendlyLookupItem = selected as XmlFriendlyLookupItemViewModel;

            if (selected is null)
                return;

            grid.Dispatcher.BeginInvoke(new System.Action(() =>
            {
                if (grid.Items.Contains(selected))
                    grid.ScrollIntoView(selected);
            }), System.Windows.Threading.DispatcherPriority.Background);
        }

        private void FriendlyGrid_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (sender is not System.Windows.Controls.DataGrid grid)
                return;

            if (e.Key == System.Windows.Input.Key.Enter)
            {
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
                return;
            }

            if (e.Key != System.Windows.Input.Key.Up && e.Key != System.Windows.Input.Key.Down)
                return;

            if (grid.Items.Count == 0)
                return;

            if (grid.CommitEdit(System.Windows.Controls.DataGridEditingUnit.Cell, true))
                grid.CommitEdit(System.Windows.Controls.DataGridEditingUnit.Row, true);

            var currentIndex = grid.Items.IndexOf(grid.CurrentItem);
            if (currentIndex < 0)
                currentIndex = grid.SelectedIndex;

            var delta = e.Key == System.Windows.Input.Key.Up ? -1 : 1;
            var targetIndex = currentIndex + delta;

            if (targetIndex < 0 || targetIndex >= grid.Items.Count)
                return;

            var item = grid.Items[targetIndex];
            grid.SelectedItem = item;
            grid.ScrollIntoView(item);

            if (grid.CurrentColumn != null)
                grid.CurrentCell = new System.Windows.Controls.DataGridCellInfo(item, grid.CurrentColumn);

            grid.Focus();
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

        private void LookupExpandAll_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not System.Windows.Controls.Button button)
                return;

            var dock = FindAncestor<System.Windows.Controls.DockPanel>(button);
            if (dock is null)
                return;

            System.Windows.Controls.DataGrid? grid = null;

            foreach (var g in FindVisualChildren<System.Windows.Controls.DataGrid>(dock))
            {
                grid = g;
                break;
            }

            if (grid is null)
                return;

            SetLookupGroupExpanders(grid, true);
        }

        private void LookupCollapseAll_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not System.Windows.Controls.Button button)
                return;

            var dock = FindAncestor<System.Windows.Controls.DockPanel>(button);
            if (dock is null)
                return;

            System.Windows.Controls.DataGrid? grid = null;

            foreach (var g in FindVisualChildren<System.Windows.Controls.DataGrid>(dock))
            {
                grid = g;
                break;
            }

            if (grid is null)
                return;

            SetLookupGroupExpanders(grid, false);
        }

        private static void SetLookupGroupExpanders(System.Windows.Controls.DataGrid grid, bool isExpanded)
        {
            foreach (var expander in FindVisualChildren<System.Windows.Controls.Expander>(grid))
            {
                if (expander.DataContext is System.Windows.Data.CollectionViewGroup)
                    expander.IsExpanded = isExpanded;
            }
        }

        private static T? FindAncestor<T>(System.Windows.DependencyObject start) where T : System.Windows.DependencyObject
        {
            var current = start;

            while (current is not null)
            {
                if (current is T match)
                    return match;

                current = System.Windows.Media.VisualTreeHelper.GetParent(current);
            }

            return null;
        }

        private static System.Collections.Generic.IEnumerable<T> FindVisualChildren<T>(System.Windows.DependencyObject parent) where T : System.Windows.DependencyObject
        {
            var count = System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent);

            for (var i = 0; i < count; i++)
            {
                var child = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);

                if (child is T match)
                    yield return match;

                foreach (var nested in FindVisualChildren<T>(child))
                    yield return nested;
            }
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
