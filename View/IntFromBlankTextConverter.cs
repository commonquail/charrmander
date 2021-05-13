using System;
using System.Windows.Data;

namespace Charrmander.View
{
    /// <summary>
    /// Converts a blank or null string to <c>0</c>.
    /// </summary>
    public class IntFromBlankTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var str = value?.ToString();
            if (string.IsNullOrWhiteSpace(str))
            {
                return 0;
            }
            _ = int.TryParse(str, out int res);

            return res;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value.ToString();
        }
    }
}
