using Charrmander.Model;
using System;
using System.Windows.Data;

namespace Charrmander.View
{
    /// <summary>
    /// Converts <see cref="CompletionState"/>s into stars imitating area progression icons.
    /// </summary>
    public class CompletionStateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string res = string.Empty;
            CompletionState state = CompletionState.NotBegun;
            switch (value)
            {
                case CompletionState v:
                    state = v;
                    break;
                case string v:
                    if (Enum.TryParse(v, out CompletionState s))
                    {
                        state = s;
                    }

                    break;
            }

            switch (state)
            {
                case CompletionState.NotBegun:
                    break;
                case CompletionState.Begun:
                    res = "\u2606"; // White star.
                    break;
                case CompletionState.Completed:
                    res = "\u2605"; // Black star.
                    break;
                default:
                    break;
            }

            return res;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
