using Charrmander.Model;
using System.Collections;

namespace Charrmander.ViewModel
{
    public class AreaNameComparer : IComparer
    {
        public int Compare(object? x, object? y)
        {
            return (x as Area, y as Area) switch
            {
                (not null, null) => 1,
                (null, not null) => -1,
                (null, null) => 0,
                (var xa, var ya) => xa.Name.CompareTo(ya.Name),
            };
        }
    }
}
