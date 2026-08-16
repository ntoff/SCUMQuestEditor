using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Text.RegularExpressions;

namespace SCUMQuestEditor
{
    public partial class AddTradeDealDialog : Window
    {
        public static List<string> AvailableItems { get; set; } = new List<string>();

        public bool IsOkClicked => DialogResult == true;
        public string SelectedItemName => LstItems?.SelectedItem?.ToString() ?? "";

        public double Price { get => double.TryParse(TxtPrice?.Text ?? "0", out double val) ? val : 0; }
        public int Amount { get => int.TryParse(TxtAmount?.Text ?? "1", out int val) ? val : 1; }
        public double Fame { get => double.TryParse(TxtFame?.Text ?? "0", out double val) ? val : 0; }
        public bool AllowExcluded => ChkAllowExcluded?.IsChecked ?? false;

        private readonly string _traderName;

        public AddTradeDealDialog(TradeDealReward? existingReward = null, string? traderName = null)
        {
            InitializeComponent();

            Title = existingReward != null ? "Edit Trade Deal" : "Add Trade Deal";
            _traderName = traderName ?? "";

            LoadTradeItems();
            FilterItems();

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

            TxtPrice.PreviewTextInput += NumericPreviewTextInput;
            TxtAmount.PreviewTextInput += NumericPreviewTextInput;
            TxtFame.PreviewTextInput += NumericPreviewTextInput;

            DataObject.AddPastingHandler(TxtPrice, new DataObjectPastingEventHandler(NumericPasting));
            DataObject.AddPastingHandler(TxtAmount, new DataObjectPastingEventHandler(NumericPasting));
            DataObject.AddPastingHandler(TxtFame, new DataObjectPastingEventHandler(NumericPasting));
        }

        private void LoadTradeItems()
        {
            AvailableItems.Clear();

            try
            {
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "_data\\TradeItems.json");
                if (File.Exists(path))
                {
                    string json = File.ReadAllText(path);
                    var traders = JsonSerializer.Deserialize<List<TraderData>>(json);

                    if (traders != null && !string.IsNullOrEmpty(_traderName))
                    {
                        var trader = traders.FirstOrDefault(t => t.TraderName == _traderName);
                        if (trader?.Children != null)
                        {
                            AvailableItems.AddRange(trader.Children);
                        }
                    }

                    if (AvailableItems.Count == 0)
                    {
                        foreach (var trader in (traders ?? Enumerable.Empty<TraderData>()))
                        {
                            if (trader?.Children != null)
                            {
                                AvailableItems.AddRange(trader.Children);
                            }
                        }
                    }
                }
                else
                {
                    AvailableItems.Add("DefaultItem");
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Error loading TradeItems.json: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                AvailableItems.Add("DefaultItem");
            }
        }

        private void FilterItems()
        {
            if (TxtSearch == null || LstItems == null) return;

            if (AvailableItems == null)
            {
                AvailableItems = new List<string>();
            }

            string filter = TxtSearch.Text.ToLower();
            LstItems.Items.Clear();

            foreach (var item in AvailableItems)
            {
                if (item != null && item.ToLower().Contains(filter))
                {
                    LstItems.Items.Add(item);
                }
            }

            if (LstItems.Items.Count > 0)
            {
                LstItems.SelectedIndex = 0;
            }
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterItems();
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

        private class TraderData
        {
            public string? TraderName { get; set; }
            public List<string>? Children { get; set; }
        }
    }
}
