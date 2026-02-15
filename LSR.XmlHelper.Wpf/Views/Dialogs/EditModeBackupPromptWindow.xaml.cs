using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace LSR.XmlHelper.Wpf.Views.Dialogs
{
    public partial class EditModeBackupPromptWindow : Window
    {
        public EditModeBackupPromptAction Action { get; private set; } = EditModeBackupPromptAction.Cancel;

        public EditModeBackupPromptWindow(IEnumerable<string> filesToEdit)
        {
            InitializeComponent();
            FilesList.ItemsSource = (filesToEdit ?? Enumerable.Empty<string>()).ToList();
        }

        private void Backup_Click(object sender, RoutedEventArgs e)
        {
            Action = EditModeBackupPromptAction.Backup;
            DialogResult = true;
        }

        private void ProceedWithoutBackup_Click(object sender, RoutedEventArgs e)
        {
            Action = EditModeBackupPromptAction.ProceedWithoutBackup;
            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Action = EditModeBackupPromptAction.Cancel;
            DialogResult = false;
        }
    }

    public enum EditModeBackupPromptAction
    {
        Backup,
        ProceedWithoutBackup,
        Cancel
    }
}
