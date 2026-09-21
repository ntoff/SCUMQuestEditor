using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Diagnostics;
using System.Windows.Navigation;

namespace SCUMQuestEditor
{
    public partial class AboutDialog : Window
    {
        public AboutDialog()
        {
            InitializeComponent();
            LblVersion.Text = $"Version {BuildInfo.Version}";
            LblBuildDate.Text = $"Built on {BuildInfo.BuildDate}";
            LoadIcon();
        }

        private void LoadIcon()
        {
            string iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ICON", "icon.ico");
            if (File.Exists(iconPath))
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new System.Uri(iconPath, System.UriKind.Absolute);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze();
                ImgIcon.Source = bitmap;
            }
        }

        private void OpenGithubLink(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            e.Handled = true;
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
