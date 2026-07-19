using System.Globalization;
using System.Windows;

namespace SCUMQuestEditor
{
    public partial class AboutDialog : Window
    {
        public AboutDialog()
        {
            InitializeComponent();
            LblVersion.Text = "Version 1.0.0";
            LblBuildDate.Text = $"Built on {BuildInfo.BuildDate}";
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
