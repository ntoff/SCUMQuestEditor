using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MaterialDesignColors;

namespace SCUMQuestEditor
{
    public partial class ColorSettingsDialog : Window
    {
        private string _selectedPrimaryColor;
        private string _selectedSecondaryColor;
        private string _selectedErrorColor;
        private bool _darkMode;

        public string PrimaryColor => _selectedPrimaryColor;
        public string SecondaryColor => _selectedSecondaryColor;
        public string ErrorColor => _selectedErrorColor;
        public bool DarkMode => _darkMode;

        private static readonly Dictionary<string, Dictionary<int, Color>> s_colorMap = new()
        {
            ["Red"] = new() { [500] = MakeColor(0xEF, 0x53, 0x50) },
            ["Pink"] = new() { [500] = MakeColor(0xEC, 0x40, 0x74) },
            ["Purple"] = new() { [500] = MakeColor(0xAB, 0x47, 0xB7) },
            ["DeepPurple"] = new() { [500] = MakeColor(0x7E, 0x57, 0xC6) },
            ["Indigo"] = new() { [500] = MakeColor(0x3F, 0x51, 0xB5) },
            ["Blue"] = new() { [500] = MakeColor(0x21, 0x96, 0xF3) },
            ["LightBlue"] = new() { [500] = MakeColor(0x03, 0xA9, 0xF4) },
            ["Cyan"] = new() { [500] = MakeColor(0x00, 0xBC, 0xD4) },
            ["Teal"] = new() { [500] = MakeColor(0x00, 0x96, 0x88) },
            ["Green"] = new() { [500] = MakeColor(0x4C, 0xAF, 0x50) },
            ["LightGreen"] = new() { [500] = MakeColor(0x8D, 0xB7, 0x56) },
            ["Lime"] = new() { [500] = MakeColor(0xC0, 0xCA, 0x11) },
            ["Amber"] = new() { [500] = MakeColor(0xFF, 0xBC, 0x4D) },
            ["Orange"] = new() { [500] = MakeColor(0xFF, 0x98, 0x00) },
            ["DeepOrange"] = new() { [500] = MakeColor(0xFB, 0x8C, 0x00) },
            ["Brown"] = new() { [500] = MakeColor(0x79, 0x55, 0x48) },
            ["Grey"] = new() { [500] = MakeColor(0x9E, 0x9E, 0x9E) },
            ["BlueGrey"] = new() { [500] = MakeColor(0x60, 0x7D, 0x8B) },
        };

        private static Color MakeColor(byte r, byte g, byte b) => Color.FromArgb(255, r, g, b);

        public ColorSettingsDialog(string primaryColor, string secondaryColor, string errorColor, bool darkMode = true)
        {
            InitializeComponent();
            _selectedPrimaryColor = primaryColor;
            _selectedSecondaryColor = secondaryColor;
            _selectedErrorColor = errorColor;
            _darkMode = darkMode;
            TglDarkMode.IsChecked = darkMode;

            LoadSwatches();
            
            this.Loaded += (s, e) => 
            {
                Dispatcher.BeginInvoke(new Action(() => UpdateAllBorders()), System.Windows.Threading.DispatcherPriority.Background);
            };
        }

        private void LoadSwatches()
        {
            var swatchInfos = new List<SwatchInfo>();
            foreach (var swatch in SwatchHelper.Swatches)
            {
                var color = GetSwatchColor(swatch.Name, "500");
                var brush = new SolidColorBrush(color);
                brush.Freeze();
                swatchInfos.Add(new SwatchInfo(swatch.Name, brush));
            }

            PrimarySwatches.ItemsSource = swatchInfos;
            SecondarySwatches.ItemsSource = swatchInfos;
            ErrorSwatches.ItemsSource = swatchInfos;

            var primarySwatch = swatchInfos.FirstOrDefault(s => s.Name == _selectedPrimaryColor);
            var secondarySwatch = swatchInfos.FirstOrDefault(s => s.Name == _selectedSecondaryColor);
            var errorSwatch = swatchInfos.FirstOrDefault(s => s.Name == _selectedErrorColor);

            if (primarySwatch != null) PrimarySwatches.SelectedItem = primarySwatch;
            else if (swatchInfos.Count > 0) PrimarySwatches.SelectedItem = swatchInfos[0];

            if (secondarySwatch != null) SecondarySwatches.SelectedItem = secondarySwatch;
            else if (swatchInfos.Count > 0) SecondarySwatches.SelectedItem = swatchInfos[0];

            if (errorSwatch != null) ErrorSwatches.SelectedItem = errorSwatch;
            else if (swatchInfos.Count > 0) ErrorSwatches.SelectedItem = swatchInfos[0];
        }

