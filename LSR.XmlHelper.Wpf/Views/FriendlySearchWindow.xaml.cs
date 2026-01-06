using System.Windows;

namespace LSR.XmlHelper.Wpf.Views
{
    public partial class FriendlySearchWindow : Window
    {
        public FriendlySearchWindow()
        {
            InitializeComponent();
        }
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
