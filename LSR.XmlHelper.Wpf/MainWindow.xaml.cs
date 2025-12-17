using System.Windows;
using LSR.XmlHelper.Wpf.ViewModels;

namespace LSR.XmlHelper.Wpf
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
