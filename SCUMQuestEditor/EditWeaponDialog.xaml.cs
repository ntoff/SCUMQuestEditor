using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Linq;

namespace SCUMQuestEditor
{
    public partial class EditWeaponDialog : Window
    {
        public List<string> SelectedItems { get; private set; } = new List<string>();
        private List<string> AvailableWeapons { get; set; } = new List<string>();
        private List<string> InitialSelections { get; set; } = new List<string>();

        public EditWeaponDialog(List<string>? initialSelections = null)
        {
            InitializeComponent();
            InitialSelections = initialSelections ?? new List<string>();

            try
            {
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "_data\\EliminationWeapons.txt");
                if (File.Exists(path))
                {
                    AvailableWeapons = File.ReadAllLines(path).Where(line => !string.IsNullOrEmpty(line)).ToList();
                }
                else
                {
                    AvailableWeapons.Add("DefaultWeapon");
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Error loading weapons: {ex.Message}");
                AvailableWeapons.Add("DefaultWeapon");
            }

            LstWeapons.ItemsSource = AvailableWeapons;

            foreach (var item in AvailableWeapons)
            {
                if (InitialSelections.Contains(item)) LstWeapons.SelectedItems.Add(item);
            }
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            SelectedItems = new List<string>();
            foreach (var item in LstWeapons.SelectedItems) SelectedItems.Add(item.ToString());
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
