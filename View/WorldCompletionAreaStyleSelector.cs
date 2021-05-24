using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Charrmander.View
{
    public class WorldCompletionAreaStyleSelector : StyleSelector
    {
        private readonly IReadOnlySet<string> _names;

        static WorldCompletionAreaStyleSelector()
        {
            WorldCompletionAreaStyle = new();

            var backgroundBrush = new BrushConverter().ConvertFrom("#FFF8E8");
            WorldCompletionAreaStyle.Setters.Add(
                new Setter(Control.BackgroundProperty, backgroundBrush));
        }

        internal WorldCompletionAreaStyleSelector(IReadOnlySet<string> names) => _names = names;

        public override Style? SelectStyle(object item, DependencyObject container)
        {
            if (item is DataRowView rowView)
            {
                var areaNameColumnValue = rowView.Row[0];
                if (areaNameColumnValue is string name && _names.Contains(name))
                {
                    return WorldCompletionAreaStyle;
                }
            }
            return null;
        }

        public static Style WorldCompletionAreaStyle { get; }
    }
}
