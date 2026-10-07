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
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using MaterialDesignColors;
using MaterialDesignThemes.Wpf;
using SCUMQuestEditor.Models;
using SCUMQuestEditor.Converters;
using SCUMQuestEditor.ViewModels;
using ModelsCondition = SCUMQuestEditor.Models.Condition;

namespace SCUMQuestEditor
{
    public partial class MainWindow : Window
    {
        public MainWindowViewModel ViewModel { get; }

        private readonly Regex s_digitsRegex = new(@"^\d*$", RegexOptions.Compiled);
        private readonly Regex s_floatRegex = new(@"^\d*\.?\d*$", RegexOptions.Compiled);

        private readonly JsonSerializerOptions s_deserializeOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new ConditionConverter() }
        };

        private readonly JsonSerializerOptions s_serializeIndentedOptions = new()
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            Converters = { new ConditionConverter() }
        };

        public List<string> TradeItems { get; private set; } = new List<string> { "Default Item" };
        public bool TargetsWarningShown { get; set; }
        public bool WeaponsWarningShown { get; set; }
        public bool FetchItemsWarningShown { get; set; }
        public bool TradeItemsWarningShown { get; set; }
        public List<string> FetchItems { get; private set; } = new List<string> { "05_Teeth_Necklace", "12_Gauge_Birdshot" };
        public const int MaxKillAmount = 1000000000;
        public AppSettings Settings { get; } = new AppSettings();
        private SolidColorBrush _errorHighlightBrush = new SolidColorBrush(Color.FromArgb(255, 0xEF, 0x53, 0x50));
        private string? _currentFilePath;

        public MainWindow()
        {
            ViewModel = new MainWindowViewModel();
            InitializeComponent();
            DataContext = ViewModel;
            this.Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            LoadSettings();
            CbTier.Items.Add("1");
            CbTier.Items.Add("2");
            CbTier.Items.Add("3");
            CbTier.Items.Add("4");
            CbTier.SelectedIndex = 0;
            UpdateJsonPreview();
            InitConditions();
            LoadDataFiles();
        }

        private void LoadDataFiles()
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
                MessageBox.Show(
                    "The EliminationTargets.txt data file is missing or could not be loaded. Target character selection may be limited.",
                    "Data File Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

            if (WeaponsWarningShown)
                MessageBox.Show(
                    "The EliminationWeapons.txt data file is missing or could not be loaded. Weapon selection may be limited.",
                    "Data File Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

            if (FetchItemsWarningShown)
                MessageBox.Show(
                    "The FetchItems.txt data file is missing or could not be loaded. Item selection may be limited.",
                    "Data File Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

            if (TradeItemsWarningShown)
                MessageBox.Show(
                    "The TradeItems.json data file is missing or could not be loaded. Trade deal items may not be available.",
                    "Data File Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
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
                string json = JsonSerializer.Serialize(Settings, options);
                File.WriteAllText(path, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving settings: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                Conditions = new List<ModelsCondition>()
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
                    LoadFileFromPath(openFileDialog.FileName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading quest file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MainWindow_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetData(DataFormats.FileDrop) is string[] files)
            {
                bool hasJson = files.Any(f => f.ToLower().EndsWith(".json"));
                if (hasJson)
                    e.Effects = DragDropEffects.Copy;
                else
                    e.Effects = DragDropEffects.None;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }

        private void MainWindow_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetData(DataFormats.FileDrop) is string[] files && files.Length > 0)
            {
                string? jsonFile = files.FirstOrDefault(f => f.ToLower().EndsWith(".json"));
                if (jsonFile != null)
                {
                    LoadFileFromPath(jsonFile);
                }
            }
            e.Handled = true;
        }

        public void LoadFileFromPath(string filePath)
        {
            try
            {
                string jsonContent = File.ReadAllText(filePath);

                TradeDeal? loadedQuest = JsonSerializer.Deserialize(jsonContent, typeof(TradeDeal), s_deserializeOptions) as TradeDeal;

                if (loadedQuest == null)
                {
                    MessageBox.Show("Failed to load quest file. The file may be corrupted or invalid.");
                    return;
                }

                _currentFilePath = filePath;
                UpdateControlsFromQuest(loadedQuest);
                CurrentTradeDeal = loadedQuest;
                UpdateJsonPreview();
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

                    string json = JsonSerializer.Serialize(CurrentTradeDeal, typeof(TradeDeal), s_serializeIndentedOptions);
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
            var dialog = new FilenameFormatDialog(Settings.FileNameFormat);
            dialog.Owner = this;
            dialog.ShowDialog();

            if (dialog.IsOkClicked)
            {
                Settings.FileNameFormat = dialog.NewFormat;
                SaveSettings();
            }
        }

        private bool _editModeEnabled = false;

        private void SetEditMode(bool enabled)
        {
            _editModeEnabled = enabled;
            
            if (_editModeEnabled)
            {
                TxtJson.IsReadOnly = false;
                EditWarningBanner.Visibility = Visibility.Visible;
                EditWarningText.Foreground = _errorHighlightBrush;
                UpdateEditWarningBackground();
                BtnSaveJson.IsEnabled = true;
                BtnSaveJson.Visibility = Visibility.Visible;
            }
            else
            {
                TxtJson.IsReadOnly = true;
                EditWarningBanner.Visibility = Visibility.Collapsed;
                BtnSaveJson.IsEnabled = false;
                BtnSaveJson.Visibility = Visibility.Collapsed;
                UpdateJsonPreview();
            }
        }

        private void UpdateEditWarningBackground()
        {
            string color = Settings.DarkMode ? "#333333" : "#F5F5F5";
            EditWarningBanner.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(color));
        }

        private void MenuEditModeToggle_Click(object sender, RoutedEventArgs e)
        {
            SetEditMode(((MenuItem)sender).IsChecked == true);
        }

        private void BtnSaveJson_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string jsonText = TxtJson.Text ?? string.Empty;
                CurrentTradeDeal = JsonSerializer.Deserialize<TradeDeal>(jsonText, s_serializeIndentedOptions)!;

                if (CurrentTradeDeal == null) return;

                List<string> emptyFields = new();
                bool rewardPoolEmpty = (CurrentTradeDeal.RewardPool == null || CurrentTradeDeal.RewardPool.Count == 0) || CurrentTradeDeal.RewardPool.All(r => r.CurrencyNormal == 0 && r.CurrencyGold == 0 && r.Fame == 0 && r.Skills == null && r.TradeDeals == null);
                if (rewardPoolEmpty) emptyFields.Add("Reward Pool");
                if (CurrentTradeDeal.Conditions == null || CurrentTradeDeal.Conditions.Count == 0) emptyFields.Add("Conditions");

                if (emptyFields.Count > 0)
                {
                    string fieldsList = string.Join(", ", emptyFields);
                    string message = $"The following fields cannot be empty:\n{fieldsList}";
                    SaveValidationDialog dialog = new SaveValidationDialog(message) { Owner = this };
                    dialog.ShowDialog();
                    return;
                }

                string fileName;
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

                if (saveFileDialog.ShowDialog() == true)
                {
                    string outputJson = JsonSerializer.Serialize(CurrentTradeDeal, typeof(TradeDeal), s_serializeIndentedOptions);
                    File.WriteAllText(saveFileDialog.FileName, outputJson);
                    _currentFilePath = saveFileDialog.FileName;
                }
            }
            catch (JsonException ex)
            {
                MessageBox.Show($"Invalid JSON format: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving quest file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AboutDialog();
            dialog.Owner = this;
            dialog.ShowDialog();
        }

        private void AppearanceSettings_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ColorSettingsDialog(this, Settings.PrimaryColor, Settings.SecondaryColor, Settings.ErrorHighlightColor, Settings.DarkMode);
            dialog.Owner = this;
            dialog.ShowDialog();

            if (dialog.DialogResult == true)
            {
                Settings.PrimaryColor = dialog.PrimaryColor;
                Settings.SecondaryColor = dialog.SecondaryColor;
                Settings.ErrorHighlightColor = dialog.ErrorColor;
                Settings.DarkMode = dialog.DarkMode;
                SaveSettings();
                ApplyDarkMode(dialog.DarkMode);
                ApplyTheme(dialog.PrimaryColor, dialog.SecondaryColor);
                ApplyErrorColor(dialog.ErrorColor);
            }
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

            if (_editModeEnabled)
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
            _errorHighlightBrush = new SolidColorBrush(color);
            _errorHighlightBrush.Freeze();
            RefreshJsonPreview();
            if (_editModeEnabled)
            {
                EditWarningText.Foreground = _errorHighlightBrush;
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

        public void RefreshJsonPreview()
        {
            UpdateJsonPreview();
        }

        private void QuestInfo_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new QuestInfoDialog();
            dialog.Owner = this;
            dialog.ShowDialog();
        }

        private string GenerateFileName()
        {
            string npc = CurrentTradeDeal?.AssociatedNpc ?? "Armorer";
            int tier = CurrentTradeDeal?.Tier ?? 1;
            string title = TxtTitle?.Text?.Replace(" ", "_") ?? "quest";
            string traderCode = GetTraderCode(npc);
            string type = GetQuestType();

            string format = Settings.FileNameFormat;
            format = format.Replace("{tier}", tier.ToString());
            format = format.Replace("{trader}", traderCode);
            format = format.Replace("{title}", title);
            format = format.Replace("{type}", type);

            return format + ".json";
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

        private void UpdateControlsFromQuest(TradeDeal quest)
        {
            if (CbNpc != null)
            {
                bool npcFound = false;
                foreach (ComboBoxItem item in CbNpc.Items)
                {
                    if (item.Tag?.ToString() == quest.AssociatedNpc)
                    {
                        CbNpc.SelectionChanged -= CbNpc_SelectionChanged;
                        CbNpc.SelectedItem = item;
                        CbNpc.SelectionChanged += CbNpc_SelectionChanged;
                        npcFound = true;
                        break;
                    }
                }

                if (!npcFound && quest.AssociatedNpc != null)
                {
                    ComboBoxItem newItem = new ComboBoxItem { Content = quest.AssociatedNpc };
                    CbNpc.Items.Add(newItem);
                    CbNpc.SelectionChanged -= CbNpc_SelectionChanged;
                    CbNpc.SelectedItem = newItem;
                    CbNpc.SelectionChanged += CbNpc_SelectionChanged;
                }
            }

            TxtTitle.TextChanged -= TxtInput_TextChanged;
            TxtDescription.TextChanged -= TxtInput_TextChanged;
            TxtTimeLimit.TextChanged -= TxtInput_TextChanged;
            CbTier.SelectionChanged -= CbTier_SelectionChanged;

            if (TxtTitle != null) TxtTitle.Text = quest.Title;
            CbTier!.SelectedIndex = quest.Tier - 1;
            if (TxtDescription != null) TxtDescription.Text = quest.Description;
            if (TxtTimeLimit != null) TxtTimeLimit.Text = quest.TimeLimitHours.ToString("0.0#");

            TxtTitle!.TextChanged += TxtInput_TextChanged;
            TxtDescription!.TextChanged += TxtInput_TextChanged;
            TxtTimeLimit!.TextChanged += TxtInput_TextChanged;
            CbTier.SelectionChanged += CbTier_SelectionChanged;

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

            // --- FIX START ---
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
            // --- FIX END ---

            LvConditions.ItemsSource = null;
            LvConditions.ItemsSource = conditionsView;
            LvConditions.SelectedItem = null;
            

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
            var dialog = new AddTradeDealDialog(this, null, traderName);
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
                AddTradeDealDialog dialog = new AddTradeDealDialog(this, selectedDeal, traderName);
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
                    CurrentTradeDeal.Conditions = conditionsView.Cast<ModelsCondition>().ToList();
                }
                else
                {
                    CurrentTradeDeal.Conditions = ConditionsList.ToList();
                }

                string json = JsonSerializer.Serialize(CurrentTradeDeal, typeof(TradeDeal), s_serializeIndentedOptions);
                if (TxtJson != null) TxtJson.Text = json;

                var warnings = BuildWarnings();

                if (TxtJsonWarning != null)
                {
                    TxtJsonWarning.Text = string.Join("\n", warnings);
                    TxtJsonWarning.Visibility = warnings.Count == 0 ? Visibility.Collapsed : Visibility.Visible;
                    TxtJsonWarning.Foreground = _errorHighlightBrush;
                }

                if (warnings.Count > 0)
                {
                    JsonPreviewTab?.SetValue(TabItem.ForegroundProperty, _errorHighlightBrush);
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

        private List<string> BuildWarnings()
        {
            var warnings = new List<string>();

            bool rewardPoolEmpty = CurrentTradeDeal.RewardPool == null || CurrentTradeDeal.RewardPool.Count == 0 || CurrentTradeDeal.RewardPool.All(r => r.CurrencyNormal == 0 && r.CurrencyGold == 0 && r.Fame == 0 && r.Skills == null && r.TradeDeals == null);
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

        private object CreateWarningHeader()
        {
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(4) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var packIcon = new MaterialDesignThemes.Wpf.PackIcon
            {
                Kind = MaterialDesignThemes.Wpf.PackIconKind.AlertCircleOutline,
                Foreground = _errorHighlightBrush,
                Width = 16,
                Height = 16,
                Margin = new Thickness(0, 0, 4, 0)
            };

            var textBlock = new TextBlock
            {
                Text = "JSON Preview",
                FontWeight = FontWeights.Bold,
                Foreground = _errorHighlightBrush,
                Margin = new Thickness(0, 2, 0, 0)
            };
            
            Grid.SetColumn(textBlock, 0);
            Grid.SetColumn(packIcon, 2);
            grid.Children.Add(textBlock);
            grid.Children.Add(packIcon);

            return grid;
        }


        private void NumericPreviewTextInput(object sender, TextCompositionEventArgs e) { e.Handled = !s_digitsRegex.IsMatch(e.Text); }
        private void NumericPasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string text = (string)e.DataObject.GetData(typeof(string));
                e.Handled = !s_digitsRegex.IsMatch(text);
            }
            else { e.Handled = true; }
        }

        private void FloatPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            string currentText = ((TextBox)sender).Text;
            if (currentText.Contains(".") && e.Text == ".") { e.Handled = true; return; }
            e.Handled = !s_floatRegex.IsMatch(e.Text);
        }

        private void FloatPasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string text = (string)e.DataObject.GetData(typeof(string));
                if (s_floatRegex.IsMatch(text))
                {
                    string currentText = ((TextBox)sender).Text;
                    if (text.Contains(".") && currentText.Contains(".")) { e.Handled = true; }
                    else { e.Handled = false; }
                }
                else { e.Handled = true; }
            }
            else { e.Handled = true; }
        }

        public ObservableCollection<ModelsCondition> ConditionsList { get; set; } = new ObservableCollection<ModelsCondition>();

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

        private void LvConditions_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateConditionTabsState();
            TabConditionEditor.SelectedIndex = 0;

            if (LvConditions.SelectedItem is ModelsCondition selectedCondition)
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

                        BtnEditInteractionLocations.Content = $"Edit Locations ({interactionCondition.Locations.Count})";
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
            bool hasSelection = LvConditions.SelectedItem is ModelsCondition;

            TabFetchProperties.IsEnabled = hasSelection;
            TabInteractionProperties.IsEnabled = hasSelection;
            TabEliminationProperties.IsEnabled = hasSelection;
            TabMapLocationsItem.IsEnabled = hasSelection;

            if (hasSelection && LvConditions.SelectedItem is ModelsCondition selectedCondition)
            {
                string type = selectedCondition.Type.ToLower();
                TabFetchProperties.IsEnabled = type == "fetch";
                TabInteractionProperties.IsEnabled = type == "interaction";
                TabEliminationProperties.IsEnabled = type == "elimination";
            }
        }

        private void LoadConditionToEditor(ModelsCondition condition)
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
                        BtnEditInteractionLocations.Content = $"Edit Locations ({interactionCondition.Locations.Count})";
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

        public void AddCondition(ModelsCondition newCondition)
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
            if (LvConditions.SelectedItem is ModelsCondition selectedCondition)
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
                var dialog = new EditTargetCharactersDialog(this, currentCondition.TargetCharacters);
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
                var dialog = new EditWeaponDialog(this, currentCondition.AllowedWeapons);
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

        private void BtnClearEliminationTargets_Click(object sender, RoutedEventArgs e)
        {
            if (LvConditions.SelectedItem is EliminationCondition currentCondition)
            {
                currentCondition.TargetCharacters = new List<string>();
                EdtEliminationTargets.ItemsSource = null;
                EdtEliminationTargets.ItemsSource = currentCondition.TargetCharacters;
                UpdateJsonPreview();
            }
        }

        private void BtnClearEliminationWeapons_Click(object sender, RoutedEventArgs e)
        {
            if (LvConditions.SelectedItem is EliminationCondition currentCondition)
            {
                currentCondition.AllowedWeapons = new List<string>();
                EdtAllowedWeapons.ItemsSource = null;
                EdtAllowedWeapons.ItemsSource = currentCondition.AllowedWeapons;
                UpdateJsonPreview();
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
            if (LvConditions.SelectedItem is ModelsCondition currentCondition)
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
                    BtnEditInteractionLocations.Content = $"Edit Locations ({currentCondition.Locations.Count})";
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
            if (sender is Button button && button.Tag is ModelsCondition selectedCondition)
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
            if (sender is Button button && button.Tag is ModelsCondition selectedCondition)
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
}
