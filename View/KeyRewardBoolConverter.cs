using System;
using System.Windows.Data;

namespace Charrmander.View
{
    /// <summary>
    /// Converts a check box checked state to a key if <c>True</c>, empty string otherwise.
    /// </summary>
    public class KeyRewardBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (value is bool hasKeyReward && hasKeyReward)
                ? "\U0001F5DD"
                : string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
