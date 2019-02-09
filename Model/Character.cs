using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Xml.Linq;
using Charrmander.Util;
using Charrmander.ViewModel;
using System.Collections.Generic;
using System.Windows;

namespace Charrmander.Model
{
    class Character : AbstractNotifier, IDisposable
    {
        private IViewModel _viewModel;

        private string _name = string.Empty;
        private string _race = string.Empty;
        private string _profession = string.Empty;
        private int _level = 0;

        private IDictionary<string, string> _biographies;

        private string _order = string.Empty;
        private string _racialSympathy = string.Empty;
        private string _retributionAlly = string.Empty;
        private string _greatestFear = string.Empty;
        private string _planOfAttack = string.Empty;

        private bool _hasWorldCompletion = false;

        private ObservableCollection<CraftingDiscipline> _craftingDisciplines;
        private ObservableCollection<Area> _areas;

        private static int _fractalTier = 1;

        private string _notes;

        private ObservableCollection<Dungeon> _dungeons = new ObservableCollection<Dungeon>();

        /// <summary>
        /// Maximum character level.
        /// </summary>
        public const int MaxLevel = 80;

        /// <summary>
        /// Minimum character level.
        /// </summary>
        public const int MinLevel = 0;

        /// <summary>
        /// Minimum Fractal of the Mists tier.
        /// </summary>
        public const int FractalTierMin = 1;

        /// <summary>
        /// Creates a new character for the specified view model.
        /// </summary>
        /// <param name="vm">The view model housing this character.</param>
        public Character(IViewModel vm)
        {
            _viewModel = vm;

            _biographies = new Dictionary<string, string>()
            {
                { "Profession", "" },
                { "Personality","" },
                { "RaceFirst",  "" },
                { "RaceSecond", "" },
                { "RaceThird",  "" }
            };

            var dungeons = XDocument.Load(System.Xml.XmlReader.Create(Application.GetResourceStream(
                new Uri("Resources/Dungeons.xml", UriKind.Relative)).Stream)).Root.Elements("Dungeon")
                .OrderBy(d => d.Element("StoryLevel").Value);

            foreach (var dungeon in dungeons)
            {
                var d = new Dungeon(dungeon.Element("Name").Value, dungeon.Element("StoryLevel").Value);
                d.PropertyChanged += _viewModel.MarkFileDirty;
                _dungeons.Add(d);
            }

            this.PropertyChanged += _viewModel.MarkFileDirty;
        }

        public ObservableCollection<Dungeon> Dungeons
        {
            get { return _dungeons; }
        }

        /// <summary>
        /// The character name.
        /// </summary>
        public string Name
        {
            get { return _name; }
            set
            {
                if (value != _name && !string.IsNullOrWhiteSpace(value))
                {
                    _name = value.Trim();
                    RaisePropertyChanged("Name");
                }
            }
        }

        /// <summary>
        /// The character's race. One of Asura, Charr, Human, Norn, Sylvari.
        /// Values are not validated.
        /// </summary>
        public string Race
        {
            get { return _race; }
            set
            {
                if (value != _race && !string.IsNullOrWhiteSpace(value))
                {
                    _race = value;
                    RaisePropertyChanged("Race");
                }
            }
        }

        /// <summary>
        /// The character's profession. Values are not validated.
        /// </summary>
        public string Profession
        {
            get { return _profession; }
            set
            {
                if (value != _profession && !string.IsNullOrWhiteSpace(value))
                {
                    _profession = value;
                    RaisePropertyChanged("Profession");
                }
            }
        }

        /// <summary>
        /// The character's level.
        /// </summary>
        public int Level
        {
            get { return _level; }
            set
            {
                if (value != _level &&
                    value >= MinLevel && value <= MaxLevel)
                {
                    _level = value;
                    RaisePropertyChanged("Level");
                }
            }
        }

        /// <summary>
        /// The character's profession-dependant biography choice.
        /// </summary>
        public string BiographyProfession
        {
            get { return _biographies["Profession"]; }
            set
            {
                if (value != _biographies["Profession"] && !string.IsNullOrWhiteSpace(value))
                {
                    _biographies["Profession"] = value;
                    RaisePropertyChanged("BiographyProfession");
                }
            }
        }

        /// <summary>
        /// The character's personality biography choice.
        /// </summary>
        public string BiographyPersonality
        {
            get { return _biographies["Personality"]; }
            set
            {
                if (value != _biographies["Personality"] && !string.IsNullOrWhiteSpace(value))
                {
                    _biographies["Personality"] = value;
                    RaisePropertyChanged("BiographyPersonality");
                }
            }
        }

