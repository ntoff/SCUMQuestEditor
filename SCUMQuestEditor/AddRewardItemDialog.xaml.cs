using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace SCUMQuestEditor
{
    public partial class AddRewardItemDialog : Window
    {
        private static readonly Func<List<string>?> s_itemsFactory = () =>
        {
            try
            {
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "_data", "rewarditems.txt");
                if (File.Exists(path))
                {
                    var lines = File.ReadAllLines(path);
                    return lines.Select(l => l.Trim()).Where(l => !string.IsNullOrEmpty(l)).ToList();
                }
                else
                {
                    MainWindow.Log("rewarditems.txt file not found.");
                }
            }
            catch (Exception ex)
            {
                MainWindow.Log($"Failed to load rewarditems.txt: {ex.Message}");
            }
            return null;
        };

        private static Lazy<List<string>?> s_cachedItems = new(s_itemsFactory);

        public static void ResetCache()
        {
            s_cachedItems = new Lazy<List<string>?>(s_itemsFactory);
        }

        public static List<string> AvailableItems { get; set; } = new List<string>();

        public bool IsOkClicked => DialogResult == true;
        public string SelectedItemName => LstItems?.SelectedItem?.ToString() ?? "";

        public AddRewardItemDialog()
        {
            InitializeComponent();

            LoadItems();
            FilterItems();
        }

        private void LoadItems()
        {
            AvailableItems.Clear();

            var items = s_cachedItems.Value;

            if (items != null)
            {
                AvailableItems.AddRange(items);
            }

            if (AvailableItems.Count == 0)
            {
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
    }
}
