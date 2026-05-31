using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Text.RegularExpressions;

namespace SCUMQuestEditor
{
    public partial class EditMapLocationsDialog : Window
    {
        public ObservableCollection<MapLocation> MapLocations { get; set; } = new ObservableCollection<MapLocation>();

        public EditMapLocationsDialog(List<MapLocation> initialLocations)
        {
            InitializeComponent();

            foreach (var loc in initialLocations)
            {
                MapLocations.Add(new MapLocation
                {
                    Location = new MapLocationEntry
                    {
                        X = loc.Location.X,
                        Y = loc.Location.Y,
                        Z = loc.Location.Z
                    },
                    SizeFactor = loc.SizeFactor
                });
            }

            LocationList.ItemsSource = MapLocations;
            LocationList.SelectionMode = SelectionMode.Single;

            // Set defaults
            TxtX.Text = "0.0000";
            TxtY.Text = "0.0000";
            TxtZ.Text = "0.0000";
            TxtSizeFactor.Text = "1.00";

            // Validation
            TxtX.PreviewTextInput += FloatPreviewTextInput;
            TxtY.PreviewTextInput += FloatPreviewTextInput;
            TxtZ.PreviewTextInput += FloatPreviewTextInput;
            TxtSizeFactor.PreviewTextInput += FloatPreviewTextInput;
        }

        private void FloatPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox textBox = (TextBox)sender;

            // Allow backspace/delete operations
            if (string.IsNullOrEmpty(e.Text)) return;

            // Simulate the text after insertion at the current caret position
            string newText = textBox.Text.Insert(textBox.CaretIndex, e.Text);

            // Prevent multiple decimal points
            if (newText.Contains(".") && e.Text == ".")
            {
                e.Handled = true;
                return;
            }

            // Allow an optional leading minus sign for negative coordinates
            bool isNegative = newText.StartsWith("-");
            string checkString = isNegative ? newText.Substring(1) : newText;

            // Validate that the remaining characters are digits and at most one decimal point
            e.Handled = !Regex.IsMatch(checkString, @"^\d*\.?\d*$");
        }


        private void BtnParse_Click(object sender, RoutedEventArgs e)
        {
            string input = TxtLocationString.Text;

            Regex regex = new Regex(@"\{X\s*=\s*(-?\d+(?:\.\d+)?)\s+Y\s*=\s*(-?\d+(?:\.\d+)?)\s+Z\s*=\s*(-?\d+(?:\.\d+)?)");

            Match match = regex.Match(input);

            if (match.Success)
            {
                // Using InvariantCulture ensures consistent decimal parsing regardless of system locale
                double x = double.Parse(match.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture);
                double y = double.Parse(match.Groups[2].Value, System.Globalization.CultureInfo.InvariantCulture);
                double z = double.Parse(match.Groups[3].Value, System.Globalization.CultureInfo.InvariantCulture);

                TxtX.Text = x.ToString("F4");
                TxtY.Text = y.ToString("F4");
                TxtZ.Text = z.ToString("F4");
                TxtSizeFactor.Text = "1.00";
            }
            else
            {
                MessageBox.Show(
                    "Could not parse location string.\n" +
                    "Accepted formats:\n" +
                    "{X=... Y=... Z=...}\n" +
                    "{X=... Y=... Z=...|P=... R=...}",
                    "Parse Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnAddUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (double.TryParse(TxtX.Text, out double x) &&
                double.TryParse(TxtY.Text, out double y) &&
                double.TryParse(TxtZ.Text, out double z) &&
                double.TryParse(TxtSizeFactor.Text, out double sizeFactor))
            {
                var newLocation = new MapLocation
                {
                    Location = new MapLocationEntry { X = x, Y = y, Z = z },
                    SizeFactor = sizeFactor
                };

                if (LocationList.SelectedItem != null && LocationList.SelectedItem is MapLocation selectedLoc)
                {
                    selectedLoc.Location.X = x;
                    selectedLoc.Location.Y = y;
                    selectedLoc.Location.Z = z;
                    selectedLoc.SizeFactor = sizeFactor;
                }
                else
                {
                    MapLocations.Add(newLocation);
                }

                LocationList.ItemsSource = null;
                LocationList.ItemsSource = MapLocations;
            }
            else
            {
                MessageBox.Show("Please enter valid coordinates and size factor.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnEditSelected_Click(object sender, RoutedEventArgs e)
        {
            if (LocationList.SelectedItem is MapLocation selectedLoc)
            {
                TxtX.Text = selectedLoc.Location.X.ToString("F4");
                TxtY.Text = selectedLoc.Location.Y.ToString("F4");
                TxtZ.Text = selectedLoc.Location.Z.ToString("F4");
                TxtSizeFactor.Text = selectedLoc.SizeFactor.ToString("F2");

                TxtLocationString.Text = $"{{X={selectedLoc.Location.X} Y={selectedLoc.Location.Y} Z={selectedLoc.Location.Z}}}";
            }
            else
            {
                MessageBox.Show("Please select a location to edit.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnRemoveSelected_Click(object sender, RoutedEventArgs e)
        {
            if (LocationList.SelectedItem is MapLocation selectedLoc)
            {
                MapLocations.Remove(selectedLoc);
                LocationList.ItemsSource = null;
                LocationList.ItemsSource = MapLocations;
            }
            else
            {
                MessageBox.Show("Please select a location to remove.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
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