        /// <summary>
        /// The character's first of three race-dependant biography choices.
        /// <seealso cref="BiographyRaceSecond"/>
        /// <seealso cref="BiographyRaceThird"/>
        /// </summary>
        public string BiographyRaceFirst
        {
            get { return _biographies["RaceFirst"]; }
            set
            {
                if (value != _biographies["RaceFirst"] && !string.IsNullOrWhiteSpace(value))
                {
                    _biographies["RaceFirst"] = value;
                    RaisePropertyChanged("BiographyRaceFirst");
                }
            }
        }

        /// <summary>
        /// The character's second of three race-dependant biography choices.
        /// <seealso cref="BiographyRaceFirst"/>
        /// <seealso cref="BiographyRaceThird"/>
        /// </summary>
        public string BiographyRaceSecond
        {
            get { return _biographies["RaceSecond"]; }
            set
            {
                if (value != _biographies["RaceSecond"] && !string.IsNullOrWhiteSpace(value))
                {
                    _biographies["RaceSecond"] = value;
                    RaisePropertyChanged("BiographyRaceSecond");
                }
            }
        }

        /// <summary>
        /// The character's third of three race-dependant biography choices.
        /// <seealso cref="BiographyRaceFirst"/>
        /// <seealso cref="BiographyRaceSecond"/>
        /// </summary>
        public string BiographyRaceThird
        {
            get { return _biographies["RaceThird"]; }
            set
            {
                if (value != _biographies["RaceThird"] && !string.IsNullOrWhiteSpace(value))
                {
                    _biographies["RaceThird"] = value;
                    RaisePropertyChanged("BiographyRaceThird");
                }
            }
        }

        /// <summary>
        /// The order this character joins.
        /// </summary>
        public string Order
        {
            get { return _order; }
            set
            {
                if (value != _order && !string.IsNullOrWhiteSpace(value))
                {
                    _order = value;
                    RaisePropertyChanged("Order");
                }
            }
        }

        /// <summary>
        /// The lesser race this character choses to aid.
        /// </summary>
        public string RacialSympathy
        {
            get { return _racialSympathy; }
            set
            {
                if (value != _racialSympathy && !string.IsNullOrWhiteSpace(value))
                {
                    _racialSympathy = value;
                    RaisePropertyChanged("RacialSympathy");
                }
            }
        }

        /// <summary>
        /// The ally this character picks for the story quest Retribution.
        /// </summary>
        public string RetributionAlly
        {
            get { return _retributionAlly; }
            set
            {
                if (value != _retributionAlly && !string.IsNullOrWhiteSpace(value))
                {
                    _retributionAlly = value;
                    RaisePropertyChanged("RetributionAlly");
                }
            }
        }

        /// <summary>
        /// This character's greatest fear, picked in A Light in the Darkness.
        /// </summary>
        public string GreatestFear
        {
            get { return _greatestFear; }
            set
            {
                if (value != _greatestFear && !string.IsNullOrWhiteSpace(value))
                {
                    _greatestFear = value;
                    RaisePropertyChanged("GreatestFear");
                }
            }
        }

        /// <summary>
        /// The Orr attack plan this character picks.
        /// </summary>
        public string PlanOfAttack
        {
            get { return _planOfAttack; }
            set
            {
                if (value != _planOfAttack && !string.IsNullOrWhiteSpace(value))
                {
                    _planOfAttack = value;
                    RaisePropertyChanged("PlanOfAttack");
                }
            }
        }

        public bool HasWorldCompletion
        {
            get { return _hasWorldCompletion; }
            set
            {
                _hasWorldCompletion = value;
                RaisePropertyChanged("HasWorldCompletion");
            }
        }

        /// <summary>
        /// A collection of all the crafting disciplines.
        /// See <see cref="CraftingDiscipline"/>.
        /// </summary>
        public ObservableCollection<CraftingDiscipline> CraftingDisciplines
        {
            get
            {
                if (_craftingDisciplines == null)
                {
                    _craftingDisciplines = new ObservableCollection<CraftingDiscipline>()
                        {
                            new CraftingDiscipline() { Name = "Armorsmith" },
                            new CraftingDiscipline() { Name = "Artificer" },
                            new CraftingDiscipline() { Name = "Chef" },
                            new CraftingDiscipline() { Name = "Huntsman" },
                            new CraftingDiscipline() { Name = "Jeweler" },
                            new CraftingDiscipline() { Name = "Leatherworker" },
                            new CraftingDiscipline() { Name = "Tailor" },
                            new CraftingDiscipline() { Name = "Weaponsmith" }
                        };
                    foreach (var d in _craftingDisciplines)
                    {
                        d.PropertyChanged += _viewModel.MarkFileDirty;
                    }
                }
                return _craftingDisciplines;
            }
        }

