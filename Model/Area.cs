using Charrmander.Util;
using System;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Charrmander.Model
{
    internal class Area : AbstractNotifier
    {
        private static readonly Regex _NaNMatch;

        private string _minLevel = "0";
        private string _maxLevel = "80";

        private string _hearts = string.Empty;
        private string _waypoints = string.Empty;
        private string _pois = string.Empty;
        private string _skills = string.Empty;
        private string _vistas = string.Empty;

        private bool _participatesInWorldCompletion;

        private CompletionState _areaState = CompletionState.NotBegun;

        public static XNamespace XmlNamespace = "https://areas.charr";

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
        public static Area ReferenceAreaFromXML(XElement area)
        {
            var cpl = area.Element(XmlNamespace + "Completion")!;
            var a = new Area(area.Element(XmlNamespace + "Name")!.Value)
            {
                Hearts = cpl.Element(XmlNamespace + "Hearts")!.Value,
                Waypoints = cpl.Element(XmlNamespace + "Waypoints")!.Value,
                PoIs = cpl.Element(XmlNamespace + "PoIs")!.Value,
                Skills = cpl.Element(XmlNamespace + "Skills")!.Value,
                Vistas = cpl.Element(XmlNamespace + "Vistas")!.Value
            };

            var participatesInWorldCompletion =
                area.Element(XmlNamespace + "ParticipatesInWorldCompletion")?.Value;
            a.ParticipatesInWorldCompletion = bool.TryParse(
                participatesInWorldCompletion,
                out bool participates) && participates;

            var releaseName = area.Element(XmlNamespace + "Release")?.Value;
            a.Release = releaseName switch
            {
                "Core" => AreaFilter.Core,
                "EoD" => AreaFilter.EoD,
                "HoT" => AreaFilter.HoT,
                "PoF" => AreaFilter.PoF,
                "Lw3" => AreaFilter.Lw3,
                "Lw4" => AreaFilter.Lw4,
                "Tis" => AreaFilter.Tis,
                _ => throw new NotImplementedException($"Release={releaseName}"),
            };

            var levelRange = area.Element(XmlNamespace + "LevelRange");
            if (levelRange != null)
            {
                var minLevel = levelRange.Element(XmlNamespace + "MinLevel");
                if (minLevel != null)
                {
                    a.MinLevel = minLevel.Value;
                }

                var maxLevel = levelRange.Element(XmlNamespace + "MaxLevel");
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
                    RaisePropertyChanged(nameof(MinLevel));
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
                    RaisePropertyChanged(nameof(MaxLevel));
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
        /// The <see cref="CompletionState"/> of this area. Since this value
        /// generally has to be calculated at runtime it is not stored with
        /// <see cref="ToXML()"/>.
        /// </summary>
        public CompletionState State
        {
            get { return _areaState; }
            set
            {
                if (value != _areaState)
                {
                    _areaState = value;
                    RaisePropertyChanged(nameof(State));
                }
            }
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
        /// If true, this area must be completed for a character to get world
        /// completion. If false, world completion is unaffected by this area.
        /// </summary>
        /// <remarks>
        /// Once a character has attained world completion, the future
        /// completion state of any area ceases to affect world completion in
        /// any way, irrespective of this property's value.
        /// </remarks>
        public bool ParticipatesInWorldCompletion
        {
            get => _participatesInWorldCompletion;
            private set
            {
                if (_participatesInWorldCompletion != value)
                {
                    _participatesInWorldCompletion = value;
                    RaisePropertyChanged(nameof(ParticipatesInWorldCompletion));
                }
            }
        }

        private AreaFilter _release;
        public AreaFilter Release
        {
            get => _release;
            private set
            {
                if (_release != value)
                {
                    _release = value;
                    RaisePropertyChanged(nameof(Release));
                }
            }
        }

        /// <summary>
        /// Getter-wrapper that returns the desired item or "0" if empty.
        /// </summary>
        /// <param name="item">The field corresponding to the desired area
        /// completion item.</param>
        /// <returns>The value of <c>item</c>, "0" if null or empty.</returns>
        private static string GetCompletionItem(string item)
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
