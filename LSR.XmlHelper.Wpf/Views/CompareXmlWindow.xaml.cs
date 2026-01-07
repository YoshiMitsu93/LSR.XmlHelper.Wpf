using System.Windows;

namespace LSR.XmlHelper.Wpf.Views
{
    public partial class CompareXmlWindow : Window
    {
        public CompareXmlWindow()
        {
            InitializeComponent();
        }

        private void CompareWindow_PreviewDragOver(object sender, System.Windows.DragEventArgs e)
        {
            if (e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop))
            {
                e.Effects = System.Windows.DragDropEffects.Copy;
                e.Handled = true;
                return;
            }

            e.Effects = System.Windows.DragDropEffects.None;
            e.Handled = true;
        }

        private void CompareWindow_Drop(object sender, System.Windows.DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop))
                return;

            var files = e.Data.GetData(System.Windows.DataFormats.FileDrop) as string[];
            if (files is null || files.Length == 0)
                return;

            string? xml = null;
            foreach (var f in files)
            {
                if (f is null)
                    continue;

                if (f.EndsWith(".xml", System.StringComparison.OrdinalIgnoreCase))
                {
                    xml = f;
                    break;
                }
            }

            if (string.IsNullOrWhiteSpace(xml))
                return;

            if (DataContext is ViewModels.Windows.CompareXmlWindowViewModel vm)
                vm.SetExternalXmlPath(xml);
        }
    }
}
