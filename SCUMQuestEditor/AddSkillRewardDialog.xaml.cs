using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Text.RegularExpressions;

namespace SCUMQuestEditor
{
    public partial class AddSkillRewardDialog : Window
    {
        public bool IsOkClicked => DialogResult == true;

        // FIX: Cast to ComboBoxItem and get the Content string
        public string SelectedSkill
        {
            get
            {
                if (CbSkill?.SelectedItem is ComboBoxItem selectedItem)
                {
                    return selectedItem.Content?.ToString() ?? "";
                }
                return "";
            }
        }

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
            InitializeComponent();

            Title = existingReward != null ? "Edit Skill Reward" : "Add Skill Reward";

            if (existingReward != null)
            {
                foreach (ComboBoxItem item in CbSkill.Items)
                {
                    if (item.Content?.ToString() == existingReward.Skill)
                    {
                        CbSkill.SelectedItem = item;
                        break;
                    }
                }
                TxtExperience.Text = existingReward.Experience.ToString();
            }
            else
            {
                CbSkill.SelectedIndex = 0;
                TxtExperience.Text = "1000";
            }

            TxtExperience.PreviewTextInput += NumericPreviewTextInput;
            DataObject.AddPastingHandler(TxtExperience, new DataObjectPastingEventHandler(NumericPasting));
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
