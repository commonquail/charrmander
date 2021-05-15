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
            string? str = value as string;
            return string.IsNullOrWhiteSpace(str) ? "[Unnamed]" : str;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
