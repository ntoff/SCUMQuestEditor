using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;

namespace SCUMQuestEditor
{
    public partial class EditWeaponDialog : Window
    {
        public List<string> SelectedItems { get; private set; } = new List<string>();
        private List<string> AvailableWeapons { get; set; } = new List<string>();
        private List<string> InitialSelections { get; set; } = new List<string>();
        private ListCollectionView _listCollectionView;
        private HashSet<string> _userSelections = new HashSet<string>();

        public EditWeaponDialog(List<string>? initialSelections = null)
        {
            InitializeComponent();
            InitialSelections = initialSelections ?? new List<string>();
            _userSelections = new HashSet<string>(InitialSelections);

            try
            {
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "_data\\EliminationWeapons.txt");
                if (File.Exists(path))
                {
                    AvailableWeapons = File.ReadAllLines(path).Where(line => !string.IsNullOrEmpty(line)).ToList();
                }
                else
                {
                    AvailableWeapons.Add("DefaultWeapon");
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Error loading weapons: {ex.Message}");
                AvailableWeapons.Add("DefaultWeapon");
            }

            _listCollectionView = (ListCollectionView)CollectionViewSource.GetDefaultView(AvailableWeapons);
            _listCollectionView.Filter = obj => true;
            LstWeapons.ItemsSource = _listCollectionView;

            foreach (var item in AvailableWeapons)
            {
                if (InitialSelections.Contains(item)) LstWeapons.SelectedItems.Add(item);
            }

            LstWeapons.SelectionChanged += LstWeapons_SelectionChanged;
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            SelectedItems = new List<string>(_userSelections);
            DialogResult = true;
            Close();
        }

        private bool _isFiltering = false;

        private void FilterWeapons()
        {
            if (TxtSearch == null || LstWeapons == null || _listCollectionView == null) return;

            _isFiltering = true;

            string filter = TxtSearch.Text.ToLower();
            _listCollectionView.Filter = obj =>
            {
                string weapon = obj as string;
                return string.IsNullOrEmpty(weapon) || weapon.ToLower().Contains(filter);
            };
            _listCollectionView.Refresh();

            foreach (var item in _userSelections)
            {
                if (_listCollectionView.Contains(item) && !LstWeapons.SelectedItems.Contains(item))
                {
                    LstWeapons.SelectedItems.Add(item);
                }
            }

            _isFiltering = false;
        }

        private void LstWeapons_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isFiltering) return;

            if (e.AddedItems != null)
            {
                foreach (var item in e.AddedItems)
                {
                    _userSelections.Add(item.ToString());
                }
            }

            if (e.RemovedItems != null)
            {
                foreach (var item in e.RemovedItems)
                {
                    _userSelections.Remove(item.ToString());
                }
            }
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterWeapons();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
