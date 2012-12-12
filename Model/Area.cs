using Charrmander.Util;

namespace Charrmander.Model
{
    class Area : AbstractNotifier
    {
        private string _hearts;
        private string _waypoints;
        private string _pois;
        private string _skills;
        private string _vistas;

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
        }

        private string GetCompletionItem(string item)
        {
            if (string.IsNullOrWhiteSpace(item))
            {
                return string.Empty;
            }
            return item;
        }

        private void SetCompletionItem(ref string item, string value, string property)
        {
            if (value != item && !string.IsNullOrWhiteSpace(value))
            {
                item = value.Trim();
                RaisePropertyChanged(property);
            }
        }
    }
}
