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
