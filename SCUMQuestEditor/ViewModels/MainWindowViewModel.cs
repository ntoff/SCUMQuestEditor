#nullable enable
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;
using MaterialDesignColors;
using MaterialDesignThemes.Wpf;
using SCUMQuestEditor.Converters;
using SCUMQuestEditor.Models;
using ModelsCondition = SCUMQuestEditor.Models.Condition;

namespace SCUMQuestEditor.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private static readonly JsonSerializerOptions s_deserializeOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new ConditionConverter() }
        };

        private static readonly JsonSerializerOptions s_serializeIndentedOptions = new()
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            Converters = { new ConditionConverter() }
        };

        // Commands
        public ICommand NewCommand { get; }
        public ICommand OpenCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand SaveJsonCommand { get; }
        public ICommand ExitCommand { get; }
        public FileNameFormatClickCommand FileNameFormatCommand { get; }
        public AboutClickCommand AboutCommand { get; }
        public AppearanceSettingsCommand AppearanceSettingsCommand { get; }
        public QuestInfoClickCommand QuestInfoCommand { get; }
        public MenuEditModeToggleCommand EditModeToggleCommand { get; }

        // Core properties
        public TradeDeal CurrentTradeDeal { get; set; } = new TradeDeal();
        public ObservableCollection<ModelsCondition> ConditionsList { get; } = new ObservableCollection<ModelsCondition>();
        public ObservableCollection<string> Warnings { get; } = new ObservableCollection<string>();
        public string JsonPreview { get; set; } = "";
        public string WarningText { get; set; } = "";
        public Visibility WarningVisibility { get; set; } = Visibility.Collapsed;
        public string TotalRewardsDisplay { get; set; } = "Total Rewards: 0/5";
        public List<string> TradeItems { get; } = new List<string> { "Default Item" };
        public List<string> FetchItems { get; set; } = new List<string> { "05_Teeth_Necklace", "12_Gauge_Birdshot" };
        public bool TargetsWarningShown { get; set; }
        public bool WeaponsWarningShown { get; set; }
        public bool FetchItemsWarningShown { get; set; }
        public bool TradeItemsWarningShown { get; set; }

        // Settings
        public AppSettings Settings { get; } = new AppSettings();

        // Edit mode
        public bool IsEditMode { get; set; }
        public SolidColorBrush ErrorHighlightBrush { get; set; } = new SolidColorBrush(Color.FromArgb(255, 0xEF, 0x53, 0x50));

        // Constructor
        public MainWindowViewModel()
        {
            NewCommand = new RelayCommand(_ => New());
            OpenCommand = new RelayCommand(_ => Open());
            SaveCommand = new RelayCommand(_ => Save());
            SaveJsonCommand = new RelayCommand(_ => SaveJson());
            ExitCommand = new RelayCommand(_ => Application.Current.Shutdown());
            FileNameFormatCommand = new FileNameFormatClickCommand(this);
            AboutCommand = new AboutClickCommand();
            AppearanceSettingsCommand = new AppearanceSettingsCommand(this);
            QuestInfoCommand = new QuestInfoClickCommand();
            EditModeToggleCommand = new MenuEditModeToggleCommand(this);

            LoadSettings();
            LoadDataFiles();
        }

        // Business logic
        private void New()
        {
            CurrentTradeDeal = new TradeDeal
            {
                AssociatedNpc = "Armorer",
                Tier = 1,
                Title = "New Quest",
                Description = "Quest description...",
                TimeLimitHours = 24,
                RewardPool = new List<RewardPool> { new RewardPool() },
                Conditions = new List<ModelsCondition>()
            };

            ConditionsList.Clear();
            UpdateJsonPreview();
        }

        private void Open()
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
                LoadFileFromPath(openFileDialog.FileName);
            }
        }

        private void Save()
        {
            List<string> emptyFields = new();
            bool rewardPoolEmpty = (CurrentTradeDeal.RewardPool == null || CurrentTradeDeal.RewardPool.Count == 0) ||
                                   CurrentTradeDeal.RewardPool.All(r =>
                                       r.CurrencyNormal == 0 && r.CurrencyGold == 0 && r.Fame == 0 &&
                                       r.Skills == null && r.TradeDeals == null);

            if (rewardPoolEmpty) emptyFields.Add("Reward Pool");
            if (CurrentTradeDeal.Conditions == null || CurrentTradeDeal.Conditions.Count == 0) emptyFields.Add("Conditions");

            if (emptyFields.Count > 0)
            {
                string fieldsList = string.Join(", ", emptyFields);
                string message = $"The following fields cannot be empty:\n{fieldsList}";
                var dialog = new SaveValidationDialog(message) { Owner = Application.Current.MainWindow };
                dialog.ShowDialog();
                return;
            }

            string fileName = string.IsNullOrEmpty(_currentFilePath) ? GenerateFileName() : Path.GetFileName(_currentFilePath);

            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                Title = "Save Quest File",
                DefaultExt = "json",
                FileName = fileName
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                UpdateJsonPreview();
                string json = JsonSerializer.Serialize(CurrentTradeDeal, typeof(TradeDeal), s_serializeIndentedOptions);
                File.WriteAllText(saveFileDialog.FileName, json);
                _currentFilePath = saveFileDialog.FileName;
            }
        }

        private void SaveJson()
        {
            try
            {
                string jsonText = JsonPreview;
                TradeDeal? deserialized = JsonSerializer.Deserialize<TradeDeal>(jsonText, s_serializeIndentedOptions);

                if (deserialized == null) return;

                CurrentTradeDeal = deserialized;

                List<string> emptyFields = new();
                bool rewardPoolEmpty = (CurrentTradeDeal.RewardPool == null || CurrentTradeDeal.RewardPool.Count == 0) ||
                                       CurrentTradeDeal.RewardPool.All(r =>
                                           r.CurrencyNormal == 0 && r.CurrencyGold == 0 && r.Fame == 0 &&
                                           r.Skills == null && r.TradeDeals == null);
                if (rewardPoolEmpty) emptyFields.Add("Reward Pool");
                if (CurrentTradeDeal.Conditions == null || CurrentTradeDeal.Conditions.Count == 0) emptyFields.Add("Conditions");

                if (emptyFields.Count > 0)
                {
                    string fieldsList = string.Join(", ", emptyFields);
                    string message = $"The following fields cannot be empty:\n{fieldsList}";
                    var dialog = new SaveValidationDialog(message) { Owner = Application.Current.MainWindow };
                    dialog.ShowDialog();
                    return;
                }

                string fileName = string.IsNullOrEmpty(_currentFilePath) ? GenerateFileName() : Path.GetFileName(_currentFilePath);

                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                    Title = "Save Quest File",
                    DefaultExt = "json",
                    FileName = fileName
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    string outputJson = JsonSerializer.Serialize(CurrentTradeDeal, typeof(TradeDeal), s_serializeIndentedOptions);
                    File.WriteAllText(saveFileDialog.FileName, outputJson);
                    _currentFilePath = saveFileDialog.FileName;
                }
            }
            catch (JsonException ex)
            {
                MessageBox.Show(Application.Current.MainWindow, $"Invalid JSON format: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(Application.Current.MainWindow, $"Error saving quest file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Exit()
        {
            Application.Current.Shutdown();
        }

        private void LoadSettings()
        {
            try
            {
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "_data\\settings.json");
                if (File.Exists(path))
                {
                    string json = File.ReadAllText(path);
                    var loaded = JsonSerializer.Deserialize<AppSettings>(json);
                    if (loaded != null)
                    {
                        Settings.FileNameFormat = loaded.FileNameFormat;
                        Settings.PrimaryColor = loaded.PrimaryColor;
                        Settings.SecondaryColor = loaded.SecondaryColor;
                        Settings.ErrorHighlightColor = loaded.ErrorHighlightColor;
                        Settings.DarkMode = loaded.DarkMode;
                    }
                }
            }
            catch
            {
            }

            ApplyDarkMode(Settings.DarkMode);
            ApplyTheme(Settings.PrimaryColor, Settings.SecondaryColor);
            ApplyErrorColor(Settings.ErrorHighlightColor);
        }

        public void SaveSettings()
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
                string json = JsonSerializer.Serialize(Settings, options);
                File.WriteAllText(path, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show(Application.Current.MainWindow, $"Error saving settings: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void LoadDataFiles()
        {
            string dataDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "_data");

            string targetsPath = Path.Combine(dataDir, "EliminationTargets.txt");
            if (!File.Exists(targetsPath))
            {
                Log("EliminationTargets.txt file not found.");
                TargetsWarningShown = true;
            }

            string weaponsPath = Path.Combine(dataDir, "EliminationWeapons.txt");
            if (!File.Exists(weaponsPath))
            {
                Log("EliminationWeapons.txt file not found.");
                WeaponsWarningShown = true;
            }

            string fetchPath = Path.Combine(dataDir, "FetchItems.txt");
            if (File.Exists(fetchPath))
                FetchItems = File.ReadAllLines(fetchPath).Where(line => !string.IsNullOrEmpty(line)).ToList();
            else
            {
                Log("FetchItems.txt file not found.");
                FetchItemsWarningShown = true;
            }

            string tradeItemsPath = Path.Combine(dataDir, "TradeItems.json");
            if (!File.Exists(tradeItemsPath))
            {
                Log("TradeItems.json file not found.");
                TradeItemsWarningShown = true;
            }

            if (TargetsWarningShown)
                MessageBox.Show(Application.Current.MainWindow,
                    "The EliminationTargets.txt data file is missing or could not be loaded. Target character selection may be limited.",
                    "Data File Error", MessageBoxButton.OK, MessageBoxImage.Warning);

            if (WeaponsWarningShown)
                MessageBox.Show(Application.Current.MainWindow,
                    "The EliminationWeapons.txt data file is missing or could not be loaded. Weapon selection may be limited.",
                    "Data File Error", MessageBoxButton.OK, MessageBoxImage.Warning);

            if (FetchItemsWarningShown)
                MessageBox.Show(Application.Current.MainWindow,
                    "The FetchItems.txt data file is missing or could not be loaded. Item selection may be limited.",
                    "Data File Error", MessageBoxButton.OK, MessageBoxImage.Warning);

            if (TradeItemsWarningShown)
                MessageBox.Show(Application.Current.MainWindow,
                    "The TradeItems.json data file is missing or could not be loaded. Trade deal items may not be available.",
                    "Data File Error", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        public void ApplyDarkMode(bool isDark)
        {
            var bundledTheme = Application.Current.Resources.MergedDictionaries
                .OfType<MaterialDesignThemes.Wpf.BundledTheme>()
                .FirstOrDefault();

            if (bundledTheme != null)
            {
                bundledTheme.BaseTheme = isDark ? MaterialDesignThemes.Wpf.BaseTheme.Dark : MaterialDesignThemes.Wpf.BaseTheme.Light;
            }

            if (IsEditMode)
            {
                UpdateEditWarningBackground();
            }
        }

        public void ApplyTheme(string primaryColor, string secondaryColor)
        {
            var paletteHelper = new MaterialDesignThemes.Wpf.PaletteHelper();
            var theme = paletteHelper.GetTheme();

            var primaryColorVal = ColorSettingsDialog.GetColorFromName(primaryColor, "500");
            var secondaryColorVal = ColorSettingsDialog.GetColorFromName(secondaryColor, "500");

            theme.PrimaryMid = new ColorPair(primaryColorVal);
            theme.PrimaryLight = new ColorPair(ColorSettingsDialog.LightenColor(primaryColorVal));
            theme.PrimaryDark = new ColorPair(ColorSettingsDialog.DarkenColor(primaryColorVal));
            theme.SecondaryMid = new ColorPair(secondaryColorVal);
            theme.SecondaryLight = new ColorPair(ColorSettingsDialog.LightenColor(secondaryColorVal));
            theme.SecondaryDark = new ColorPair(ColorSettingsDialog.DarkenColor(secondaryColorVal));

            paletteHelper.SetTheme(theme);
        }

        public void ApplyErrorColor(string errorColorName)
        {
            var color = ColorSettingsDialog.GetColorFromName(errorColorName, "500");
            ErrorHighlightBrush = new SolidColorBrush(color);
            ErrorHighlightBrush.Freeze();
            UpdateJsonPreview();
            if (IsEditMode)
            {
                EditWarningTextForeground = ErrorHighlightBrush;
            }
        }

        private void UpdateEditWarningBackground()
        {
            string color = Settings.DarkMode ? "#333333" : "#F5F5F5";
            EditWarningBannerBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(color));
        }

        public void UpdateJsonPreview()
        {
            try
            {
                CurrentTradeDeal.AssociatedNpc = AssociatedNpc;
                CurrentTradeDeal.Tier = Tier;
                CurrentTradeDeal.Title = Title;
                CurrentTradeDeal.Description = Description;
                CurrentTradeDeal.TimeLimitHours = TimeLimitHours;

                RewardPool reward = GetOrCreateCurrentReward();

                reward.CurrencyNormal = NormalReward;
                reward.CurrencyGold = GoldReward;
                reward.Fame = FameReward;

                int totalRewards = CalculateTotalRewards(reward);
                TotalRewardsDisplay = $"Total Rewards: {totalRewards}/5";

                CurrentTradeDeal.Conditions = ConditionsList.ToList();

                string json = JsonSerializer.Serialize(CurrentTradeDeal, typeof(TradeDeal), s_serializeIndentedOptions);
                JsonPreview = json;

                var warnings = BuildWarnings();
                WarningText = string.Join("\n", warnings);
                WarningVisibility = warnings.Count == 0 ? Visibility.Collapsed : Visibility.Visible;

                OnPropertyChanged(nameof(JsonPreview));
                OnPropertyChanged(nameof(WarningText));
                OnPropertyChanged(nameof(WarningVisibility));
                OnPropertyChanged(nameof(TotalRewardsDisplay));
            }
            catch (Exception ex)
            {
                JsonPreview = $"Error generating JSON: {ex.Message}";
                WarningVisibility = Visibility.Collapsed;
                OnPropertyChanged(nameof(JsonPreview));
                OnPropertyChanged(nameof(WarningVisibility));
            }
        }

        private List<string> BuildWarnings()
        {
            var warnings = new List<string>();

            bool rewardPoolEmpty = CurrentTradeDeal.RewardPool == null || CurrentTradeDeal.RewardPool.Count == 0 ||
                                   CurrentTradeDeal.RewardPool.All(r =>
                                       r.CurrencyNormal == 0 && r.CurrencyGold == 0 && r.Fame == 0 &&
                                       r.Skills == null && r.TradeDeals == null);
            bool conditionsEmpty = CurrentTradeDeal.Conditions == null || CurrentTradeDeal.Conditions.Count == 0;

            if (rewardPoolEmpty && conditionsEmpty)
                warnings.Add("Both reward pool and condition pool are empty. Neither should be left empty.");
            else if (rewardPoolEmpty)
                warnings.Add("Reward pool is empty. It should not be left empty.");
            else if (conditionsEmpty)
                warnings.Add("Condition pool is empty. It should not be left empty.");

            if (CurrentTradeDeal.Conditions != null)
            {
                for (int i = 0; i < CurrentTradeDeal.Conditions.Count; i++)
                {
                    var condition = CurrentTradeDeal.Conditions[i];
                    string caption = string.IsNullOrEmpty(condition.TrackingCaption) ? $"Condition {i + 1}" : condition.TrackingCaption;

                    if (condition is FetchCondition fetchCondition)
                    {
                        if (fetchCondition.RequiredItems == null || fetchCondition.RequiredItems.Count == 0)
                            warnings.Add($"'{caption}' (Fetch) has missing or empty RequiredItems.");
                    }

                    if (condition is EliminationCondition eliminationCondition)
                    {
                        if (eliminationCondition.TargetCharacters == null || eliminationCondition.TargetCharacters.Count == 0)
                            warnings.Add($"'{caption}' (Elimination) has missing or empty TargetCharacters.");
                    }

                    if (condition is InteractionCondition interactionCondition)
                    {
                        if (interactionCondition.Locations == null || interactionCondition.Locations.Count == 0)
                            warnings.Add($"'{caption}' (Interaction) has missing or empty Locations.");
                    }
                }
            }

            return warnings;
        }

        private string GenerateFileName()
        {
            string npc = CurrentTradeDeal?.AssociatedNpc ?? "Armorer";
            int tier = CurrentTradeDeal?.Tier ?? 1;
            string title = Title?.Replace(" ", "_") ?? "quest";
            string traderCode = GetTraderCode(npc);
            string type = GetQuestType();

            string format = Settings.FileNameFormat;
            format = format.Replace("{tier}", tier.ToString());
            format = format.Replace("{trader}", traderCode);
            format = format.Replace("{title}", title);
            format = format.Replace("{type}", type);

            return format + ".json";
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

        private string GetQuestType()
        {
            if (CurrentTradeDeal?.Conditions == null || CurrentTradeDeal.Conditions.Count == 0)
                return "Mixed";

            bool hasElimination = false;
            bool hasFetch = false;
            bool hasInteraction = false;

            foreach (var condition in CurrentTradeDeal.Conditions)
            {
                if (condition is EliminationCondition) hasElimination = true;
                else if (condition is FetchCondition) hasFetch = true;
                else if (condition is InteractionCondition) hasInteraction = true;
            }

            int typeCount = (hasElimination ? 1 : 0) + (hasFetch ? 1 : 0) + (hasInteraction ? 1 : 0);

            return typeCount switch
            {
                1 => hasElimination ? "Eliminate" : hasFetch ? "Fetch" : "Interact",
                _ => "Mixed"
            };
        }

        private RewardPool GetOrCreateCurrentReward()
        {
            if (CurrentTradeDeal.RewardPool == null) CurrentTradeDeal.RewardPool = new List<RewardPool>();
            if (CurrentTradeDeal.RewardPool.Count == 0) CurrentTradeDeal.RewardPool.Add(new RewardPool());
            return CurrentTradeDeal.RewardPool[0];
        }

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

        // Loading
        public void LoadFileFromPath(string filePath)
        {
            try
            {
                string jsonContent = File.ReadAllText(filePath);
                TradeDeal? loadedQuest = JsonSerializer.Deserialize(jsonContent, typeof(TradeDeal), s_deserializeOptions) as TradeDeal;

                if (loadedQuest == null)
                {
                    MessageBox.Show(Application.Current.MainWindow, "Failed to load quest file. The file may be corrupted or invalid.");
                    return;
                }

                _currentFilePath = filePath;
                UpdateControlsFromQuest(loadedQuest);
                CurrentTradeDeal = loadedQuest;
                UpdateJsonPreview();
            }
            catch (Exception ex)
            {
                MessageBox.Show(Application.Current.MainWindow, $"Error loading quest file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateControlsFromQuest(TradeDeal quest)
        {
            AssociatedNpc = quest.AssociatedNpc;
            Title = quest.Title;
            Tier = quest.Tier;
            Description = quest.Description;
            TimeLimitHours = quest.TimeLimitHours;

            if (quest.RewardPool != null && quest.RewardPool.Count > 0)
            {
                RewardPool reward = quest.RewardPool[0];
                NormalReward = reward.CurrencyNormal;
                GoldReward = reward.CurrencyGold;
                FameReward = reward.Fame;
                SkillsList = reward.Skills != null && reward.Skills.Count > 0 ? new ObservableCollection<SkillReward>(reward.Skills) : new ObservableCollection<SkillReward>();
                TradeDealsList = reward.TradeDeals != null && reward.TradeDeals.Count > 0 ? new ObservableCollection<TradeDealReward>(reward.TradeDeals) : new ObservableCollection<TradeDealReward>();
            }
            else
            {
                NormalReward = 0;
                GoldReward = 0;
                FameReward = 0;
                SkillsList = new ObservableCollection<SkillReward>();
                TradeDealsList = new ObservableCollection<TradeDealReward>();
            }

            ConditionsList.Clear();
            if (quest.Conditions != null)
            {
                foreach (var condition in quest.Conditions)
                {
                    ConditionsList.Add(condition);
                }
            }
        }

        // Data binding helpers
        private string _associatedNpc = "Armorer";
        public string AssociatedNpc
        {
            get => _associatedNpc;
            set { _associatedNpc = value; OnPropertyChanged(); UpdateJsonPreview(); }
        }

        private string _title = "New Quest";
        public string Title
        {
            get => _title;
            set { _title = value; OnPropertyChanged(); _currentFilePath = null; UpdateJsonPreview(); }
        }

        private int _tier = 1;
        public int Tier
        {
            get => _tier;
            set { _tier = value; OnPropertyChanged(); _currentFilePath = null; UpdateJsonPreview(); }
        }

        private string _description = "Quest description...";
        public string Description
        {
            get => _description;
            set { _description = value; OnPropertyChanged(); UpdateJsonPreview(); }
        }

        private double _timeLimitHours = 24;
        public double TimeLimitHours
        {
            get => _timeLimitHours;
            set { _timeLimitHours = value; OnPropertyChanged(); _currentFilePath = null; UpdateJsonPreview(); }
        }

        private int _normalReward = 0;
        public int NormalReward
        {
            get => _normalReward;
            set { _normalReward = value; OnPropertyChanged(); UpdateJsonPreview(); }
        }

        private int _goldReward = 0;
        public int GoldReward
        {
            get => _goldReward;
            set { _goldReward = value; OnPropertyChanged(); UpdateJsonPreview(); }
        }

        private int _fameReward = 0;
        public int FameReward
        {
            get => _fameReward;
            set { _fameReward = value; OnPropertyChanged(); UpdateJsonPreview(); }
        }

        public ObservableCollection<SkillReward> SkillsList { get; set; } = new ObservableCollection<SkillReward>();
        public ObservableCollection<TradeDealReward> TradeDealsList { get; set; } = new ObservableCollection<TradeDealReward>();

        // Theme properties for XAML binding
        public Brush EditWarningBannerBackground { get; set; } = new SolidColorBrush(Color.FromArgb(255, 51, 51, 51));
        public Brush EditWarningTextForeground { get; set; } = new SolidColorBrush(Color.FromArgb(255, 0xEF, 0x53, 0x50));

        private string? _currentFilePath;

        // Commands for condition management (exposed as methods that commands call)
        public void AddEliminationCondition()
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

        public void AddFetchCondition()
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

        public void AddInteractionCondition()
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

        public void RemoveSelectedCondition()
        {
            // This will be called from XAML via a command with parameter
        }

        public void AddCondition(ModelsCondition newCondition)
        {
            ConditionsList.Add(newCondition);
            UpdateJsonPreview();
        }

        public void AddSkillReward()
        {
            RewardPool reward = GetOrCreateCurrentReward();
            if (reward.Skills == null) reward.Skills = new List<SkillReward>();
            if (CalculateTotalRewards(reward) == 5)
            {
                MessageBox.Show(Application.Current.MainWindow, "Maximum 5 total reward points allowed.");
                return;
            }
            // Dialog will be shown from XAML command
        }

        public void AddTradeDeal()
        {
            RewardPool reward = GetOrCreateCurrentReward();
            if (reward.TradeDeals == null) reward.TradeDeals = new List<TradeDealReward>();

            int currentTotal = CalculateTotalRewards(reward);
            int pointsToAdd = (reward.TradeDeals.Count == 0) ? 2 : 1;

            if (currentTotal + pointsToAdd > 5)
            {
                MessageBox.Show(Application.Current.MainWindow,
                    "Maximum 5 total reward points allowed.\n(Reminder: The first trade deal = 2 reward points.)");
                return;
            }
        }

        public static void Log(string message)
        {
            try
            {
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log.txt");
                File.AppendAllText(path, $"{DateTime.Now:O} {message}\n");
            }
            catch { }
        }

        // INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // RelayCommand implementation
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Predicate<object>? _canExecute;

        public RelayCommand(Action<object> execute, Predicate<object?>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;
        public void Execute(object? parameter) => _execute(parameter);
        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }

    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        private readonly Predicate<T?>? _canExecute;

        public RelayCommand(Action<T> execute, Predicate<T?> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter) => _canExecute?.Invoke((T?)parameter) ?? true;
        public void Execute(object? parameter) => _execute((T)parameter!);
        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }

    // Special command classes for dialogs
    public class FileNameFormatClickCommand : ICommand
    {
        private readonly MainWindowViewModel _vm;
        public FileNameFormatClickCommand(MainWindowViewModel vm) => _vm = vm;
        public bool CanExecute(object? parameter) => true;
        public void Execute(object? parameter)
        {
            var dialog = new FilenameFormatDialog(_vm.Settings.FileNameFormat);
            dialog.Owner = Application.Current.MainWindow;
            dialog.ShowDialog();
            if (dialog.IsOkClicked)
            {
                _vm.Settings.FileNameFormat = dialog.NewFormat;
                _vm.SaveSettings();
            }
        }
        public event EventHandler? CanExecuteChanged;
    }

    public class AboutClickCommand : ICommand
    {
        public bool CanExecute(object? parameter) => true;
        public void Execute(object? parameter)
        {
            var dialog = new AboutDialog();
            dialog.Owner = Application.Current.MainWindow;
            dialog.ShowDialog();
        }
        public event EventHandler? CanExecuteChanged;
    }

    public class AppearanceSettingsCommand : ICommand
    {
        private readonly MainWindowViewModel _vm;
        public AppearanceSettingsCommand(MainWindowViewModel vm) => _vm = vm;
        public bool CanExecute(object? parameter) => true;
        public void Execute(object? parameter)
        {
            var dialog = new ColorSettingsDialog(
                Application.Current.MainWindow as MainWindow ?? throw new InvalidOperationException(),
                _vm.Settings.PrimaryColor,
                _vm.Settings.SecondaryColor,
                _vm.Settings.ErrorHighlightColor,
                _vm.Settings.DarkMode);
            dialog.Owner = Application.Current.MainWindow;
            dialog.ShowDialog();
            if (dialog.DialogResult == true)
            {
                _vm.Settings.PrimaryColor = dialog.PrimaryColor;
                _vm.Settings.SecondaryColor = dialog.SecondaryColor;
                _vm.Settings.ErrorHighlightColor = dialog.ErrorColor;
                _vm.Settings.DarkMode = dialog.DarkMode;
                _vm.SaveSettings();
                _vm.ApplyDarkMode(dialog.DarkMode);
                _vm.ApplyTheme(dialog.PrimaryColor, dialog.SecondaryColor);
                _vm.ApplyErrorColor(dialog.ErrorColor);
            }
        }
        public event EventHandler? CanExecuteChanged;
    }

    public class QuestInfoClickCommand : ICommand
    {
        public bool CanExecute(object? parameter) => true;
        public void Execute(object? parameter)
        {
            var dialog = new QuestInfoDialog();
            dialog.Owner = Application.Current.MainWindow;
            dialog.ShowDialog();
        }
        public event EventHandler? CanExecuteChanged;
    }

    public class MenuEditModeToggleCommand : ICommand
    {
        private readonly MainWindowViewModel _vm;
        public MenuEditModeToggleCommand(MainWindowViewModel vm) => _vm = vm;
        public bool CanExecute(object? parameter) => true;
        public void Execute(object? parameter)
        {
            if (parameter is MenuItem menuItem)
            {
                SetEditMode(menuItem.IsChecked == true);
            }
        }
        private void SetEditMode(bool enabled)
        {
            _vm.IsEditMode = enabled;
        }
        public event EventHandler? CanExecuteChanged;
    }
}
