using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Finance.Settings;

namespace Finance.Converters
{
    public class CurrencyVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var currencyCharCode = (string) value;
            return currencyCharCode == AppSettings.CurrentCurrencyCharCode ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}