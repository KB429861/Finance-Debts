using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Finance.Converters
{
    public class AmountColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var amount = (double) value;

            if (amount > 0)
                return (Brush) Application.Current.Resources["GreenAccentBrush"];

            if (amount < 0)
                return (Brush) Application.Current.Resources["RedAccentBrush"];

            return (Brush) Application.Current.Resources["GreyAccentBrush"];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}