using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Text.RegularExpressions;

namespace SCUMQuestEditor
{
    public partial class EditRequiredItemsDialog : Window
    {
        private bool _isEditing = false;

        private void UpdateEditButtonsState()
        {
            BtnApply.IsEnabled = _isEditing;
            BtnCancelEdit.IsEnabled = _isEditing;
        }

        public ObservableCollection<RequiredItem> RequiredItems { get; set; } = new ObservableCollection<RequiredItem>();
        public List<string> AvailableItems { get; set; } = new List<string>();

        public EditRequiredItemsDialog(List<RequiredItem> initialItems, List<string> fetchItems)
        {
            InitializeComponent();

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

            AvailableItems = fetchItems ?? new List<string>();
            if (AvailableItems.Count == 0)
            {
                AvailableItems.Add("Empty Item");
            }

            LvRequiredItems.ItemsSource = RequiredItems;
            LvRequiredItems.SelectionMode = SelectionMode.Multiple;
            UpdateEditButtonsState();

            // Validation
            TxtRequiredQty.PreviewTextInput += NumericPreviewTextInput;
            TxtRandomQty.PreviewTextInput += NumericPreviewTextInput;
            TxtMinHealth.PreviewTextInput += NumericPreviewTextInput;
            TxtMinUses.PreviewTextInput += NumericPreviewTextInput;
            TxtMinMass.PreviewTextInput += NumericPreviewTextInput;
            TxtMinResourcePct.PreviewTextInput += NumericPreviewTextInput;
            TxtMinResourceMl.PreviewTextInput += NumericPreviewTextInput;

            DataObject.AddPastingHandler(TxtRequiredQty, new DataObjectPastingEventHandler(NumericPasting));
            DataObject.AddPastingHandler(TxtRandomQty, new DataObjectPastingEventHandler(NumericPasting));
            DataObject.AddPastingHandler(TxtMinHealth, new DataObjectPastingEventHandler(NumericPasting));
            DataObject.AddPastingHandler(TxtMinUses, new DataObjectPastingEventHandler(NumericPasting));
            DataObject.AddPastingHandler(TxtMinMass, new DataObjectPastingEventHandler(NumericPasting));
            DataObject.AddPastingHandler(TxtMinResourcePct, new DataObjectPastingEventHandler(NumericPasting));
            DataObject.AddPastingHandler(TxtMinResourceMl, new DataObjectPastingEventHandler(NumericPasting));

            FilterAvailableItems();
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
            if (LstAcceptedItems.Items.Count > 0)
            {
                if (LstAcceptedItems.SelectedItems.Count > 0)
                {
                    List<object> keepSelection = new List<object>();
                    foreach (var sel in LstAcceptedItems.SelectedItems)
                    {
                        if (LstAcceptedItems.Items.Contains(sel)) keepSelection.Add(sel);
                    }
                    LstAcceptedItems.SelectedItems.Clear();
                    foreach (var sel in keepSelection) LstAcceptedItems.SelectedItems.Add(sel);
                }
            }
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterAvailableItems();
        }

        private void ApplySelectedItem()
        {
            if (LstAcceptedItems.SelectedItems.Count == 0)
            {
                MessageBox.Show("Please select at least one item.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Handle multiple selected items in the top list view
            var selectedReqItems = LvRequiredItems.SelectedItems.Cast<RequiredItem>().ToList();

            if (selectedReqItems.Count > 0)
            {
                // Update all selected required items
                foreach (var selectedReqItem in selectedReqItems)
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
            }
            else
            {
                // Add new items
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
            _isEditing = false;
            UpdateEditButtonsState();

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

            // Create a SINGLE RequiredItem containing ALL selected items in its AcceptedItems list
            var newItem = new RequiredItem
            {
                AcceptedItems = LstAcceptedItems.SelectedItems.Cast<object>().Select(s => s.ToString()).ToList(),
                RequiredNum = 1 // Default value. Change to int.TryParse(TxtRequiredQty.Text, out int q) ? q : 1 if you want it to respect the input field.
            };

            RequiredItems.Add(newItem);

            // Refresh ListView and select the newly added item
            LvRequiredItems.ItemsSource = null;
            LvRequiredItems.ItemsSource = RequiredItems;

            if (newItem != null)
            {
                LvRequiredItems.SelectedItem = newItem;
                LvRequiredItems.ScrollIntoView(newItem);
                LoadSelectedRequiredItemIntoEditor();
            }
        }


        private void LoadSelectedRequiredItemIntoEditor()
        {
            // Load the first selected item if multiple are selected, or the single selected item
            if (LvRequiredItems.SelectedItems.Count == 0) return;

            var selectedReq = LvRequiredItems.SelectedItems[0] as RequiredItem;
            if (selectedReq == null) return;

            FilterAvailableItems();

            LstAcceptedItems.SelectedItems.Clear();
            foreach (var acceptedItem in selectedReq.AcceptedItems)
            {
                if (LstAcceptedItems.Items.Contains(acceptedItem))
                    LstAcceptedItems.SelectedItems.Add(acceptedItem);
            }

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

        private void BtnEditSelectedRequiredItem_Click(object sender, RoutedEventArgs e)
        {
            LoadSelectedRequiredItemIntoEditor();
            _isEditing = true;
            UpdateEditButtonsState();
        }

        private void BtnRemoveSelectedRequiredItem_Click(object sender, RoutedEventArgs e)
        {
            if (LvRequiredItems.SelectedItems.Count == 0)
            {
                MessageBox.Show("Please select an item to remove.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var itemsToRemove = LvRequiredItems.SelectedItems.Cast<RequiredItem>().ToList();
            foreach (var item in itemsToRemove)
            {
                RequiredItems.Remove(item);
            }

            LvRequiredItems.ItemsSource = null;
            LvRequiredItems.ItemsSource = RequiredItems;
        }

        private void BtnApply_Click(object sender, RoutedEventArgs e)
        {
            ApplySelectedItem();
        }

        private void BtnCancelEdit_Click(object sender, RoutedEventArgs e)
        {
            _isEditing = false;
            UpdateEditButtonsState();

            ClearInputFields();
            FilterAvailableItems();
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
    }
}
