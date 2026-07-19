using System.Windows;

namespace SCUMQuestEditor
{
    public partial class FilenameFormatDialog : Window
    {
        public string NewFormat => TxtFormat.Text ?? "";
        public bool IsOkClicked => DialogResult == true;

        public FilenameFormatDialog(string currentFormat)
        {
            InitializeComponent();
            TxtFormat.Text = currentFormat;
            /*LblPlaceholders.Text = "Available placeholders:";
            LblTier.Text = "{tier} - Quest tier (1, 2, 3)";
            LblTrader.Text = "{trader} - Trader code (AR, BK, BA, BT, DC, GG, HM, HT, MC)";
            LblTitle.Text = "{title} - Quest title (spaces replaced with underscores)";*/
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
