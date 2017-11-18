using System;
using System.Globalization;
using System.Windows.Data;
using Finance.Enums;
using Finance.Resources;

namespace Finance.Converters
{
    public class GroupConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((string) value == UseGroup.Deleted.ToString())
                return " - " + AppResources.AccountDeleted;
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}