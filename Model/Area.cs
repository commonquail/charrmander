﻿using Charrmander.Util;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Charrmander.Model
{
    class Area : AbstractNotifier
    {
        private static Regex _NaNMatch;

        private string _minLevel = "1";
        private string _maxLevel = "80";

        private string _hearts = string.Empty;
        private string _waypoints = string.Empty;
        private string _pois = string.Empty;
        private string _skills = string.Empty;
        private string _vistas = string.Empty;

        static Area()
        {
            _NaNMatch = new Regex("[^0-9]");
        }

        /// <summary>
        /// Creates a new <c>Area</c> with the specified name.
        /// </summary>
        /// <param name="name">The name of the area.</param>
        public Area(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Creates a new <c>Area</c> with property values given by the
        /// <see cref="XElement"/> argument.
        /// </summary>
        /// <param name="area">An <c>XElement</c> instance describing the
        /// properties of the area to be created.</param>
        /// <returns>A new instance of <c>Area</c>.</returns>
        public static Area FromXML(XElement area)
        {
            var a = new Area(area.Element("Name").Value)
            {
                Hearts = area.Element("Completion").Element("Hearts").Value,
                Waypoints = area.Element("Completion").Element("Waypoints").Value,
                PoIs = area.Element("Completion").Element("PoIs").Value,
                Skills = area.Element("Completion").Element("Skills").Value,
                Vistas = area.Element("Completion").Element("Vistas").Value
            };

            var levelRange = area.Element("LevelRange");
            if (levelRange != null)
            {
                var minLevel = levelRange.Element("MinLevel");
                if (minLevel != null)
                {
                    a.MinLevel = minLevel.Value;
                }

                var maxLevel = levelRange.Element("MaxLevel");
                if (maxLevel != null)
                {
                    a.MaxLevel = maxLevel.Value;
                }
            }

            return a;
        }

        /// <summary>
        /// The name of this area.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// This area's recommended minimum level, default 1.
        /// </summary>
        public string MinLevel
        {
            get { return _minLevel; }
            set
            {
                if (value != _minLevel)
                {
                    _minLevel = value;
                    RaisePropertyChanged("MinLevel");
                }
            }
        }

        /// <summary>
        /// This area's recommended maximum level, default 80.
        /// </summary>
        public string MaxLevel
        {
            get { return _maxLevel; }
            set
            {
                if (value != _maxLevel)
                {
                    _maxLevel = value;
                    RaisePropertyChanged("MaxLevel");
                }
            }
        }

        /// <summary>
        /// A human readable string representation of this area's level range.
        /// </summary>
        public string LevelRange
        {
            get { return string.Format("Level range: {0}-{1}", _minLevel, _maxLevel); }
        }

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
            if (value != item && !Area._NaNMatch.IsMatch(value))
            {
                item = value;
                RaisePropertyChanged(property);
            }
        }
    }
}