        private void OnSwatchSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender == PrimarySwatches && PrimarySwatches.SelectedItem is SwatchInfo s1)
            {
                _selectedPrimaryColor = s1.Name;
                UpdateSwatchBorder(PrimarySwatches, s1.Name);
            }
            else if (sender == SecondarySwatches && SecondarySwatches.SelectedItem is SwatchInfo s2)
            {
                _selectedSecondaryColor = s2.Name;
                UpdateSwatchBorder(SecondarySwatches, s2.Name);
            }
            else if (sender == ErrorSwatches && ErrorSwatches.SelectedItem is SwatchInfo s3)
            {
                _selectedErrorColor = s3.Name;
                UpdateSwatchBorder(ErrorSwatches, s3.Name);
            }
        }

        private void UpdateSwatchBorder(ItemsControl itemsControl, string selectedName)
        {
            foreach (var item in itemsControl.Items)
            {
                if (item is SwatchInfo si)
                {
                    var container = itemsControl.ItemContainerGenerator.ContainerFromItem(si) as Border;
                    if (container != null)
                    {
                        if (si.Name == selectedName)
                        {
                            var brush = FindResource("PrimaryHueMidBrush") as Brush;
                            container.BorderBrush = brush ?? new SolidColorBrush(Colors.White);
                        }
                        else
                        {
                            container.BorderBrush = Brushes.Transparent;
                        }
                    }
                }
            }
        }

        private void UpdateAllBorders()
        {
            var primary = PrimarySwatches.SelectedItem as SwatchInfo;
            if (primary != null) UpdateSwatchBorder(PrimarySwatches, primary.Name);
            
            var secondary = SecondarySwatches.SelectedItem as SwatchInfo;
            if (secondary != null) UpdateSwatchBorder(SecondarySwatches, secondary.Name);
            
            var error = ErrorSwatches.SelectedItem as SwatchInfo;
            if (error != null) UpdateSwatchBorder(ErrorSwatches, error.Name);
        }

        private void TglDarkMode_Checked(object sender, RoutedEventArgs e)
        {
            _darkMode = true;
        }

        private void TglDarkMode_Unchecked(object sender, RoutedEventArgs e)
        {
            _darkMode = false;
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void BtnApply_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.ApplyDarkMode(_darkMode);
            MainWindow.ApplyTheme(_selectedPrimaryColor, _selectedSecondaryColor);
            MainWindow.ApplyErrorColor(_selectedErrorColor);
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void ResetPrimaryToDefault(object sender, RoutedEventArgs e)
        {
            var swatch = PrimarySwatches.Items.Cast<SwatchInfo>().FirstOrDefault(s => s.Name == "BlueGrey");
            if (swatch != null)
            {
                PrimarySwatches.SelectedItem = swatch;
                _selectedPrimaryColor = "BlueGrey";
            }
        }

        private void ResetSecondaryToDefault(object sender, RoutedEventArgs e)
        {
            var swatch = SecondarySwatches.Items.Cast<SwatchInfo>().FirstOrDefault(s => s.Name == "Red");
            if (swatch != null)
            {
                SecondarySwatches.SelectedItem = swatch;
                _selectedSecondaryColor = "Red";
            }
        }

        private void ResetErrorToDefault(object sender, RoutedEventArgs e)
        {
            var swatch = ErrorSwatches.Items.Cast<SwatchInfo>().FirstOrDefault(s => s.Name == "Red");
            if (swatch != null)
            {
                ErrorSwatches.SelectedItem = swatch;
                _selectedErrorColor = "Red";
            }
        }

        public class SwatchInfo
        {
            public string Name { get; }
            public SolidColorBrush Brush { get; }

            public SwatchInfo(string name, SolidColorBrush brush)
            {
                Name = name;
                Brush = brush;
            }
        }

        public static Color GetColorFromName(string swatchName, string chromaticity) => GetSwatchColor(swatchName, chromaticity);

        public static Color LightenColor(Color color) => Color.FromArgb(color.A, 
            (byte)Math.Min(255, color.R + 80), 
            (byte)Math.Min(255, color.G + 80), 
            (byte)Math.Min(255, color.B + 80));

        private static Color GetSwatchColor(string swatchName, string chromaticity)
        {
            if (s_colorMap.TryGetValue(swatchName, out var colors))
            {
                if (colors.TryGetValue(int.Parse(chromaticity), out var color))
                    return color;
            }
            return Colors.Gray;
        }
    }
}
