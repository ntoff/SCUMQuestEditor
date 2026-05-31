using System.Collections.ObjectModel;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;

namespace SCUMQuestEditor
{
    public partial class EditInteractionLocationsDialog : Window
    {
        public ObservableCollection<InteractionLocation> Locations { get; set; } = new ObservableCollection<InteractionLocation>();

        public EditInteractionLocationsDialog(List<InteractionLocation> initialLocations)
        {
            InitializeComponent();

            foreach (var loc in initialLocations)
            {
                Locations.Add(new InteractionLocation
                {
                    AnchorMesh = loc.AnchorMesh,
                    FallbackTransform = loc.FallbackTransform,
                    VisibleMesh = loc.VisibleMesh,
                    Instance = loc.Instance
                });
            }

            LocationList.ItemsSource = Locations;
            LocationList.SelectionMode = SelectionMode.Single;
        }

        private void ParseMeshInfoInput()
        {
            string input = TxtMeshInfoInput.Text.Trim();
            if (string.IsNullOrEmpty(input))
            {
                MessageBox.Show("Please paste the #GetMeshInfo output.", "Empty Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                InteractionLocation? location = null;
                List<InteractionLocation>? locations = null;

                location = JsonSerializer.Deserialize<InteractionLocation>(input, options);

                if (location == null)
                {
                    locations = JsonSerializer.Deserialize<List<InteractionLocation>>(input, options);
                }

                if (location != null)
                {
                    if (LocationList.SelectedItem is InteractionLocation selectedLoc)
                    {
                        selectedLoc.AnchorMesh = location.AnchorMesh;
                        selectedLoc.FallbackTransform = location.FallbackTransform;
                        selectedLoc.VisibleMesh = location.VisibleMesh;
                        selectedLoc.Instance = location.Instance;
                    }
                    else
                    {
                        Locations.Add(location);
                    }
                }
                else if (locations != null)
                {
                    foreach (var loc in locations)
                    {
                        Locations.Add(loc);
                    }
                }
                else
                {
                    MessageBox.Show("Failed to parse the JSON input.", "Parse Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                LocationList.ItemsSource = null;
                LocationList.ItemsSource = Locations;
                TxtMeshInfoInput.Text = "";
            }
            catch (JsonException ex)
            {
                MessageBox.Show($"Error parsing JSON: {ex.Message}", "JSON Parse Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnAddLocation_Click(object sender, RoutedEventArgs e)
        {
            var newLocation = new InteractionLocation
            {
                VisibleMesh = "New Mesh",
                FallbackTransform = "0,0,0|0,0,0|1,1,1",
                Instance = Locations.Count + 1
            };
            Locations.Add(newLocation);
            LocationList.ItemsSource = null;
            LocationList.ItemsSource = Locations;
        }

        private void BtnEditSelectedLocation_Click(object sender, RoutedEventArgs e)
        {
            if (LocationList.SelectedItem is InteractionLocation selectedLoc)
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };
                TxtMeshInfoInput.Text = JsonSerializer.Serialize(selectedLoc, options);
                TxtMeshInfoInput.Focus();
                TxtMeshInfoInput.SelectionStart = TxtMeshInfoInput.Text.Length;
            }
            else
            {
                MessageBox.Show("Please select a location to edit.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnRemoveSelectedLocation_Click(object sender, RoutedEventArgs e)
        {
            if (LocationList.SelectedItem is InteractionLocation selectedLoc)
            {
                Locations.Remove(selectedLoc);
                LocationList.ItemsSource = null;
                LocationList.ItemsSource = Locations;
            }
            else
            {
                MessageBox.Show("Please select a location to remove.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnApply_Click(object sender, RoutedEventArgs e)
        {
            ParseMeshInfoInput();
        }

        private void BtnCancelInput_Click(object sender, RoutedEventArgs e)
        {
            LocationList.ItemsSource = null;
            LocationList.ItemsSource = Locations;
            TxtMeshInfoInput.Text = "";
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
