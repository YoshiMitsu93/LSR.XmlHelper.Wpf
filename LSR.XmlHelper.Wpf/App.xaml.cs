using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Threading;
using LSR.XmlHelper.Wpf.Infrastructure;

namespace LSR.XmlHelper.Wpf
{
    public partial class App : System.Windows.Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            EventManager.RegisterClassHandler(typeof(Window), FrameworkElement.LoadedEvent, new RoutedEventHandler(OnAnyWindowLoaded));

            DispatcherUnhandledException += OnDispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += OnDomainUnhandledException;
        }
        private static void OnAnyWindowLoaded(object sender, RoutedEventArgs e)
        {
            if (sender is Window window)
            {
                ApplyArrowHoldScrollSettingsToWindow(window);
            }
        }

        private static void ApplyArrowHoldScrollSettingsToWindow(Window window)
        {
            var styleObj = Current.Resources[typeof(Window)];
            if (styleObj is not Style style)
            {
                return;
            }

            object? enableValue = null;
            object? speedScaleValue = null;

            foreach (var setterBase in style.Setters)
            {
                if (setterBase is not Setter setter)
                {
                    continue;
                }

                if (setter.Property == ScrollBarArrowHoldScrollBehavior.EnableProperty)
                {
                    enableValue = setter.Value;
                    continue;
                }

                if (setter.Property == ScrollBarArrowHoldScrollBehavior.SpeedScaleProperty)
                {
                    speedScaleValue = setter.Value;
                }
            }

            if (enableValue is bool enabled)
            {
                ScrollBarArrowHoldScrollBehavior.SetEnable(window, enabled);
            }

            if (speedScaleValue is double scale && scale > 0)
            {
                ScrollBarArrowHoldScrollBehavior.SetSpeedScale(window, scale);
            }
        }

        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            WriteCrashLog("DispatcherUnhandledException", e.Exception);
            e.Handled = false;
        }

        private void OnDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
                WriteCrashLog("DomainUnhandledException", ex);
            else
                WriteCrashLog("DomainUnhandledException", new Exception("Unknown exception object"));
        }

        private static void WriteCrashLog(string source, Exception ex)
        {
            try
            {
                var sb = new StringBuilder();
                sb.AppendLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                sb.AppendLine(source);
                sb.AppendLine(ex.ToString());
                sb.AppendLine(new string('-', 80));

                var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CrashLog.txt");
                File.AppendAllText(path, sb.ToString());
            }
            catch
            {
            }
        }
    }
}
