using System;
using System.Globalization;
using System.Windows.Data;
using Finance.Settings;

namespace Finance.Converters
{
    public class AmountConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var amount = (double) value;
            string result;
            switch (AppSettings.NumberOfDigits)
            {
                case 0:
                    result = amount.ToString("#0", CultureInfo.InvariantCulture);
                    break;
                case 1:
                    result = amount.ToString("#0.0", CultureInfo.InvariantCulture);
                    break;
                default:
                    result = amount.ToString("#0.00", CultureInfo.InvariantCulture);
                    break;
            }
            if (amount > 0)
                return "+" + result;
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}