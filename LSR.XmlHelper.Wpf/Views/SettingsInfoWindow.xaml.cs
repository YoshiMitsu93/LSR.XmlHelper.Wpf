using LSR.XmlHelper.Wpf.ViewModels;
using LSR.XmlHelper.Wpf.ViewModels.Windows;
using System.Windows;

namespace LSR.XmlHelper.Wpf.Views
{
    public partial class SettingsInfoWindow : Window
    {
        public SettingsInfoWindow()
        {
            InitializeComponent();
        }

        private void Folders_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is SettingsInfoWindowViewModel vm)
                vm.ViewMode = XmlListViewMode.Folders;
        }

        private void Flat_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is SettingsInfoWindowViewModel vm)
                vm.ViewMode = XmlListViewMode.Flat;
        }
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
