using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Search;
using LSR.XmlHelper.Core.Services;
using LSR.XmlHelper.Wpf.Infrastructure;
using LSR.XmlHelper.Wpf.Infrastructure.Behaviors;
using LSR.XmlHelper.Wpf.Infrastructure.Commands;
using LSR.XmlHelper.Wpf.Services.Appearance;
using LSR.XmlHelper.Wpf.Services.RawXml;
using LSR.XmlHelper.Wpf.Services.Updates;
using LSR.XmlHelper.Wpf.Services.UndoRedo;
using LSR.XmlHelper.Wpf.ViewModels;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace LSR.XmlHelper.Wpf
{
    public partial class MainWindow : Window
    {
        private Views.SettingsInfoWindow? _settingsInfoWindow;
        private Views.HelpDocumentationWindow? _helpDocumentationWindow;
        private Views.XmlGuidesWindow? _xmlGuidesWindow;
        private readonly XmlHelperRootService _helperRoot = new XmlHelperRootService();
        private SearchPanel? _searchPanel;
        private AvalonEditTextMarkerService? _rawXmlMarkerService;
        private AvalonEditScopeShadingService? _rawXmlScopeShadingService;
        private AvalonEditIndentGuidesService? _rawXmlIndentGuidesService;
        private AvalonEditBackgroundRendererGuard? _rawXmlRendererGuard;
        private AvalonEditXmlFoldingService? _rawXmlFoldingService;
        private TextEditor? _xmlEditor;
        private AvalonEditCaretKeepAliveService? _rawXmlCaretKeepAliveService;
        private readonly RawXmlContextActionService _rawXmlContextActionService = new RawXmlContextActionService();
        private System.Windows.Threading.DispatcherTimer? _breadcrumbTimer;
        private int _breadcrumbPendingOffset;
        private bool _breadcrumbPending;
        private Views.FriendlySearchWindow? _friendlySearchWindow;
        private Views.ReplaceWindow? _replaceWindow;
        private bool _checkedUpdatesOnStartup;
        private readonly FriendlyUndoRedoService _friendlyUndoRedo = new FriendlyUndoRedoService();
        private string _lastFriendlyXmlText = "";
        private bool _suppressFriendlyUndoCapture;
        public MainWindow()
        {
            InitializeComponent();

            var editor = FindName("XmlEditor") as TextEditor;
            if (editor is not null)
            {
                _rawXmlCaretKeepAliveService = new AvalonEditCaretKeepAliveService(editor);
                _rawXmlCaretKeepAliveService.Start();
                _xmlEditor = editor;
                editor.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("XML");
                _searchPanel = SearchPanel.Install(editor);
                var markerService = new AvalonEditTextMarkerService(editor.Document);
                _rawXmlMarkerService = markerService;
                editor.TextArea.TextView.BackgroundRenderers.Add(markerService);
                var scopeShadingService = new AvalonEditScopeShadingService(editor.Document);
                _rawXmlScopeShadingService = scopeShadingService;
                editor.TextArea.TextView.BackgroundRenderers.Add(scopeShadingService);
                var indentGuidesService = new AvalonEditIndentGuidesService(editor.Document, scopeShadingService);
                _rawXmlIndentGuidesService = indentGuidesService;
                editor.TextArea.TextView.BackgroundRenderers.Add(indentGuidesService);
                _rawXmlRendererGuard = new AvalonEditBackgroundRendererGuard(editor, markerService, scopeShadingService, indentGuidesService);
                _rawXmlFoldingService = new AvalonEditXmlFoldingService(editor);
                _rawXmlFoldingService.OutlineChanged += (_, __) => RefreshRawOutline();
                editor.TextArea.Caret.PositionChanged += (_, __) =>
                {
                    if (_rawXmlScopeShadingService is not null)
                        _rawXmlScopeShadingService.CaretLine = editor.TextArea.Caret.Line;

                    if (_rawXmlIndentGuidesService is not null)
                    {
                        _rawXmlIndentGuidesService.CaretLine = editor.TextArea.Caret.Position.Line;
                        _rawXmlIndentGuidesService.CaretVisualColumn = editor.TextArea.Caret.Position.VisualColumn;
                        ScheduleBreadcrumbUpdate();
                    }
                };

                ScheduleBreadcrumbUpdate();
                editor.TextChanged += (_, __) => ScheduleBreadcrumbUpdate();

                if (_rawXmlScopeShadingService is not null)
                    _rawXmlScopeShadingService.CaretLine = editor.TextArea.Caret.Line;

                if (_rawXmlIndentGuidesService is not null)
                {
                    _rawXmlIndentGuidesService.CaretLine = editor.TextArea.Caret.Position.Line;
                    _rawXmlIndentGuidesService.CaretVisualColumn = editor.TextArea.Caret.Position.VisualColumn;
                }
            }

            DataContext = new MainWindowViewModel();
            if (DataContext is MainWindowViewModel vm)
            {
                vm.PropertyChanged += Vm_PropertyChanged;
                vm.RawXmlProblemsChanged += VmOnRawXmlProblemsChanged;
                _lastFriendlyXmlText = vm.XmlText ?? "";
                _rawXmlFoldingService?.SetDocumentKey(vm.SelectedXmlFile?.FullPath);
                InitializeBreadcrumbTimer();
                ScheduleBreadcrumbUpdate();
                if (editor is not null)
                {
                    XmlSyntaxHighlightingService.Apply(editor, vm.Appearance.EditorXmlSyntaxForeground);

                    if (_rawXmlScopeShadingService is not null)
                        _rawXmlScopeShadingService.ScopeShadingColor = vm.Appearance.EditorScopeShadingColor;
                    if (_rawXmlScopeShadingService is not null)
                        _rawXmlScopeShadingService.RegionHighlightColor = vm.Appearance.EditorRegionHighlightColor;
                    if (_rawXmlScopeShadingService is not null)
                        _rawXmlScopeShadingService.IsScopeShadingEnabled = vm.IsScopeShadingEnabled;
                    if (_rawXmlScopeShadingService is not null)
                        _rawXmlScopeShadingService.IsRegionHighlightEnabled = vm.IsRegionHighlightEnabled;
                    if (_rawXmlIndentGuidesService is not null)
                        _rawXmlIndentGuidesService.GuidesColor =
                            string.IsNullOrWhiteSpace(vm.Appearance.EditorIndentGuidesColor)
                                ? vm.Appearance.EditorScopeShadingColor
                                : vm.Appearance.EditorIndentGuidesColor;
                    if (_rawXmlIndentGuidesService is not null)
                        _rawXmlIndentGuidesService.IsIndentGuidesEnabled = vm.IsIndentGuidesEnabled;
                    if (_rawXmlIndentGuidesService is not null)
                        _rawXmlIndentGuidesService.IsDarkMode = vm.IsDarkMode;
                    if (_rawXmlIndentGuidesService is not null)
                        _rawXmlIndentGuidesService.IsEnabled = !vm.IsFriendlyView;

                    vm.Appearance.PropertyChanged += (_, args) =>
                    {
                        if (args.PropertyName == nameof(Services.AppearanceService.EditorXmlSyntaxForeground))
                            XmlSyntaxHighlightingService.Apply(editor, vm.Appearance.EditorXmlSyntaxForeground);

                        if (args.PropertyName == nameof(Services.AppearanceService.EditorScopeShadingColor) && _rawXmlScopeShadingService is not null)
                            _rawXmlScopeShadingService.ScopeShadingColor = vm.Appearance.EditorScopeShadingColor;
                        if ((args.PropertyName == nameof(Services.AppearanceService.EditorIndentGuidesColor) || args.PropertyName == nameof(Services.AppearanceService.EditorScopeShadingColor)) && _rawXmlIndentGuidesService is not null)
                            _rawXmlIndentGuidesService.GuidesColor =
                                string.IsNullOrWhiteSpace(vm.Appearance.EditorIndentGuidesColor)
                                    ? vm.Appearance.EditorScopeShadingColor
                                    : vm.Appearance.EditorIndentGuidesColor;
                        if (args.PropertyName == nameof(Services.AppearanceService.EditorRegionHighlightColor) && _rawXmlScopeShadingService is not null)
                            _rawXmlScopeShadingService.RegionHighlightColor = vm.Appearance.EditorRegionHighlightColor;
                    };
                }
            }
            Loaded += MainWindow_Loaded;
            Closed += (_, __) => { _rawXmlCaretKeepAliveService?.Dispose(); _rawXmlRendererGuard?.Dispose(); _rawXmlFoldingService?.Dispose(); };
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
            if (DataContext is not MainWindowViewModel vm)
                return;

            if (_rawXmlScopeShadingService is not null)
            {
                if (e.PropertyName == nameof(MainWindowViewModel.IsDarkMode))
                    _rawXmlScopeShadingService.IsDarkMode = vm.IsDarkMode;

                if (e.PropertyName == nameof(MainWindowViewModel.IsFriendlyView))
                    _rawXmlScopeShadingService.IsEnabled = !vm.IsFriendlyView;

                if (e.PropertyName == nameof(MainWindowViewModel.IsScopeShadingEnabled))
                    _rawXmlScopeShadingService.IsScopeShadingEnabled = vm.IsScopeShadingEnabled;

                if (e.PropertyName == nameof(MainWindowViewModel.IsRegionHighlightEnabled))
                    _rawXmlScopeShadingService.IsRegionHighlightEnabled = vm.IsRegionHighlightEnabled;
            }

            if (_rawXmlIndentGuidesService is not null)
            {
                if (e.PropertyName == nameof(MainWindowViewModel.IsDarkMode))
                    _rawXmlIndentGuidesService.IsDarkMode = vm.IsDarkMode;

                if (e.PropertyName == nameof(MainWindowViewModel.IsFriendlyView))
                    _rawXmlIndentGuidesService.IsEnabled = !vm.IsFriendlyView;

                if (e.PropertyName == nameof(MainWindowViewModel.IsIndentGuidesEnabled))
                    _rawXmlIndentGuidesService.IsIndentGuidesEnabled = vm.IsIndentGuidesEnabled;
            }

            if (e.PropertyName == nameof(MainWindowViewModel.IsFriendlyView))
            {
                if (vm.IsFriendlyView)
                {
                    if (_replaceWindow is not null)
                    {
                        var w = _replaceWindow;
                        _replaceWindow = null;
                        w.Close();
                    }
                }
            }

            if (e.PropertyName == nameof(MainWindowViewModel.SelectedXmlFile))
            {
                _rawXmlFoldingService?.SetDocumentKey(vm.SelectedXmlFile?.FullPath);
                ScheduleBreadcrumbUpdate();
            }

            if (e.PropertyName == nameof(MainWindowViewModel.XmlText))
            {
                if (vm.IsFriendlyView && !_suppressFriendlyUndoCapture)
                {
                    var current = vm.XmlText ?? "";
                    if (!string.Equals(current, _lastFriendlyXmlText, StringComparison.Ordinal))
                        _friendlyUndoRedo.Record(_lastFriendlyXmlText);

                    _lastFriendlyXmlText = current;
                }
                else
                {
                    _lastFriendlyXmlText = vm.XmlText ?? "";
                }
            }

            if (e.PropertyName == nameof(MainWindowViewModel.IsFriendlyView))
            {
                _lastFriendlyXmlText = vm.XmlText ?? "";
                _friendlyUndoRedo.Clear();
            }

            var editor = FindName("XmlEditor") as TextEditor;
            editor?.TextArea.TextView.InvalidateVisual();
        }

        private void EditorUndoRedo_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {

            if (DataContext is not MainWindowViewModel vm)
            {
                e.CanExecute = false;
                return;
            }

            if (vm.IsFriendlyView)
            {
                if (e.Command == EditorUndoRedoCommands.Undo)
                    e.CanExecute = _friendlyUndoRedo.CanUndo;
                else if (e.Command == EditorUndoRedoCommands.Redo)
                    e.CanExecute = _friendlyUndoRedo.CanRedo;
                else
                    e.CanExecute = false;

                return;
            }

            if (XmlEditor is null)
            {
                e.CanExecute = false;
                return;
            }

            if (e.Command == EditorUndoRedoCommands.Undo)
                e.CanExecute = XmlEditor.CanUndo;
            else if (e.Command == EditorUndoRedoCommands.Redo)
                e.CanExecute = XmlEditor.CanRedo;
            else
                e.CanExecute = false;
        }

        private void EditorUndoRedo_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (DataContext is not MainWindowViewModel vm)
                return;

            if (vm.IsFriendlyView)
            {
                var current = vm.XmlText ?? "";

                if (e.Command == EditorUndoRedoCommands.Undo)
                {
                    if (_friendlyUndoRedo.TryUndo(current, out var previous))
                    {
                        _suppressFriendlyUndoCapture = true;
                        vm.XmlText = previous;
                        _lastFriendlyXmlText = previous;
                        _suppressFriendlyUndoCapture = false;
                    }

                    return;
                }

                if (e.Command == EditorUndoRedoCommands.Redo)
                {
                    if (_friendlyUndoRedo.TryRedo(current, out var next))
                    {
                        _suppressFriendlyUndoCapture = true;
                        vm.XmlText = next;
                        _lastFriendlyXmlText = next;
                        _suppressFriendlyUndoCapture = false;
                    }
                }

                return;
            }

            if (XmlEditor is null)
                return;

            if (e.Command == EditorUndoRedoCommands.Undo)
            {
                XmlEditor.Undo();
                return;
            }

            if (e.Command == EditorUndoRedoCommands.Redo)
                XmlEditor.Redo();
        }

        private void InitializeBreadcrumbTimer()
        {
            if (_breadcrumbTimer is not null)
                return;

            _breadcrumbTimer = new System.Windows.Threading.DispatcherTimer(System.Windows.Threading.DispatcherPriority.Input)
            {
                Interval = TimeSpan.FromMilliseconds(40),
            };

            _breadcrumbTimer.Tick += BreadcrumbTimerOnTick;
        }

        private void ScheduleBreadcrumbUpdate()
        {
            if (_xmlEditor is null)
                return;

            if (DataContext is not MainWindowViewModel)
                return;

            if (_breadcrumbTimer is null)
                return;

            _breadcrumbPendingOffset = _xmlEditor.TextArea.Caret.Offset;
            _breadcrumbPending = true;

            _breadcrumbTimer.Stop();
            _breadcrumbTimer.Start();
        }
      
        private void BreadcrumbTimerOnTick(object? sender, EventArgs e)
        {
            if (_breadcrumbTimer is null)
                return;

            _breadcrumbTimer.Stop();

            if (!_breadcrumbPending)
                return;

            _breadcrumbPending = false;

            if (_xmlEditor is null)
                return;

            if (DataContext is not MainWindowViewModel vm)
                return;

            var doc = _xmlEditor.Document;
            if (doc is null)
                return;

            vm.RawBreadcrumbSegments.Clear();

            if (_rawXmlFoldingService is null)
            {
                var fallback = Infrastructure.Breadcrumbs.XmlBreadcrumbBuilder.Build(doc, _breadcrumbPendingOffset);
                foreach (var s in fallback)
                    vm.RawBreadcrumbSegments.Add(s);
                return;
            }

            var segments = _rawXmlFoldingService.GetBreadcrumbSegments(_breadcrumbPendingOffset);

            foreach (var s in segments)
                vm.RawBreadcrumbSegments.Add(new BreadcrumbSegmentViewModel { Title = s.Title, Offset = s.Offset });
        }

        private void RawBreadcrumbButton_Click(object sender, RoutedEventArgs e)
        {
            if (_xmlEditor is null)
                return;

            if (sender is not System.Windows.Controls.Button b)
                return;

            if (b.Tag is not int offset)
                return;

            if (offset < 0 || offset > _xmlEditor.Document.TextLength)
                return;

            _xmlEditor.TextArea.Caret.Offset = offset;
            var line = _xmlEditor.Document.GetLocation(offset).Line;
            _xmlEditor.ScrollToLine(line);
            _xmlEditor.TextArea.Focus();
        }
        private void RefreshRawOutline()
        {
            if (_rawXmlFoldingService is null)
                return;

            if (DataContext is not MainWindowViewModel vm)
                return;

            var roots = _rawXmlFoldingService.GetOutlineRoots();

            vm.RawOutlineNodes.Clear();

            foreach (var r in roots)
                vm.RawOutlineNodes.Add(BuildOutlineNode(r));

            Dispatcher.BeginInvoke(() =>
            {
                ExpandRawOutlineRootItems();
            }, System.Windows.Threading.DispatcherPriority.Loaded);
        }

        private void ExpandRawOutlineRootItems()
        {
            if (DataContext is not MainWindowViewModel vm)
                return;

            RawOutlineTreeView.UpdateLayout();

            for (var i = 0; i < vm.RawOutlineNodes.Count; i++)
            {
                var root = vm.RawOutlineNodes[i];
                var container = RawOutlineTreeView.ItemContainerGenerator.ContainerFromItem(root) as TreeViewItem;
                if (container is null)
                    continue;

                container.IsExpanded = true;
            }
        }

        private static ScrollViewer? FindDescendantScrollViewer(DependencyObject root)
        {
            if (root is ScrollViewer sv)
                return sv;

            var count = VisualTreeHelper.GetChildrenCount(root);
            for (var i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(root, i);
                var result = FindDescendantScrollViewer(child);
                if (result is not null)
                    return result;
            }

            return null;
        }
        private RawXmlOutlineNodeViewModel BuildOutlineNode(LSR.XmlHelper.Wpf.Infrastructure.Outline.RawXmlOutlineEntry entry)
        {
            var node = new RawXmlOutlineNodeViewModel
            {
                Title = entry.Title,
                Offset = entry.Offset
            };

            foreach (var c in entry.Children)
                node.Children.Add(BuildOutlineNode(c));

            return node;
        }
        private void RawOutlineTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is not RawXmlOutlineNodeViewModel node)
                return;

            NavigateToRawOutlineNode(node, preserveTreeScroll: true);
        }

        private void RawOutlineTreeView_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is not DependencyObject dep)
                return;

            if (FindAncestor<System.Windows.Controls.Primitives.ToggleButton>(dep) is not null)
                return;

            if (FindAncestor<System.Windows.Controls.Primitives.ScrollBar>(dep) is not null)
                return;

            var item = FindAncestor<TreeViewItem>(dep);
            if (item is null)
                return;

            if (item.DataContext is not RawXmlOutlineNodeViewModel node)
                return;

            NavigateToRawOutlineNode(node, preserveTreeScroll: true);
        }
        private void NavigateToRawOutlineNode(RawXmlOutlineNodeViewModel node, bool preserveTreeScroll)
        {
            if (_xmlEditor is null)
                return;

            var doc = _xmlEditor.Document;
            if (doc is null)
                return;

            var offset = node.Offset;
            if (offset < 0 || offset > doc.TextLength)
                return;

            ScrollViewer? scrollViewer = null;
            var scrollOffset = 0.0;

            if (preserveTreeScroll)
            {
                scrollViewer = FindDescendantScrollViewer(RawOutlineTreeView);
                scrollOffset = scrollViewer?.VerticalOffset ?? 0;
            }

            _xmlEditor.TextArea.Caret.Offset = offset;

            var line = doc.GetLocation(offset).Line;
            _xmlEditor.ScrollToLine(line);
            _xmlEditor.TextArea.Focus();

            Dispatcher.BeginInvoke(() =>
            {
                if (scrollViewer is not null)
                    scrollViewer.ScrollToVerticalOffset(scrollOffset);
            }, System.Windows.Threading.DispatcherPriority.Background);
        }

        private void RawOutlineDuplicateEntry_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not MenuItem mi)
                return;

            if (mi.DataContext is not RawXmlOutlineNodeViewModel node)
                return;

            if (XmlEditor is null)
                return;

            XmlEditor.CaretOffset = node.Offset;

            RawDuplicateEntry_Click(sender, e);
        }

        private void VmOnRawXmlProblemsChanged(object? sender, EventArgs e)
        {
            var editor = XmlEditor;
            if (editor is null)
                return;

            if (_rawXmlMarkerService is null)
                return;

            if (DataContext is not MainWindowViewModel vm)
                return;

            _rawXmlMarkerService.Clear();

            foreach (var p in vm.RawXmlProblems)
            {
                var offset = p.Offset;
                if (offset < 0)
                    continue;

                if (offset >= editor.Document.TextLength)
                    offset = Math.Max(0, editor.Document.TextLength - 1);

                var length = GetUnderlineLength(editor.Document, offset);
                _rawXmlMarkerService.AddSquiggle(offset, length, p.DisplayText, p.IsWarning);
            }

            if (_rawXmlScopeShadingService is not null)
            {
                _rawXmlScopeShadingService.IsDarkMode = vm.IsDarkMode;
                _rawXmlScopeShadingService.IsEnabled = !vm.IsFriendlyView;
            }

            if (_rawXmlIndentGuidesService is not null)
            {
                _rawXmlIndentGuidesService.IsDarkMode = vm.IsDarkMode;
                _rawXmlIndentGuidesService.IsEnabled = !vm.IsFriendlyView;
            }

            editor.TextArea.TextView.InvalidateVisual();
        }

        private void RawXmlProblemsList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is not MainWindowViewModel vm)
                return;

            var p = vm.SelectedRawXmlProblem;
            if (p is null)
                return;

            var editor = XmlEditor;
            if (editor is null)
                return;

            var offset = p.Offset;
            if (offset < 0)
                return;

            if (offset >= editor.Document.TextLength)
                offset = Math.Max(0, editor.Document.TextLength - 1);

            var length = GetUnderlineLength(editor.Document, offset);
            editor.Select(offset, length);
            editor.TextArea.Caret.Offset = offset;
            editor.TextArea.Caret.BringCaretToView();
            editor.Focus();
        }

        private static int GetUnderlineLength(TextDocument doc, int offset)
        {
            if (offset < 0)
                return 1;

            if (offset >= doc.TextLength)
                return 1;

            var line = doc.GetLineByOffset(offset);
            var max = Math.Min(line.EndOffset, offset + 60);
            var text = doc.GetText(offset, max - offset);
            for (var i = 0; i < text.Length; i++)
            {
                var c = text[i];
                if (char.IsWhiteSpace(c) || c == '>' || c == '<' || c == '\r' || c == '\n')
                    return Math.Max(1, i);
            }
            return Math.Max(1, text.Length);
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
        private void RawDuplicateLine_Click(object sender, RoutedEventArgs e)
        {
            if (XmlEditor is null)
                return;

            var doc = XmlEditor.Document;
            if (doc is null)
                return;

            var caret = Math.Max(0, Math.Min(doc.TextLength, XmlEditor.CaretOffset));
            var line = doc.GetLineByOffset(caret);
            var lineText = doc.GetText(line);

            var newline = doc.Text.Contains("\r\n", StringComparison.Ordinal) ? "\r\n" : "\n";
            var insertOffset = line.EndOffset;

            doc.Insert(insertOffset, newline + lineText);
            XmlEditor.CaretOffset = Math.Max(0, Math.Min(doc.TextLength, insertOffset + newline.Length));

            if (DataContext is MainWindowViewModel vm)
                vm.Status = "Duplicated line.";
        }

        private void RawDuplicateEntry_Click(object sender, RoutedEventArgs e)
        {
            if (XmlEditor is null || _rawXmlFoldingService is null)
                return;

            var doc = XmlEditor.Document;
            if (doc is null)
                return;

            if (!_rawXmlFoldingService.TryGetEntrySpan(XmlEditor.CaretOffset, out var start, out var end))
                return;

            if (!_rawXmlContextActionService.TryDuplicateSpan(doc.Text, start, end, out var updated, out var newCaret))
                return;

            doc.UndoStack.StartUndoGroup();
            doc.Replace(0, doc.TextLength, updated);
            doc.UndoStack.EndUndoGroup();
            XmlEditor.CaretOffset = Math.Max(0, Math.Min(doc.TextLength, newCaret));

            if (DataContext is MainWindowViewModel vm)
                vm.Status = "Duplicated entry.";
        }
        private void RawDeleteEntry_Click(object sender, RoutedEventArgs e)
        {
            if (XmlEditor is null || _rawXmlFoldingService is null)
                return;

            var doc = XmlEditor.Document;
            if (doc is null)
                return;

            if (!_rawXmlFoldingService.TryGetEntrySpan(XmlEditor.CaretOffset, out var start, out var end))
                return;

            if (!_rawXmlContextActionService.TryDeleteSpan(doc.Text, start, end, out var updated, out var newCaret))
                return;

            doc.UndoStack.StartUndoGroup();
            doc.Replace(0, doc.TextLength, updated);
            doc.UndoStack.EndUndoGroup();
            XmlEditor.CaretOffset = Math.Max(0, Math.Min(doc.TextLength, newCaret));

            if (DataContext is MainWindowViewModel vm)
                vm.Status = "Deleted entry.";
        }

        private void RawCollapseElement_Click(object sender, RoutedEventArgs e)
        {
            if (XmlEditor is null || _rawXmlFoldingService is null)
                return;

            _rawXmlFoldingService.CollapseContainingElement(XmlEditor.CaretOffset);
        }

        private void RawExpandElement_Click(object sender, RoutedEventArgs e)
        {
            if (XmlEditor is null || _rawXmlFoldingService is null)
                return;

            _rawXmlFoldingService.ExpandContainingElement(XmlEditor.CaretOffset);
        }

        private void RawCollapseAll_Click(object sender, RoutedEventArgs e)
        {
            if (_rawXmlFoldingService is null)
                return;

            _rawXmlFoldingService.CollapseAll();
        }

        private void RawExpandAll_Click(object sender, RoutedEventArgs e)
        {
            if (_rawXmlFoldingService is null)
                return;

            _rawXmlFoldingService.ExpandAll();
        }
        private void RawOutlineCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (XmlEditor is null)
            {
                e.CanExecute = false;
                return;
            }

            if (_rawXmlFoldingService is null)
            {
                e.CanExecute = false;
                return;
            }

            if (e.Parameter is not RawXmlOutlineNodeViewModel node)
            {
                e.CanExecute = false;
                return;
            }

            e.CanExecute = _rawXmlFoldingService.TryGetEntrySpan(node.Offset, out _, out _);
        }

        private void RawOutlineCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (XmlEditor is null)
                return;

            if (e.Parameter is not RawXmlOutlineNodeViewModel node)
                return;

            XmlEditor.CaretOffset = node.Offset;

            if (e.Command == RawOutlineCommands.DuplicateEntry)
            {
                RawDuplicateEntry_Click(sender, new RoutedEventArgs());
                return;
            }

            if (e.Command == RawOutlineCommands.DeleteEntry)
            {
                RawDeleteEntry_Click(sender, new RoutedEventArgs());
            }
        }

        private void RawXmlQuickAction_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (XmlEditor is null)
            {
                e.CanExecute = false;
                return;
            }

            if (_rawXmlFoldingService is null)
            {
                e.CanExecute = false;
                return;
            }

            var doc = XmlEditor.Document;
            if (doc is null)
            {
                e.CanExecute = false;
                return;
            }

            if (e.Command == RawXmlQuickActionsCommands.DuplicateEntry || e.Command == RawXmlQuickActionsCommands.DeleteEntry)
            {
                e.CanExecute = _rawXmlFoldingService.TryGetEntrySpan(XmlEditor.CaretOffset, out _, out _);
                return;
            }

            e.CanExecute = true;
        }

        private void RawXmlQuickAction_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Command == RawXmlQuickActionsCommands.DuplicateEntry)
            {
                RawDuplicateEntry_Click(sender, new RoutedEventArgs());
                return;
            }

            if (e.Command == RawXmlQuickActionsCommands.DeleteEntry)
            {
                RawDeleteEntry_Click(sender, new RoutedEventArgs());
                return;
            }

            if (e.Command == RawXmlQuickActionsCommands.DuplicateLine)
            {
                RawDuplicateLine_Click(sender, new RoutedEventArgs());
                return;
            }

            if (e.Command == RawXmlQuickActionsCommands.CollapseElement)
            {
                RawCollapseElement_Click(sender, new RoutedEventArgs());
                return;
            }

            if (e.Command == RawXmlQuickActionsCommands.ExpandElement)
            {
                RawExpandElement_Click(sender, new RoutedEventArgs());
                return;
            }

            if (e.Command == RawXmlQuickActionsCommands.CollapseAll)
            {
                RawCollapseAll_Click(sender, new RoutedEventArgs());
                return;
            }

            if (e.Command == RawXmlQuickActionsCommands.ExpandAll)
            {
                RawExpandAll_Click(sender, new RoutedEventArgs());
            }
        }

        private void RawFormatElement_Click(object sender, RoutedEventArgs e)
        {
            if (XmlEditor is null)
                return;

            var doc = XmlEditor.Document;
            if (doc is null)
                return;

            if (!_rawXmlContextActionService.TryFormatContainingElement(doc.Text, XmlEditor.CaretOffset, out var updated, out var newCaret))
                return;

            doc.UndoStack.StartUndoGroup();
            doc.Replace(0, doc.TextLength, updated);
            doc.UndoStack.EndUndoGroup();
            XmlEditor.CaretOffset = Math.Max(0, Math.Min(doc.TextLength, newCaret));

            if (DataContext is MainWindowViewModel vm)
                vm.Status = "Formatted element.";
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

        private void VmOnRawNavigationRequested(object? sender, EventArgs e)
        {
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
