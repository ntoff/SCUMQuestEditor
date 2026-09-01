#nullable enable
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace SCUMQuestEditor
{
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

    public class AppSettings
    {
        public string FileNameFormat { get; set; } = "T{tier}_{trader}_{title}";
    }

    public class Condition : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private string _trackingCaption = "";
        public string TrackingCaption
        {
            get => _trackingCaption;
            set
            {
                if (_trackingCaption != value)
                {
                    _trackingCaption = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _sequenceIndex = 0;
        public int SequenceIndex
        {
            get => _sequenceIndex;
            set
            {
                if (value < 0) value = 0;
                else if (value > 10) value = 10;

                if (_sequenceIndex != value)
                {
                    _sequenceIndex = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _canBeAutoCompleted;
        public bool CanBeAutoCompleted
        {
            get => _canBeAutoCompleted;
            set
            {
                if (_canBeAutoCompleted != value)
                {
                    _canBeAutoCompleted = value;
                    OnPropertyChanged();
                }
            }
        }

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

        [JsonPropertyName("DisablePurchaseOfRequiredItems")]
        public bool DisablePurchaseOfRequiredItems { get; set; } = true;

        [JsonPropertyName("PlayerKeepsItems")]
        public bool PlayerKeepsItems { get; set; } = false;


        [JsonPropertyName("RequiredItems")]
        public List<RequiredItem> RequiredItems { get; set; } = new List<RequiredItem>();
    }

    public class RequiredItem
    {
        [JsonPropertyName("AcceptedItems")]
        public List<string> AcceptedItems { get; set; } = new List<string>();

        // Display property for the ListView
        public string ItemName => string.Join(", ", AcceptedItems);

        public string AcceptedItemsDisplay => string.Join(", ", AcceptedItems);

        [JsonPropertyName("RequiredNum")]
        public int RequiredNum { get; set; } = 0;

        // Display property for the ListView
        public string Quantity => RequiredNum.ToString();

        // Display property for the ListView
        public string Properties
        {
            get
            {
                var props = new List<string>();
                if (MinAcceptedItemUses > 0) props.Add($"Uses>={MinAcceptedItemUses}");
                if (MinAcceptedItemMass > 0) props.Add($"Mass>={MinAcceptedItemMass}");
                if (MinAcceptedItemHealth > 0) props.Add($"Health>={MinAcceptedItemHealth}");
                if (!string.IsNullOrEmpty(MinAcceptedCookLevel)) props.Add($"MinCook>={MinAcceptedCookLevel}");
                if (!string.IsNullOrEmpty(MaxAcceptedCookLevel)) props.Add($"MaxCook<={MaxAcceptedCookLevel}");
                if (!string.IsNullOrEmpty(MinAcceptedCookQuality)) props.Add($"CookedQuality>={MinAcceptedCookQuality}");
                if (MinAcceptedItemResourceRatio > 0) props.Add($"Resource%>={MinAcceptedItemResourceRatio}");
                if (MinAcceptedItemResourceAmount > 0) props.Add($"Resource>={MinAcceptedItemResourceAmount}");
                if (RandomAdditionalRequiredNum > 0) props.Add($"Random+={RandomAdditionalRequiredNum}");
                return string.Join("; ", props);
            }
        }

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

        // CHANGED: Make Instance nullable so it can be null if missing in JSON
        [JsonPropertyName("Instance")]
        public int? Instance { get; set; } = null;

        // NEW: Helper for display purposes
        [JsonIgnore]
        public string InstanceDisplay => Instance.HasValue ? Instance.Value.ToString() : "N/A";

        [JsonIgnore]
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

    public class ConditionConverter : JsonConverter<Condition>
    {
        public override Condition? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // Deserialize to JsonElement
            JsonElement element = JsonSerializer.Deserialize<JsonElement>(ref reader);

            // Get 'Type' property
            if (!element.TryGetProperty("Type", out JsonElement typeElement))
            {
                throw new JsonException("Condition JSON must contain a 'Type' property.");
            }

            string? conditionType = typeElement.GetString();
            if (string.IsNullOrEmpty(conditionType))
            {
                throw new JsonException("Condition 'Type' property is missing or empty.");
            }

            // Map type string to concrete Type
            Type targetType = conditionType switch
            {
                "Elimination" => typeof(EliminationCondition),
                "Fetch" => typeof(FetchCondition),
                "Interaction" => typeof(InteractionCondition),
                _ => throw new JsonException($"Unknown condition type: {conditionType}")
            };

            // Deserialize to concrete type — returns object?
            var result = JsonSerializer.Deserialize(element.GetRawText(), targetType, options);

            // Ensure result is of the expected type and non-null
            if (result is not Condition condition)
            {
                throw new JsonException($"Deserialized object is not a Condition (got {result?.GetType()?.Name ?? "null"}).");
            }

            return condition;
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

                if (elimination.TargetCharacters != null && elimination.TargetCharacters.Count > 0)
                {
                    writer.WriteStartArray("TargetCharacters");
                    foreach (var item in elimination.TargetCharacters) writer.WriteStringValue(item);
                    writer.WriteEndArray();
                }

                if (elimination.AllowedWeapons != null && elimination.AllowedWeapons.Count > 0)
                {
                    writer.WriteStartArray("AllowedWeapons");
                    foreach (var item in elimination.AllowedWeapons) writer.WriteStringValue(item);
                    writer.WriteEndArray();
                }
            }
            else if (value is FetchCondition fetch)
            {
                writer.WriteBoolean("DisablePurchaseOfRequiredItems", fetch.DisablePurchaseOfRequiredItems);
                writer.WriteBoolean("PlayerKeepsItems", fetch.PlayerKeepsItems);

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

                if (interaction.Locations != null && interaction.Locations.Count > 0)
                {
                    writer.WriteStartArray("Locations");
                    foreach (var loc in interaction.Locations)
                    {
                        writer.WriteStartObject();
                        writer.WriteString("AnchorMesh", loc.AnchorMesh);
                        writer.WriteString("FallbackTransform", loc.FallbackTransform);
                        writer.WriteString("VisibleMesh", loc.VisibleMesh);
                        if (loc.Instance.HasValue)
                        {
                            writer.WriteNumber("Instance", loc.Instance.Value);
                        }
                        writer.WriteEndObject();
                    }
                    writer.WriteEndArray();
                }

                writer.WriteNumber("MinNeeded", interaction.MinNeeded);
                writer.WriteNumber("MaxNeeded", interaction.MaxNeeded);
                writer.WriteBoolean("SpawnOnlyNeeded", interaction.SpawnOnlyNeeded);
                writer.WriteNumber("WorldMarkerShowDistance", interaction.WorldMarkerShowDistance);
            }

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

    public partial class MainWindow : Window
    {
        public static List<string> TradeItems { get; private set; } = new List<string> { "Default Item" };
        public static List<string> FetchItems { get; private set; } = new List<string>();
        public const int MaxKillAmount = 1000000000;
        private static AppSettings _settings = new AppSettings();
        private string? _currentFilePath;
        private string? _pendingFilePath;

        public MainWindow(string? filePath = null)
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
            _pendingFilePath = filePath;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            LoadSettings();
            LoadFetchItems();
            CbTier.Items.Add("1");
            CbTier.Items.Add("2");
            CbTier.Items.Add("3");
            CbTier.Items.Add("4");
            CbTier.SelectedIndex = 0;
            UpdateJsonPreview();
            InitConditions();

            if (!string.IsNullOrEmpty(_pendingFilePath))
            {
                var filePath = _pendingFilePath;
                _pendingFilePath = null;
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    LoadFileFromCommandLine(filePath);
                }), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
            }
        }

        private void LoadFileFromCommandLine(string filePath)
        {
            if (!File.Exists(filePath))
            {
                MessageBox.Show(this, $"The file '{filePath}' does not exist.", "File Not Found", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                string jsonContent = File.ReadAllText(filePath);

                bool isLikelyQuestFile = IsLikelyQuestFile(jsonContent);

                if (!isLikelyQuestFile)
                {
                    var result = MessageBox.Show(
                        this,
                        $"The file '{Path.GetFileName(filePath)}' does not appear to be a valid quest file.\n\n" +
                        "It may be a different JSON file or the file may be corrupted.\n\n" +
                        "You can still open it, but the editor may not display any meaningful data.\n\n" +
                        "Do you want to continue loading?",
                        "Unrecognized File Format",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning);

                    if (result != MessageBoxResult.Yes)
                    {
                        return;
                    }
                }

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Converters = { new ConditionConverter() }
                };

                TradeDeal? loadedQuest = JsonSerializer.Deserialize<TradeDeal>(jsonContent, options);

                if (loadedQuest == null)
                {
                    MessageBox.Show(this, $"Failed to parse the file as a quest file.\n\nThe file may not contain valid quest data.", "Invalid Quest Data", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                _currentFilePath = filePath;
                UpdateControlsFromQuest(loadedQuest);
                CurrentTradeDeal = loadedQuest;
            }
            catch (JsonException ex)
            {
                MessageBox.Show($"Error parsing quest file: {ex.Message}", "Parse Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading quest file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool IsLikelyQuestFile(string jsonContent)
        {
            if (string.IsNullOrWhiteSpace(jsonContent))
                return false;

            int score = 0;

            if (jsonContent.Contains("\"AssociatedNpc\"") || jsonContent.Contains("\"associatedNpc\"")) score++;
            if (jsonContent.Contains("\"Tier\"") || jsonContent.Contains("\"tier\"")) score++;
            if (jsonContent.Contains("\"Title\"") || jsonContent.Contains("\"title\"")) score++;
            if (jsonContent.Contains("\"Description\"") || jsonContent.Contains("\"description\"")) score++;
            if (jsonContent.Contains("\"TimeLimitHours\"") || jsonContent.Contains("\"timeLimitHours\"")) score++;
            if (jsonContent.Contains("\"RewardPool\"") || jsonContent.Contains("\"rewardPool\"")) score++;
            if (jsonContent.Contains("\"Conditions\"") || jsonContent.Contains("\"conditions\"")) score++;

            if (jsonContent.Contains("\"Type\"") || jsonContent.Contains("\"type\""))
            {
                if (jsonContent.Contains("\"Elimination\"") || jsonContent.Contains("\"Fetch\"") || jsonContent.Contains("\"Interaction\""))
                    score += 3;
                else
                    score++;
            }

            if (score >= 4)
                return true;

            return false;
        }

        private void LoadSettings()
        {
            try
            {
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "_data\\settings.json");
                if (File.Exists(path))
                {
                    string json = File.ReadAllText(path);
                    _settings = JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
                }
            }
            catch
            {
                _settings = new AppSettings();
            }
        }

        private void SaveSettings()
        {
            try
            {
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "_data\\settings.json");
                string? directory = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(_settings, options);
                File.WriteAllText(path, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving settings: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadFetchItems()
        {
            try
            {
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "_data\\FetchItems.txt");
                if (File.Exists(path))
                {
                    FetchItems = File.ReadAllLines(path).Where(line => !string.IsNullOrEmpty(line)).ToList();
                }
                else
                {
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
            _currentFilePath = null;
            CurrentTradeDeal = new TradeDeal
            {
                AssociatedNpc = "Armorer",
                Tier = 1,
                Title = "New Quest",
                Description = "Quest description...",
                TimeLimitHours = 24,
                RewardPool = new List<RewardPool> { new RewardPool() },
                Conditions = new List<Condition>()
            };

            if (CbNpc != null)
            {
                foreach (ComboBoxItem item in CbNpc.Items)
                {
                    if (item.Tag?.ToString() == "Armorer")
                    {
                        CbNpc.SelectedItem = item;
                        break;
                    }
                }
            }

            if (TxtTitle != null) TxtTitle.Text = "New Quest";
            CbTier.SelectedIndex = 0;
            if (TxtDescription != null) TxtDescription.Text = "Quest description...";
            if (TxtTimeLimit != null) TxtTimeLimit.Text = "24";

            if (TxtNormalReward != null) TxtNormalReward.Text = "0";
            if (TxtGoldReward != null) TxtGoldReward.Text = "0";
            if (TxtFameReward != null) TxtFameReward.Text = "0";

            if (LvSkills != null) LvSkills.ItemsSource = null;
            if (LvTradeDeals != null) LvTradeDeals.ItemsSource = null;

            ConditionsList.Clear();
            LvConditions.ItemsSource = null;
            LvConditions.ItemsSource = conditionsView;
            LvConditions.SelectedItem = null;

            RewardPool currentReward = GetOrCreateCurrentReward();
            int totalRewards = CalculateTotalRewards(currentReward);
            if (TxtTotalRewards != null) TxtTotalRewards.Text = $"Total Rewards: {totalRewards}/5";

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

                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        Converters = { new ConditionConverter() }
                    };

                    TradeDeal? loadedQuest = JsonSerializer.Deserialize<TradeDeal>(jsonContent, options);

                    if (loadedQuest == null)
                    {
                        MessageBox.Show("Failed to load quest file. The file may be corrupted or invalid.");
                        return;
                    }

                    _currentFilePath = filePath;
                    UpdateControlsFromQuest(loadedQuest);
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
                string fileName;

                List<string> emptyFields = new();

                bool rewardPoolEmpty = (CurrentTradeDeal.RewardPool == null || CurrentTradeDeal.RewardPool.Count == 0) || CurrentTradeDeal.RewardPool.All(r => r.CurrencyNormal == 0 && r.CurrencyGold == 0 && r.Fame == 0 && r.Skills == null && r.TradeDeals == null);

                if (rewardPoolEmpty)
                {
                    emptyFields.Add("Reward Pool");
                }

                if (CurrentTradeDeal.Conditions == null || CurrentTradeDeal.Conditions.Count == 0)
                {
                    emptyFields.Add("Conditions");
                }

                if (emptyFields.Count > 0)
                {
                    string fieldsList = string.Join(", ", emptyFields);
                    string message = $"The following fields cannot be empty:\n{fieldsList}";
                    SaveValidationDialog dialog = new SaveValidationDialog(message) { Owner = this };
                    dialog.ShowDialog();
                    return;
                }

                if (!string.IsNullOrEmpty(_currentFilePath))
                {
                    fileName = Path.GetFileName(_currentFilePath);
                }
                else
                {
                    fileName = GenerateFileName();
                }

                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                    Title = "Save Quest File",
                    DefaultExt = "json",
                    FileName = fileName
                };

                bool? result = saveFileDialog.ShowDialog();
                if (result == true)
                {
                    UpdateJsonPreview();

                    var options = new JsonSerializerOptions
                    {
                        WriteIndented = true,
                        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                        Converters = { new ConditionConverter() }
                    };

                    string json = JsonSerializer.Serialize(CurrentTradeDeal, options);
                    File.WriteAllText(saveFileDialog.FileName, json);
                    _currentFilePath = saveFileDialog.FileName;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving quest file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string GetTraderCode(string npc)
        {
            return npc switch
            {
                "Armorer" => "AR",
                "Banker" => "BK",
                "Barber" => "BA",
                "Bartender" => "BT",
                "Doctor" => "DC",
                "GeneralGoods" => "GG",
                "Harbourmaster" => "HM",
                "Hunter" => "RH",
                "MasterHunter" => "MH",
                "Mechanic" => "MC",
                _ => "AR"
            };
        }

        private void FileNameFormat_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new FilenameFormatDialog(_settings.FileNameFormat);
            dialog.Owner = this;
            dialog.ShowDialog();

            if (dialog.IsOkClicked)
            {
                _settings.FileNameFormat = dialog.NewFormat;
                SaveSettings();
            }
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AboutDialog();
            dialog.Owner = this;
            dialog.ShowDialog();
        }

        private string GenerateFileName()
        {
            string npc = CurrentTradeDeal?.AssociatedNpc ?? "Armorer";
            int tier = CurrentTradeDeal?.Tier ?? 1;
            string title = TxtTitle?.Text?.Replace(" ", "_") ?? "quest";
            string traderCode = GetTraderCode(npc);

            string format = _settings.FileNameFormat;
            format = format.Replace("{tier}", tier.ToString());
            format = format.Replace("{trader}", traderCode);
            format = format.Replace("{title}", title);

            return format + ".json";
        }
        private void UpdateControlsFromQuest(TradeDeal quest)
        {
            // Disable all change handlers that would trigger UpdateJsonPreview during load
            CbNpc?.SelectionChanged -= CbNpc_SelectionChanged;
            CbTier?.SelectionChanged -= CbTier_SelectionChanged;
            TxtTitle?.TextChanged -= TxtInput_TextChanged;
            TxtDescription?.TextChanged -= TxtInput_TextChanged;
            TxtTimeLimit?.TextChanged -= TxtInput_TextChanged;

            if (CbNpc != null)
            {
                bool npcFound = false;
                foreach (ComboBoxItem item in CbNpc.Items)
                {
                    if (item.Tag?.ToString() == quest.AssociatedNpc)
                    {
                        CbNpc.SelectedItem = item;
                        npcFound = true;
                        break;
                    }
                }

                if (!npcFound && quest.AssociatedNpc != null)
                {
                    ComboBoxItem newItem = new ComboBoxItem { Content = quest.AssociatedNpc };
                    CbNpc.Items.Add(newItem);
                    CbNpc.SelectedItem = newItem;
                }
            }

            if (TxtTitle != null) TxtTitle.Text = quest.Title;
            CbTier!.SelectedIndex = quest.Tier - 1;
            if (TxtDescription != null) TxtDescription.Text = quest.Description;
            if (TxtTimeLimit != null) TxtTimeLimit.Text = quest.TimeLimitHours.ToString("0.0#");

            if (quest.RewardPool != null && quest.RewardPool.Count > 0)
            {
                RewardPool reward = quest.RewardPool[0];

                if (TxtNormalReward != null) TxtNormalReward.Text = reward.CurrencyNormal.ToString();
                if (TxtGoldReward != null) TxtGoldReward.Text = reward.CurrencyGold.ToString();
                if (TxtFameReward != null) TxtFameReward.Text = reward.Fame.ToString();

                if (LvSkills != null)
                {
                    if (reward.Skills != null && reward.Skills.Count > 0)
                    {
                        LvSkills.ItemsSource = reward.Skills;
                    }
                    else
                    {
                        LvSkills.ItemsSource = null;
                    }
                }

                if (LvTradeDeals != null)
                {
                    if (reward.TradeDeals != null && reward.TradeDeals.Count > 0)
                    {
                        LvTradeDeals.ItemsSource = reward.TradeDeals;
                    }
                    else
                    {
                        LvTradeDeals.ItemsSource = null;
                    }
                }

                int totalRewards = CalculateTotalRewards(reward);
                if (TxtTotalRewards != null) TxtTotalRewards.Text = $"Total Rewards: {totalRewards}/5";
            }
            else
            {
                if (TxtNormalReward != null) TxtNormalReward.Text = "0";
                if (TxtGoldReward != null) TxtGoldReward.Text = "0";
                if (TxtFameReward != null) TxtFameReward.Text = "0";
                if (LvSkills != null) LvSkills.ItemsSource = null;
                if (LvTradeDeals != null) LvTradeDeals.ItemsSource = null;
            }

            // Clear the observable collection
            ConditionsList.Clear();

            // Add the loaded conditions back into the collection
            if (quest.Conditions != null)
            {
                foreach (var condition in quest.Conditions)
                {
                    ConditionsList.Add(condition);
                }
            }

            LvConditions.ItemsSource = null;
            LvConditions.ItemsSource = conditionsView;
            LvConditions.SelectedItem = null;

            // Re-enable change handlers after all data is loaded
            CbNpc?.SelectionChanged += CbNpc_SelectionChanged;
            CbTier?.SelectionChanged += CbTier_SelectionChanged;
            TxtTitle?.TextChanged += TxtInput_TextChanged;
            TxtDescription?.TextChanged += TxtInput_TextChanged;
            TxtTimeLimit?.TextChanged += TxtInput_TextChanged;

            UpdateJsonPreview();
        }



        private void TxtInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateJsonPreview();
            if (sender == TxtTitle)
            {
                _currentFilePath = null;
            }
        }
        private void CbNpc_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateJsonPreview();
            _currentFilePath = null;
        }

        private void CbTier_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateJsonPreview();
            _currentFilePath = null;
        }

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

            var traderName = CurrentTradeDeal?.AssociatedNpc ?? "";
            var dialog = new AddTradeDealDialog(null, traderName);
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
                var traderName = CurrentTradeDeal?.AssociatedNpc ?? "";
                AddTradeDealDialog dialog = new AddTradeDealDialog(selectedDeal, traderName);
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
                string npc = "Armorer";
                if (CbNpc?.SelectedItem is ComboBoxItem selectedItem) npc = selectedItem.Tag?.ToString() ?? "Armorer";

                int tier = 1;
                if (CbTier?.SelectedIndex >= 0) tier = CbTier.SelectedIndex + 1;

                string title = TxtTitle?.Text ?? "New Quest";
                string description = TxtDescription?.Text ?? "Quest description...";
                double timeLimit = 24;
                if (!double.TryParse(TxtTimeLimit?.Text ?? "24", out timeLimit)) timeLimit = 24;

                CurrentTradeDeal.AssociatedNpc = npc;
                CurrentTradeDeal.Tier = tier;
                CurrentTradeDeal.Title = title;
                CurrentTradeDeal.Description = description;
                CurrentTradeDeal.TimeLimitHours = timeLimit;

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

                int totalRewards = CalculateTotalRewards(reward);
                if (TxtTotalRewards != null) TxtTotalRewards.Text = $"Total Rewards: {totalRewards}/5";

                // Use the sorted view if available, otherwise the raw list
                if (conditionsView != null)
                {
                    CurrentTradeDeal.Conditions = conditionsView.Cast<Condition>().ToList();
                }
                else
                {
                    CurrentTradeDeal.Conditions = ConditionsList.ToList();
                }

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                    Converters = { new ConditionConverter() }
                };

                string json = JsonSerializer.Serialize(CurrentTradeDeal, options);
                if (TxtJson != null) TxtJson.Text = json;

                bool rewardPoolEmpty = CurrentTradeDeal.RewardPool == null || CurrentTradeDeal.RewardPool.Count == 0 || CurrentTradeDeal.RewardPool.All(r => r.CurrencyNormal == 0 && r.CurrencyGold == 0 && r.Fame == 0 && r.Skills == null && r.TradeDeals == null);
                bool conditionsEmpty = CurrentTradeDeal.Conditions == null || CurrentTradeDeal.Conditions.Count == 0;

                var conditionErrors = new List<string>();
                if (!conditionsEmpty)
                {
                    var conditions = CurrentTradeDeal.Conditions!;
                    for (int i = 0; i < conditions.Count; i++)
                    {
                        var cond = conditions[i];
                        string condLabel = $"Condition {i + 1} ({cond.Type})";

                        if (cond is EliminationCondition elim)
                        {
                            if (elim.TargetCharacters == null || elim.TargetCharacters.Count == 0)
                                conditionErrors.Add($"{condLabel}: TargetCharacters is missing or empty");
                        }
                        else if (cond is FetchCondition fetch)
                        {
                            if (fetch.RequiredItems == null || fetch.RequiredItems.Count == 0)
                                conditionErrors.Add($"{condLabel}: RequiredItems is missing or empty");
                        }
                        else if (cond is InteractionCondition interaction)
                        {
                            if (interaction.Locations == null || interaction.Locations.Count == 0)
                                conditionErrors.Add($"{condLabel}: Locations is missing or empty");
                        }
                    }
                }

                string warning = "";
                bool hasIssues = rewardPoolEmpty || conditionsEmpty || conditionErrors.Count > 0;

                if (rewardPoolEmpty)
                    warning += "Reward pool is empty. It should not be left empty.";
                
                if (conditionsEmpty)
                {
                    if (!string.IsNullOrEmpty(warning))
                        warning += "\n";
                    warning += "Conditions is empty. It should not be left empty.";
                }

                foreach (var error in conditionErrors)
                {
                    if (!string.IsNullOrEmpty(warning))
                        warning += "\n";
                    warning += $"Warning: {error}";
                }

                if (TxtJsonWarning != null)
                {
                    TxtJsonWarning.Text = warning;
                    TxtJsonWarning.Visibility = hasIssues ? Visibility.Visible : Visibility.Collapsed;
                }

                if (hasIssues)
                {
                    JsonPreviewTab?.SetValue(TabItem.ForegroundProperty, new SolidColorBrush(Color.FromArgb(255, 255, 50, 50)));
                    JsonPreviewTab?.Header = CreateWarningHeader();
                }
                else
                {
                    JsonPreviewTab?.ClearValue(TabItem.ForegroundProperty);
                    JsonPreviewTab?.Header = "JSON Preview";
                }
            }
            catch (Exception ex)
            {
                if (TxtJson != null) TxtJson.Text = $"Error generating JSON: {ex.Message}";
                if (TxtJsonWarning != null)
                {
                    TxtJsonWarning.Visibility = Visibility.Collapsed;
                }
                JsonPreviewTab?.ClearValue(TabItem.ForegroundProperty);
            }
        }

        private object CreateWarningHeader()
        {
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(4) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var packIcon = new MaterialDesignThemes.Wpf.PackIcon
            {
                Kind = MaterialDesignThemes.Wpf.PackIconKind.AlertCircleOutline,
                Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 50, 50)),
                Width = 16,
                Height = 16,
                Margin = new Thickness(0, 0, 4, 0)
            };

            var textBlock = new TextBlock
            {
                Text = "JSON Preview",
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 50, 50)),
                Margin = new Thickness(0, 2, 0, 0)
            };
            
            Grid.SetColumn(textBlock, 0);
            Grid.SetColumn(packIcon, 2);
            grid.Children.Add(textBlock);
            grid.Children.Add(packIcon);

            return grid;
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

        public ObservableCollection<Condition> ConditionsList { get; set; } = new ObservableCollection<Condition>();

        private ListCollectionView? conditionsView;

        private void InitConditions()
        {
            LvConditions.ItemsSource = ConditionsList;

            // Create a ListCollectionView to manage sorting
            conditionsView = new ListCollectionView(ConditionsList);
            LvConditions.ItemsSource = conditionsView;

            LvConditions.SelectionChanged += LvConditions_SelectionChanged;
            UpdateConditionTabsState();
        }

        private void SortBySequence(object sender, RoutedEventArgs e)
        {
            if (conditionsView == null) return;

            conditionsView.SortDescriptions.Clear();
            conditionsView.SortDescriptions.Add(new SortDescription("SequenceIndex", ListSortDirection.Ascending));
            UpdateJsonPreview();
        }
        private void SortByCaption(object sender, RoutedEventArgs e)
        {
            if (conditionsView == null) return;

            conditionsView.SortDescriptions.Clear();
            conditionsView.SortDescriptions.Add(new SortDescription("TrackingCaption", ListSortDirection.Ascending));
            UpdateJsonPreview();
        }


        private void LvConditions_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateConditionTabsState();
            //ClearConditionEditor(); //I remember this fixing something, but can't remember what. It causes issues though and resets stuff to default.
            TabConditionEditor.SelectedIndex = 0;

            if (LvConditions.SelectedItem is Condition selectedCondition)
            {
                BindingOperations.ClearAllBindings(EdtCaption);
                BindingOperations.ClearAllBindings(EdtSequence);
                BindingOperations.ClearAllBindings(EdtAutoComplete);

                var bindingCaption = new Binding("TrackingCaption")
                {
                    Source = selectedCondition,
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                };
                BindingOperations.SetBinding(EdtCaption, TextBox.TextProperty, bindingCaption);

                var bindingSequence = new Binding("SequenceIndex")
                {
                    Source = selectedCondition,
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                };
                BindingOperations.SetBinding(EdtSequence, TextBox.TextProperty, bindingSequence);

                var bindingAuto = new Binding("CanBeAutoCompleted")
                {
                    Source = selectedCondition,
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                };
                BindingOperations.SetBinding(EdtAutoComplete, CheckBox.IsCheckedProperty, bindingAuto);

                EdtCaption.Text = selectedCondition.TrackingCaption;
                EdtSequence.Text = selectedCondition.SequenceIndex.ToString();
                EdtAutoComplete.IsChecked = selectedCondition.CanBeAutoCompleted;

                string type = selectedCondition.Type.ToLower();

                if (type == "fetch")
                {
                    if (selectedCondition is FetchCondition fetchCondition)
                    {
                        BindingOperations.ClearAllBindings(ChkPlayerKeepsItems);
                        BindingOperations.ClearAllBindings(ChkDisablePurchase);

                        var bindingPlayerKeeps = new Binding("PlayerKeepsItems")
                        {
                            Source = fetchCondition,
                            Mode = BindingMode.TwoWay,
                            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                        };
                        BindingOperations.SetBinding(ChkPlayerKeepsItems, CheckBox.IsCheckedProperty, bindingPlayerKeeps);

                        var bindingDisablePurchase = new Binding("DisablePurchaseOfRequiredItems")
                        {
                            Source = fetchCondition,
                            Mode = BindingMode.TwoWay,
                            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                        };
                        BindingOperations.SetBinding(ChkDisablePurchase, CheckBox.IsCheckedProperty, bindingDisablePurchase);

                        ChkPlayerKeepsItems.IsChecked = fetchCondition.PlayerKeepsItems;
                        ChkDisablePurchase.IsChecked = fetchCondition.DisablePurchaseOfRequiredItems;
                        LvCurrentRequiredItems.ItemsSource = fetchCondition.RequiredItems;
                    }
                }

                if (type == "interaction")
                {
                    if (selectedCondition is InteractionCondition interactionCondition)
                    {
                        BindingOperations.ClearAllBindings(ChkSpawnOnlyNeeded);
                        BindingOperations.ClearAllBindings(EdtMinNeeded);
                        BindingOperations.ClearAllBindings(EdtMaxNeeded);
                        BindingOperations.ClearAllBindings(EdtMarkerDistance);

                        var bindingSpawnOnly = new Binding("SpawnOnlyNeeded")
                        {
                            Source = interactionCondition,
                            Mode = BindingMode.TwoWay,
                            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                        };
                        BindingOperations.SetBinding(ChkSpawnOnlyNeeded, CheckBox.IsCheckedProperty, bindingSpawnOnly);

                        var bindingMin = new Binding("MinNeeded")
                        {
                            Source = interactionCondition,
                            Mode = BindingMode.TwoWay,
                            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                        };
                        BindingOperations.SetBinding(EdtMinNeeded, TextBox.TextProperty, bindingMin);

                        var bindingMax = new Binding("MaxNeeded")
                        {
                            Source = interactionCondition,
                            Mode = BindingMode.TwoWay,
                            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                        };
                        BindingOperations.SetBinding(EdtMaxNeeded, TextBox.TextProperty, bindingMax);

                        var bindingDistance = new Binding("WorldMarkerShowDistance")
                        {
                            Source = interactionCondition,
                            Mode = BindingMode.TwoWay,
                            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                        };
                        BindingOperations.SetBinding(EdtMarkerDistance, TextBox.TextProperty, bindingDistance);

                        ChkSpawnOnlyNeeded.IsChecked = interactionCondition.SpawnOnlyNeeded;
                        EdtMinNeeded.Text = interactionCondition.MinNeeded.ToString();
                        EdtMaxNeeded.Text = interactionCondition.MaxNeeded.ToString();
                        EdtMarkerDistance.Text = interactionCondition.WorldMarkerShowDistance.ToString();
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

            TabFetchProperties.IsEnabled = hasSelection;
            TabInteractionProperties.IsEnabled = hasSelection;
            TabEliminationProperties.IsEnabled = hasSelection;
            TabMapLocationsItem.IsEnabled = hasSelection;

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
                    ChkDisablePurchase.IsChecked = fetchCondition.DisablePurchaseOfRequiredItems;
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
            BindingOperations.ClearAllBindings(EdtCaption);
            BindingOperations.ClearAllBindings(EdtSequence);
            BindingOperations.ClearAllBindings(EdtAutoComplete);

            EdtCaption.Text = "";
            EdtSequence.Text = "0";
            EdtAutoComplete.IsChecked = false;

            ChkPlayerKeepsItems.IsChecked = false;
            ChkDisablePurchase.IsChecked = true;
            ChkSpawnOnlyNeeded.IsChecked = true;
            EdtMinNeeded.Text = "1";
            EdtMaxNeeded.Text = "1";
            EdtMarkerDistance.Text = "5";

            EdtEliminationTargets.ItemsSource = null;
            EdtAllowedWeapons.ItemsSource = null;
            LvCurrentRequiredItems.ItemsSource = null;
        }


        private void TabConditionEditor_SelectionChanged(object sender, SelectionChangedEventArgs e) { }

        public void AddCondition(Condition newCondition)
        {
            ConditionsList.Add(newCondition);
            LvConditions.ItemsSource = null;
            LvConditions.ItemsSource = conditionsView;
            LvConditions.SelectedItem = newCondition;
            LvConditions.ScrollIntoView(newCondition);
            UpdateJsonPreview();
        }

        private void BtnAddElimination_Click(object sender, RoutedEventArgs e)
        {
            var condition = new EliminationCondition
            {
                TrackingCaption = "Eliminate Target",
                SequenceIndex = 0,
                CanBeAutoCompleted = true,
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
                SequenceIndex = 0,
                CanBeAutoCompleted = true,
                DisablePurchaseOfRequiredItems = true,
                PlayerKeepsItems = false,
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
                SequenceIndex = 0,
                CanBeAutoCompleted = true,
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
                LvConditions.ItemsSource = null;
                LvConditions.ItemsSource = conditionsView;
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
                var locations = currentCondition.LocationsShownOnMap ?? new List<MapLocation>();
                var dialog = new EditMapLocationsDialog(locations);
                dialog.Owner = this;
                if (dialog.ShowDialog() == true)
                {
                    currentCondition.LocationsShownOnMap = dialog.MapLocations.ToList();
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

        private void TabMapLocations_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void ConditionField_LostFocus(object sender, RoutedEventArgs e)
        {
            UpdateJsonPreview();
        }

        private void ConditionField_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (sender is TextBox textBox)
                {
                    var binding = textBox.GetBindingExpression(TextBox.TextProperty)?.ParentBinding;
                    if (binding != null)
                    {
                        var expr = textBox.GetBindingExpression(TextBox.TextProperty);
                        if (expr != null) expr.UpdateSource();
                    }
                }
                UpdateJsonPreview();
            }
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                DragMove();
        }

        private void TitleBar_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.ContextMenu.IsOpen = true; // Optional: Add a ContextMenu to the Window for minimize/restore/close
        }

        private void MinimizeWindow_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void MaximizeRestoreWindow_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
                this.WindowState = WindowState.Normal;
            else
                this.WindowState = WindowState.Maximized;
        }

        private void Window_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data?.GetData(DataFormats.FileDrop) is string[] files && files.Length > 0)
            {
                e.Effects = DragDropEffects.Copy;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            if (e.Data?.GetData(DataFormats.FileDrop) is string[] files && files.Length > 0)
            {
                var filePath = files[0];
                
                if (!File.Exists(filePath))
                {
                    MessageBox.Show(this, $"The file '{filePath}' does not exist.", "File Not Found", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                Dispatcher.BeginInvoke(new Action(() => LoadFileFromCommandLine(filePath)), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
            }
        }

        private void MenuItem_Exit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Btn_MouseEnter(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                button.Background = new SolidColorBrush(Color.FromArgb(255, 50, 50, 50));
            }
        }

        private void Btn_MouseLeave(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                button.Background = Brushes.Transparent;
            }
        }

        private void MoveConditionUp_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Condition selectedCondition)
            {
                int currentIndex = ConditionsList.IndexOf(selectedCondition);
                if (currentIndex > 0)
                {
                    ConditionsList.RemoveAt(currentIndex);
                    ConditionsList.Insert(currentIndex - 1, selectedCondition);
                    LvConditions.ItemsSource = null;
                    LvConditions.ItemsSource = conditionsView;
                    LvConditions.SelectedItem = selectedCondition;
                    UpdateJsonPreview();
                }
            }
        }

        private void MoveConditionDown_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Condition selectedCondition)
            {
                int currentIndex = ConditionsList.IndexOf(selectedCondition);
                if (currentIndex >= 0 && currentIndex < ConditionsList.Count - 1)
                {
                    ConditionsList.RemoveAt(currentIndex);
                    ConditionsList.Insert(currentIndex + 1, selectedCondition);
                    LvConditions.ItemsSource = null;
                    LvConditions.ItemsSource = conditionsView;
                    LvConditions.SelectedItem = selectedCondition;
                    UpdateJsonPreview();
                }
            }
        }

    }

    public class ConditionsIndexEqualsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is Condition condition)
            {
                var mainWindow = Application.Current.MainWindow as MainWindow;
                if (mainWindow?.ConditionsList != null)
                {
                    bool isFirst = mainWindow.ConditionsList.IndexOf(condition) == 0;
                    return isFirst ? Visibility.Collapsed : Visibility.Visible;
                }
            }
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ConditionsIndexLessThanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is Condition condition)
            {
                var mainWindow = Application.Current.MainWindow as MainWindow;
                if (mainWindow?.ConditionsList != null)
                {
                    int index = mainWindow.ConditionsList.IndexOf(condition);
                    bool isLast = index < 0 || index >= mainWindow.ConditionsList.Count - 1;
                    return isLast ? Visibility.Collapsed : Visibility.Visible;
                }
            }
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
