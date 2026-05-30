using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Linq;

namespace SCUMQuestEditor
{
    public partial class EditTargetCharactersDialog : Window
    {
        public List<string> SelectedItems { get; private set; } = new List<string>();
        private List<string> AvailableTypes { get; set; } = new List<string>();
        private List<string> InitialSelections { get; set; } = new List<string>();

        public EditTargetCharactersDialog(List<string>? initialSelections = null)
        {
            InitializeComponent();
            InitialSelections = initialSelections ?? new List<string>();

            try
            {
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "_data\\EliminationTargets.txt");
                if (File.Exists(path))
                {
                    AvailableTypes = File.ReadAllLines(path).Where(line => !string.IsNullOrEmpty(line)).ToList();
                }
                else
                {
                    AvailableTypes.Add("DefaultTarget");
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Error loading targets: {ex.Message}");
                AvailableTypes.Add("DefaultTarget");
            }

            LstTargetTypes.ItemsSource = AvailableTypes;

            foreach (var item in AvailableTypes)
            {
                if (InitialSelections.Contains(item)) LstTargetTypes.SelectedItems.Add(item);
            }
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            SelectedItems = new List<string>();
            foreach (var item in LstTargetTypes.SelectedItems) SelectedItems.Add(item.ToString());
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
