using System.Windows;
using LSR.XmlHelper.Wpf.ViewModels.Builders;

namespace LSR.XmlHelper.Wpf.Views.Dialogs
{
    public partial class PreBuildValidationWindow : Window
    {
        public PreBuildValidationWindow()
        {
            InitializeComponent();
        }

        public string RequestedFocusTarget { get; private set; } = "";

        public string RequestedMessage { get; private set; } = "";

        private void GoButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not FrameworkElement fe)
                return;

            if (fe.Tag is not PreBuildValidationIssueViewModel issue)
                return;

            RequestedFocusTarget = issue.FocusTarget ?? "";
            RequestedMessage = issue.Message ?? "";
            DialogResult = true;
        }
    }
}
