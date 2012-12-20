using Charrmander.Util;
using System.Text.RegularExpressions;

namespace Charrmander.Model
{
    class Area : AbstractNotifier
    {
        private Regex _NaNMatch;

        private string _hearts = string.Empty;
        private string _waypoints = string.Empty;
        private string _pois = string.Empty;
        private string _skills = string.Empty;
        private string _vistas = string.Empty;

        public Area(string name)
        {
            Name = name;
            _NaNMatch = new Regex("[^0-9]");
        }

        /// <summary>
        /// The name of this area.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The number of hearts in this area.
        /// </summary>
        public string Hearts
        {
            get { return GetCompletionItem(_hearts); }
            set { SetCompletionItem(ref _hearts, value, "Hearts"); }
        }

        /// <summary>
        /// The number of waypoints in this area.
        /// </summary>
        public string Waypoints
        {
            get { return GetCompletionItem(_waypoints); }
            set { SetCompletionItem(ref _waypoints, value, "Waypoints"); }
        }

        /// <summary>
        /// The number of points of interest in this area.
        /// </summary>
        public string PoIs
        {
            get { return GetCompletionItem(_pois); }
            set { SetCompletionItem(ref _pois, value, "PoIs"); }
        }

        /// <summary>
        /// The number of skill challenges in this area.
        /// </summary>
        public string Skills
        {
            get { return GetCompletionItem(_skills); }
            set { SetCompletionItem(ref _skills, value, "Skills"); }
        }

        /// <summary>
        /// The number of vistas in this area.
        /// </summary>
        public string Vistas
        {
            get { return GetCompletionItem(_vistas); }
            set { SetCompletionItem(ref _vistas, value, "Vistas"); }
        }

        /// <summary>
        /// Getter-wrapper that returns the desired item or "0" if empty.
        /// </summary>
        /// <param name="item">The field corresponding to the desired area
        /// completion item.</param>
        /// <returns>The value of <c>item</c>, "0" if null or empty.</returns>
        private string GetCompletionItem(string item)
        {
            return string.IsNullOrWhiteSpace(item) ? "0" : item;
        }

        /// <summary>
        /// Setter-wrapper that handles validation logic for area completion
        /// items.
        /// </summary>
        /// <param name="item">The field cvorresponding to the desired area
        /// completion item.</param>
        /// <param name="value">The new value of <c>item</c>.</param>
        /// <param name="property">The property to signal.</param>
        private void SetCompletionItem(ref string item, string value, string property)
        {
            value = value.Trim();
            if (value != item && !_NaNMatch.IsMatch(value))
            {
                item = value;
                RaisePropertyChanged(property);
            }
        }
    }
}
