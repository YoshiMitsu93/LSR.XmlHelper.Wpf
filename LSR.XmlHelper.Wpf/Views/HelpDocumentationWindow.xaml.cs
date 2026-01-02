using System.Windows;

namespace LSR.XmlHelper.Wpf.Views
{
    public partial class HelpDocumentationWindow : Window
    {
        public HelpDocumentationWindow()
        {
            InitializeComponent();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
