using System.Net.Http;
using System.IO;
using System.Text.Json;
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

        private async void BtnCheckUpdates_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                BtnCheckUpdates.IsEnabled = false;
                BtnCheckUpdates.Content = "Checking...";

                using var client = new HttpClient();
                client.Timeout = TimeSpan.FromSeconds(10);
                client.DefaultRequestHeaders.UserAgent.ParseAdd("SCUMQuestEditor");

                var response = await client.GetAsync("https://api.github.com/repos/ntoff/SCUMQuestEditor/tags");
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();

                using var doc = JsonDocument.Parse(json);
                var tags = doc.RootElement.EnumerateArray()
                    .Select(e => e.GetProperty("name").GetString())
                    .Where(n => !string.IsNullOrEmpty(n) && n != "autobuild")
                    .ToList();

                if (tags.Count == 0)
                {
                    MessageBox.Show("No tags found on GitHub.", "Check Updates", MessageBoxButton.OK, MessageBoxImage.Information);
                    BtnCheckUpdates.Content = "Check Updates";
                    BtnCheckUpdates.IsEnabled = true;
                    return;
                }

                var latestTag = tags[0]!;
                var latestVersion = latestTag.TrimStart('v');

                int cmp = CompareVersions(BuildInfo.Version, latestVersion);

                if (cmp < 0)
                {
                    var result = MessageBox.Show($"A new version is available!\n\nCurrent: v{BuildInfo.Version}\nLatest: v{latestVersion}\n\nOpen GitHub releases page?", "Update Available", MessageBoxButton.YesNo, MessageBoxImage.Information);
                    if (result == MessageBoxResult.Yes)
                    {
                        Process.Start(new ProcessStartInfo("https://github.com/ntoff/SCUMQuestEditor/releases") { UseShellExecute = true });
                    }
                }
                else if (cmp == 0)
                {
                    MessageBox.Show($"You are running the latest version: v{BuildInfo.Version}", "Check Updates", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    var result = MessageBox.Show($"Your version ({BuildInfo.Version}) is newer than the latest tag ({latestVersion}).\n\nThis may be a pre-release or custom build.\n\nContinue to GitHub releases?", "Check Updates", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                    {
                        Process.Start(new ProcessStartInfo("https://github.com/ntoff/SCUMQuestEditor/releases") { UseShellExecute = true });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to check for updates:\n{ex.Message}", "Check Updates", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                BtnCheckUpdates.Content = "Check Updates";
                BtnCheckUpdates.IsEnabled = true;
            }
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private int CompareVersions(string current, string latest)
        {
            var partsA = current.Split('.');
            var partsB = latest.Split('.');
            var length = Math.Max(partsA.Length, partsB.Length);

            for (int i = 0; i < length; i++)
            {
                int a = i < partsA.Length ? int.Parse(partsA[i]) : 0;
                int b = i < partsB.Length ? int.Parse(partsB[i]) : 0;

                if (a < b) return -1;
                if (a > b) return 1;
            }

            return 0;
        }
    }
}
