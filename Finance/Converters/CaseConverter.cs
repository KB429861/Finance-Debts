using System;
using System.Globalization;
using System.Windows.Data;

namespace Finance.Converters
{
    public class CaseConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var str = (string) value;
            if (parameter.ToString() == "Upper")
                str = str.ToUpperInvariant();
            return str;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}