using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Linq;

namespace SCUMQuestEditor
{
    public partial class EditTargetCharactersDialog : Window
    {
        private static readonly Lazy<List<string>> s_cachedTypes = new(() =>
        {
            try
            {
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "_data", "EliminationTargets.txt");
                if (File.Exists(path))
                    return File.ReadAllLines(path).Where(line => !string.IsNullOrEmpty(line)).ToList();
            }
            catch { /* ignore */ }
            return new List<string> { "DefaultTarget" };
        });

        public List<string> SelectedItems { get; private set; } = new List<string>();
        private List<string> InitialSelections { get; set; } = new List<string>();

        public EditTargetCharactersDialog(List<string>? initialSelections = null)
        {
            InitializeComponent();
            InitialSelections = initialSelections ?? new List<string>();

            var availableTypes = s_cachedTypes.Value;
            LstTargetTypes.ItemsSource = availableTypes;

            foreach (var item in availableTypes)
            {
                if (InitialSelections.Contains(item)) LstTargetTypes.SelectedItems.Add(item);
            }
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            SelectedItems = new List<string>();
            foreach (var item in LstTargetTypes.SelectedItems) SelectedItems.Add(item.ToString()!);
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
