using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Search;
using LSR.XmlHelper.Core.Services;
using LSR.XmlHelper.Wpf.Infrastructure;
using LSR.XmlHelper.Wpf.ViewModels;
using System.Windows.Controls;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using LSR.XmlHelper.Wpf.Services.Updates;
using System.Windows;

namespace LSR.XmlHelper.Wpf
{
    public partial class MainWindow : Window
    {
        private readonly XmlHelperRootService _helperRoot = new XmlHelperRootService();
        private SearchPanel? _searchPanel;

        public MainWindow()
        {
            InitializeComponent();

            var editor = FindName("XmlEditor") as TextEditor;
            if (editor is not null)
            {
                editor.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("XML");
                _searchPanel = SearchPanel.Install(editor);
            }

            DataContext = new MainWindowViewModel();
            if (DataContext is MainWindowViewModel vm)
                vm.RawNavigationRequested += VmOnRawNavigationRequested;
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

        private async void CheckForUpdates_Click(object sender, RoutedEventArgs e)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var currentVersion = assembly.GetName().Version;
            var currentText = currentVersion is null ? "Unknown" : currentVersion.ToString(3);

            try
            {
                var service = new GitHubReleaseService(new HttpClient());
                var latest = await service.GetLatestReleaseAsync("YoshiMitsu93", "LSR.XmlHelper.Wpf");

                if (latest is null)
                {
                    System.Windows.MessageBox.Show("Could not check for updates (no release info returned).", "Check for Updates", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var latestText = string.IsNullOrWhiteSpace(latest.TagName) ? "Unknown" : latest.TagName;
                var hasUpdate = currentVersion is not null && latest.Version is not null && latest.Version > currentVersion;

                var message =
                    hasUpdate
                        ? $"An update is available.\n\nCurrent: {currentText}\nLatest: {latestText}\n\nOpen the release page?"
                        : $"You are up to date.\n\nCurrent: {currentText}\nLatest: {latestText}\n\nOpen the release page anyway?";

                var result = System.Windows.MessageBox.Show(message, "Check for Updates", MessageBoxButton.YesNo, MessageBoxImage.Information);
                if (result != MessageBoxResult.Yes)
                    return;

                if (!string.IsNullOrWhiteSpace(latest.HtmlUrl))
                {
                    Process.Start(new ProcessStartInfo(latest.HtmlUrl) { UseShellExecute = true });
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Could not check for updates.\n\n{ex.Message}", "Check for Updates", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void OpenSettingsInfo_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is not LSR.XmlHelper.Wpf.ViewModels.MainWindowViewModel mainVm)
                return;

            var settingsService = new LSR.XmlHelper.Wpf.Services.AppSettingsService();
            var vm = new LSR.XmlHelper.Wpf.ViewModels.Windows.SettingsInfoWindowViewModel(mainVm, settingsService);

            var win = new LSR.XmlHelper.Wpf.Views.SettingsInfoWindow
            {
                Owner = System.Windows.Application.Current?.MainWindow,
                DataContext = vm
            };

            win.ShowDialog();
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
                if (DataContext is MainWindowViewModel vm && !string.IsNullOrWhiteSpace(vm.RootFolderPath) && Directory.Exists(vm.RootFolderPath))
                    xmlPath = Path.Combine(vm.RootFolderPath, "__folder__.xml");
                else
                {
                    System.Windows.MessageBox.Show("No folder is currently open.", "Open Helper Root Folder", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
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
                if (DataContext is MainWindowViewModel vm && !string.IsNullOrWhiteSpace(vm.RootFolderPath) && Directory.Exists(vm.RootFolderPath))
                    xmlPath = Path.Combine(vm.RootFolderPath, "__folder__.xml");
                else
                {
                    System.Windows.MessageBox.Show("No folder is currently open.", "Open Backups Folder", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
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

        private void VmOnRawNavigationRequested(object? sender, RawNavigationRequest e)
        {
            TrySelectFileInPane1(e.FilePath);

            var editor = FindName("XmlEditor") as TextEditor;
            if (editor is null)
                return;

            var offset = e.Offset;
            if (offset < 0)
                offset = 0;

            if (offset > editor.Text.Length)
                offset = editor.Text.Length;

            var length = e.Length;
            if (length < 0)
                length = 0;

            if (offset + length > editor.Text.Length)
                length = editor.Text.Length - offset;

            editor.Select(offset, length);
            editor.TextArea.Caret.Offset = offset;
            editor.TextArea.Caret.BringCaretToView();
            editor.Focus();
        }

        private void TrySelectFileInPane1(string filePath)
        {
            if (DataContext is not MainWindowViewModel vm)
                return;

            if (!vm.IsFoldersMode)
                return;

            var tree = XmlTreeView;
            if (tree is null)
                return;

            if (tree.Items.Count == 0)
                return;

            tree.UpdateLayout();

            if (!SelectTreeViewItemByPath(tree, filePath))
            {
                tree.UpdateLayout();
                SelectTreeViewItemByPath(tree, filePath);
            }
        }

        private bool SelectTreeViewItemByPath(ItemsControl parent, string filePath)
        {
            parent.UpdateLayout();

            for (var i = 0; i < parent.Items.Count; i++)
            {
                var item = parent.Items[i];
                if (item is not XmlExplorerNode node)
                    continue;

                var container = parent.ItemContainerGenerator.ContainerFromItem(item) as TreeViewItem;
                if (container is null)
                {
                    parent.UpdateLayout();
                    container = parent.ItemContainerGenerator.ContainerFromItem(item) as TreeViewItem;
                }

                if (container is null)
                    continue;

                if (node.IsFile && node.FullPath is not null &&
                    string.Equals(node.FullPath, filePath, StringComparison.OrdinalIgnoreCase))
                {
                    container.IsSelected = true;
                    container.BringIntoView();
                    container.Focus();
                    return true;
                }

                if (node.Children.Count > 0)
                {
                    if (!container.IsExpanded)
                    {
                        container.IsExpanded = true;
                        container.UpdateLayout();
                        parent.UpdateLayout();
                    }

                    if (SelectTreeViewItemByPath(container, filePath))
                        return true;
                }
            }

            return false;
        }

        private void OpenLocalSearch_Click(object sender, RoutedEventArgs e)
        {
            _searchPanel?.Open();
        }

        private void Pane3DuplicateEntry_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is ViewModels.MainWindowViewModel vm)
                vm.DuplicateSelectedFriendlyEntry();
        }

        private void Pane3DeleteEntry_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is ViewModels.MainWindowViewModel vm)
                vm.DeleteSelectedFriendlyEntry();
        }

        private void Pane3DuplicateItem_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is ViewModels.MainWindowViewModel vm)
                vm.DuplicateSelectedFriendlyLookupItem();
        }

        private void Pane3DeleteItem_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is ViewModels.MainWindowViewModel vm)
                vm.DeleteSelectedFriendlyLookupItem();
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
