using System.Windows;
using System.Windows.Threading;

namespace LSR.XmlHelper.Wpf.Views
{
    public partial class FriendlySearchWindow : Window
    {
        public FriendlySearchWindow()
        {
            InitializeComponent();
        }
        public void FocusQuery()
        {
            Dispatcher.BeginInvoke(
                DispatcherPriority.Input,
                new Action(() =>
                {
                    QueryTextBox.Focus();
                    QueryTextBox.SelectAll();
                }));
        }
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
