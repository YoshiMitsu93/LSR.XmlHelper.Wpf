using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Search;
using LSR.XmlHelper.Wpf.ViewModels;
using System.Windows;

namespace LSR.XmlHelper.Wpf
{
    public partial class MainWindow : Window
    {
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
    }
}
