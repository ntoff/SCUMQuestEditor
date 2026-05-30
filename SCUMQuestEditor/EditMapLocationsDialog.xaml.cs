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
            string currentText = ((TextBox)sender).Text;
            if (currentText.Contains(".") && e.Text == ".") { e.Handled = true; return; }
            e.Handled = !Regex.IsMatch(e.Text, @"^\d*\.?\d*$");
        }

        private void BtnParse_Click(object sender, RoutedEventArgs e)
        {
            string input = TxtLocationString.Text;
            Regex regex = new Regex(@"\{X=([0-9.-]+)\s+Y=([0-9.-]+)\s+Z=([0-9.-]+)\|");
            Match match = regex.Match(input);

            if (match.Success)
            {
                double x = double.Parse(match.Groups[1].Value);
                double y = double.Parse(match.Groups[2].Value);
                double z = double.Parse(match.Groups[3].Value);

                TxtX.Text = x.ToString("F4");
                TxtY.Text = y.ToString("F4");
                TxtZ.Text = z.ToString("F4");
                TxtSizeFactor.Text = "1.00";
            }
            else
            {
                MessageBox.Show("Could not parse location string. Ensure format is {X=... Y=... Z=...|...}", "Parse Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
