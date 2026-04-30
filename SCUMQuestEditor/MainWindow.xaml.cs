#nullable enable
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

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

        // Changed to default null so it doesn't appear in JSON unless items are added
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public List<SkillReward>? Skills { get; set; } = null;

        // Changed to default null so it doesn't appear in JSON unless items are added
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
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
    }

    public class AddSkillRewardDialog : Window
    {
        // Explicitly nullable to handle the case where properties aren't set by XAML
        public ComboBox? CbSkill;
        public TextBox? TxtExperience;
        public bool IsOkClicked => DialogResult == true;
        public string SelectedSkill => CbSkill?.SelectedItem?.ToString() ?? "";

        public double Experience
        {
            get
            {
                string text = TxtExperience?.Text ?? "0";
                if (double.TryParse(text, out double exp))
                    return exp;
                return 0;
            }
        }

        public AddSkillRewardDialog()
        {
            Title = "Add Skill Reward";
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

            // --- Skill Row ---
            Label lblSkill = new Label
            {
                Content = "Skill:",
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 5, 10, 5)
            };

            CbSkill = new ComboBox
            {
                Margin = new Thickness(10, 5, 10, 5),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                SelectedIndex = 0
            };

            List<string> skills = new List<string>
            {
                "Archery", "Aviation", "Awareness", "Boxing", "Camouflage", "Cooking", "Demolition",
                "Driving", "Endurance", "Engineering", "Farming", "Handgun", "Medical", "Melee Weapons",
                "Motorcycle", "Rifles", "Running", "Sniping", "Stealth", "Survival", "Tactics", "Thievery"
            };
            foreach (var skill in skills)
            {
                CbSkill.Items.Add(skill);
            }
            CbSkill.SelectedItem = skills[0];

            Grid.SetRow(lblSkill, 0);
            Grid.SetColumn(lblSkill, 0);
            Grid.SetRow(CbSkill, 0);
            Grid.SetColumn(CbSkill, 1);

            grid.Children.Add(lblSkill);
            grid.Children.Add(CbSkill);

            // --- Experience Row ---
            Label lblExp = new Label
            {
                Content = "Experience:",
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 5, 10, 5)
            };

            TxtExperience = new TextBox
            {
                Margin = new Thickness(10, 5, 10, 5),
                Text = "1000",
                TextAlignment = TextAlignment.Left
            };
            TxtExperience.PreviewTextInput += NumericPreviewTextInput;

            DataObjectPastingEventHandler pastingHandler = new DataObjectPastingEventHandler(NumericPasting);
            DataObject.AddPastingHandler(TxtExperience, pastingHandler);

            Grid.SetRow(lblExp, 1);
            Grid.SetColumn(lblExp, 0);
            Grid.SetRow(TxtExperience, 1);
            Grid.SetColumn(TxtExperience, 1);

            grid.Children.Add(lblExp);
            grid.Children.Add(TxtExperience);

            // --- Buttons Row ---
            Button btnOk = new Button
            {
                Content = "OK",
                Width = 75,
                Height = 25,
                Margin = new Thickness(10, 10, 5, 10),
                HorizontalAlignment = HorizontalAlignment.Right
            };
            Button btnCancel = new Button
            {
                Content = "Cancel",
                Width = 75,
                Height = 25,
                Margin = new Thickness(5, 10, 10, 10),
                HorizontalAlignment = HorizontalAlignment.Right
            };
            btnOk.Click += (s, e) => { DialogResult = true; Close(); };
            btnCancel.Click += (s, e) => { DialogResult = false; Close(); };

            StackPanel btnPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right
            };
            btnPanel.Children.Add(btnOk);
            btnPanel.Children.Add(btnCancel);

            Grid.SetRow(btnPanel, 2);
            Grid.SetColumn(btnPanel, 0);
            Grid.SetColumnSpan(btnPanel, 2);

            grid.Children.Add(btnPanel);

            Content = grid;
        }

        private void NumericPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, @"^\d*$");
        }

        private void NumericPasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string text = (string)e.DataObject.GetData(typeof(string));
                e.Handled = !Regex.IsMatch(text, @"^\d*$");
            }
            else
            {
                e.Handled = true;
            }
        }
    }

    // NEW DIALOG CLASS FOR TRADE DEALS
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

        public double Price
        {
            get
            {
                if (double.TryParse(TxtPrice?.Text ?? "0", out double val)) return val;
                return 0;
            }
        }

        public int Amount
        {
            get
            {
                if (int.TryParse(TxtAmount?.Text ?? "1", out int val)) return val;
                return 1;
            }
        }

        public double Fame
        {
            get
            {
                if (double.TryParse(TxtFame?.Text ?? "0", out double val)) return val;
                return 0;
            }
        }

        public bool AllowExcluded => ChkAllowExcluded?.IsChecked ?? false;

        public AddTradeDealDialog()
        {
            Title = "Add Trade Deal";
            Width = 500;
            Height = 480;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            ResizeMode = ResizeMode.NoResize;

            Grid grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(350) });

            // Rows: Search, List, Price, Amount, Fame, Checkbox, Buttons
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Search
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(200) }); // List
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Price
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Amount
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Fame
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Checkbox
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Buttons

            // --- Row 0: Search ---
            TxtSearch = new TextBox
            {
                Text = "Type to search items",
                Margin = new Thickness(10),
                Padding = new Thickness(5),
            };
            TxtSearch.TextChanged += (s, e) => FilterItems();
            Grid.SetRow(TxtSearch, 0);
            Grid.SetColumnSpan(TxtSearch, 2);
            grid.Children.Add(TxtSearch);

            // --- Row 1: List Box ---
            LstItems = new ListBox
            {
                Margin = new Thickness(10),
                HorizontalContentAlignment = HorizontalAlignment.Stretch
            };
            foreach (var item in AvailableItems)
            {
                LstItems.Items.Add(item);
            }
            if (LstItems.Items.Count > 0) LstItems.SelectedIndex = 0;

            Grid.SetRow(LstItems, 1);
            Grid.SetColumnSpan(LstItems, 2);
            grid.Children.Add(LstItems);

            // --- Row 2: Price ---
            Label lblPrice = new Label { Content = "Price:", HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 5, 10, 5) };
            TxtPrice = new TextBox
            {
                Text = "50",
                Margin = new Thickness(10, 5, 10, 5),
                TextAlignment = TextAlignment.Left
            };
            TxtPrice.PreviewTextInput += NumericPreviewTextInput;

            Grid.SetRow(lblPrice, 2);
            Grid.SetColumn(lblPrice, 0);
            Grid.SetRow(TxtPrice, 2);
            Grid.SetColumn(TxtPrice, 1);

            grid.Children.Add(lblPrice);
            grid.Children.Add(TxtPrice);

            // --- Row 3: Amount ---
            Label lblAmount = new Label { Content = "Amount:", HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 5, 10, 5) };
            TxtAmount = new TextBox
            {
                Text = "1",
                Margin = new Thickness(10, 5, 10, 5),
                TextAlignment = TextAlignment.Left
            };
            TxtAmount.PreviewTextInput += NumericPreviewTextInput;

            Grid.SetRow(lblAmount, 3);
            Grid.SetColumn(lblAmount, 0);
            Grid.SetRow(TxtAmount, 3);
            Grid.SetColumn(TxtAmount, 1);

            grid.Children.Add(lblAmount);
            grid.Children.Add(TxtAmount);

            // --- Row 4: Fame ---
            Label lblFame = new Label { Content = "Fame Required:", HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 5, 10, 5) };
            TxtFame = new TextBox
            {
                Text = "25",
                Margin = new Thickness(10, 5, 10, 5),
                TextAlignment = TextAlignment.Left
            };
            TxtFame.PreviewTextInput += NumericPreviewTextInput;

            Grid.SetRow(lblFame, 4);
            Grid.SetColumn(lblFame, 0);
            Grid.SetRow(TxtFame, 4);
            Grid.SetColumn(TxtFame, 1);

            grid.Children.Add(lblFame);
            grid.Children.Add(TxtFame);

            // --- Row 5: Checkbox ---
            ChkAllowExcluded = new CheckBox
            {
                Content = "Allow Excluded",
                IsChecked = true,
                Margin = new Thickness(10, 10, 0, 10),
                HorizontalAlignment = HorizontalAlignment.Left
            };
            Grid.SetRow(ChkAllowExcluded, 5);
            Grid.SetColumnSpan(ChkAllowExcluded, 2);
            grid.Children.Add(ChkAllowExcluded);

            // --- Row 6: Buttons ---
            Button btnOk = new Button
            {
                Content = "OK",
                Width = 75,
                Height = 25,
                Margin = new Thickness(0, 10, 10, 10),
                HorizontalAlignment = HorizontalAlignment.Right
            };
            Button btnCancel = new Button
            {
                Content = "Cancel",
                Width = 75,
                Height = 25,
                Margin = new Thickness(0, 10, 10, 10),
                HorizontalAlignment = HorizontalAlignment.Right
            };
            btnOk.Click += (s, e) => { DialogResult = true; Close(); };
            btnCancel.Click += (s, e) => { DialogResult = false; Close(); };

            StackPanel btnPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right
            };
            btnPanel.Children.Add(btnOk);
            btnPanel.Children.Add(btnCancel);

            Grid.SetRow(btnPanel, 6);
            Grid.SetColumn(btnPanel, 1);
            grid.Children.Add(btnPanel);

            Content = grid;
        }

        private void FilterItems()
        {
            if (TxtSearch == null || LstItems == null) return;
            string filter = TxtSearch.Text.ToLower();

            LstItems.Items.Clear();
            foreach (var item in AvailableItems)
            {
                if (item.ToLower().Contains(filter))
                {
                    LstItems.Items.Add(item);
                }
            }
            if (LstItems.Items.Count > 0) LstItems.SelectedIndex = 0;
        }

        private void NumericPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, @"^\d*$");
        }
    }

    // Helper class for Conditions Tab
    public class Condition
    {
        public string Type { get; set; } = "";
        public string Caption { get; set; } = "";
        public int Sequence { get; set; } = 0;
        public bool AutoComplete { get; set; } = false;
        public string Details { get; set; } = "";
        public List<string> TargetCharacterTypes { get; set; } = new List<string>();
    }

    public partial class MainWindow : Window
    {
        // Static list to hold items loaded from file
        public static List<string> TradeItems { get; private set; } = new List<string> { "Default Item" };

        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            LoadTradeItems();
            AddTradeDealDialog.AvailableItems = TradeItems;
            UpdateJsonPreview();

            // Initialize Conditions List
            InitConditions();
        }

        private void LoadTradeItems()
        {
            try
            {
                // Looks for TradeItems.txt in the directory where the executable is running
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TradeItems.txt");
                if (File.Exists(path))
                {
                    // FIX: Convert string[] to List<string>
                    TradeItems = File.ReadAllLines(path).ToList();
                    Console.WriteLine($"Loaded {TradeItems.Count} items from TradeItems.txt");
                }
                else
                {
                    Console.WriteLine("TradeItems.txt not found.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading TradeItems.txt: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MenuItem_New_Click(object sender, RoutedEventArgs e) { }
        private void MenuItem_Open_Click(object sender, RoutedEventArgs e) { }
        private void MenuItem_Save_Click(object sender, RoutedEventArgs e) { }

        private void TxtInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateJsonPreview();
        }

        private void CbNpc_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateJsonPreview();
        }

        private void BtnAddSkillReward_Click(object sender, RoutedEventArgs e)
        {
            RewardPool reward = GetOrCreateCurrentReward();

            if (reward.Skills == null)
            {
                reward.Skills = new List<SkillReward>();
            }
            // Check against TOTAL rewards limit (5)
            int currentTotal = CalculateTotalRewards(reward);

            if (currentTotal == 5)
            {
                MessageBox.Show("Maximum 5 total reward points allowed.");
                return;
            }

            AddSkillRewardDialog dialog = new AddSkillRewardDialog();
            dialog.Owner = this;

            if (dialog.ShowDialog() == true)
            {

                reward.Skills.Add(new SkillReward
                {
                    Skill = dialog.SelectedSkill,
                    Experience = dialog.Experience
                });

                RefreshSkillListView();
                UpdateJsonPreview();
            }
        }


        private void BtnRemoveSelectedSkill_Click(object sender, RoutedEventArgs e)
        {
            RewardPool reward = GetOrCreateCurrentReward();
            if (reward.Skills == null) return;

            if (LvSkills.SelectedItem is SkillReward selectedSkill)
            {
                reward.Skills.Remove(selectedSkill);
                // CRITICAL FIX: Do not set to null. Keep it as an empty list to maintain binding.
                if (reward.Skills.Count == 0)
                {
                    reward.Skills = new List<SkillReward>();
                }

                RefreshSkillListView();
                UpdateJsonPreview();
            }
        }

        private void BtnAddTradeDeal_Click(object sender, RoutedEventArgs e)
        {
            RewardPool reward = GetOrCreateCurrentReward();

            if (reward.TradeDeals == null)
            {
                reward.TradeDeals = new List<TradeDealReward>();
            }

            int currentTotal = CalculateTotalRewards(reward);

            // Determine how many points the new trade deal will add
            int pointsToAdd = (reward.TradeDeals.Count == 0) ? 2 : 1;

            if (currentTotal + pointsToAdd > 5)
            {
                MessageBox.Show("Maximum 5 total reward points allowed.\n(Reminder: The first trade deal = 2 reward points.)");
                return;
            }

            // Open the new dialog
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


        private void BtnRemoveSelectedTradeDeal_Click(object sender, RoutedEventArgs e)
        {
            RewardPool reward = GetOrCreateCurrentReward();
            if (reward.TradeDeals == null) return;

            if (LvTradeDeals.SelectedItem is TradeDealReward selectedDeal)
            {
                reward.TradeDeals.Remove(selectedDeal);
                // CRITICAL FIX: Do not set to null. Keep it as an empty list to maintain binding.
                if (reward.TradeDeals.Count == 0)
                {
                    reward.TradeDeals = new List<TradeDealReward>();
                }

                RefreshTradeDealListView();
                UpdateJsonPreview();
            }
        }

        private RewardPool GetOrCreateCurrentReward()
        {
            if (CurrentTradeDeal.RewardPool == null)
            {
                CurrentTradeDeal.RewardPool = new List<RewardPool>();
            }

            if (CurrentTradeDeal.RewardPool.Count == 0)
            {
                CurrentTradeDeal.RewardPool.Add(new RewardPool());
            }
            return CurrentTradeDeal.RewardPool[0];
        }

        private void RefreshSkillListView()
        {
            RewardPool reward = GetOrCreateCurrentReward();
            // Force a refresh by temporarily clearing and re-setting the ItemsSource
            LvSkills.ItemsSource = null;
            LvSkills.ItemsSource = reward.Skills;
        }

        private void RefreshTradeDealListView()
        {
            RewardPool reward = GetOrCreateCurrentReward();
            // Force a refresh by temporarily clearing and re-setting the ItemsSource
            LvTradeDeals.ItemsSource = null;
            LvTradeDeals.ItemsSource = reward.TradeDeals;
        }

        private TradeDeal CurrentTradeDeal { get; set; } = new TradeDeal();

        private int CalculateTotalRewards(RewardPool reward)
        {
            int total = 0;

            if (reward.CurrencyNormal > 0 || reward.CurrencyGold > 0 || reward.Fame > 0)
            {
                total += 1;
            }

            if (reward.Skills != null && reward.Skills.Count > 0)
            {
                total += reward.Skills.Count;
            }

            if (reward.TradeDeals != null && reward.TradeDeals.Count > 0)
            {
                total += 2;
                if (reward.TradeDeals.Count > 1)
                {
                    total += (reward.TradeDeals.Count - 1);
                }
            }

            return total;
        }

        private void UpdateJsonPreview()
        {
            try
            {
                RewardPool reward = GetOrCreateCurrentReward();

                string npc = "Armorer";
                if (CbNpc?.SelectedItem is ComboBoxItem selectedItem)
                {
                    npc = selectedItem.Content?.ToString() ?? "Armorer";
                }

                int tier = 1;
                if (!int.TryParse(TxtTier?.Text ?? "1", out tier)) tier = 1;

                string title = TxtTitle?.Text ?? "New Quest";
                string description = TxtDescription?.Text ?? "Quest description...";

                double timeLimit = 0.5;
                if (!double.TryParse(TxtTimeLimit?.Text ?? "0.5", out timeLimit)) timeLimit = 0.5;

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

                if (TxtTotalRewards != null)
                {
                    TxtTotalRewards.Text = $"Total Rewards: {totalRewards}/5";
                }

                CurrentTradeDeal.AssociatedNpc = npc;
                CurrentTradeDeal.Tier = tier;
                CurrentTradeDeal.Title = title;
                CurrentTradeDeal.Description = description;
                CurrentTradeDeal.TimeLimitHours = timeLimit;
                CurrentTradeDeal.RewardPool[0] = reward;

                var options = new JsonSerializerOptions { WriteIndented = true };
                options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault;

                string json = JsonSerializer.Serialize(CurrentTradeDeal, options);

                if (TxtJson != null)
                {
                    TxtJson.Text = json;
                }
            }
            catch (Exception ex)
            {
                if (TxtJson != null)
                {
                    TxtJson.Text = $"Error generating JSON: {ex.Message}";
                }
            }
        }

        private void NumericPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, @"^\d*$");
        }

        private void NumericPasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string text = (string)e.DataObject.GetData(typeof(string));
                e.Handled = !Regex.IsMatch(text, @"^\d*$");
            }
            else
            {
                e.Handled = true;
            }
        }

        private void FloatPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            string currentText = ((TextBox)sender).Text;
            if (currentText.Contains(".") && e.Text == ".")
            {
                e.Handled = true;
                return;
            }
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
                    if (text.Contains(".") && currentText.Contains("."))
                    {
                        e.Handled = true;
                    }
                    else
                    {
                        e.Handled = false;
                    }
                }
                else
                {
                    e.Handled = true;
                }
            }
            else
            {
                e.Handled = true;
            }
        }

        // Conditions Tab Logic
        // FIX: Changed from List<Condition> to ObservableCollection<Condition> to ensure UI updates immediately
        private ObservableCollection<Condition> ConditionsList { get; set; } = new ObservableCollection<Condition>();

        private void InitConditions()
        {
            LvConditions.ItemsSource = ConditionsList;
        }

        // FIX: Made public so AddEliminationDialog can call it
        public void AddCondition(Condition newCondition)
        {
            ConditionsList.Add(newCondition);
            // Scroll to bottom or select new item
            LvConditions.SelectedItem = newCondition;
            LvConditions.ScrollIntoView(newCondition);
            UpdateJsonPreview();
        }

        private void BtnAddElimination_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AddEliminationDialog();
            dialog.Owner = this;
            dialog.ShowDialog();
        }

        private void BtnAddFetchCondition_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implement actual logic for Fetch Condition
            var condition = new Condition
            {
                Type = "Fetch",
                Caption = "Fetch Condition",
                Sequence = ConditionsList.Count + 1,
                AutoComplete = false,
                Details = "Item to collect"
            };
            AddCondition(condition);
        }

        private void BtnAddInteraction_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implement actual logic for Interaction
            var condition = new Condition
            {
                Type = "Interaction",
                Caption = "Interaction",
                Sequence = ConditionsList.Count + 1,
                AutoComplete = false,
                Details = "Interaction type"
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
    }

    public class AddEliminationDialog : Window
    {
        public TextBox TblTrackingCaption = new TextBox();
        public NumericUpDown TblSequenceIndex = new NumericUpDown();
        public CheckBox CblCanAutoComplete = new CheckBox();
        public ListBox LstTargetCharacters = new ListBox();
        public ListBox LstCurrentTargets = new ListBox();
        public NumericUpDown NudAmountToKill = new NumericUpDown();
        public ListBox LstAllowedWeapons = new ListBox();
        public ListBox LstCurrentWeapons = new ListBox();

        // CHANGE: Changed from ListBox to ListView here
        public ListView LstCurrentMapLocations = new ListView();

        // Property to hold selected targets from the Edit dialog
        public List<string> SelectedTargetCharacters { get; set; } = new List<string>();

        public AddEliminationDialog()
        {
            Title = "Add New Condition";
            Width = 450;
            Height = 500;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            ResizeMode = ResizeMode.NoResize;

            Grid mainGrid = new Grid();
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // Tab Control
            TabControl tabControl = new TabControl();

            // Tab 1: Common Properties
            TabItem tabCommon = new TabItem { Header = "Common Properties" };
            Grid gridCommon = new Grid();
            gridCommon.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            gridCommon.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            gridCommon.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            gridCommon.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            gridCommon.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(150) });
            gridCommon.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            TblTrackingCaption = new TextBox { Text = "Kill targets", Margin = new Thickness(10) };
            TblSequenceIndex = new NumericUpDown { Value = 0, Margin = new Thickness(10) };
            CblCanAutoComplete = new CheckBox { Margin = new Thickness(10, 10, 0, 10) };
            TextBox txtConditionType = new TextBox { Text = "Elimination", IsReadOnly = true, Margin = new Thickness(10) };

            Grid.SetRow(new Label { Content = "Tracking Caption:", HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 5, 10, 5) }, 0);
            Grid.SetRow(TblTrackingCaption, 0);
            Grid.SetRow(new Label { Content = "Sequence Index:", HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 5, 10, 5) }, 1);
            Grid.SetRow(TblSequenceIndex, 1);
            Grid.SetRow(new Label { Content = "Can Be Auto Completed:", HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 5, 10, 5) }, 2);
            Grid.SetRow(CblCanAutoComplete, 2);
            Grid.SetRow(new Label { Content = "Condition Type:", HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 5, 10, 5) }, 3);
            Grid.SetRow(txtConditionType, 3);

            // Helper to add controls to grid
            void AddToGrid(int row, UIElement element, bool isLabel = false)
            {
                // If it's a label, we need to add the label first, then the element
                if (isLabel)
                {
                    // This logic assumes we are adding the label reference stored in the grid definition 
                    // but since we are creating them inline, we need a different approach for this specific loop.
                    // The original code had a bug in adding children if we just look at the indices, 
                    // but let's stick to the original structure's intent.
                }
            }

            // Re-adding children manually as per original logic to ensure exact match
            // Row 0
            var lbl0 = new Label { Content = "Tracking Caption:", HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 5, 10, 5) };
            gridCommon.Children.Add(lbl0);
            Grid.SetRow(lbl0, 0);
            Grid.SetColumn(lbl0, 0);

            var lbl1 = new Label { Content = "Sequence Index:", HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 5, 10, 5) };
            gridCommon.Children.Add(lbl1);
            Grid.SetRow(lbl1, 1);
            Grid.SetColumn(lbl1, 0);

            var lbl2 = new Label { Content = "Can Be Auto Completed:", HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 5, 10, 5) };
            gridCommon.Children.Add(lbl2);
            Grid.SetRow(lbl2, 2);
            Grid.SetColumn(lbl2, 0);

            var lbl3 = new Label { Content = "Condition Type:", HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 5, 10, 5) };
            gridCommon.Children.Add(lbl3);
            Grid.SetRow(lbl3, 3);
            Grid.SetColumn(lbl3, 0);

            // Actually, the original code did this:
            // gridCommon.Children.Add(gridCommon.Children[0]);
            // This implies the labels were added in a specific order or the code provided in the prompt had a slight logical flaw in how it added children 
            // (it adds the label, then tries to add the label again from the list).
            // To be safe and clean, I will reconstruct the grid additions properly.

            gridCommon.Children.Clear();

            // Row 0
            var lblTracking = new Label { Content = "Tracking Caption:", HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 5, 10, 5) };
            Grid.SetRow(lblTracking, 0);
            Grid.SetColumn(lblTracking, 0);
            gridCommon.Children.Add(lblTracking);

            Grid.SetRow(TblTrackingCaption, 0);
            Grid.SetColumn(TblTrackingCaption, 1);
            gridCommon.Children.Add(TblTrackingCaption);

            // Row 1
            var lblSeq = new Label { Content = "Sequence Index:", HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 5, 10, 5) };
            Grid.SetRow(lblSeq, 1);
            Grid.SetColumn(lblSeq, 0);
            gridCommon.Children.Add(lblSeq);

            Grid.SetRow(TblSequenceIndex, 1);
            Grid.SetColumn(TblSequenceIndex, 1);
            gridCommon.Children.Add(TblSequenceIndex);

            // Row 2
            var lblAuto = new Label { Content = "Can Be Auto Completed:", HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 5, 10, 5) };
            Grid.SetRow(lblAuto, 2);
            Grid.SetColumn(lblAuto, 0);
            gridCommon.Children.Add(lblAuto);

            Grid.SetRow(CblCanAutoComplete, 2);
            Grid.SetColumn(CblCanAutoComplete, 1);
            gridCommon.Children.Add(CblCanAutoComplete);

            // Row 3
            var lblType = new Label { Content = "Condition Type:", HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 5, 10, 5) };
            Grid.SetRow(lblType, 3);
            Grid.SetColumn(lblType, 0);
            gridCommon.Children.Add(lblType);

            Grid.SetRow(txtConditionType, 3);
            Grid.SetColumn(txtConditionType, 1);
            gridCommon.Children.Add(txtConditionType);

            tabCommon.Content = new ScrollViewer { Content = gridCommon, Margin = new Thickness(10) };

            // Tab 2: Elimination Properties
            TabItem tabElimination = new TabItem { Header = "Elimination Properties" };
            Grid gridElimination = new Grid();
            gridElimination.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            gridElimination.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            gridElimination.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            gridElimination.RowDefinitions.Add(new RowDefinition { Height = new GridLength(100) });
            gridElimination.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            gridElimination.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            gridElimination.RowDefinitions.Add(new RowDefinition { Height = new GridLength(100) });
            gridElimination.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(150) });
            gridElimination.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            Button btnEditTargets = new Button { Content = "Edit Target Characters", Margin = new Thickness(10, 5, 10, 5) };
            LstCurrentTargets = new ListBox { Margin = new Thickness(10), Height = 80 };
            NudAmountToKill = new NumericUpDown { Value = 1, Margin = new Thickness(10, 5, 10, 5) };
            Button btnEditWeapons = new Button { Content = "Edit Allowed Weapons", Margin = new Thickness(10, 5, 10, 5) };
            LstCurrentWeapons = new ListBox { Margin = new Thickness(10), Height = 80 };
            TextBlock tbWeaponInfo = new TextBlock
            {
                Text = "If no weapons are specified, all weapons are allowed.\nIf weapons are specified, only kills with those weapons will advance the quest",
                Foreground = Brushes.Blue,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(10, 5, 10, 5),
                FontSize = 10
            };

            // Re-adding children for gridElimination to be safe and correct
            gridElimination.Children.Clear();

            // Row 0
            var lblTargets = new Label { Content = "Target Characters:", HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 5, 10, 5) };
            Grid.SetRow(lblTargets, 0);
            Grid.SetColumn(lblTargets, 0);
            gridElimination.Children.Add(lblTargets);

            Grid.SetRow(btnEditTargets, 0);
            Grid.SetColumn(btnEditTargets, 1);
            gridElimination.Children.Add(btnEditTargets);

            // Row 1
            var lblCurrTargets = new Label { Content = "Current Targets:", HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 5, 10, 5) };
            Grid.SetRow(lblCurrTargets, 1);
            Grid.SetColumn(lblCurrTargets, 0);
            gridElimination.Children.Add(lblCurrTargets);

            Grid.SetRow(LstCurrentTargets, 1);
            Grid.SetColumn(LstCurrentTargets, 1);
            gridElimination.Children.Add(LstCurrentTargets);

            // Row 2
            var lblAmt = new Label { Content = "Amount to kill:", HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 5, 10, 5) };
            Grid.SetRow(lblAmt, 2);
            Grid.SetColumn(lblAmt, 0);
            gridElimination.Children.Add(lblAmt);

            Grid.SetRow(NudAmountToKill, 2);
            Grid.SetColumn(NudAmountToKill, 1);
            gridElimination.Children.Add(NudAmountToKill);

            // Row 3
            var lblWeaps = new Label { Content = "Allowed Weapons:", HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 5, 10, 5) };
            Grid.SetRow(lblWeaps, 3);
            Grid.SetColumn(lblWeaps, 0);
            gridElimination.Children.Add(lblWeaps);

            Grid.SetRow(btnEditWeapons, 3);
            Grid.SetColumn(btnEditWeapons, 1);
            gridElimination.Children.Add(btnEditWeapons);

            // Row 4
            var lblCurrWeaps = new Label { Content = "Current Weapons:", HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 5, 10, 5) };
            Grid.SetRow(lblCurrWeaps, 4);
            Grid.SetColumn(lblCurrWeaps, 0);
            gridElimination.Children.Add(lblCurrWeaps);

            Grid.SetRow(LstCurrentWeapons, 4);
            Grid.SetColumn(LstCurrentWeapons, 1);
            gridElimination.Children.Add(LstCurrentWeapons);

            // Row 5
            Grid.SetRow(tbWeaponInfo, 5);
            Grid.SetColumnSpan(tbWeaponInfo, 2);
            gridElimination.Children.Add(tbWeaponInfo);

            tabElimination.Content = new ScrollViewer { Content = gridElimination, Margin = new Thickness(10) };

            // Tab 3: Map Locations
            TabItem tabMap = new TabItem { Header = "Map Locations" };
            Grid gridMap = new Grid();
            gridMap.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            gridMap.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            gridMap.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            gridMap.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(150) });
            gridMap.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            Button btnEditMapLocations = new Button { Content = "Edit Map Locations", Margin = new Thickness(10, 5, 10, 5) };

            // This is now a ListView, so .View is valid
            GridView lvMapLocationsView = new GridView();
            lvMapLocationsView.Columns.Add(new GridViewColumn { Header = "X", Width = 80, DisplayMemberBinding = new Binding("X") });
            lvMapLocationsView.Columns.Add(new GridViewColumn { Header = "Y", Width = 80, DisplayMemberBinding = new Binding("Y") });
            lvMapLocationsView.Columns.Add(new GridViewColumn { Header = "Z", Width = 80, DisplayMemberBinding = new Binding("Z") });
            lvMapLocationsView.Columns.Add(new GridViewColumn { Header = "Size Factor", Width = 100, DisplayMemberBinding = new Binding("SizeFactor") });

            LstCurrentMapLocations.View = lvMapLocationsView;

            TextBlock tbMapInfo = new TextBlock
            {
                Text = "These locations are shown as circles on the in-game map.",
                Foreground = Brushes.Blue,
                Margin = new Thickness(10, 5, 10, 5),
                FontSize = 10
            };

            // Re-adding children for gridMap to be safe and correct
            gridMap.Children.Clear();

            // Row 0
            var lblMap = new Label { Content = "Map Locations:", HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 5, 10, 5) };
            Grid.SetRow(lblMap, 0);
            Grid.SetColumn(lblMap, 0);
            gridMap.Children.Add(lblMap);

            Grid.SetRow(btnEditMapLocations, 0);
            Grid.SetColumn(btnEditMapLocations, 1);
            gridMap.Children.Add(btnEditMapLocations);

            // Row 1
            var lblCurrMap = new Label { Content = "Current Map Locations:", HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 5, 10, 5) };
            Grid.SetRow(lblCurrMap, 1);
            Grid.SetColumn(lblCurrMap, 0);
            gridMap.Children.Add(lblCurrMap);

            Grid.SetRow(LstCurrentMapLocations, 1);
            Grid.SetColumn(LstCurrentMapLocations, 1);
            gridMap.Children.Add(LstCurrentMapLocations);

            // Row 2
            Grid.SetRow(tbMapInfo, 2);
            Grid.SetColumnSpan(tbMapInfo, 2);
            gridMap.Children.Add(tbMapInfo);

            tabMap.Content = new ScrollViewer { Content = gridMap, Margin = new Thickness(10) };

            tabControl.Items.Add(tabCommon);
            tabControl.Items.Add(tabElimination);
            tabControl.Items.Add(tabMap);
            tabControl.SelectedIndex = 0;

            Grid.SetRow(tabControl, 0);
            mainGrid.Children.Add(tabControl);

            // Buttons
            StackPanel btnPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 10, 10, 10) };
            Button btnOk = new Button { Content = "Save", Width = 75, Height = 25, Margin = new Thickness(0, 0, 10, 0) };
            Button btnCancel = new Button { Content = "Cancel", Width = 75, Height = 25 };

            btnOk.Click += (s, e) => {
                // Create the condition and add it to the list
                int rawSequence = TblSequenceIndex.Value;
                int sequence = Math.Max(0, Math.Min(10, rawSequence));

                List<string> selectedTargets = SelectedTargetCharacters;
                string detailsText = "Target: " + string.Join(", ", selectedTargets);
                if (selectedTargets.Count == 0)
                {
                    detailsText = "No targets selected";
                }

                var condition = new Condition
                {
                    Type = "Elimination",
                    Caption = TblTrackingCaption.Text,
                    Sequence = sequence,
                    AutoComplete = CblCanAutoComplete.IsChecked ?? false,
                    Details = detailsText,
                    TargetCharacterTypes = selectedTargets
                };

                // Access the MainWindow to add the condition
                var mainWindow = Application.Current.MainWindow as MainWindow;
                if (mainWindow != null)
                {
                    mainWindow.AddCondition(condition);
                }

                DialogResult = true;
                Close();
            };
            btnCancel.Click += (s, e) => { DialogResult = false; Close(); };

            btnPanel.Children.Add(btnOk);
            btnPanel.Children.Add(btnCancel);

            Grid.SetRow(btnPanel, 1);
            mainGrid.Children.Add(btnPanel);

            Content = mainGrid;

            // Wire up the Edit Target Characters button
            btnEditTargets.Click += (s, e) => {
                EditTargetCharactersDialog targetDialog = new EditTargetCharactersDialog(SelectedTargetCharacters);
                targetDialog.Owner = this;
                if (targetDialog.ShowDialog() == true)
                {
                    // Update the SelectedTargetCharacters list
                    SelectedTargetCharacters = new List<string>(targetDialog.SelectedItems);

                    // Update the LstCurrentTargets listbox with selected items
                    LstCurrentTargets.Items.Clear();
                    foreach (var item in SelectedTargetCharacters)
                    {
                        LstCurrentTargets.Items.Add(item);
                    }
                }
            };
        }
    }

    // NEW DIALOG: Edit Target Characters
    public class EditTargetCharactersDialog : Window
    {
        public ListBox LstTargetTypes = new ListBox();
        public List<string> SelectedItems { get; private set; } = new List<string>();
        private List<string> AvailableTypes { get; set; } = new List<string>();
        private List<string> InitialSelections { get; set; } = new List<string>();

        public EditTargetCharactersDialog(List<string> initialSelections = null)
        {
            InitialSelections = initialSelections ?? new List<string>();
            Title = "Edit Target Characters";
            Width = 300;
            Height = 400;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            ResizeMode = ResizeMode.NoResize;

            // Load data
            try
            {
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "EliminationTargets.txt");
                if (File.Exists(path))
                {
                    AvailableTypes = File.ReadAllLines(path).ToList();
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

            // Create a DataTemplate for the ListBox items
            DataTemplate checkboxTemplate = new DataTemplate();
            FrameworkElementFactory checkBoxFactory = new FrameworkElementFactory(typeof(CheckBox));

            // Bind the CheckBox content to the item itself
            checkBoxFactory.SetValue(CheckBox.ContentProperty, new Binding("."));

            // Bind IsChecked to the IsSelected property of the parent ListBoxItem
            Binding selectionBinding = new Binding("IsSelected");
            selectionBinding.RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(ListBoxItem), 1);
            selectionBinding.Mode = BindingMode.TwoWay;

            checkBoxFactory.SetBinding(CheckBox.IsCheckedProperty, selectionBinding);

            checkboxTemplate.VisualTree = checkBoxFactory;

            LstTargetTypes.ItemTemplate = checkboxTemplate;
            LstTargetTypes.ItemsSource = AvailableTypes;

            // Set SelectionMode to Multiple to allow checking multiple boxes
            LstTargetTypes.SelectionMode = SelectionMode.Multiple;

            // Set initial selections
            foreach (var item in AvailableTypes)
            {
                if (InitialSelections.Contains(item))
                {
                    LstTargetTypes.SelectedItems.Add(item);
                }
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
                foreach (var item in LstTargetTypes.SelectedItems)
                {
                    SelectedItems.Add(item.ToString());
                }
                DialogResult = true;
                Close();
            };
            btnCancel.Click += (s, e) => {
                DialogResult = false;
                Close();
            };

            btnPanel.Children.Add(btnOk);
            btnPanel.Children.Add(btnCancel);

            Grid.SetRow(btnPanel, 1);
            grid.Children.Add(btnPanel);

            Content = grid;
        }
    }



    // Custom NumericUpDown control (simplified)
    public class NumericUpDown : Grid
    {
        public TextBox TextBox { get; private set; }
        public int Value
        {
            get
            {
                if (int.TryParse(TextBox.Text, out int val))
                    return val;
                return 0;
            }
            set
            {
                TextBox.Text = value.ToString();
            }
        }

        public NumericUpDown()
        {
            ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            TextBox = new TextBox();
            TextBox.PreviewTextInput += NumericPreviewTextInput;
            Grid.SetColumn(TextBox, 0);
            Children.Add(TextBox);

            StackPanel btnPanel = new StackPanel { Orientation = Orientation.Vertical };
            Button btnUp = new Button { Content = "▲", FontSize = 8, Padding = new Thickness(0) };
            Button btnDown = new Button { Content = "▼", FontSize = 8, Padding = new Thickness(0) };
            btnUp.Click += (s, e) => Value++;
            btnDown.Click += (s, e) => Value--;
            btnPanel.Children.Add(btnUp);
            btnPanel.Children.Add(btnDown);
            Grid.SetColumn(btnPanel, 1);
            Children.Add(btnPanel);
        }

        private void NumericPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, @"^\d*$");
        }
    }
}
