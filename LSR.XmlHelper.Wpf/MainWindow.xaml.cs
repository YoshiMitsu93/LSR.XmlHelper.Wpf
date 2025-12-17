using LSR.XmlHelper.Wpf.ViewModels;
using System.Windows;

namespace LSR.XmlHelper.Wpf
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
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
