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

        public string Name { get; set; }

        public string Hearts
        {
            get { return GetCompletionItem(_hearts); }
            set { SetCompletionItem(ref _hearts, value, "Hearts"); }
        }

        public string Waypoints
        {
            get { return GetCompletionItem(_waypoints); }
            set { SetCompletionItem(ref _waypoints, value, "Waypoints"); }
        }

        public string PoIs
        {
            get { return GetCompletionItem(_pois); }
            set { SetCompletionItem(ref _pois, value, "PoIs"); }
        }

        public string Skills
        {
            get { return GetCompletionItem(_skills); }
            set { SetCompletionItem(ref _skills, value, "Skills"); }
        }

        public string Vistas
        {
            get { return GetCompletionItem(_vistas); }
            set { SetCompletionItem(ref _vistas, value, "Vistas"); }
        }

        public Area(string name)
        {
            Name = name;
            _NaNMatch = new Regex("[^0-9]");
        }

        private string GetCompletionItem(string item)
        {
            return string.IsNullOrWhiteSpace(item) ? "0" : item;
        }

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