        /// <summary>
        /// A collection of the areas this character has information about.
        /// </summary>
        public ObservableCollection<Area> Areas
        {
            get
            {
                if (_areas == null)
                {
                    _areas = new ObservableCollection<Area>();
                    _areas.CollectionChanged += Areas_CollectionChanged;
                }
                return _areas;
            }
        }

        /// <summary>
        /// The character's Fractal of the Mists tier.
        /// <seealso cref="Character.FractalTierMin"/>
        /// </summary>
        public int FractalTier
        {
            get { return _fractalTier; }
            set
            {
                if (value != _fractalTier && value >= FractalTierMin)
                {
                    _fractalTier = value;
                    RaisePropertyChanged("FractalTier");
                }
            }
        }

        /// <summary>
        /// Any personal notes the user wishes to record for this character.
        /// </summary>
        public string Notes
        {
            get { return _notes; }
            set
            {
                if (value != _notes)
                {
                    _notes = value;
                    RaisePropertyChanged("Notes");
                }
            }
        }

        /// <summary>
        /// Serializes this character to XML.
        /// <seealso cref="XElement"/>
        /// </summary>
        /// <returns>A <see cref="CharrElement"/> XML representation of this
        /// character.</returns>
        public CharrElement ToXML()
        {
            return new CharrElement("Character",
                new CharrElement("Name", Name),
                new CharrElement("Race", Race),
                new CharrElement("Profession", Profession),
                new CharrElement("Level", Level),
                new CharrElement("Biographies",
                    new CharrElement("Profession", BiographyProfession),
                    new CharrElement("Personality", BiographyPersonality),
                    new CharrElement("RaceFirst", BiographyRaceFirst),
                    new CharrElement("RaceSecond", BiographyRaceSecond),
                    new CharrElement("RaceThird", BiographyRaceThird)
                ),
                new CharrElement("StoryChoices",
                    new CharrElement("Order", Order),
                    new CharrElement("RacialSympathy", RacialSympathy),
                    new CharrElement("RetributionAlly", RetributionAlly),
                    new CharrElement("GreatestFear", GreatestFear),
                    new CharrElement("PlanOfAttack", PlanOfAttack)
                ),
                new CharrElement("CraftingDisciplines",
                    from d in CraftingDisciplines
                    select new CharrElement(d.Name,
                        new CharrElement("Level", d.Level)
                    )
                ),
                new CharrElement("HasWorldCompletion", HasWorldCompletion),
                (Areas.Count > 0 ?
                new CharrElement("Areas",
                    from a in Areas
                    select new CharrElement("Area",
                        new CharrElement("Name", a.Name),
                        new CharrElement("Completion",
                            new CharrElement("Hearts", a.Hearts),
                            new CharrElement("Waypoints", a.Waypoints),
                            new CharrElement("PoIs", a.PoIs),
                            new CharrElement("Skills", a.Skills),
                            new CharrElement("Vistas", a.Vistas)
                        )
                    )
                ) : null),
                new CharrElement("FractalTier", FractalTier),
                new CharrElement("Dungeons",
                    from d in Dungeons
                    select new CharrElement("Dungeon",
                        new CharrElement("Name", d.Name),
                        new CharrElement("StoryCompleted", d.StoryCompleted)
                    )
                ),
                new CharrElement("Notes", Notes)
            );
        }

        /// <summary>
        /// IDisposable implementation.
        /// </summary>
        public void Dispose()
        {
            this.PropertyChanged -= _viewModel.MarkFileDirty;
            foreach (var d in _craftingDisciplines)
            {
                d.PropertyChanged -= _viewModel.MarkFileDirty;
            }
            foreach (var a in Areas)
            {
                a.PropertyChanged -= _viewModel.MarkFileDirty;
            }
            Areas.CollectionChanged -= Areas_CollectionChanged;
            foreach (var dungeon in Dungeons)
            {
                dungeon.PropertyChanged -= _viewModel.MarkFileDirty;
            }
        }

        /// <summary>
        /// Handles adding and removal of PropertyChanged listeners for areas.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">Contains the items added or removed.</param>
        private void Areas_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (Area a in e.NewItems)
                {
                    a.PropertyChanged += _viewModel.MarkFileDirty;
                }
            }

            if (e.OldItems != null)
            {
                foreach (Area a in e.OldItems)
                {
                    a.PropertyChanged -= _viewModel.MarkFileDirty;
                }
            }
        }

    }
}
