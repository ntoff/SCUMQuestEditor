#nullable enable
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using SCUMQuestEditor.Models;

using ModelsCondition = SCUMQuestEditor.Models.Condition;

namespace SCUMQuestEditor.Converters
{
    public class ConditionsIndexEqualsConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is ModelsCondition condition)
            {
                var mainWindow = System.Windows.Application.Current.MainWindow as MainWindow;
                if (mainWindow?.ConditionsList != null)
                {
                    bool isFirst = mainWindow.ConditionsList.IndexOf(condition) == 0;
                    return isFirst ? Visibility.Collapsed : Visibility.Visible;
                }
            }
            return Visibility.Visible;
        }

        public object ConvertBack(object value, System.Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ConditionsIndexLessThanConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is ModelsCondition condition)
            {
                var mainWindow = System.Windows.Application.Current.MainWindow as MainWindow;
                if (mainWindow?.ConditionsList != null)
                {
                    int index = mainWindow.ConditionsList.IndexOf(condition);
                    bool isLast = index < 0 || index >= mainWindow.ConditionsList.Count - 1;
                    return isLast ? Visibility.Collapsed : Visibility.Visible;
                }
            }
            return Visibility.Visible;
        }

        public object ConvertBack(object value, System.Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class DarkModeToBackgroundColorConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool darkMode)
            {
                return darkMode ? "#7F000000" : "#E8F5E9";
            }
            return "#7F000000";
        }

        public object ConvertBack(object value, System.Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
