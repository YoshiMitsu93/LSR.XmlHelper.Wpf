using LSR.XmlHelper.Wpf.ViewModels;
using System.Windows;
using System.Windows.Input;

namespace LSR.XmlHelper.Wpf.Views
{
    public partial class GlobalSearchWindow : Window
    {
        public GlobalSearchWindow()
        {
            InitializeComponent();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Results_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is not GlobalSearchWindowViewModel vm)
                return;

            vm.OpenSelectedCommand.Execute(null);
        }
    }
}
