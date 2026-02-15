using System.Windows;

namespace LSR.XmlHelper.Wpf.Views.Dialogs
{
    public partial class LargeTextDialogWindow : Window
    {
        public LargeTextDialogWindow(string headerText, string bodyText)
        {
            InitializeComponent();
            HeaderText = headerText ?? "";
            BodyText = bodyText ?? "";
            DataContext = this;
        }

        public string HeaderText { get; }
        public string BodyText { get; }

        private void Copy_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(BodyText))
                System.Windows.Clipboard.SetText(BodyText);
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
