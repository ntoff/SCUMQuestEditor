using System.Windows;

namespace SCUMQuestEditor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            ShutdownMode = ShutdownMode.OnLastWindowClose;

            string? filePath = null;
            var args = Environment.GetCommandLineArgs();

            if (args.Length > 1)
            {
                filePath = args[1];
            }

            var mainWindow = !string.IsNullOrEmpty(filePath) ? new MainWindow(filePath) : new MainWindow();
            mainWindow.Show();
            this.MainWindow = mainWindow;
        }
    }

}
