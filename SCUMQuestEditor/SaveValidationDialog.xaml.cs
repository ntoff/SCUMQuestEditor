using System.Windows;

namespace SCUMQuestEditor
{
    public partial class SaveValidationDialog : Window
    {
        public SaveValidationDialog(string message)
        {
            InitializeComponent();
            TxtMessage.Text = message;
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
