using Charrmander.Model;
using System;
using System.Windows.Data;

namespace Charrmander.View
{
    /// <summary>
    /// Converts the <see cref="Character.Name"/> property if it is unset.
    /// </summary>
    public class NamelessCharacterConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return string.IsNullOrWhiteSpace((value as string)) ? "[Unnamed]" : value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
