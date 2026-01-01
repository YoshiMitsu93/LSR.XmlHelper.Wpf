using System.Windows;

namespace LSR.XmlHelper.Wpf.Views
{
    public partial class EditSummaryExportOptionsWindow : Window
    {
        public EditSummaryExportOptionsWindow()
        {
            InitializeComponent();
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
