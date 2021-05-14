using Charrmander.Model;
using System;
using System.Windows.Data;

namespace Charrmander.View
{
    /// <summary>
    /// Converts the <see cref="Character.Profession"/> property to the corresponding icon.
    /// </summary>
    public class GameIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return "/Icons/Game/" + parameter + "/" + value + ".png";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
