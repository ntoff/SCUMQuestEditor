using System.Configuration;
using System.Data;
using System.Linq;
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
            base.OnStartup(e);

            var mainWindow = new MainWindow();

            if (e.Args != null && e.Args.Length > 0)
            {
                string? firstFile = e.Args.FirstOrDefault(f => f.ToLower().EndsWith(".json"));
                if (firstFile != null && System.IO.File.Exists(firstFile))
                {
                    mainWindow.LoadFileFromPath(firstFile);
                }
            }

            mainWindow.Show();
        }
    }

}
