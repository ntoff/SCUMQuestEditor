using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Linq;

namespace SCUMQuestEditor
{
    public partial class EditTargetCharactersDialog : Window
    {
        private MainWindow? _mainWindow;
        private static readonly Lazy<List<string>> s_cachedTypes = new(() =>
        {
            try
            {
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "_data", "EliminationTargets.txt");
                if (File.Exists(path))
                    return File.ReadAllLines(path).Where(line => !string.IsNullOrEmpty(line)).ToList();
                else
                    MainWindow.Log("EliminationTargets.txt file not found.");
            }
            catch (Exception ex)
            {
                MainWindow.Log($"Failed to load EliminationTargets.txt: {ex.Message}");
            }
            return new List<string> { "DefaultTarget" };
        });

        public List<string> SelectedItems { get; private set; } = new List<string>();
        private List<string> InitialSelections { get; set; } = new List<string>();

        public EditTargetCharactersDialog(MainWindow mainWindow, List<string>? initialSelections = null)
        {
            _mainWindow = mainWindow;
            InitializeComponent();
            InitialSelections = initialSelections ?? new List<string>();

            var availableTypes = s_cachedTypes.Value;
            LstTargetTypes.ItemsSource = availableTypes;

            foreach (var item in availableTypes)
            {
                if (InitialSelections.Contains(item)) LstTargetTypes.SelectedItems.Add(item);
            }

            if (_mainWindow != null && availableTypes.Count == 1 && availableTypes[0] == "DefaultTarget" && !(_mainWindow?.TargetsWarningShown ?? false))
            {
                    _mainWindow!.TargetsWarningShown = true;
                MessageBox.Show(
                    "The EliminationTargets.txt data file is missing or could not be loaded. Target character selection may be limited.",
                    "Data File Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
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
