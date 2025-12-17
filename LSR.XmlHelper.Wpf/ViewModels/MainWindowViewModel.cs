using System.Windows;
using System.Windows.Input;
using LSR.XmlHelper.Wpf.Infrastructure;

namespace LSR.XmlHelper.Wpf.ViewModels
{
    public sealed class MainWindowViewModel
    {
        public string Title => "LSR XML Helper";

        public ICommand TestCommand { get; }

        public MainWindowViewModel()
        {
            TestCommand = new RelayCommand(() => MessageBox.Show("RelayCommand works!"));
        }
    }
}
