using System;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace LSR.XmlHelper.Wpf.Views
{
    public partial class GuideImageViewerWindow : Window
    {
        private readonly string _imagePath;

        public GuideImageViewerWindow(string imagePath)
        {
            InitializeComponent();
            _imagePath = imagePath ?? "";

            ZoomSlider.ValueChanged += (_, _) => ApplyZoom();

            LoadImage();
            ApplyZoom();
        }

        private void LoadImage()
        {
            if (string.IsNullOrWhiteSpace(_imagePath) || !File.Exists(_imagePath))
                return;

            var bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.UriSource = new Uri(_imagePath, UriKind.Absolute);
            bmp.CacheOption = BitmapCacheOption.OnLoad;
            bmp.EndInit();

            MainImage.Source = bmp;
        }

        private void ApplyZoom()
        {
            var scale = ZoomSlider.Value / 100.0;

            if (MainImage.LayoutTransform is System.Windows.Media.ScaleTransform st)
            {
                st.ScaleX = scale;
                st.ScaleY = scale;
                return;
            }

            MainImage.LayoutTransform = new System.Windows.Media.ScaleTransform(scale, scale);
        }
    }
}
