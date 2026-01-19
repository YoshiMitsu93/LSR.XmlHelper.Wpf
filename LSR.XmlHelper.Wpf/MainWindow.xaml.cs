using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Search;
using LSR.XmlHelper.Core.Services;
using LSR.XmlHelper.Wpf.Infrastructure;
using LSR.XmlHelper.Wpf.ViewModels;
using LSR.XmlHelper.Wpf.Services.Appearance;
using System.Windows.Controls;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using LSR.XmlHelper.Wpf.Services.Updates;
using System.Windows;
using System.Windows.Input;


namespace LSR.XmlHelper.Wpf
{
    public partial class MainWindow : Window
    {
        private Views.SettingsInfoWindow? _settingsInfoWindow;
        private Views.HelpDocumentationWindow? _helpDocumentationWindow;
        private Views.XmlGuidesWindow? _xmlGuidesWindow;
        private readonly XmlHelperRootService _helperRoot = new XmlHelperRootService();
        private SearchPanel? _searchPanel;
        private Views.FriendlySearchWindow? _friendlySearchWindow;
        private Views.ReplaceWindow? _replaceWindow;
        private bool _checkedUpdatesOnStartup;

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
            {
                vm.RawNavigationRequested += VmOnRawNavigationRequested;
                vm.PropertyChanged += Vm_PropertyChanged;
                if (editor is not null)
                {
                    XmlSyntaxHighlightingService.Apply(editor, vm.Appearance.EditorXmlSyntaxForeground);

                    vm.Appearance.PropertyChanged += (_, args) =>
                    {
                        if (args.PropertyName == nameof(Services.AppearanceService.EditorXmlSyntaxForeground))
                            XmlSyntaxHighlightingService.Apply(editor, vm.Appearance.EditorXmlSyntaxForeground);
                    };
                }
            }
            Loaded += MainWindow_Loaded;
        }
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is not MainWindowViewModel vm)
                return;

            if (vm.IsFirstRun)
                About_Click(this, new RoutedEventArgs());

            _ = AutoCheckForUpdatesOnStartupAsync();
        }

        private void MainWindowRoot_PreviewDragOver(object sender, System.Windows.DragEventArgs e)
        {
            if (e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop))
            {
                e.Effects = System.Windows.DragDropEffects.Copy;
                e.Handled = true;
                return;
            }

            e.Effects = System.Windows.DragDropEffects.None;
            e.Handled = true;
        }

        private void Vm_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(MainWindowViewModel.IsFriendlyView))
                return;

            if (DataContext is not MainWindowViewModel vm)
                return;

            if (vm.IsFriendlyView == false)
                return;

            if (_replaceWindow is null)
                return;

            var w = _replaceWindow;
            _replaceWindow = null;
            w.Close();
        }


        private void MainWindowRoot_Drop(object sender, System.Windows.DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop))
                return;

            var files = e.Data.GetData(System.Windows.DataFormats.FileDrop) as string[];
            if (files is null || files.Length == 0)
                return;

            string? xml = null;
            foreach (var f in files)
            {
                if (f is null)
                    continue;

                if (f.EndsWith(".xml", System.StringComparison.OrdinalIgnoreCase))
                {
                    xml = f;
                    break;
                }
            }

            if (string.IsNullOrWhiteSpace(xml))
                return;

            if (DataContext is MainWindowViewModel vm)
                vm.StartCompareXml(xml);
        }

        protected override void OnPreviewKeyDown(System.Windows.Input.KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);

            if (e.Handled)
                return;

            var mods = System.Windows.Input.Keyboard.Modifiers;
            var ctrl = (mods & System.Windows.Input.ModifierKeys.Control) == System.Windows.Input.ModifierKeys.Control;
            var shift = (mods & System.Windows.Input.ModifierKeys.Shift) == System.Windows.Input.ModifierKeys.Shift;

            if (ctrl && shift && e.Key == System.Windows.Input.Key.F)
            {
                if (DataContext is ViewModels.MainWindowViewModel vm)
                {
                    if (vm.OpenGlobalSearchCommand.CanExecute(null))
                        vm.OpenGlobalSearchCommand.Execute(null);
                }

                e.Handled = true;
                return;
            }

            if (ctrl && !shift && e.Key == System.Windows.Input.Key.F)
            {
                if (DataContext is ViewModels.MainWindowViewModel vm && vm.IsFriendlyView)
                {
                    ShowFriendlySearchWindow(vm.FindNextFriendly);
                    e.Handled = true;
                    return;
                }

                _searchPanel?.Open();
                FocusRawSearch();
                e.Handled = true;
                return;
            }

            if (e.Key != System.Windows.Input.Key.D)
                return;

            if (!ctrl)
                return;

            if (DataContext is not MainWindowViewModel vm2)
                return;

            if (!vm2.IsFriendlyView || vm2.SelectedFriendlyEntry is null)
                return;

            if (shift)
            {
                if (vm2.SelectedFriendlyLookupItem is null)
                    return;

                vm2.DuplicateSelectedFriendlyLookupItem();
                e.Handled = true;
                return;
            }

            vm2.DuplicateSelectedFriendlyEntry();
            e.Handled = true;
        }
        private void ShowFriendlySearchWindow(Action<string, bool> findNext)
        {
            if (_friendlySearchWindow is null)
            {
                _friendlySearchWindow = new Views.FriendlySearchWindow
                {
                    DataContext = new ViewModels.FriendlySearchWindowViewModel(findNext)
                };

                _friendlySearchWindow.Closed += (_, __) => _friendlySearchWindow = null;
            }

            if (!_friendlySearchWindow.IsVisible)
                _friendlySearchWindow.Show();

            if (_friendlySearchWindow.WindowState == WindowState.Minimized)
                _friendlySearchWindow.WindowState = WindowState.Normal;

            _friendlySearchWindow.Activate();
            _friendlySearchWindow.FocusQuery();
        }

        private void FocusRawSearch()
        {
            if (_searchPanel is null)
                return;

            _searchPanel.Dispatcher.BeginInvoke(
                System.Windows.Threading.DispatcherPriority.Input,
                new Action(() =>
                {
                    _searchPanel.Focus();

                    System.Windows.Controls.TextBox? firstTextBox = null;

                    foreach (var tb in FindVisualChildren<System.Windows.Controls.TextBox>(_searchPanel))
                    {
                        if (!tb.IsVisible)
                            continue;

                        if (!tb.IsEnabled)
                            continue;

                        if (!tb.Focusable)
                            continue;

                        firstTextBox = tb;
                        break;
                    }

                    if (firstTextBox is null)
                        return;

                    firstTextBox.Focus();
                    firstTextBox.SelectAll();
                }));
        }

        private void FriendlyGroupsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is not System.Windows.Controls.ListBox lb)
                return;

            if (lb.SelectedItem is null)
                return;

            lb.Dispatcher.BeginInvoke(() =>
            {
                lb.ScrollIntoView(lb.SelectedItem);
            });
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

        private async Task AutoCheckForUpdatesOnStartupAsync()
        {
            if (_checkedUpdatesOnStartup)
                return;

            _checkedUpdatesOnStartup = true;

            var assembly = Assembly.GetExecutingAssembly();
            var currentVersion = assembly.GetName().Version;
            if (currentVersion is null)
                return;

            try
            {
                var service = new GitHubReleaseService(new HttpClient());
                var latest = await service.GetLatestReleaseAsync("YoshiMitsu93", "LSR.XmlHelper.Wpf");

                if (latest is null)
                    return;

                if (latest.Version is null)
                    return;

                if (latest.Version <= currentVersion)
                    return;

                if (string.IsNullOrWhiteSpace(latest.HtmlUrl))
                    return;

                var result = System.Windows.MessageBox.Show(
                    $"A new update is available.\n\nCurrent: {currentVersion}\nLatest: {latest.Version}\n\nDo you want to open the download page now?",
                    "Update available",
                    System.Windows.MessageBoxButton.YesNo,
                    System.Windows.MessageBoxImage.Information);

                if (result != System.Windows.MessageBoxResult.Yes)
                    return;

                Process.Start(new ProcessStartInfo(latest.HtmlUrl) { UseShellExecute = true });
            }
            catch
            {
                return;
            }
        }

        private void OpenSettingsInfo_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is not LSR.XmlHelper.Wpf.ViewModels.MainWindowViewModel mainVm)
                return;

            if (_settingsInfoWindow is null)
            {
                var settingsService = new LSR.XmlHelper.Wpf.Services.AppSettingsService();
                var vm = new LSR.XmlHelper.Wpf.ViewModels.Windows.SettingsInfoWindowViewModel(mainVm, settingsService, mainVm.Appearance);

                _settingsInfoWindow = new LSR.XmlHelper.Wpf.Views.SettingsInfoWindow
                {
                    Owner = System.Windows.Application.Current?.MainWindow,
                    ShowInTaskbar = true,
                    DataContext = vm
                };

                _settingsInfoWindow.Closed += (_, _) => _settingsInfoWindow = null;
            }

            if (!_settingsInfoWindow.IsVisible)
                _settingsInfoWindow.Show();

            if (_settingsInfoWindow.WindowState == WindowState.Minimized)
                _settingsInfoWindow.WindowState = WindowState.Normal;

            _settingsInfoWindow.Activate();
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is not MainWindowViewModel mainVm)
                return;

            if (_helpDocumentationWindow is null)
            {
                var vm = new LSR.XmlHelper.Wpf.ViewModels.Windows.HelpDocumentationWindowViewModel(mainVm.Appearance);
                _helpDocumentationWindow = new LSR.XmlHelper.Wpf.Views.HelpDocumentationWindow
                {
                    Owner = this,
                    ShowInTaskbar = true,
                    DataContext = vm
                };


                _helpDocumentationWindow.Closed += (_, _) => _helpDocumentationWindow = null;
            }

            if (!_helpDocumentationWindow.IsVisible)
                _helpDocumentationWindow.Show();

            if (_helpDocumentationWindow.WindowState == WindowState.Minimized)
                _helpDocumentationWindow.WindowState = WindowState.Normal;

            _helpDocumentationWindow.Activate();
        }
        private void XmlGuides_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is not MainWindowViewModel mainVm)
                return;

            var xmlPath = GetCurrentXmlPath();
            if (string.IsNullOrWhiteSpace(xmlPath) || !File.Exists(xmlPath))
            {
                if (DataContext is MainWindowViewModel vm && !string.IsNullOrWhiteSpace(vm.RootFolderPath) && Directory.Exists(vm.RootFolderPath))
                    xmlPath = Path.Combine(vm.RootFolderPath, "__folder__.xml");
                else
                {
                    System.Windows.MessageBox.Show("No folder is currently open.", "Open guides", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
            }

            var root = _helperRoot.GetHelperRootForXmlPath(xmlPath);
            if (!Directory.Exists(root))
                Directory.CreateDirectory(root);

            if (_xmlGuidesWindow is null)
            {
                var vm = new LSR.XmlHelper.Wpf.ViewModels.Windows.XmlGuidesWindowViewModel(mainVm.Appearance, root);
                _xmlGuidesWindow = new LSR.XmlHelper.Wpf.Views.XmlGuidesWindow
                {
                    Owner = this,
                    DataContext = vm
                };

                _xmlGuidesWindow.Closed += (_, _) => _xmlGuidesWindow = null;
            }

            if (!_xmlGuidesWindow.IsVisible)
                _xmlGuidesWindow.Show();

            if (_xmlGuidesWindow.WindowState == WindowState.Minimized)
                _xmlGuidesWindow.WindowState = WindowState.Normal;

            _xmlGuidesWindow.Activate();
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
            if (DataContext is ViewModels.MainWindowViewModel vm && vm.IsFriendlyView)
            {
                ShowFriendlySearchWindow(vm.FindNextFriendly);
                return;
            }

            _searchPanel?.Open();
        }

        private void Replace_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (DataContext is MainWindowViewModel vm && vm.IsFriendlyView)
                return;

            var editor = FindName("XmlEditor") as TextEditor;
            if (editor is null)
                return;

            if (_replaceWindow is null)
            {
                _replaceWindow = new Views.ReplaceWindow(editor);
                _replaceWindow.Owner = this;
                _replaceWindow.Closed += (_, __) => _replaceWindow = null;
                _replaceWindow.Show();
                _replaceWindow.Activate();
                return;
            }

            _replaceWindow.Activate();
        }

        private void Replace_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (DataContext is MainWindowViewModel vm)
                e.CanExecute = vm.IsFriendlyView == false;
            else
                e.CanExecute = true;

            e.Handled = true;
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
