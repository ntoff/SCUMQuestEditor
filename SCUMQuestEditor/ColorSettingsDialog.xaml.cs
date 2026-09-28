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

        public string PrimaryColor => _selectedPrimaryColor;
        public string SecondaryColor => _selectedSecondaryColor;
        public string ErrorColor => _selectedErrorColor;

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

        public ColorSettingsDialog(string primaryColor, string secondaryColor, string errorColor)
        {
            InitializeComponent();
            _selectedPrimaryColor = primaryColor;
            _selectedSecondaryColor = secondaryColor;
            _selectedErrorColor = errorColor;

            LoadSwatches();
            
            // Defer preview update until resources are loaded
            this.Loaded += (s, e) => UpdatePreview();
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

            foreach (var swatch in swatchInfos)
            {
                if (swatch.Name == _selectedPrimaryColor)
                    SelectPrimary(swatch);
                if (swatch.Name == _selectedSecondaryColor)
                    SelectSecondary(swatch);
                if (swatch.Name == _selectedErrorColor)
                    SelectError(swatch);
            }

            if (!swatchInfos.Any(s => s.Name == _selectedPrimaryColor) && swatchInfos.Count > 0)
            {
                SelectPrimary(swatchInfos[0]);
            }
            if (!swatchInfos.Any(s => s.Name == _selectedSecondaryColor) && swatchInfos.Count > 0)
            {
                SelectSecondary(swatchInfos[0]);
            }
            if (!swatchInfos.Any(s => s.Name == _selectedErrorColor) && swatchInfos.Count > 0)
            {
                SelectError(swatchInfos[0]);
            }
        }

        private void SelectPrimary(SwatchInfo swatch)
        {
            _selectedPrimaryColor = swatch.Name;
            
            foreach (var item in PrimarySwatches.Items)
            {
                if (item is SwatchInfo si)
                {
                    var bdr = FindSwatchBorder(PrimarySwatches, si.Name);
                    if (bdr != null)
                    {
                        bdr.SetValue(Border.BorderBrushProperty, FindResource("MaterialDesignDivider"));
                        bdr.BorderThickness = new Thickness(2);
                    }
                }
            }
            
            var bdr2 = FindSwatchBorder(PrimarySwatches, swatch.Name);
            if (bdr2 != null)
            {
                bdr2.SetValue(Border.BorderBrushProperty, FindResource("PrimaryHueMidBrush"));
                bdr2.BorderThickness = new Thickness(3);
            }
        }

        private void SelectSecondary(SwatchInfo swatch)
        {
            _selectedSecondaryColor = swatch.Name;
            
            foreach (var item in SecondarySwatches.Items)
            {
                if (item is SwatchInfo si)
                {
                    var bdr = FindSwatchBorder(SecondarySwatches, si.Name);
                    if (bdr != null)
                    {
                        bdr.SetValue(Border.BorderBrushProperty, FindResource("MaterialDesignDivider"));
                        bdr.BorderThickness = new Thickness(2);
                    }
                }
            }
            
            var bdr2 = FindSwatchBorder(SecondarySwatches, swatch.Name);
            if (bdr2 != null)
            {
                bdr2.SetValue(Border.BorderBrushProperty, FindResource("PrimaryHueMidBrush"));
                bdr2.BorderThickness = new Thickness(3);
            }
        }

        private void SelectError(SwatchInfo swatch)
        {
            _selectedErrorColor = swatch.Name;
            
            foreach (var item in ErrorSwatches.Items)
            {
                if (item is SwatchInfo si)
                {
                    var bdr = FindSwatchBorder(ErrorSwatches, si.Name);
                    if (bdr != null)
                    {
                        bdr.SetValue(Border.BorderBrushProperty, FindResource("MaterialDesignDivider"));
                        bdr.BorderThickness = new Thickness(2);
                    }
                }
            }
            
            var bdr2 = FindSwatchBorder(ErrorSwatches, swatch.Name);
            if (bdr2 != null)
            {
                bdr2.SetValue(Border.BorderBrushProperty, FindResource("PrimaryHueMidBrush"));
                bdr2.BorderThickness = new Thickness(3);
            }
        }

        private static Border? FindSwatchBorder(ItemsControl itemsControl, string name)
        {
            itemsControl.UpdateLayout();
            foreach (var item in itemsControl.Items)
            {
                if (item is SwatchInfo si && si.Name == name)
                {
                    var container = itemsControl.ItemContainerGenerator.ContainerFromItem(si) as Border;
                    if (container != null)
                        return container;
                }
            }
            return null;
        }

        private void UpdatePreview()
        {
            var color1 = GetSwatchColor(_selectedPrimaryColor, "500");
            TxtPrimaryPreview.Text = _selectedPrimaryColor;
            PrimaryColorSwatch.Background = new SolidColorBrush(color1);

            var color2 = GetSwatchColor(_selectedSecondaryColor, "500");
            TxtSecondaryPreview.Text = _selectedSecondaryColor;
            SecondaryColorSwatch.Background = new SolidColorBrush(color2);

            var errorColor = GetSwatchColor(_selectedErrorColor, "500");
            TxtErrorPreview.Text = _selectedErrorColor;
            ErrorColorSwatch.Background = new SolidColorBrush(errorColor);
        }

        private void OnPrimarySwatchClick(object sender, RoutedEventArgs e)
        {
            if (sender is Border border && border.DataContext is SwatchInfo swatch)
            {
                SelectPrimary(swatch);
                UpdatePreview();
            }
        }

        private void OnSecondarySwatchClick(object sender, RoutedEventArgs e)
        {
            if (sender is Border border && border.DataContext is SwatchInfo swatch)
            {
                SelectSecondary(swatch);
                UpdatePreview();
            }
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void BtnApply_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.ApplyTheme(_selectedPrimaryColor, _selectedSecondaryColor);
            MainWindow.ApplyErrorColor(_selectedErrorColor);
            UpdatePreview();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void ResetPrimaryToDefault(object sender, RoutedEventArgs e)
        {
            SelectPrimary(new SwatchInfo("BlueGrey", new SolidColorBrush(GetSwatchColor("BlueGrey", "500"))));
            UpdatePreview();
        }

        private void ResetSecondaryToDefault(object sender, RoutedEventArgs e)
        {
            SelectSecondary(new SwatchInfo("Green", new SolidColorBrush(GetSwatchColor("Green", "500"))));
            UpdatePreview();
        }

        private void OnErrorSwatchClick(object sender, RoutedEventArgs e)
        {
            if (sender is Border border && border.DataContext is SwatchInfo swatch)
            {
                SelectError(swatch);
                UpdatePreview();
            }
        }

        private void ResetErrorToDefault(object sender, RoutedEventArgs e)
        {
            SelectError(new SwatchInfo("Red", new SolidColorBrush(GetSwatchColor("Red", "500"))));
            UpdatePreview();
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
