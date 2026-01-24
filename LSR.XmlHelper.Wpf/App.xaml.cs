using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Threading;

namespace LSR.XmlHelper.Wpf
{
    public partial class App : System.Windows.Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            DispatcherUnhandledException += OnDispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += OnDomainUnhandledException;
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
