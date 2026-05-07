#nullable enable
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;

namespace TabbedApp
{
    // Helper classes for the new data structure
    public class SkillReward
    {
        public string Skill { get; set; } = "";
        public double Experience { get; set; } = 0;
    }

    public class TradeDealReward
    {
        public string Item { get; set; } = "";
        public double Price { get; set; } = 0;
        public int Amount { get; set; } = 1;
        public double Fame { get; set; } = 0;
        public bool AllowExcluded { get; set; } = false;
    }

    public class RewardPool
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public int CurrencyNormal { get; set; } = 0;

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public int CurrencyGold { get; set; } = 0;

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public int Fame { get; set; } = 0;

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<SkillReward>? Skills { get; set; } = null;

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<TradeDealReward>? TradeDeals { get; set; } = null;
    }

    public class TradeDeal
    {
        public string AssociatedNpc { get; set; } = "";
        public int Tier { get; set; }
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public double TimeLimitHours { get; set; }
        public List<RewardPool> RewardPool { get; set; } = new List<RewardPool>();
        public List<Condition> Conditions { get; set; } = new List<Condition>();
    }

    // Base Condition Class
    public class Condition
    {
        [JsonPropertyName("TrackingCaption")]
        public string TrackingCaption { get; set; } = "";

        [JsonPropertyName("SequenceIndex")]
        public int SequenceIndex { get; set; } = 0;

        [JsonPropertyName("CanBeAutoCompleted")]
        public bool CanBeAutoCompleted { get; set; } = false;

        [JsonPropertyName("Type")]
        public string Type { get; set; } = "";

        [JsonPropertyName("LocationsShownOnMap")]
        public List<MapLocation> LocationsShownOnMap { get; set; } = new List<MapLocation>();
    }

    public class MapLocation
    {
        [JsonPropertyName("Location")]
        public MapLocationEntry Location { get; set; } = new MapLocationEntry();

        [JsonPropertyName("SizeFactor")]
        public double SizeFactor { get; set; } = 1.0;

        // For display in the dialog
        public string FormatString => $"X={Location.X} Y={Location.Y} Z={Location.Z}";
    }

    public class MapLocationEntry
    {
        [JsonPropertyName("X")]
        public double X { get; set; }

        [JsonPropertyName("Y")]
        public double Y { get; set; }

        [JsonPropertyName("Z")]
        public double Z { get; set; }
    }

    public class EliminationCondition : Condition
    {
        public EliminationCondition()
        {
            Type = "Elimination";
        }

        public int Amount { get; set; } = 1;
        public List<string>? TargetCharacters { get; set; } = new List<string>();
        public List<string>? AllowedWeapons { get; set; } = new List<string>();
    }

    public class FetchCondition : Condition
    {
        public FetchCondition()
        {
            Type = "Fetch";
        }

        [JsonPropertyName("PlayerKeepsItems")]
        public bool PlayerKeepsItems { get; set; } = false;

        [JsonPropertyName("DisablePurchase")]
        public bool DisablePurchase { get; set; } = true;

        [JsonPropertyName("RequiredItems")]
        public List<RequiredItem> RequiredItems { get; set; } = new List<RequiredItem>();
    }

    // Updated RequiredItem to match the new JSON structure
    public class RequiredItem
    {
        [JsonPropertyName("AcceptedItems")]
        public List<string> AcceptedItems { get; set; } = new List<string>();

        // Helper property for display in ListView
        public string AcceptedItemsDisplay => string.Join(", ", AcceptedItems);

        [JsonPropertyName("RequiredNum")]
        public int RequiredNum { get; set; } = 0;

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public int MinAcceptedItemUses { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public double MinAcceptedItemMass { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public double MinAcceptedItemHealth { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string? MinAcceptedCookLevel { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string? MaxAcceptedCookLevel { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string? MinAcceptedCookQuality { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public double MinAcceptedItemResourceRatio { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public double MinAcceptedItemResourceAmount { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public int RandomAdditionalRequiredNum { get; set; }
    }

    public class InteractionLocation
    {
        [JsonPropertyName("AnchorMesh")]
        public string AnchorMesh { get; set; } = "";

        [JsonPropertyName("FallbackTransform")]
        public string FallbackTransform { get; set; } = "";

        [JsonPropertyName("VisibleMesh")]
        public string VisibleMesh { get; set; } = "";

        [JsonPropertyName("Instance")]
        public int Instance { get; set; } = 0;

        // Helper property for display
        public bool HasAnchor => !string.IsNullOrEmpty(AnchorMesh);
    }

    public class InteractionCondition : Condition
    {
        public InteractionCondition()
        {
            Type = "Interaction";
        }

        [JsonPropertyName("SpawnOnlyNeeded")]
        public bool SpawnOnlyNeeded { get; set; } = true;

        [JsonPropertyName("MinNeeded")]
        public int MinNeeded { get; set; } = 1;

        [JsonPropertyName("MaxNeeded")]
        public int MaxNeeded { get; set; } = 1;

        [JsonPropertyName("WorldMarkerShowDistance")]
        public int WorldMarkerShowDistance { get; set; } = 5;

        [JsonPropertyName("Locations")]
        public List<InteractionLocation> Locations { get; set; } = new List<InteractionLocation>();
    }

    // Custom Converter to handle polymorphism, order, and no $type
    public class ConditionConverter : JsonConverter<Condition>
    {
        public override Condition Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // Read the entire JSON object into a JsonElement
            JsonElement element = JsonSerializer.Deserialize<JsonElement>(ref reader);

            // Get the "Type" property to determine which subclass to instantiate
            if (!element.TryGetProperty("Type", out JsonElement typeElement) || string.IsNullOrEmpty(typeElement.GetString()))
            {
                throw new JsonException("Condition JSON must contain a 'Type' property.");
            }

            string conditionType = typeElement.GetString();

            // Determine the target type based on the "Type" value
            Type targetType;
            switch (conditionType)
            {
                case "Elimination":
                    targetType = typeof(EliminationCondition);
                    break;
                case "Fetch":
                    targetType = typeof(FetchCondition);
                    break;
                case "Interaction":
                    targetType = typeof(InteractionCondition);
                    break;
                default:
                    throw new JsonException($"Unknown condition type: {conditionType}");
            }

            // Deserialize the JsonElement into the correct subclass
            return (Condition)JsonSerializer.Deserialize(element.GetRawText(), targetType, options);
        }

        public override void Write(Utf8JsonWriter writer, Condition value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteString("TrackingCaption", value.TrackingCaption);
            writer.WriteNumber("SequenceIndex", value.SequenceIndex);
            writer.WriteBoolean("CanBeAutoCompleted", value.CanBeAutoCompleted);
            writer.WriteString("Type", value.Type);

            if (value is EliminationCondition elimination)
            {
                writer.WriteNumber("Amount", elimination.Amount);

                // Only write TargetCharacters if it contains items
                if (elimination.TargetCharacters != null && elimination.TargetCharacters.Count > 0)
                {
                    writer.WriteStartArray("TargetCharacters");
                    foreach (var item in elimination.TargetCharacters) writer.WriteStringValue(item);
                    writer.WriteEndArray();
                }

                // Only write AllowedWeapons if it contains items
                if (elimination.AllowedWeapons != null && elimination.AllowedWeapons.Count > 0)
                {
                    writer.WriteStartArray("AllowedWeapons");
                    foreach (var item in elimination.AllowedWeapons) writer.WriteStringValue(item);
                    writer.WriteEndArray();
                }
            }
            else if (value is FetchCondition fetch)
            {
                writer.WriteBoolean("PlayerKeepsItems", fetch.PlayerKeepsItems);
                writer.WriteBoolean("DisablePurchase", fetch.DisablePurchase);

                if (fetch.RequiredItems != null && fetch.RequiredItems.Count > 0)
                {
                    writer.WriteStartArray("RequiredItems");
                    foreach (var item in fetch.RequiredItems)
                    {
                        writer.WriteStartObject();
                        writer.WriteStartArray("AcceptedItems");
                        foreach (var acceptedItem in item.AcceptedItems) writer.WriteStringValue(acceptedItem);
                        writer.WriteEndArray();
                        writer.WriteNumber("RequiredNum", item.RequiredNum);

                        if (item.MinAcceptedItemUses > 0) writer.WriteNumber("MinAcceptedItemUses", item.MinAcceptedItemUses);
                        if (item.MinAcceptedItemMass > 0) writer.WriteNumber("MinAcceptedItemMass", item.MinAcceptedItemMass);
                        if (item.MinAcceptedItemHealth > 0) writer.WriteNumber("MinAcceptedItemHealth", item.MinAcceptedItemHealth);
                        if (!string.IsNullOrEmpty(item.MinAcceptedCookLevel)) writer.WriteString("MinAcceptedCookLevel", item.MinAcceptedCookLevel);
                        if (!string.IsNullOrEmpty(item.MaxAcceptedCookLevel)) writer.WriteString("MaxAcceptedCookLevel", item.MaxAcceptedCookLevel);
                        if (!string.IsNullOrEmpty(item.MinAcceptedCookQuality)) writer.WriteString("MinAcceptedCookQuality", item.MinAcceptedCookQuality);
                        if (item.MinAcceptedItemResourceRatio > 0) writer.WriteNumber("MinAcceptedItemResourceRatio", item.MinAcceptedItemResourceRatio);
                        if (item.MinAcceptedItemResourceAmount > 0) writer.WriteNumber("MinAcceptedItemResourceAmount", item.MinAcceptedItemResourceAmount);
                        if (item.RandomAdditionalRequiredNum > 0) writer.WriteNumber("RandomAdditionalRequiredNum", item.RandomAdditionalRequiredNum);

                        writer.WriteEndObject();
                    }
                    writer.WriteEndArray();
                }
            }
            else if (value is InteractionCondition interaction)
            {
                writer.WriteBoolean("SpawnOnlyNeeded", interaction.SpawnOnlyNeeded);
                writer.WriteNumber("MinNeeded", interaction.MinNeeded);
                writer.WriteNumber("MaxNeeded", interaction.MaxNeeded);
                writer.WriteNumber("WorldMarkerShowDistance", interaction.WorldMarkerShowDistance);

                if (interaction.Locations != null && interaction.Locations.Count > 0)
                {
                    writer.WriteStartArray("Locations");
                    foreach (var loc in interaction.Locations)
                    {
                        writer.WriteStartObject();
                        writer.WriteString("AnchorMesh", loc.AnchorMesh);
                        writer.WriteString("FallbackTransform", loc.FallbackTransform);
                        writer.WriteString("VisibleMesh", loc.VisibleMesh);
                        writer.WriteNumber("Instance", loc.Instance);
                        writer.WriteEndObject();
                    }
                    writer.WriteEndArray();
                }
            }

            // Write Map Locations
            if (value.LocationsShownOnMap != null && value.LocationsShownOnMap.Count > 0)
            {
                writer.WriteStartArray("LocationsShownOnMap");
                foreach (var loc in value.LocationsShownOnMap)
                {
                    writer.WriteStartObject();
                    writer.WriteStartObject("Location");
                    writer.WriteNumber("X", loc.Location.X);
                    writer.WriteNumber("Y", loc.Location.Y);
                    writer.WriteNumber("Z", loc.Location.Z);
                    writer.WriteEndObject();
                    writer.WriteNumber("SizeFactor", loc.SizeFactor);
                    writer.WriteEndObject();
                }
                writer.WriteEndArray();
            }

            writer.WriteEndObject();
        }
    }

    public class AddSkillRewardDialog : Window
    {
        public ComboBox? CbSkill;
        public TextBox? TxtExperience;
        public bool IsOkClicked => DialogResult == true;
        public string SelectedSkill => CbSkill?.Text ?? "";

        public double Experience
        {
            get
            {
                string text = TxtExperience?.Text ?? "0";
                if (double.TryParse(text, out double exp)) return exp;
                return 0;
            }
        }

        public AddSkillRewardDialog(SkillReward? existingReward = null)
        {
            Title = existingReward != null ? "Edit Skill Reward" : "Add Skill Reward";
            Width = 350;
            Height = 180;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            ResizeMode = ResizeMode.NoResize;

            Grid grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(250) });

            Label lblSkill = new Label { Content = "Skill:", HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 5, 10, 5) };
            CbSkill = new ComboBox { Margin = new Thickness(10, 5, 10, 5), HorizontalAlignment = HorizontalAlignment.Stretch };
            List<string> skills = new List<string>
            {
                "Archery", "Aviation", "Awareness", "Boxing", "Camouflage", "Cooking", "Demolition",
                "Driving", "Endurance", "Engineering", "Farming", "Handgun", "Medical", "MeleeWeapons",
                "Motorcycle", "Rifles", "Running", "Sniping", "Stealth", "Survival", "Tactics", "Thievery"
            };
            foreach (var skill in skills) CbSkill.Items.Add(skill);

            Label lblExp = new Label { Content = "Experience:", HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 5, 10, 5) };
            TxtExperience = new TextBox { Margin = new Thickness(10, 5, 10, 5), TextAlignment = TextAlignment.Left };
            TxtExperience.PreviewTextInput += NumericPreviewTextInput;
            DataObject.AddPastingHandler(TxtExperience, new DataObjectPastingEventHandler(NumericPasting));

            Button btnOk = new Button { Content = "OK", Width = 75, Height = 25, Margin = new Thickness(10, 10, 5, 10), HorizontalAlignment = HorizontalAlignment.Right };
            Button btnCancel = new Button { Content = "Cancel", Width = 75, Height = 25, Margin = new Thickness(5, 10, 10, 10), HorizontalAlignment = HorizontalAlignment.Right };
            btnOk.Click += (s, e) => { DialogResult = true; Close(); };
            btnCancel.Click += (s, e) => { DialogResult = false; Close(); };

            StackPanel btnPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
            btnPanel.Children.Add(btnOk); btnPanel.Children.Add(btnCancel);

            Grid.SetRow(lblSkill, 0); Grid.SetColumn(lblSkill, 0);
            Grid.SetRow(CbSkill, 0); Grid.SetColumn(CbSkill, 1);
            grid.Children.Add(lblSkill); grid.Children.Add(CbSkill);

            Grid.SetRow(lblExp, 1); Grid.SetColumn(lblExp, 0);
            Grid.SetRow(TxtExperience, 1); Grid.SetColumn(TxtExperience, 1);
            grid.Children.Add(lblExp); grid.Children.Add(TxtExperience);

            Grid.SetRow(btnPanel, 2); Grid.SetColumn(btnPanel, 0); Grid.SetColumnSpan(btnPanel, 2);
            grid.Children.Add(btnPanel);
            Content = grid;

            if (existingReward != null)
            {
                CbSkill.Text = existingReward.Skill;
                TxtExperience.Text = existingReward.Experience.ToString();
            }
            else
            {
                CbSkill.SelectedItem = skills[0];
                TxtExperience.Text = "1000";
            }
        }

        private void NumericPreviewTextInput(object sender, TextCompositionEventArgs e) { e.Handled = !Regex.IsMatch(e.Text, @"^\d*$"); }
        private void NumericPasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string text = (string)e.DataObject.GetData(typeof(string));
                e.Handled = !Regex.IsMatch(text, @"^\d*$");
            }
            else { e.Handled = true; }
        }
    }

    public class AddTradeDealDialog : Window
    {
        public static List<string> AvailableItems { get; set; } = new List<string> { "Empty Item" };
        public TextBox? TxtSearch;
        public ListBox? LstItems;
        public TextBox? TxtPrice;
        public TextBox? TxtAmount;
        public TextBox? TxtFame;
        public CheckBox? ChkAllowExcluded;
        public bool IsOkClicked => DialogResult == true;
        public string SelectedItemName => LstItems?.SelectedItem?.ToString() ?? "";

        public double Price { get => double.TryParse(TxtPrice?.Text ?? "0", out double val) ? val : 0; }
        public int Amount { get => int.TryParse(TxtAmount?.Text ?? "1", out int val) ? val : 1; }
        public double Fame { get => double.TryParse(TxtFame?.Text ?? "0", out double val) ? val : 0; }
        public bool AllowExcluded => ChkAllowExcluded?.IsChecked ?? false;

        // Added parameter to support editing existing rewards
        public AddTradeDealDialog(TradeDealReward? existingReward = null)
        {
            Title = existingReward != null ? "Edit Trade Deal" : "Add Trade Deal";
            Width = 500;
            Height = 480;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            ResizeMode = ResizeMode.NoResize;

            Grid grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(350) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(200) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            TxtSearch = new TextBox { Text = "Type to search items", Margin = new Thickness(10), Padding = new Thickness(5) };
            TxtSearch.TextChanged += (s, e) => FilterItems();
            Grid.SetRow(TxtSearch, 0); Grid.SetColumnSpan(TxtSearch, 2);
            grid.Children.Add(TxtSearch);

            LstItems = new ListBox { Margin = new Thickness(10), HorizontalContentAlignment = HorizontalAlignment.Stretch };
            foreach (var item in AvailableItems) LstItems.Items.Add(item);
            if (LstItems.Items.Count > 0) LstItems.SelectedIndex = 0;
            Grid.SetRow(LstItems, 1); Grid.SetColumnSpan(LstItems, 2);
            grid.Children.Add(LstItems);

            Label lblPrice = new Label { Content = "Price:", HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 5, 10, 5) };
            TxtPrice = new TextBox { Text = "50", Margin = new Thickness(10, 5, 10, 5), TextAlignment = TextAlignment.Left };
            TxtPrice.PreviewTextInput += NumericPreviewTextInput;
            Grid.SetRow(lblPrice, 2); Grid.SetColumn(lblPrice, 0);
            Grid.SetRow(TxtPrice, 2); Grid.SetColumn(TxtPrice, 1);
            grid.Children.Add(lblPrice); grid.Children.Add(TxtPrice);

            Label lblAmount = new Label { Content = "Amount:", HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 5, 10, 5) };
            TxtAmount = new TextBox { Text = "1", Margin = new Thickness(10, 5, 10, 5), TextAlignment = TextAlignment.Left };
            TxtAmount.PreviewTextInput += NumericPreviewTextInput;
            Grid.SetRow(lblAmount, 3); Grid.SetColumn(lblAmount, 0);
            Grid.SetRow(TxtAmount, 3); Grid.SetColumn(TxtAmount, 1);
            grid.Children.Add(lblAmount); grid.Children.Add(TxtAmount);

            Label lblFame = new Label { Content = "Fame Required:", HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 5, 10, 5) };
            TxtFame = new TextBox { Text = "25", Margin = new Thickness(10, 5, 10, 5), TextAlignment = TextAlignment.Left };
            TxtFame.PreviewTextInput += NumericPreviewTextInput;
            Grid.SetRow(lblFame, 4); Grid.SetColumn(lblFame, 0);
            Grid.SetRow(TxtFame, 4); Grid.SetColumn(TxtFame, 1);
            grid.Children.Add(lblFame); grid.Children.Add(TxtFame);

            ChkAllowExcluded = new CheckBox { Content = "Allow Excluded", IsChecked = true, Margin = new Thickness(10, 10, 0, 10), HorizontalAlignment = HorizontalAlignment.Left };
            Grid.SetRow(ChkAllowExcluded, 5); Grid.SetColumnSpan(ChkAllowExcluded, 2);
            grid.Children.Add(ChkAllowExcluded);

            Button btnOk = new Button { Content = "OK", Width = 75, Height = 25, Margin = new Thickness(0, 10, 10, 10), HorizontalAlignment = HorizontalAlignment.Right };
            Button btnCancel = new Button { Content = "Cancel", Width = 75, Height = 25, Margin = new Thickness(0, 10, 10, 10), HorizontalAlignment = HorizontalAlignment.Right };
            btnOk.Click += (s, e) => { DialogResult = true; Close(); };
            btnCancel.Click += (s, e) => { DialogResult = false; Close(); };

            StackPanel btnPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
            btnPanel.Children.Add(btnOk); btnPanel.Children.Add(btnCancel);
            Grid.SetRow(btnPanel, 6); Grid.SetColumn(btnPanel, 1);
            grid.Children.Add(btnPanel);
            Content = grid;

            // Pre-fill UI if editing an existing reward
            if (existingReward != null)
            {
                TxtSearch.Text = "";
                FilterItems();
                if (LstItems.Items.Contains(existingReward.Item))
                {
                    LstItems.SelectedItem = existingReward.Item;
                }
                TxtPrice.Text = existingReward.Price.ToString();
                TxtAmount.Text = existingReward.Amount.ToString();
                TxtFame.Text = existingReward.Fame.ToString();
                ChkAllowExcluded.IsChecked = existingReward.AllowExcluded;
            }
        }

        private void FilterItems()
        {
            if (TxtSearch == null || LstItems == null) return;
            string filter = TxtSearch.Text.ToLower();
            LstItems.Items.Clear();
            foreach (var item in AvailableItems)
            {
                if (item != null && item.ToLower().Contains(filter)) LstItems.Items.Add(item);
            }
            if (LstItems.Items.Count > 0) LstItems.SelectedIndex = 0;
        }

        private void NumericPreviewTextInput(object sender, TextCompositionEventArgs e) { e.Handled = !Regex.IsMatch(e.Text, @"^\d*$"); }
    }

    public partial class MainWindow : Window
    {
        public static List<string> TradeItems { get; private set; } = new List<string> { "Default Item" };
        public static List<string> FetchItems { get; private set; } = new List<string>();
        public const int MaxKillAmount = 1000000000;

        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            LoadTradeItems();
            LoadFetchItems();
            AddTradeDealDialog.AvailableItems = TradeItems;
            UpdateJsonPreview();
            InitConditions();
        }

        private void LoadTradeItems()
        {
            try
            {
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TradeItems.txt");
                if (File.Exists(path))
                {
                    TradeItems = File.ReadAllLines(path).Where(line => !string.IsNullOrEmpty(line)).ToList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading TradeItems.txt: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadFetchItems()
        {
            try
            {
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FetchItems.txt");
                if (File.Exists(path))
                {
                    FetchItems = File.ReadAllLines(path).Where(line => !string.IsNullOrEmpty(line)).ToList();
                }
                else
                {
                    // Fallback if file doesn't exist
                    FetchItems.Add("05_Teeth_Necklace");
                    FetchItems.Add("12_Gauge_Birdshot");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading FetchItems.txt: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MenuItem_New_Click(object sender, RoutedEventArgs e)
        {
            // Create a new quest
            CurrentTradeDeal = new TradeDeal
            {
                AssociatedNpc = "Armorer",
                Tier = 1,
                Title = "New Quest",
                Description = "Quest description...",
                TimeLimitHours = 0.5,
                RewardPool = new List<RewardPool> { new RewardPool() },
                Conditions = new List<Condition>()
            };

            UpdateControlsFromQuest(CurrentTradeDeal);
            UpdateJsonPreview();
        }

        private void MenuItem_Open_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog
                {
                    Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                    Title = "Open Quest File",
                    DefaultExt = "json",
                    RestoreDirectory = true
                };

                bool? result = openFileDialog.ShowDialog();
                if (result == true)
                {
                    string filePath = openFileDialog.FileName;
                    string jsonContent = File.ReadAllText(filePath);

                    // Deserialize the JSON into our data structure
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        Converters = { new ConditionConverter() }
                    };

                    TradeDeal loadedQuest = JsonSerializer.Deserialize<TradeDeal>(jsonContent, options);

                    if (loadedQuest == null)
                    {
                        MessageBox.Show("Failed to load quest file. The file may be corrupted or invalid.");
                        return;
                    }

                    // Update all controls with the loaded data
                    UpdateControlsFromQuest(loadedQuest);

                    // Update the JSON preview window with the loaded JSON
                    TxtJson.Text = jsonContent;

                    // Update the current trade deal reference to the loaded quest
                    CurrentTradeDeal = loadedQuest;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading quest file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MenuItem_Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                    Title = "Save Quest File",
                    DefaultExt = "json",
                    FileName = $"{TxtTitle.Text?.Replace(" ", "_") ?? "quest"}.json"
                };

                bool? result = saveFileDialog.ShowDialog();
                if (result == true)
                {
                    UpdateJsonPreview(); // Ensure all UI data is reflected in the current state

                    var options = new JsonSerializerOptions
                    {
                        WriteIndented = true,
                        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                        Converters = { new ConditionConverter() }
                    };

                    string json = JsonSerializer.Serialize(CurrentTradeDeal, options);
                    File.WriteAllText(saveFileDialog.FileName, json);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving quest file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateControlsFromQuest(TradeDeal quest)
        {
            // Basic Info Tab
            // Update NPC selection
            if (CbNpc != null)
            {
                bool npcFound = false;
                foreach (ComboBoxItem item in CbNpc.Items)
                {
                    if (item.Content?.ToString() == quest.AssociatedNpc)
                    {
                        CbNpc.SelectedItem = item;
                        npcFound = true;
                        break;
                    }
                }

                // If NPC wasn't found in the list, add it (for compatibility with future NPC types)
                if (!npcFound && quest.AssociatedNpc != null)
                {
                    ComboBoxItem newItem = new ComboBoxItem { Content = quest.AssociatedNpc };
                    CbNpc.Items.Add(newItem);
                    CbNpc.SelectedItem = newItem;
                }
            }

            if (TxtTitle != null) TxtTitle.Text = quest.Title;
            if (TxtTier != null) TxtTier.Text = quest.Tier.ToString();
            if (TxtDescription != null) TxtDescription.Text = quest.Description;
            if (TxtTimeLimit != null) TxtTimeLimit.Text = quest.TimeLimitHours.ToString("0.0#");

            // Rewards Tab
            if (quest.RewardPool != null && quest.RewardPool.Count > 0)
            {
                RewardPool reward = quest.RewardPool[0];

                if (TxtNormalReward != null) TxtNormalReward.Text = reward.CurrencyNormal.ToString();
                if (TxtGoldReward != null) TxtGoldReward.Text = reward.CurrencyGold.ToString();
                if (TxtFameReward != null) TxtFameReward.Text = reward.Fame.ToString();

                // Update Skills
                if (LvSkills != null)
                {
                    LvSkills.ItemsSource = null;
                    if (reward.Skills != null && reward.Skills.Count > 0)
                    {
                        LvSkills.ItemsSource = reward.Skills;
                    }
                    else
                    {
                        LvSkills.ItemsSource = null;
                    }
                }

                // Update Trade Deals
                if (LvTradeDeals != null)
                {
                    LvTradeDeals.ItemsSource = null;
                    if (reward.TradeDeals != null && reward.TradeDeals.Count > 0)
                    {
                        LvTradeDeals.ItemsSource = reward.TradeDeals;
                    }
                    else
                    {
                        LvTradeDeals.ItemsSource = null;
                    }
                }
            }

            // Conditions Tab
            if (ConditionsList != null)
            {
                ConditionsList.Clear();
                if (quest.Conditions != null)
                {
                    foreach (var condition in quest.Conditions)
                    {
                        ConditionsList.Add(condition);
                    }
                }
            }

            // Update the total rewards display
            RewardPool currentReward = GetOrCreateCurrentReward();
            int totalRewards = CalculateTotalRewards(currentReward);
            if (TxtTotalRewards != null) TxtTotalRewards.Text = $"Total Rewards: {totalRewards}/5";
        }

        private void TxtInput_TextChanged(object sender, TextChangedEventArgs e) { UpdateJsonPreview(); }
        private void CbNpc_SelectionChanged(object sender, SelectionChangedEventArgs e) { UpdateJsonPreview(); }

        private void BtnAddSkillReward_Click(object sender, RoutedEventArgs e)
        {
            RewardPool reward = GetOrCreateCurrentReward();
            if (reward.Skills == null) reward.Skills = new List<SkillReward>();
            if (CalculateTotalRewards(reward) == 5)
            {
                MessageBox.Show("Maximum 5 total reward points allowed.");
                return;
            }

            AddSkillRewardDialog dialog = new AddSkillRewardDialog();
            dialog.Owner = this;
            if (dialog.ShowDialog() == true)
            {
                reward.Skills.Add(new SkillReward { Skill = dialog.SelectedSkill, Experience = dialog.Experience });
                RefreshSkillListView();
                UpdateJsonPreview();
            }
        }

        private void LvSkills_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            RewardPool reward = GetOrCreateCurrentReward();
            if (reward.Skills == null || reward.Skills.Count == 0) return;

            if (LvSkills.SelectedItem is SkillReward selectedSkill)
            {
                AddSkillRewardDialog dialog = new AddSkillRewardDialog(selectedSkill);
                dialog.Owner = this;
                if (dialog.ShowDialog() == true)
                {
                    selectedSkill.Skill = dialog.SelectedSkill;
                    selectedSkill.Experience = dialog.Experience;
                    RefreshSkillListView();
                    UpdateJsonPreview();
                }
            }
        }

        private void BtnRemoveSelectedSkill_Click(object sender, RoutedEventArgs e)
        {
            RewardPool reward = GetOrCreateCurrentReward();
            if (reward.Skills == null) return;
            if (LvSkills.SelectedItem is SkillReward selectedSkill)
            {
                reward.Skills.Remove(selectedSkill);
                if (reward.Skills.Count == 0) reward.Skills = null;
                RefreshSkillListView();
                UpdateJsonPreview();
            }
        }

        private void BtnAddTradeDeal_Click(object sender, RoutedEventArgs e)
        {
            RewardPool reward = GetOrCreateCurrentReward();
            if (reward.TradeDeals == null) reward.TradeDeals = new List<TradeDealReward>();

            int currentTotal = CalculateTotalRewards(reward);
            int pointsToAdd = (reward.TradeDeals.Count == 0) ? 2 : 1;

            if (currentTotal + pointsToAdd > 5)
            {
                MessageBox.Show("Maximum 5 total reward points allowed.\n(Reminder: The first trade deal = 2 reward points.)");
                return;
            }

            var dialog = new AddTradeDealDialog();
            dialog.Owner = this;
            if (dialog.ShowDialog() == true)
            {
                reward.TradeDeals.Add(new TradeDealReward
                {
                    Item = dialog.SelectedItemName,
                    Price = dialog.Price,
                    Amount = dialog.Amount,
                    Fame = dialog.Fame,
                    AllowExcluded = dialog.AllowExcluded
                });
                RefreshTradeDealListView();
                UpdateJsonPreview();
            }
        }

        private void LvTradeDeals_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            RewardPool reward = GetOrCreateCurrentReward();
            if (reward.TradeDeals == null || reward.TradeDeals.Count == 0) return;

            if (LvTradeDeals.SelectedItem is TradeDealReward selectedDeal)
            {
                AddTradeDealDialog dialog = new AddTradeDealDialog(selectedDeal);
                dialog.Owner = this;
                if (dialog.ShowDialog() == true)
                {
                    selectedDeal.Item = dialog.SelectedItemName;
                    selectedDeal.Price = dialog.Price;
                    selectedDeal.Amount = dialog.Amount;
                    selectedDeal.Fame = dialog.Fame;
                    selectedDeal.AllowExcluded = dialog.AllowExcluded;
                    RefreshTradeDealListView();
                    UpdateJsonPreview();
                }
            }
        }

        private void BtnRemoveSelectedTradeDeal_Click(object sender, RoutedEventArgs e)
        {
            RewardPool reward = GetOrCreateCurrentReward();
            if (reward.TradeDeals == null) return;
            if (LvTradeDeals.SelectedItem is TradeDealReward selectedDeal)
            {
                reward.TradeDeals.Remove(selectedDeal);
                if (reward.TradeDeals.Count == 0) reward.TradeDeals = null;
                RefreshTradeDealListView();
                UpdateJsonPreview();
            }
        }

        private RewardPool GetOrCreateCurrentReward()
        {
            if (CurrentTradeDeal.RewardPool == null) CurrentTradeDeal.RewardPool = new List<RewardPool>();
            if (CurrentTradeDeal.RewardPool.Count == 0) CurrentTradeDeal.RewardPool.Add(new RewardPool());
            return CurrentTradeDeal.RewardPool[0];
        }

        private void RefreshSkillListView()
        {
            RewardPool reward = GetOrCreateCurrentReward();
            LvSkills.ItemsSource = null;
            LvSkills.ItemsSource = reward.Skills;
        }

        private void RefreshTradeDealListView()
        {
            RewardPool reward = GetOrCreateCurrentReward();
            LvTradeDeals.ItemsSource = null;
            LvTradeDeals.ItemsSource = reward.TradeDeals;
        }

        private TradeDeal CurrentTradeDeal { get; set; } = new TradeDeal();

        private int CalculateTotalRewards(RewardPool reward)
        {
            int total = 0;
            if (reward.CurrencyNormal > 0 || reward.CurrencyGold > 0 || reward.Fame > 0) total += 1;
            if (reward.Skills != null && reward.Skills.Count > 0) total += reward.Skills.Count;
            if (reward.TradeDeals != null && reward.TradeDeals.Count > 0)
            {
                total += 2;
                if (reward.TradeDeals.Count > 1) total += (reward.TradeDeals.Count - 1);
            }
            return total;
        }

        private void UpdateJsonPreview()
        {
            try
            {
                // Update basic info
                string npc = "Armorer";
                if (CbNpc?.SelectedItem is ComboBoxItem selectedItem) npc = selectedItem.Content?.ToString() ?? "Armorer";

                int tier = 1;
                if (!int.TryParse(TxtTier?.Text ?? "1", out tier)) tier = 1;

                string title = TxtTitle?.Text ?? "New Quest";
                string description = TxtDescription?.Text ?? "Quest description...";
                double timeLimit = 0.5;
                if (!double.TryParse(TxtTimeLimit?.Text ?? "0.5", out timeLimit)) timeLimit = 0.5;

                CurrentTradeDeal.AssociatedNpc = npc;
                CurrentTradeDeal.Tier = tier;
                CurrentTradeDeal.Title = title;
                CurrentTradeDeal.Description = description;
                CurrentTradeDeal.TimeLimitHours = timeLimit;

                // Update reward pool
                RewardPool reward = GetOrCreateCurrentReward();

                int currencyNormal = 0;
                int.TryParse(TxtNormalReward?.Text ?? "0", out currencyNormal);
                reward.CurrencyNormal = currencyNormal;

                int currencyGold = 0;
                int.TryParse(TxtGoldReward?.Text ?? "0", out currencyGold);
                reward.CurrencyGold = currencyGold;

                int fame = 0;
                int.TryParse(TxtFameReward?.Text ?? "0", out fame);
                reward.Fame = fame;

                // Calculate and display total rewards
                int totalRewards = CalculateTotalRewards(reward);
                if (TxtTotalRewards != null) TxtTotalRewards.Text = $"Total Rewards: {totalRewards}/5";

                // Update conditions
                List<Condition> serializedConditions = new List<Condition>();
                foreach (var condition in ConditionsList)
                {
                    if (condition.Type.Equals("Elimination", StringComparison.OrdinalIgnoreCase))
                    {
                        if (condition is EliminationCondition eliminationCondition)
                        {
                            serializedConditions.Add(eliminationCondition);
                        }
                        else
                        {
                            var newEliminationCondition = new EliminationCondition
                            {
                                TrackingCaption = condition.TrackingCaption,
                                SequenceIndex = condition.SequenceIndex,
                                CanBeAutoCompleted = condition.CanBeAutoCompleted,
                                Amount = 1,
                                TargetCharacters = new List<string>(),
                                AllowedWeapons = new List<string>(),
                                LocationsShownOnMap = condition.LocationsShownOnMap
                            };
                            serializedConditions.Add(newEliminationCondition);
                        }
                    }
                    else if (condition.Type.Equals("Fetch", StringComparison.OrdinalIgnoreCase))
                    {
                        var fetchCondition = new FetchCondition
                        {
                            TrackingCaption = condition.TrackingCaption,
                            SequenceIndex = condition.SequenceIndex,
                            CanBeAutoCompleted = condition.CanBeAutoCompleted,
                            PlayerKeepsItems = (condition as FetchCondition)?.PlayerKeepsItems ?? false,
                            DisablePurchase = (condition as FetchCondition)?.DisablePurchase ?? true,
                            RequiredItems = (condition as FetchCondition)?.RequiredItems ?? new List<RequiredItem>(),
                            LocationsShownOnMap = condition.LocationsShownOnMap
                        };
                        serializedConditions.Add(fetchCondition);
                    }
                    else if (condition.Type.Equals("Interaction", StringComparison.OrdinalIgnoreCase))
                    {
                        InteractionCondition interactionCondition;
                        if (condition is InteractionCondition existingInteraction)
                        {
                            interactionCondition = existingInteraction;
                        }
                        else
                        {
                            interactionCondition = new InteractionCondition
                            {
                                TrackingCaption = condition.TrackingCaption,
                                SequenceIndex = condition.SequenceIndex,
                                CanBeAutoCompleted = condition.CanBeAutoCompleted,
                                SpawnOnlyNeeded = true,
                                MinNeeded = 1,
                                MaxNeeded = 1,
                                WorldMarkerShowDistance = 5,
                                Locations = new List<InteractionLocation>(),
                                LocationsShownOnMap = condition.LocationsShownOnMap
                            };
                        }
                        serializedConditions.Add(interactionCondition);
                    }
                    else
                    {
                        serializedConditions.Add(condition);
                    }
                }
                CurrentTradeDeal.Conditions = serializedConditions;

                // Serialize to JSON
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                    Converters = { new ConditionConverter() }
                };

                string json = JsonSerializer.Serialize(CurrentTradeDeal, options);
                if (TxtJson != null) TxtJson.Text = json;
            }
            catch (Exception ex)
            {
                if (TxtJson != null) TxtJson.Text = $"Error generating JSON: {ex.Message}";
            }
        }

        private void NumericPreviewTextInput(object sender, TextCompositionEventArgs e) { e.Handled = !Regex.IsMatch(e.Text, @"^\d*$"); }
        private void NumericPasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string text = (string)e.DataObject.GetData(typeof(string));
                e.Handled = !Regex.IsMatch(text, @"^\d*$");
            }
            else { e.Handled = true; }
        }

        private void FloatPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            string currentText = ((TextBox)sender).Text;
            if (currentText.Contains(".") && e.Text == ".") { e.Handled = true; return; }
            e.Handled = !Regex.IsMatch(e.Text, @"^\d*\.?\d*$");
        }

        private void FloatPasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string text = (string)e.DataObject.GetData(typeof(string));
                if (Regex.IsMatch(text, @"^\d*\.?\d*$"))
                {
                    string currentText = ((TextBox)sender).Text;
                    if (text.Contains(".") && currentText.Contains(".")) { e.Handled = true; }
                    else { e.Handled = false; }
                }
                else { e.Handled = true; }
            }
            else { e.Handled = true; }
        }

        private ObservableCollection<Condition> ConditionsList { get; set; } = new ObservableCollection<Condition>();

        private void InitConditions()
        {
            LvConditions.ItemsSource = ConditionsList;
            LvConditions.SelectionChanged += LvConditions_SelectionChanged;
            UpdateConditionTabsState(); // Ensure correct initial state
        }

        private void LvConditions_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateConditionTabsState();
            ClearConditionEditor();
            TabConditionEditor.SelectedIndex = 0;

            if (LvConditions.SelectedItem is Condition selectedCondition)
            {
                EdtCaption.Text = selectedCondition.TrackingCaption;
                BindingOperations.SetBinding(EdtCaption, TextBox.TextProperty,
                    new Binding("TrackingCaption") { Source = selectedCondition, Mode = BindingMode.TwoWay });

                EdtSequence.Text = selectedCondition.SequenceIndex.ToString();
                BindingOperations.SetBinding(EdtSequence, TextBox.TextProperty,
                    new Binding("SequenceIndex") { Source = selectedCondition, Mode = BindingMode.TwoWay });

                EdtAutoComplete.IsChecked = selectedCondition.CanBeAutoCompleted;
                BindingOperations.SetBinding(EdtAutoComplete, CheckBox.IsCheckedProperty,
                    new Binding("CanBeAutoCompleted") { Source = selectedCondition, Mode = BindingMode.TwoWay });

                string type = selectedCondition.Type.ToLower();

                // Tab enablement is now handled by UpdateConditionTabsState()

                if (type == "fetch")
                {
                    if (selectedCondition is FetchCondition fetchCondition)
                    {
                        ChkPlayerKeepsItems.IsChecked = fetchCondition.PlayerKeepsItems;
                        ChkDisablePurchase.IsChecked = fetchCondition.DisablePurchase;
                        LvCurrentRequiredItems.ItemsSource = fetchCondition.RequiredItems;
                    }
                }

                if (type == "interaction")
                {
                    if (selectedCondition is InteractionCondition interactionCondition)
                    {
                        ChkSpawnOnlyNeeded.IsChecked = interactionCondition.SpawnOnlyNeeded;
                        EdtMinNeeded.Text = interactionCondition.MinNeeded.ToString();
                        EdtMaxNeeded.Text = interactionCondition.MaxNeeded.ToString();
                        EdtMarkerDistance.Text = interactionCondition.WorldMarkerShowDistance.ToString();
                    }
                    else
                    {
                        ChkSpawnOnlyNeeded.IsChecked = true;
                        EdtMinNeeded.Text = "1";
                        EdtMaxNeeded.Text = "1";
                        EdtMarkerDistance.Text = "5";
                    }
                }

                if (type == "elimination")
                {
                    if (selectedCondition is EliminationCondition eliminationCondition)
                    {
                        EdtKillAmount.Text = eliminationCondition.Amount.ToString();
                        BindingOperations.SetBinding(EdtKillAmount, TextBox.TextProperty,
                            new Binding("Amount") { Source = eliminationCondition, Mode = BindingMode.TwoWay });

                        EdtEliminationTargets.ItemsSource = eliminationCondition.TargetCharacters ?? new List<string>();
                        EdtAllowedWeapons.ItemsSource = eliminationCondition.AllowedWeapons ?? new List<string>();
                    }
                }

                // Update Map Locations List View
                TabMapLocations.ItemsSource = selectedCondition.LocationsShownOnMap;
            }
            else
            {
                TabMapLocations.ItemsSource = null;
                LvCurrentRequiredItems.ItemsSource = null;
            }
        }

        private void UpdateConditionTabsState()
        {
            bool hasSelection = LvConditions.SelectedItem is Condition;

            // Base state: disable all property tabs if nothing is selected
            TabFetchProperties.IsEnabled = hasSelection;
            TabInteractionProperties.IsEnabled = hasSelection;
            TabEliminationProperties.IsEnabled = hasSelection;
            TabMapLocationsItem.IsEnabled = hasSelection;

            // Refine based on condition type if something is selected
            if (hasSelection && LvConditions.SelectedItem is Condition selectedCondition)
            {
                string type = selectedCondition.Type.ToLower();
                TabFetchProperties.IsEnabled = type == "fetch";
                TabInteractionProperties.IsEnabled = type == "interaction";
                TabEliminationProperties.IsEnabled = type == "elimination";
            }
        }

        private void LoadConditionToEditor(Condition condition)
        {
            EdtCaption.Text = condition.TrackingCaption;
            EdtSequence.Text = condition.SequenceIndex.ToString();
            EdtAutoComplete.IsChecked = condition.CanBeAutoCompleted;

            string type = condition.Type.ToLower();

            TabFetchProperties.IsEnabled = type == "fetch";
            if (TabFetchProperties.IsEnabled)
            {
                if (condition is FetchCondition fetchCondition)
                {
                    ChkPlayerKeepsItems.IsChecked = fetchCondition.PlayerKeepsItems;
                    ChkDisablePurchase.IsChecked = fetchCondition.DisablePurchase;
                    LvCurrentRequiredItems.ItemsSource = fetchCondition.RequiredItems;
                }
            }

            TabInteractionProperties.IsEnabled = type == "interaction";
            if (TabInteractionProperties.IsEnabled)
            {
                if (condition is InteractionCondition interactionCondition)
                {
                    ChkSpawnOnlyNeeded.IsChecked = interactionCondition.SpawnOnlyNeeded;
                    EdtMinNeeded.Text = interactionCondition.MinNeeded.ToString();
                    EdtMaxNeeded.Text = interactionCondition.MaxNeeded.ToString();
                    EdtMarkerDistance.Text = interactionCondition.WorldMarkerShowDistance.ToString();
                }
            }

            TabEliminationProperties.IsEnabled = type == "elimination";
            if (TabEliminationProperties.IsEnabled && condition is EliminationCondition eliminationCondition)
            {
                EdtEliminationTargets.ItemsSource = eliminationCondition.TargetCharacters ?? new List<string>();
                EdtAllowedWeapons.ItemsSource = eliminationCondition.AllowedWeapons ?? new List<string>();
                EdtKillAmount.Text = eliminationCondition.Amount.ToString();
            }
        }

        private void ClearConditionEditor()
        {
            BindingOperations.ClearBinding(EdtCaption, TextBox.TextProperty);
            BindingOperations.ClearBinding(EdtSequence, TextBox.TextProperty);
            BindingOperations.ClearBinding(EdtAutoComplete, CheckBox.IsCheckedProperty);
            BindingOperations.ClearBinding(ChkPlayerKeepsItems, CheckBox.IsCheckedProperty);
            BindingOperations.ClearBinding(ChkDisablePurchase, CheckBox.IsCheckedProperty);
            BindingOperations.ClearBinding(EdtKillAmount, TextBox.TextProperty);

            BindingOperations.ClearBinding(ChkSpawnOnlyNeeded, CheckBox.IsCheckedProperty);
            BindingOperations.ClearBinding(EdtMinNeeded, TextBox.TextProperty);
            BindingOperations.ClearBinding(EdtMaxNeeded, TextBox.TextProperty);
            BindingOperations.ClearBinding(EdtMarkerDistance, TextBox.TextProperty);

            EdtCaption.Text = "";
            EdtSequence.Text = "0";
            EdtAutoComplete.IsChecked = false;
            ChkPlayerKeepsItems.IsChecked = false;
            ChkDisablePurchase.IsChecked = true;
            EdtKillAmount.Text = "1";

            ChkSpawnOnlyNeeded.IsChecked = true;
            EdtMinNeeded.Text = "1";
            EdtMaxNeeded.Text = "1";
            EdtMarkerDistance.Text = "5";

            EdtEliminationTargets.ItemsSource = null;
            EdtAllowedWeapons.ItemsSource = null;
            LvCurrentRequiredItems.ItemsSource = null;
        }

        private void TabConditionEditor_SelectionChanged(object sender, SelectionChangedEventArgs e) { }

        private void BtnSaveCondition_Click(object sender, RoutedEventArgs e) { SaveConditionFromEditor(null, null); }

        private void SaveConditionFromEditor(object? sender, RoutedEventArgs? e)
        {
            if (LvConditions.SelectedItem is Condition currentCondition)
            {
                if (currentCondition.Type.Equals("Fetch", StringComparison.OrdinalIgnoreCase) && currentCondition is FetchCondition fetch)
                {
                    fetch.PlayerKeepsItems = ChkPlayerKeepsItems.IsChecked ?? false;
                    fetch.DisablePurchase = ChkDisablePurchase.IsChecked ?? true;
                }
                else if (currentCondition.Type.Equals("Interaction", StringComparison.OrdinalIgnoreCase) && currentCondition is InteractionCondition interaction)
                {
                    interaction.SpawnOnlyNeeded = ChkSpawnOnlyNeeded.IsChecked ?? true;
                    int.TryParse(EdtMinNeeded.Text, out int min);
                    interaction.MinNeeded = min;
                    int.TryParse(EdtMaxNeeded.Text, out int max);
                    interaction.MaxNeeded = max;
                    int.TryParse(EdtMarkerDistance.Text, out int dist);
                    interaction.WorldMarkerShowDistance = dist;
                }
                UpdateJsonPreview();
            }
        }

        public void AddCondition(Condition newCondition)
        {
            ConditionsList.Add(newCondition);
            LvConditions.SelectedItem = newCondition;
            LvConditions.ScrollIntoView(newCondition);
            UpdateJsonPreview();
        }

        private void BtnAddElimination_Click(object sender, RoutedEventArgs e)
        {
            var condition = new EliminationCondition
            {
                TrackingCaption = "Eliminate Target",
                SequenceIndex = ConditionsList.Count,
                CanBeAutoCompleted = false,
                Amount = 1,
                TargetCharacters = new List<string>(),
                AllowedWeapons = new List<string>(),
                LocationsShownOnMap = new List<MapLocation>()
            };
            AddCondition(condition);
        }

        private void BtnAddFetchCondition_Click(object sender, RoutedEventArgs e)
        {
            var condition = new FetchCondition
            {
                TrackingCaption = "Fetch Item",
                SequenceIndex = ConditionsList.Count,
                CanBeAutoCompleted = false,
                PlayerKeepsItems = false,
                DisablePurchase = true,
                RequiredItems = new List<RequiredItem>(),
                LocationsShownOnMap = new List<MapLocation>()
            };
            AddCondition(condition);
        }

        private void BtnAddInteraction_Click(object sender, RoutedEventArgs e)
        {
            var condition = new InteractionCondition
            {
                TrackingCaption = "Interact",
                SequenceIndex = ConditionsList.Count,
                CanBeAutoCompleted = false,
                SpawnOnlyNeeded = true,
                MinNeeded = 1,
                MaxNeeded = 1,
                WorldMarkerShowDistance = 5,
                Locations = new List<InteractionLocation>(),
                LocationsShownOnMap = new List<MapLocation>()
            };
            AddCondition(condition);
        }

        private void BtnRemoveSelectedCondition_Click(object sender, RoutedEventArgs e)
        {
            if (LvConditions.SelectedItem is Condition selectedCondition)
            {
                ConditionsList.Remove(selectedCondition);
                UpdateJsonPreview();
            }
            else
            {
                MessageBox.Show("Please select a condition to remove.");
            }
        }

        private void BtnEditEliminationTargets_Click(object sender, RoutedEventArgs e)
        {
            if (LvConditions.SelectedItem is EliminationCondition currentCondition)
            {
                var dialog = new EditTargetCharactersDialog(currentCondition.TargetCharacters);
                dialog.Owner = this;
                if (dialog.ShowDialog() == true)
                {
                    currentCondition.TargetCharacters = dialog.SelectedItems.Count == 0 ? new List<string>() : dialog.SelectedItems;
                    EdtEliminationTargets.ItemsSource = null;
                    EdtEliminationTargets.ItemsSource = currentCondition.TargetCharacters;
                    UpdateJsonPreview();
                }
            }
        }

        private void BtnEditEliminationWeapons_Click(object sender, RoutedEventArgs e)
        {
            if (LvConditions.SelectedItem is EliminationCondition currentCondition)
            {
                var dialog = new EditWeaponDialog(currentCondition.AllowedWeapons);
                dialog.Owner = this;
                if (dialog.ShowDialog() == true)
                {
                    currentCondition.AllowedWeapons = dialog.SelectedItems.Count == 0 ? new List<string>() : dialog.SelectedItems;
                    EdtAllowedWeapons.ItemsSource = null;
                    EdtAllowedWeapons.ItemsSource = currentCondition.AllowedWeapons;
                    UpdateJsonPreview();
                }
            }
        }

        private void EdtKillAmount_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                if (long.TryParse(textBox.Text, out long value))
                {
                    if (value > MaxKillAmount)
                    {
                        MessageBox.Show(
                            $"Amount cannot exceed {MaxKillAmount:N0}. The value has been clamped to {MaxKillAmount:N0}.",
                            "Limit Exceeded",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
                        textBox.Text = MaxKillAmount.ToString();
                        textBox.CaretIndex = textBox.Text.Length;
                    }
                    else if (value < 0)
                    {
                        MessageBox.Show("Amount cannot be negative.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Error);
                        textBox.Text = "0";
                        textBox.CaretIndex = textBox.Text.Length;
                    }
                    else
                    {
                        if (LvConditions.SelectedItem is EliminationCondition currentCondition)
                        {
                            currentCondition.Amount = (int)value;
                        }
                    }
                }
                else if (!string.IsNullOrEmpty(textBox.Text))
                {
                    textBox.Text = "0";
                    MessageBox.Show("Please enter a valid number.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnEditMapLocations_Click(object sender, RoutedEventArgs e)
        {
            if (LvConditions.SelectedItem is Condition currentCondition)
            {
                var dialog = new EditMapLocationsDialog(currentCondition.LocationsShownOnMap);
                dialog.Owner = this;
                if (dialog.ShowDialog() == true)
                {
                    // Fix: Convert ObservableCollection to List to match property type
                    currentCondition.LocationsShownOnMap = dialog.MapLocations.ToList();
                    // Refresh the list view in the main tab so the changes are visible immediately
                    TabMapLocations.ItemsSource = currentCondition.LocationsShownOnMap;
                    UpdateJsonPreview();
                }
            }
            else
            {
                MessageBox.Show("Please select a condition to edit map locations for.");
            }
        }

        private void BtnEditInteractionLocations_Click(object sender, RoutedEventArgs e)
        {
            if (LvConditions.SelectedItem is InteractionCondition currentCondition)
            {
                var dialog = new EditInteractionLocationsDialog(currentCondition.Locations);
                dialog.Owner = this;
                if (dialog.ShowDialog() == true)
                {
                    currentCondition.Locations = dialog.Locations.ToList();
                    UpdateJsonPreview();
                }
            }
            else
            {
                MessageBox.Show("Please select an Interaction condition to edit locations for.");
            }
        }

        private void BtnEditRequiredItems_Click(object sender, RoutedEventArgs e)
        {
            if (LvConditions.SelectedItem is FetchCondition currentCondition)
            {
                var dialog = new EditRequiredItemsDialog(currentCondition.RequiredItems, FetchItems);
                dialog.Owner = this;
                if (dialog.ShowDialog() == true)
                {
                    currentCondition.RequiredItems = dialog.RequiredItems.ToList();
                    LvCurrentRequiredItems.ItemsSource = currentCondition.RequiredItems;
                    UpdateJsonPreview();
                }
            }
        }

    }

    public class EditTargetCharactersDialog : Window
    {
        public ListBox LstTargetTypes = new ListBox();
        public List<string> SelectedItems { get; private set; } = new List<string>();
        private List<string> AvailableTypes { get; set; } = new List<string>();
        private List<string> InitialSelections { get; set; } = new List<string>();

        public EditTargetCharactersDialog(List<string>? initialSelections = null)
        {
            InitialSelections = initialSelections ?? new List<string>();
            Title = "Edit Target Characters";
            Width = 300;
            Height = 400;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            ResizeMode = ResizeMode.NoResize;

            try
            {
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "EliminationTargets.txt");
                if (File.Exists(path))
                {
                    AvailableTypes = File.ReadAllLines(path).Where(line => !string.IsNullOrEmpty(line)).ToList();
                }
                else
                {
                    AvailableTypes.Add("DefaultTarget");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading targets: {ex.Message}");
                AvailableTypes.Add("DefaultTarget");
            }

            DataTemplate checkboxTemplate = new DataTemplate();
            FrameworkElementFactory checkBoxFactory = new FrameworkElementFactory(typeof(CheckBox));
            checkBoxFactory.SetValue(CheckBox.ContentProperty, new Binding("."));
            Binding selectionBinding = new Binding("IsSelected");
            selectionBinding.RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(ListBoxItem), 1);
            selectionBinding.Mode = BindingMode.TwoWay;
            checkBoxFactory.SetBinding(CheckBox.IsCheckedProperty, selectionBinding);
            checkboxTemplate.VisualTree = checkBoxFactory;

            LstTargetTypes.ItemTemplate = checkboxTemplate;
            LstTargetTypes.ItemsSource = AvailableTypes;
            LstTargetTypes.SelectionMode = SelectionMode.Multiple;

            foreach (var item in AvailableTypes)
            {
                if (InitialSelections.Contains(item)) LstTargetTypes.SelectedItems.Add(item);
            }

            Grid grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            Grid.SetRow(LstTargetTypes, 0);
            grid.Children.Add(LstTargetTypes);

            StackPanel btnPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 10, 10, 10) };
            Button btnOk = new Button { Content = "OK", Width = 75, Height = 25, Margin = new Thickness(0, 0, 10, 0) };
            Button btnCancel = new Button { Content = "Cancel", Width = 75, Height = 25 };

            btnOk.Click += (s, e) => {
                SelectedItems = new List<string>();
                foreach (var item in LstTargetTypes.SelectedItems) SelectedItems.Add(item.ToString());
                DialogResult = true; Close();
            };
            btnCancel.Click += (s, e) => { DialogResult = false; Close(); };

            btnPanel.Children.Add(btnOk); btnPanel.Children.Add(btnCancel);
            Grid.SetRow(btnPanel, 1);
            grid.Children.Add(btnPanel);
            Content = grid;
        }
    }

    public class EditWeaponDialog : Window
    {
        public ListBox LstWeapons = new ListBox();
        public List<string> SelectedItems { get; private set; } = new List<string>();
        private List<string> AvailableWeapons { get; set; } = new List<string>();
        private List<string> InitialSelections { get; set; } = new List<string>();

        public EditWeaponDialog(List<string>? initialSelections = null)
        {
            InitialSelections = initialSelections ?? new List<string>();
            Title = "Edit Allowed Weapons";
            Width = 300;
            Height = 400;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            ResizeMode = ResizeMode.NoResize;

            try
            {
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "EliminationWeapons.txt");
                if (File.Exists(path))
                {
                    AvailableWeapons = File.ReadAllLines(path).Where(line => !string.IsNullOrEmpty(line)).ToList();
                }
                else
                {
                    AvailableWeapons.Add("DefaultWeapon");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading weapons: {ex.Message}");
                AvailableWeapons.Add("DefaultWeapon");
            }

            DataTemplate checkboxTemplate = new DataTemplate();
            FrameworkElementFactory checkBoxFactory = new FrameworkElementFactory(typeof(CheckBox));
            checkBoxFactory.SetValue(CheckBox.ContentProperty, new Binding("."));
            Binding selectionBinding = new Binding("IsSelected");
            selectionBinding.RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(ListBoxItem), 1);
            selectionBinding.Mode = BindingMode.TwoWay;
            checkBoxFactory.SetBinding(CheckBox.IsCheckedProperty, selectionBinding);
            checkboxTemplate.VisualTree = checkBoxFactory;

            LstWeapons.ItemTemplate = checkboxTemplate;
            LstWeapons.ItemsSource = AvailableWeapons;
            LstWeapons.SelectionMode = SelectionMode.Multiple;

            foreach (var item in AvailableWeapons)
            {
                if (InitialSelections.Contains(item)) LstWeapons.SelectedItems.Add(item);
            }

            Grid grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            Grid.SetRow(LstWeapons, 0);
            grid.Children.Add(LstWeapons);

            StackPanel btnPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 10, 10, 10) };
            Button btnOk = new Button { Content = "OK", Width = 75, Height = 25, Margin = new Thickness(0, 0, 10, 0) };
            Button btnCancel = new Button { Content = "Cancel", Width = 75, Height = 25 };

            btnOk.Click += (s, e) => {
                SelectedItems = new List<string>();
                foreach (var item in LstWeapons.SelectedItems) SelectedItems.Add(item.ToString());
                DialogResult = true; Close();
            };
            btnCancel.Click += (s, e) => { DialogResult = false; Close(); };

            btnPanel.Children.Add(btnOk); btnPanel.Children.Add(btnCancel);
            Grid.SetRow(btnPanel, 1);
            grid.Children.Add(btnPanel);
            Content = grid;
        }
    }

    public class EditMapLocationsDialog : Window
    {
        public ObservableCollection<MapLocation> MapLocations { get; set; } = new ObservableCollection<MapLocation>();
        public ListView LocationList { get; private set; }
        public TextBox TxtLocationString { get; private set; }
        public TextBox TxtX { get; private set; }
        public TextBox TxtY { get; private set; }
        public TextBox TxtZ { get; private set; }
        public TextBox TxtSizeFactor { get; private set; }

        public EditMapLocationsDialog(List<MapLocation> initialLocations)
        {
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

            Title = "Edit Map Locations";
            Width = 600;
            Height = 500;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            ResizeMode = ResizeMode.CanResize;

            Grid mainGrid = new Grid();
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }); // List
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Input String
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Manual Input Label
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Manual Input Controls
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Action Buttons
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // OK/Cancel

            // 1. ListView
            LocationList = new ListView
            {
                Margin = new Thickness(5),
                BorderBrush = Brushes.Gray,
                BorderThickness = new Thickness(1)
            };
            LocationList.View = new GridView
            {
                Columns =
                {
                    new GridViewColumn { Header = "X", Width = 80, DisplayMemberBinding = new Binding("Location.X") },
                    new GridViewColumn { Header = "Y", Width = 80, DisplayMemberBinding = new Binding("Location.Y") },
                    new GridViewColumn { Header = "Z", Width = 80, DisplayMemberBinding = new Binding("Location.Z") },
                    new GridViewColumn { Header = "Size Factor", Width = 80, DisplayMemberBinding = new Binding("SizeFactor") },
                    new GridViewColumn { Header = "Format", Width = 150, DisplayMemberBinding = new Binding("FormatString") }
                }
            };
            LocationList.ItemsSource = MapLocations;
            LocationList.SelectionMode = SelectionMode.Single;
            Grid.SetRow(LocationList, 0);
            mainGrid.Children.Add(LocationList);

            // 2. Location String Input
            Grid stringGrid = new Grid { Margin = new Thickness(10, 5, 10, 5) };
            stringGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            stringGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            Label lblString = new Label { Content = "Location String (copy from game):", HorizontalAlignment = HorizontalAlignment.Left };
            TxtLocationString = new TextBox { Margin = new Thickness(0, 5, 0, 5), AcceptsReturn = true, TextWrapping = TextWrapping.Wrap };
            Button btnParse = new Button { Content = "Parse", Width = 75, Margin = new Thickness(10, 0, 0, 0) };
            btnParse.Click += BtnParse_Click;

            Grid.SetColumn(lblString, 0); Grid.SetColumn(btnParse, 1);
            Grid.SetColumnSpan(lblString, 2);
            Grid.SetRow(lblString, 0);
            Grid.SetRow(TxtLocationString, 1);
            Grid.SetRow(btnParse, 1);

            stringGrid.Children.Add(lblString);
            stringGrid.Children.Add(TxtLocationString);
            stringGrid.Children.Add(btnParse);
            Grid.SetRow(stringGrid, 1);
            mainGrid.Children.Add(stringGrid);

            // 3. "Or enter coordinates manually:" Label
            Label lblManual = new Label { Content = "Or enter coordinates manually:", Margin = new Thickness(10, 10, 0, 5) };
            Grid.SetRow(lblManual, 2);
            mainGrid.Children.Add(lblManual);

            // 4. Manual Input Controls - Redesigned with proper alignment
            Grid manualGrid = new Grid { Margin = new Thickness(10, 0, 10, 10) };
            manualGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // Labels & Inputs
            manualGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // Size Factor & Info
            manualGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Row 0: X
            manualGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Row 1: Y
            manualGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Row 2: Z
            manualGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Row 3: Size Factor
            manualGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Row 4: Info text

            // X Coordinate
            Label lblX = new Label { Content = "X Coordinate:", HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 5, 10, 5) };
            TxtX = new TextBox { Text = "0.0000", Margin = new Thickness(0, 5, 10, 5), Width = 100, TextAlignment = TextAlignment.Right };
            TxtX.PreviewTextInput += FloatPreviewTextInput;
            Grid.SetRow(lblX, 0); Grid.SetColumn(lblX, 0);
            Grid.SetRow(TxtX, 0); Grid.SetColumn(TxtX, 1);
            manualGrid.Children.Add(lblX); manualGrid.Children.Add(TxtX);

            // Y Coordinate
            Label lblY = new Label { Content = "Y Coordinate:", HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 5, 10, 5) };
            TxtY = new TextBox { Text = "0.0000", Margin = new Thickness(0, 5, 10, 5), Width = 100, TextAlignment = TextAlignment.Right };
            TxtY.PreviewTextInput += FloatPreviewTextInput;
            Grid.SetRow(lblY, 1); Grid.SetColumn(lblY, 0);
            Grid.SetRow(TxtY, 1); Grid.SetColumn(TxtY, 1);
            manualGrid.Children.Add(lblY); manualGrid.Children.Add(TxtY);

            // Z Coordinate
            Label lblZ = new Label { Content = "Z Coordinate:", HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 5, 10, 5) };
            TxtZ = new TextBox { Text = "0.0000", Margin = new Thickness(0, 5, 10, 5), Width = 100, TextAlignment = TextAlignment.Right };
            TxtZ.PreviewTextInput += FloatPreviewTextInput;
            Grid.SetRow(lblZ, 2); Grid.SetColumn(lblZ, 0);
            Grid.SetRow(TxtZ, 2); Grid.SetColumn(TxtZ, 1);
            manualGrid.Children.Add(lblZ); manualGrid.Children.Add(TxtZ);

            // Size Factor Label & Input
            Label lblSize = new Label { Content = "Size Factor:", HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 5, 10, 5) };
            TxtSizeFactor = new TextBox { Text = "1.00", Margin = new Thickness(0, 5, 10, 5), Width = 100, TextAlignment = TextAlignment.Right };
            TxtSizeFactor.PreviewTextInput += FloatPreviewTextInput;
            Grid.SetRow(lblSize, 3); Grid.SetColumn(lblSize, 0);
            Grid.SetRow(TxtSizeFactor, 3); Grid.SetColumn(TxtSizeFactor, 1);
            manualGrid.Children.Add(lblSize); manualGrid.Children.Add(TxtSizeFactor);

            // Info Text
            TextBlock txtSizeInfo = new TextBlock
            {
                Text = "Size Factor determines the radius of the circle shown on the game map.\nA value of 1.0 is approximately 300m diameter.",
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(10, 5, 0, 0),
                Foreground = Brushes.Gray,
                FontSize = 10,
                VerticalAlignment = VerticalAlignment.Top
            };
            Grid.SetRow(txtSizeInfo, 4); Grid.SetColumn(txtSizeInfo, 0); Grid.SetColumnSpan(txtSizeInfo, 2);
            manualGrid.Children.Add(txtSizeInfo);

            Grid.SetRow(manualGrid, 3);
            mainGrid.Children.Add(manualGrid);

            // 5. Action Buttons (Add/Update, Edit Selected, Remove Selected)
            StackPanel actionPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(10, 0, 10, 5) };
            Button btnAddUpdate = new Button { Content = "Add/Update", Width = 100, Margin = new Thickness(0, 0, 5, 0) };
            Button btnEditSelected = new Button { Content = "Edit Selected", Width = 100, Margin = new Thickness(0, 0, 5, 0) };
            Button btnRemoveSelected = new Button { Content = "Remove Selected", Width = 100 };

            btnAddUpdate.Click += BtnAddUpdate_Click;
            btnEditSelected.Click += BtnEditSelected_Click;
            btnRemoveSelected.Click += BtnRemoveSelected_Click;

            actionPanel.Children.Add(btnAddUpdate);
            actionPanel.Children.Add(btnEditSelected);
            actionPanel.Children.Add(btnRemoveSelected);
            Grid.SetRow(actionPanel, 4);
            mainGrid.Children.Add(actionPanel);

            // 6. OK/Cancel Buttons
            StackPanel okCancelPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(10, 5, 10, 10) };
            Button btnOk = new Button { Content = "OK", Width = 75, Height = 25, Margin = new Thickness(0, 0, 10, 0) };
            Button btnCancel = new Button { Content = "Cancel", Width = 75, Height = 25 };

            btnOk.Click += (s, e) => { DialogResult = true; Close(); };
            btnCancel.Click += (s, e) => { DialogResult = false; Close(); };

            okCancelPanel.Children.Add(btnOk);
            okCancelPanel.Children.Add(btnCancel);
            Grid.SetRow(okCancelPanel, 5);
            mainGrid.Children.Add(okCancelPanel);

            Content = mainGrid;
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
            // Regex to capture X, Y, Z from {X=... Y=... Z=...|...}
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
                    // Update existing
                    selectedLoc.Location.X = x;
                    selectedLoc.Location.Y = y;
                    selectedLoc.Location.Z = z;
                    selectedLoc.SizeFactor = sizeFactor;
                }
                else
                {
                    // Add new
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
    }

    public class EditInteractionLocationsDialog : Window
    {
        public ObservableCollection<InteractionLocation> Locations { get; set; } = new ObservableCollection<InteractionLocation>();
        public ListView LocationList { get; private set; }
        public TextBox TxtMeshInfoInput { get; private set; }

        public EditInteractionLocationsDialog(List<InteractionLocation> initialLocations)
        {
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

            Title = "Edit Interaction Locations";
            Width = 700;
            Height = 600;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            ResizeMode = ResizeMode.CanResize;

            Grid mainGrid = new Grid();
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }); // List
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Action Buttons
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Input Section
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // OK/Cancel

            // 1. ListView
            LocationList = new ListView
            {
                Margin = new Thickness(5),
                BorderBrush = Brushes.Gray,
                BorderThickness = new Thickness(1)
            };
            LocationList.View = new GridView
            {
                Columns =
                {
                    new GridViewColumn { Header = "Visible Mesh", Width = 200, DisplayMemberBinding = new Binding("VisibleMesh") },
                    new GridViewColumn { Header = "Fallback Transform", Width = 200, DisplayMemberBinding = new Binding("FallbackTransform") },
                    new GridViewColumn { Header = "Has Anchor", Width = 80, DisplayMemberBinding = new Binding("HasAnchor") },
                    new GridViewColumn { Header = "Instance", Width = 60, DisplayMemberBinding = new Binding("Instance") }
                }
            };
            LocationList.ItemsSource = Locations;
            LocationList.SelectionMode = SelectionMode.Single;
            Grid.SetRow(LocationList, 0);
            mainGrid.Children.Add(LocationList);

            // 2. Action Buttons
            StackPanel actionPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(10, 5, 10, 5) };
            Button btnAdd = new Button { Content = "Add Location", Width = 100, Margin = new Thickness(0, 0, 5, 0) };
            Button btnEdit = new Button { Content = "Edit Selected", Width = 100, Margin = new Thickness(0, 0, 5, 0) };
            Button btnRemove = new Button { Content = "Remove Selected", Width = 100 };

            btnAdd.Click += BtnAddLocation_Click;
            btnEdit.Click += BtnEditSelectedLocation_Click;
            btnRemove.Click += BtnRemoveSelectedLocation_Click;

            actionPanel.Children.Add(btnAdd);
            actionPanel.Children.Add(btnEdit);
            actionPanel.Children.Add(btnRemove);
            Grid.SetRow(actionPanel, 1);
            mainGrid.Children.Add(actionPanel);

            // 3. Input Section
            Grid inputGrid = new Grid { Margin = new Thickness(10, 5, 10, 5) };
            inputGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Label
            inputGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }); // TextBox
            inputGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Help Text
            inputGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Apply/Cancel

            Label lblInput = new Label { Content = "#GetMeshInfo Output:", Margin = new Thickness(0, 0, 0, 5) };
            TxtMeshInfoInput = new TextBox
            {
                Margin = new Thickness(0, 5, 0, 5),
                AcceptsReturn = true,
                TextWrapping = TextWrapping.Wrap,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Height = 100
            };

            TextBlock txtHelp = new TextBlock
            {
                Text = "Paste the complete output from the #GetMeshInfo command.\nLook at an object in-game and type #GetMeshInfo to get this data.\nThe output is automatically copied to your clipboard when the command is executed.",
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 5, 0, 5),
                Foreground = Brushes.Gray,
                FontSize = 10
            };

            StackPanel btnPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
            Button btnApply = new Button { Content = "Apply", Width = 75, Margin = new Thickness(0, 0, 10, 0) };
            Button btnCancelInput = new Button { Content = "Cancel", Width = 75 };

            btnApply.Click += (s, e) => {
                // Parse the JSON input and add to the list
                ParseMeshInfoInput();
            };
            btnCancelInput.Click += (s, e) => {
                // Do nothing, just close the dialog
            };

            btnPanel.Children.Add(btnApply);
            btnPanel.Children.Add(btnCancelInput);

            Grid.SetRow(lblInput, 0);
            Grid.SetRow(TxtMeshInfoInput, 1);
            Grid.SetRow(txtHelp, 2);
            Grid.SetRow(btnPanel, 3);

            inputGrid.Children.Add(lblInput);
            inputGrid.Children.Add(TxtMeshInfoInput);
            inputGrid.Children.Add(txtHelp);
            inputGrid.Children.Add(btnPanel);

            Grid.SetRow(inputGrid, 2);
            mainGrid.Children.Add(inputGrid);

            // 4. OK/Cancel Buttons
            StackPanel okCancelPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(10, 10, 10, 10) };
            Button btnOk = new Button { Content = "OK", Width = 75, Height = 25, Margin = new Thickness(0, 0, 10, 0) };
            Button btnCancel = new Button { Content = "Cancel", Width = 75, Height = 25 };

            btnOk.Click += (s, e) => { DialogResult = true; Close(); };
            btnCancel.Click += (s, e) => { DialogResult = false; Close(); };

            okCancelPanel.Children.Add(btnOk);
            okCancelPanel.Children.Add(btnCancel);
            Grid.SetRow(okCancelPanel, 3);
            mainGrid.Children.Add(okCancelPanel);

            Content = mainGrid;
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
                        // Update existing selected item
                        selectedLoc.AnchorMesh = location.AnchorMesh;
                        selectedLoc.FallbackTransform = location.FallbackTransform;
                        selectedLoc.VisibleMesh = location.VisibleMesh;
                        selectedLoc.Instance = location.Instance;
                    }
                    else
                    {
                        // Add new item
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
    }

    public class EditRequiredItemsDialog : Window
    {
        public ObservableCollection<RequiredItem> RequiredItems { get; set; } = new ObservableCollection<RequiredItem>();
        public ListView LvRequiredItems { get; private set; }
        public ListBox LstAcceptedItems { get; private set; }
        public TextBox TxtSearch { get; private set; }
        public TextBox TxtRequiredQty { get; private set; }
        public TextBox TxtRandomQty { get; private set; }

        // Item Condition
        public CheckBox ChkMinHealth { get; private set; }
        public TextBox TxtMinHealth { get; private set; }
        public CheckBox ChkMinUses { get; private set; }
        public TextBox TxtMinUses { get; private set; }
        public CheckBox ChkMinMass { get; private set; }
        public TextBox TxtMinMass { get; private set; }

        // Cooking Properties
        public CheckBox ChkMinCookLevel { get; private set; }
        public ComboBox CmbMinCookLevel { get; private set; }
        public CheckBox ChkMaxCookLevel { get; private set; }
        public ComboBox CmbMaxCookLevel { get; private set; }
        public CheckBox ChkMinCookQuality { get; private set; }
        public ComboBox CmbMinCookQuality { get; private set; }

        // Resource Properties
        public CheckBox ChkMinResourcePct { get; private set; }
        public TextBox TxtMinResourcePct { get; private set; }
        public CheckBox ChkMinResourceMl { get; private set; }
        public TextBox TxtMinResourceMl { get; private set; }

        public List<string> AvailableItems { get; set; } = new List<string>();

        public EditRequiredItemsDialog(List<RequiredItem> initialItems, List<string> fetchItems)
        {
            // FIX: Copy data from initialItems instead of creating empty items
            foreach (var item in initialItems)
            {
                RequiredItems.Add(new RequiredItem
                {
                    AcceptedItems = new List<string>(item.AcceptedItems),
                    RequiredNum = item.RequiredNum,
                    MinAcceptedItemUses = item.MinAcceptedItemUses,
                    MinAcceptedItemMass = item.MinAcceptedItemMass,
                    MinAcceptedItemHealth = item.MinAcceptedItemHealth,
                    MinAcceptedCookLevel = item.MinAcceptedCookLevel,
                    MaxAcceptedCookLevel = item.MaxAcceptedCookLevel,
                    MinAcceptedCookQuality = item.MinAcceptedCookQuality,
                    MinAcceptedItemResourceRatio = item.MinAcceptedItemResourceRatio,
                    MinAcceptedItemResourceAmount = item.MinAcceptedItemResourceAmount,
                    RandomAdditionalRequiredNum = item.RandomAdditionalRequiredNum
                });
            }

            // Ensure we have items to display
            AvailableItems = fetchItems ?? new List<string>();
            if (AvailableItems.Count == 0)
            {
                AvailableItems.Add("Empty Item");
            }

            Title = "Edit Required Items";
            Width = 850;
            Height = 700;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            ResizeMode = ResizeMode.NoResize;

            Grid mainGrid = new Grid();
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }); // Top List
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Buttons
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }); // Input Area
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // OK/Cancel

            // 1. Top ListView (Current Required Items)
            LvRequiredItems = new ListView
            {
                Margin = new Thickness(5),
                BorderBrush = Brushes.Gray,
                BorderThickness = new Thickness(1)
            };
            LvRequiredItems.View = new GridView
            {
                Columns =
                {
                    // FIX: Bind to the new display property
                    new GridViewColumn { Header = "Accepted Items", Width = 250, DisplayMemberBinding = new Binding("AcceptedItemsDisplay") },
                    new GridViewColumn { Header = "Required Num", Width = 100, DisplayMemberBinding = new Binding("RequiredNum") }
                }
            };
            LvRequiredItems.ItemsSource = RequiredItems;
            LvRequiredItems.SelectionMode = SelectionMode.Single;
            Grid.SetRow(LvRequiredItems, 0);
            mainGrid.Children.Add(LvRequiredItems);

            // 2. Buttons (Add, Edit, Remove)
            StackPanel btnPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(5, 5, 5, 5) };
            Button btnAdd = new Button { Content = "Add Required Item", Width = 130, Margin = new Thickness(0, 0, 5, 0) };
            Button btnEdit = new Button { Content = "Edit Selected", Width = 100, Margin = new Thickness(0, 0, 5, 0) };
            Button btnRemove = new Button { Content = "Remove Selected", Width = 120 };

            btnAdd.Click += BtnAddRequiredItem_Click;
            btnEdit.Click += BtnEditSelectedRequiredItem_Click;
            btnRemove.Click += BtnRemoveSelectedRequiredItem_Click;

            btnPanel.Children.Add(btnAdd);
            btnPanel.Children.Add(btnEdit);
            btnPanel.Children.Add(btnRemove);
            Grid.SetRow(btnPanel, 1);
            mainGrid.Children.Add(btnPanel);

            // 3. Input Area (Bottom Section) - Redesigned with Column-based StackPanels
            Grid inputGrid = new Grid { Margin = new Thickness(5, 5, 5, 5) };
            inputGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(220) }); // Col 0: List
            inputGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(250) }); // Col 1: Inputs
            inputGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // Col 2: Properties

            // Left Column: List (Takes full height using a nested Grid with * row)
            Grid leftGrid = new Grid();
            leftGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            leftGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            Label lblLeft = new Label { Content = "Accepted Items:", Margin = new Thickness(0, 0, 0, 5) };
            Grid.SetRow(lblLeft, 0);
            leftGrid.Children.Add(lblLeft);

            ListBox lstLeft = new ListBox { HorizontalContentAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Stretch };
            lstLeft.ItemTemplate = CreateCheckboxTemplate();
            lstLeft.SelectionMode = SelectionMode.Multiple;
            Grid.SetRow(lstLeft, 1);
            leftGrid.Children.Add(lstLeft);
            LstAcceptedItems = lstLeft;

            Grid.SetColumn(leftGrid, 0);
            Grid.SetRow(leftGrid, 0);
            inputGrid.Children.Add(leftGrid);

            // Middle Column: Inputs
            StackPanel middlePanel = new StackPanel { Margin = new Thickness(5, 0, 5, 0) };

            TxtSearch = new TextBox { Margin = new Thickness(0, 0, 0, 5), Height = 25, Text = "", Padding = new Thickness(5) };
            TxtSearch.TextChanged += (s, e) => FilterAvailableItems();
            ToolTipService.SetToolTip(TxtSearch, "Search items...");
            middlePanel.Children.Add(TxtSearch);

            StackPanel spRequired = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 5) };
            spRequired.Children.Add(new Label { Content = "Required:", HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 0, 5, 0) });
            TxtRequiredQty = new TextBox { Text = "1", Width = 60, Margin = new Thickness(0, 0, 0, 0) };
            TxtRequiredQty.PreviewTextInput += NumericPreviewTextInput;
            DataObject.AddPastingHandler(TxtRequiredQty, new DataObjectPastingEventHandler(NumericPasting));
            spRequired.Children.Add(TxtRequiredQty);
            middlePanel.Children.Add(spRequired);

            StackPanel spRandom = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 5) };
            spRandom.Children.Add(new Label { Content = "Random:", HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 0, 5, 0) });
            TxtRandomQty = new TextBox { Text = "0", Width = 60, Margin = new Thickness(0, 0, 0, 0) };
            TxtRandomQty.PreviewTextInput += NumericPreviewTextInput;
            DataObject.AddPastingHandler(TxtRandomQty, new DataObjectPastingEventHandler(NumericPasting));
            spRandom.Children.Add(TxtRandomQty);
            middlePanel.Children.Add(spRandom);

            middlePanel.Children.Add(new Label { Content = "Item Condition:", Margin = new Thickness(0, 10, 0, 5) });

            // Item Condition Grid
            Grid itemConditionGrid = new Grid { Margin = new Thickness(0, 0, 0, 5) };
            itemConditionGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // Checkbox Column
            itemConditionGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // Input Column
            itemConditionGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Health
            itemConditionGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Uses
            itemConditionGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Mass

            ChkMinHealth = new CheckBox { Content = "Min Health %", Margin = new Thickness(0, 2, 10, 2) };
            TxtMinHealth = new TextBox { Text = "50", Width = 60, Margin = new Thickness(10, 2, 0, 2) };
            TxtMinHealth.PreviewTextInput += NumericPreviewTextInput;
            Grid.SetRow(ChkMinHealth, 0); Grid.SetColumn(ChkMinHealth, 0);
            Grid.SetRow(TxtMinHealth, 0); Grid.SetColumn(TxtMinHealth, 1);
            itemConditionGrid.Children.Add(ChkMinHealth); itemConditionGrid.Children.Add(TxtMinHealth);

            ChkMinUses = new CheckBox { Content = "Min Uses", Margin = new Thickness(0, 2, 10, 2) };
            TxtMinUses = new TextBox { Text = "1", Width = 60, Margin = new Thickness(10, 2, 0, 2) };
            TxtMinUses.PreviewTextInput += NumericPreviewTextInput;
            Grid.SetRow(ChkMinUses, 1); Grid.SetColumn(ChkMinUses, 0);
            Grid.SetRow(TxtMinUses, 1); Grid.SetColumn(TxtMinUses, 1);
            itemConditionGrid.Children.Add(ChkMinUses); itemConditionGrid.Children.Add(TxtMinUses);

            ChkMinMass = new CheckBox { Content = "Min Mass (g)", Margin = new Thickness(0, 2, 10, 2) };
            TxtMinMass = new TextBox { Text = "100", Width = 60, Margin = new Thickness(10, 2, 0, 2) };
            TxtMinMass.PreviewTextInput += NumericPreviewTextInput;
            Grid.SetRow(ChkMinMass, 2); Grid.SetColumn(ChkMinMass, 0);
            Grid.SetRow(TxtMinMass, 2); Grid.SetColumn(TxtMinMass, 1);
            itemConditionGrid.Children.Add(ChkMinMass); itemConditionGrid.Children.Add(TxtMinMass);

            middlePanel.Children.Add(itemConditionGrid);

            StackPanel applyPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 10, 0, 0) };
            Button btnApply = new Button { Content = "Apply", Width = 75, Margin = new Thickness(0, 0, 10, 0) };
            Button btnCancelEdit = new Button { Content = "Cancel Edit", Width = 90 };
            btnApply.Click += (s, e) => ApplySelectedItem();
            btnCancelEdit.Click += (s, e) => { ClearInputFields(); FilterAvailableItems(); };
            applyPanel.Children.Add(btnApply);
            applyPanel.Children.Add(btnCancelEdit);
            middlePanel.Children.Add(applyPanel);

            Grid.SetColumn(middlePanel, 1);
            Grid.SetRow(middlePanel, 0);
            inputGrid.Children.Add(middlePanel);

            // Right Column: Properties
            StackPanel rightPanel = new StackPanel { Margin = new Thickness(5, 0, 0, 0) };
            rightPanel.Children.Add(new Label { Content = "Cooking Properties:", Margin = new Thickness(0, 0, 0, 5) });

            // Cooking Properties Grid
            Grid cookingGrid = new Grid { Margin = new Thickness(0, 0, 0, 5) };
            cookingGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // Checkbox
            cookingGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // Input
            cookingGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Min Cook Level
            cookingGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Max Cook Level
            cookingGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Min Cook Quality

            ChkMinCookLevel = new CheckBox { Content = "Min Cook Level", Margin = new Thickness(0, 2, 10, 2) };
            CmbMinCookLevel = new ComboBox { Width = 100, Margin = new Thickness(10, 2, 0, 2) };
            CmbMinCookLevel.Items.Add("Raw"); CmbMinCookLevel.Items.Add("Medium"); CmbMinCookLevel.Items.Add("Well Done");
            Grid.SetRow(ChkMinCookLevel, 0); Grid.SetColumn(ChkMinCookLevel, 0);
            Grid.SetRow(CmbMinCookLevel, 0); Grid.SetColumn(CmbMinCookLevel, 1);
            cookingGrid.Children.Add(ChkMinCookLevel); cookingGrid.Children.Add(CmbMinCookLevel);

            ChkMaxCookLevel = new CheckBox { Content = "Max Cook Level", Margin = new Thickness(0, 2, 10, 2) };
            CmbMaxCookLevel = new ComboBox { Width = 100, Margin = new Thickness(10, 2, 0, 2) };
            CmbMaxCookLevel.Items.Add("Raw"); CmbMaxCookLevel.Items.Add("Medium"); CmbMaxCookLevel.Items.Add("Well Done");
            Grid.SetRow(ChkMaxCookLevel, 1); Grid.SetColumn(ChkMaxCookLevel, 0);
            Grid.SetRow(CmbMaxCookLevel, 1); Grid.SetColumn(CmbMaxCookLevel, 1);
            cookingGrid.Children.Add(ChkMaxCookLevel); cookingGrid.Children.Add(CmbMaxCookLevel);

            ChkMinCookQuality = new CheckBox { Content = "Min Cook Quality", Margin = new Thickness(0, 2, 10, 2) };
            CmbMinCookQuality = new ComboBox { Width = 100, Margin = new Thickness(10, 2, 0, 2) };
            CmbMinCookQuality.Items.Add("Ruined"); CmbMinCookQuality.Items.Add("Bad"); CmbMinCookQuality.Items.Add("Average"); CmbMinCookQuality.Items.Add("Good"); CmbMinCookQuality.Items.Add("Delicious");
            Grid.SetRow(ChkMinCookQuality, 2); Grid.SetColumn(ChkMinCookQuality, 0);
            Grid.SetRow(CmbMinCookQuality, 2); Grid.SetColumn(CmbMinCookQuality, 1);
            cookingGrid.Children.Add(ChkMinCookQuality); cookingGrid.Children.Add(CmbMinCookQuality);

            rightPanel.Children.Add(cookingGrid);

            rightPanel.Children.Add(new Label { Content = "Resource Properties:", Margin = new Thickness(0, 10, 0, 5) });

            // Resource Properties Grid
            Grid resourceGrid = new Grid { Margin = new Thickness(0, 0, 0, 5) };
            resourceGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // Checkbox
            resourceGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // Input
            resourceGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Min Resource Pct
            resourceGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Min Resource Ml

            ChkMinResourcePct = new CheckBox { Content = "Min Resource (%)", Margin = new Thickness(0, 2, 10, 2) };
            TxtMinResourcePct = new TextBox { Text = "50", Width = 60, Margin = new Thickness(10, 2, 0, 2) };
            TxtMinResourcePct.PreviewTextInput += NumericPreviewTextInput;
            Grid.SetRow(ChkMinResourcePct, 0); Grid.SetColumn(ChkMinResourcePct, 0);
            Grid.SetRow(TxtMinResourcePct, 0); Grid.SetColumn(TxtMinResourcePct, 1);
            resourceGrid.Children.Add(ChkMinResourcePct); resourceGrid.Children.Add(TxtMinResourcePct);

            ChkMinResourceMl = new CheckBox { Content = "Min Resource (ml)", Margin = new Thickness(0, 2, 10, 2) };
            TxtMinResourceMl = new TextBox { Text = "100.0", Width = 60, Margin = new Thickness(10, 2, 0, 2) };
            TxtMinResourceMl.PreviewTextInput += NumericPreviewTextInput;
            Grid.SetRow(ChkMinResourceMl, 1); Grid.SetColumn(ChkMinResourceMl, 0);
            Grid.SetRow(TxtMinResourceMl, 1); Grid.SetColumn(TxtMinResourceMl, 1);
            resourceGrid.Children.Add(ChkMinResourceMl); resourceGrid.Children.Add(TxtMinResourceMl);

            rightPanel.Children.Add(resourceGrid);

            Grid.SetColumn(rightPanel, 2);
            Grid.SetRow(rightPanel, 0);
            inputGrid.Children.Add(rightPanel);

            Grid.SetRow(inputGrid, 2);
            mainGrid.Children.Add(inputGrid);

            // 4. OK/Cancel Buttons
            StackPanel okCancelPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(5, 5, 5, 5) };
            Button btnOk = new Button { Content = "OK", Width = 75, Height = 25, Margin = new Thickness(0, 0, 10, 0) };
            Button btnCancel = new Button { Content = "Cancel", Width = 75, Height = 25 };

            btnOk.Click += (s, e) => { DialogResult = true; Close(); };
            btnCancel.Click += (s, e) => { DialogResult = false; Close(); };

            okCancelPanel.Children.Add(btnOk);
            okCancelPanel.Children.Add(btnCancel);
            Grid.SetRow(okCancelPanel, 3);
            mainGrid.Children.Add(okCancelPanel);

            Content = mainGrid;
            FilterAvailableItems();
        }

        private DataTemplate CreateCheckboxTemplate()
        {
            DataTemplate checkboxTemplate = new DataTemplate();
            FrameworkElementFactory checkBoxFactory = new FrameworkElementFactory(typeof(CheckBox));
            checkBoxFactory.SetValue(CheckBox.ContentProperty, new Binding("."));
            Binding selectionBinding = new Binding("IsSelected");
            selectionBinding.RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(ListBoxItem), 1);
            selectionBinding.Mode = BindingMode.TwoWay;
            checkBoxFactory.SetBinding(CheckBox.IsCheckedProperty, selectionBinding);
            checkboxTemplate.VisualTree = checkBoxFactory;
            return checkboxTemplate;
        }

        private void FilterAvailableItems()
        {
            if (TxtSearch == null || LstAcceptedItems == null) return;
            string filter = TxtSearch.Text.ToLower();
            LstAcceptedItems.Items.Clear();
            foreach (var item in AvailableItems)
            {
                if (item != null && item.ToLower().Contains(filter))
                    LstAcceptedItems.Items.Add(item);
            }
            // Keep selection if possible, or clear if not found
            if (LstAcceptedItems.Items.Count > 0)
            {
                if (LstAcceptedItems.SelectedItems.Count > 0)
                {
                    // Try to re-select items if they still exist
                    List<object> keepSelection = new List<object>();
                    foreach (var sel in LstAcceptedItems.SelectedItems)
                    {
                        if (LstAcceptedItems.Items.Contains(sel)) keepSelection.Add(sel);
                    }
                    LstAcceptedItems.SelectedItems.Clear();
                    foreach (var sel in keepSelection) LstAcceptedItems.SelectedItems.Add(sel);
                }
                // Removed: else { LstAcceptedItems.SelectedIndex = 0; }
            }
        }

        private void ApplySelectedItem()
        {
            if (LstAcceptedItems.SelectedItems.Count == 0)
            {
                MessageBox.Show("Please select at least one item.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (LvRequiredItems.SelectedItem is RequiredItem selectedReqItem)
            {
                selectedReqItem.AcceptedItems.Clear();
                foreach (var item in LstAcceptedItems.SelectedItems)
                {
                    selectedReqItem.AcceptedItems.Add(item.ToString());
                }
                int.TryParse(TxtRequiredQty.Text, out int qty);
                selectedReqItem.RequiredNum = qty;

                selectedReqItem.MinAcceptedItemUses = ChkMinUses.IsChecked == true && int.TryParse(TxtMinUses.Text, out int minUses) ? minUses : 0;
                selectedReqItem.MinAcceptedItemMass = ChkMinMass.IsChecked == true && double.TryParse(TxtMinMass.Text, out double minMass) ? minMass : 0;
                selectedReqItem.MinAcceptedItemHealth = ChkMinHealth.IsChecked == true && double.TryParse(TxtMinHealth.Text, out double minHealth) ? minHealth : 0;

                selectedReqItem.MinAcceptedCookLevel = ChkMinCookLevel.IsChecked == true && CmbMinCookLevel.SelectedItem != null ? CmbMinCookLevel.SelectedItem.ToString() : null;
                selectedReqItem.MaxAcceptedCookLevel = ChkMaxCookLevel.IsChecked == true && CmbMaxCookLevel.SelectedItem != null ? CmbMaxCookLevel.SelectedItem.ToString() : null;
                selectedReqItem.MinAcceptedCookQuality = ChkMinCookQuality.IsChecked == true && CmbMinCookQuality.SelectedItem != null ? CmbMinCookQuality.SelectedItem.ToString() : null;

                selectedReqItem.MinAcceptedItemResourceRatio = ChkMinResourcePct.IsChecked == true && double.TryParse(TxtMinResourcePct.Text, out double minResPct) ? minResPct : 0;
                selectedReqItem.MinAcceptedItemResourceAmount = ChkMinResourceMl.IsChecked == true && double.TryParse(TxtMinResourceMl.Text, out double minResMl) ? minResMl : 0;

                int.TryParse(TxtRandomQty.Text, out int randomQty);
                selectedReqItem.RandomAdditionalRequiredNum = randomQty;
            }
            else
            {
                foreach (var item in LstAcceptedItems.SelectedItems)
                {
                    RequiredItems.Add(new RequiredItem
                    {
                        AcceptedItems = new List<string> { item.ToString() },
                        RequiredNum = int.TryParse(TxtRequiredQty.Text, out int q) ? q : 1,
                        MinAcceptedItemUses = ChkMinUses.IsChecked == true && int.TryParse(TxtMinUses.Text, out int minUses) ? minUses : 0,
                        MinAcceptedItemMass = ChkMinMass.IsChecked == true && double.TryParse(TxtMinMass.Text, out double minMass) ? minMass : 0,
                        MinAcceptedItemHealth = ChkMinHealth.IsChecked == true && double.TryParse(TxtMinHealth.Text, out double minHealth) ? minHealth : 0,
                        MinAcceptedCookLevel = ChkMinCookLevel.IsChecked == true && CmbMinCookLevel.SelectedItem != null ? CmbMinCookLevel.SelectedItem.ToString() : null,
                        MaxAcceptedCookLevel = ChkMaxCookLevel.IsChecked == true && CmbMaxCookLevel.SelectedItem != null ? CmbMaxCookLevel.SelectedItem.ToString() : null,
                        MinAcceptedCookQuality = ChkMinCookQuality.IsChecked == true && CmbMinCookQuality.SelectedItem != null ? CmbMinCookQuality.SelectedItem.ToString() : null,
                        MinAcceptedItemResourceRatio = ChkMinResourcePct.IsChecked == true && double.TryParse(TxtMinResourcePct.Text, out double minResPct) ? minResPct : 0,
                        MinAcceptedItemResourceAmount = ChkMinResourceMl.IsChecked == true && double.TryParse(TxtMinResourceMl.Text, out double minResMl) ? minResMl : 0,
                        RandomAdditionalRequiredNum = int.TryParse(TxtRandomQty.Text, out int rq) ? rq : 0
                    });
                }
            }

            LvRequiredItems.ItemsSource = null;
            LvRequiredItems.ItemsSource = RequiredItems;
            ClearInputFields();
            FilterAvailableItems();
        }

        private void ClearInputFields()
        {
            TxtRequiredQty.Text = "1";
            TxtRandomQty.Text = "0";
            TxtMinHealth.Text = "50";
            TxtMinUses.Text = "1";
            TxtMinMass.Text = "100";
            TxtMinResourcePct.Text = "50";
            TxtMinResourceMl.Text = "100.0";
            ChkMinHealth.IsChecked = false;
            ChkMinUses.IsChecked = false;
            ChkMinMass.IsChecked = false;
            ChkMinCookLevel.IsChecked = false;
            ChkMaxCookLevel.IsChecked = false;
            ChkMinCookQuality.IsChecked = false;
            ChkMinResourcePct.IsChecked = false;
            ChkMinResourceMl.IsChecked = false;
        }

        private void BtnAddRequiredItem_Click(object sender, RoutedEventArgs e)
        {
            if (LstAcceptedItems.SelectedItems.Count == 0)
            {
                MessageBox.Show("Please select an item from the list to add.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            RequiredItem firstAddedItem = null;

            foreach (var item in LstAcceptedItems.SelectedItems)
            {
                var newItem = new RequiredItem
                {
                    AcceptedItems = new List<string> { item.ToString() },
                    RequiredNum = 1
                };
                RequiredItems.Add(newItem);
                if (firstAddedItem == null)
                {
                    firstAddedItem = newItem;
                }
            }

            // Refresh the list view
            LvRequiredItems.ItemsSource = null;
            LvRequiredItems.ItemsSource = RequiredItems;

            // Auto-select the first added item and load its data into the editor
            if (firstAddedItem != null)
            {
                LvRequiredItems.SelectedItem = firstAddedItem;
                LvRequiredItems.ScrollIntoView(firstAddedItem);
                LoadSelectedRequiredItemIntoEditor();
            }
        }

        private void LoadSelectedRequiredItemIntoEditor()
        {
            if (LvRequiredItems.SelectedItem is RequiredItem selectedReq)
            {
                FilterAvailableItems(); // Update the available items list based on current search

                LstAcceptedItems.SelectedItems.Clear();
                foreach (var acceptedItem in selectedReq.AcceptedItems)
                {
                    // Check if the item exists in the filtered list
                    if (LstAcceptedItems.Items.Contains(acceptedItem))
                        LstAcceptedItems.SelectedItems.Add(acceptedItem);
                }

                // Scroll to the first selected item in the bottom list
                if (LstAcceptedItems.SelectedItems.Count > 0)
                {
                    LstAcceptedItems.ScrollIntoView(LstAcceptedItems.SelectedItems[0]);
                }

                TxtRequiredQty.Text = selectedReq.RequiredNum.ToString();
                TxtRandomQty.Text = selectedReq.RandomAdditionalRequiredNum.ToString();

                ChkMinHealth.IsChecked = selectedReq.MinAcceptedItemHealth > 0;
                TxtMinHealth.Text = selectedReq.MinAcceptedItemHealth.ToString();
                ChkMinUses.IsChecked = selectedReq.MinAcceptedItemUses > 0;
                TxtMinUses.Text = selectedReq.MinAcceptedItemUses.ToString();
                ChkMinMass.IsChecked = selectedReq.MinAcceptedItemMass > 0;
                TxtMinMass.Text = selectedReq.MinAcceptedItemMass.ToString();

                ChkMinCookLevel.IsChecked = !string.IsNullOrEmpty(selectedReq.MinAcceptedCookLevel);
                CmbMinCookLevel.SelectedItem = selectedReq.MinAcceptedCookLevel;
                ChkMaxCookLevel.IsChecked = !string.IsNullOrEmpty(selectedReq.MaxAcceptedCookLevel);
                CmbMaxCookLevel.SelectedItem = selectedReq.MaxAcceptedCookLevel;
                ChkMinCookQuality.IsChecked = !string.IsNullOrEmpty(selectedReq.MinAcceptedCookQuality);
                CmbMinCookQuality.SelectedItem = selectedReq.MinAcceptedCookQuality;

                ChkMinResourcePct.IsChecked = selectedReq.MinAcceptedItemResourceRatio > 0;
                TxtMinResourcePct.Text = selectedReq.MinAcceptedItemResourceRatio.ToString();
                ChkMinResourceMl.IsChecked = selectedReq.MinAcceptedItemResourceAmount > 0;
                TxtMinResourceMl.Text = selectedReq.MinAcceptedItemResourceAmount.ToString();
            }
        }

        private void BtnEditSelectedRequiredItem_Click(object sender, RoutedEventArgs e)
        {
            LoadSelectedRequiredItemIntoEditor();
        }

        private void BtnRemoveSelectedRequiredItem_Click(object sender, RoutedEventArgs e)
        {
            if (LvRequiredItems.SelectedItem is RequiredItem selectedReq)
            {
                RequiredItems.Remove(selectedReq);
                LvRequiredItems.ItemsSource = null;
                LvRequiredItems.ItemsSource = RequiredItems;
            }
            else
            {
                MessageBox.Show("Please select an item to remove.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void NumericPreviewTextInput(object sender, TextCompositionEventArgs e) { e.Handled = !Regex.IsMatch(e.Text, @"^\d*$"); }
        private void NumericPasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string text = (string)e.DataObject.GetData(typeof(string));
                e.Handled = !Regex.IsMatch(text, @"^\d*$");
            }
            else { e.Handled = true; }
        }
    }
}
