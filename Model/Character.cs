using Charrmander.Util;
using Charrmander.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace Charrmander.Model
{
    internal class Character : AbstractNotifier, IDisposable
    {
        private readonly IViewModel _viewModel;

        private string _name = string.Empty;
        private string _race = string.Empty;
        private string _profession = string.Empty;
        private int _level = 0;
        private int _defaultSortOrder = 1;

        private readonly IDictionary<string, string> _biographies;

        private string _order = string.Empty;
        private string _racialSympathy = string.Empty;
        private string _retributionAlly = string.Empty;
        private string _greatestFear = string.Empty;
        private string _planOfAttack = string.Empty;

        private bool _hasWorldCompletion = false;

        private static int _fractalTier = 1;

        private string _notes = string.Empty;

        private readonly ObservableCollection<Act> _lw2acts = new();
        private readonly ObservableCollection<Act> _hotacts = new();
        private readonly ObservableCollection<Act> _kottacts = new();
        private readonly ObservableCollection<Act> _lw3acts = new();
        private readonly ObservableCollection<Act> _pofacts = new();
        private readonly ObservableCollection<Act> _lw4acts = new();
        private readonly ObservableCollection<Act> _tisacts = new();

        private readonly ObservableCollection<Dungeon> _dungeons = new();

        /// <summary>
        /// Maximum sort order; greater than max account character slot of 69
        /// (situationally 70) but effectively "limited" to 1 account to
        /// conserve UI space.
        /// </summary>
        public const int MaxSortOrder = 99;

        /// <summary>
        /// Minimum sort order.
        /// </summary>
        public const int MinSortOrder = 1;

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

        private static readonly XElement storyChapters = XDocument.Load(
            XmlReader.Create(
                App.GetPackResourceStream("Resources/StoryChapters.xml")
                .Stream))
            .Root!;

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

            PrepareStoryChapters(_lw2acts, "Lw2");
            PrepareStoryChapters(_hotacts, "HoT");
            PrepareStoryChapters(_kottacts, "KotT");
            PrepareStoryChapters(_lw3acts, "Lw3");
            PrepareStoryChapters(_pofacts, "PoF");
            PrepareStoryChapters(_lw4acts, "Lw4");
            PrepareStoryChapters(_tisacts, "Tis");

            var dungeons = XDocument.Load(XmlReader.Create(
                App.GetPackResourceStream("Resources/Dungeons.xml").Stream))
                .Root!.Elements("Dungeon")
                .Select(d => new Dungeon(d.Element("Name")!.Value, d.Element("StoryLevel")!.Value))
                .OrderBy(d => d.StoryLevel);

            foreach (var dungeon in dungeons)
            {
                dungeon.PropertyChanged += _viewModel.MarkFileDirty;
                _dungeons.Add(dungeon);
            }

            CraftingDisciplines = new ObservableCollection<CraftingDiscipline>()
            {
                new CraftingDiscipline() { Name = "Armorsmith" },
                new CraftingDiscipline() { Name = "Artificer" },
                new CraftingDiscipline() { Name = "Chef" },
                new CraftingDiscipline() { Name = "Huntsman" },
                new CraftingDiscipline() { Name = "Jeweler", MaxLevel = 400 },
                new CraftingDiscipline() { Name = "Leatherworker" },
                new CraftingDiscipline() { Name = "Scribe", MaxLevel = 400 },
                new CraftingDiscipline() { Name = "Tailor" },
                new CraftingDiscipline() { Name = "Weaponsmith" }
            };

            Areas = new();
            Areas.CollectionChanged += Areas_CollectionChanged;

            foreach (var d in CraftingDisciplines)
            {
                d.PropertyChanged += _viewModel.MarkFileDirty;
            }

            this.PropertyChanged += _viewModel.MarkFileDirty;
        }

        private void PrepareStoryChapters(ObservableCollection<Act> acts, string storyline)
        {
            XNamespace ns = "https://storychapters.charr";
            foreach (var act in storyChapters.Element(ns + storyline)!.Descendants(ns + "Act"))
            {
                var chapters = new ObservableCollection<Chapter>();
                foreach (var c in act.Descendants(ns + "Chapter"))
                {
                    var m = new Chapter(c.Value);
                    m.PropertyChanged += _viewModel.MarkFileDirty;
                    chapters.Add(m);
                }
                var a = new Act(act.Element(ns + "Name")!.Value, chapters);
                a.PropertyChanged += _viewModel.MarkFileDirty;
                acts.Add(a);
            }
        }

        public ObservableCollection<Act> Lw2Acts => _lw2acts;

        public ObservableCollection<Act> HoTActs => _hotacts;

        public ObservableCollection<Act> KotTActs => _kottacts;

        public ObservableCollection<Act> Lw3Acts => _lw3acts;

        public ObservableCollection<Act> PoFActs => _pofacts;

        public ObservableCollection<Act> Lw4Acts => _lw4acts;

        public ObservableCollection<Act> TisActs => _tisacts;

        public ObservableCollection<Dungeon> Dungeons
        {
            get { return _dungeons; }
        }

        /// <summary>
        /// The persisted sort order, sorted on by default at launch. The order
        /// of 2 characters with identical sort order is unspecified.
        /// </summary>
        public int DefaultSortOrder
        {
            get { return _defaultSortOrder; }
            set
            {
                if (value != _defaultSortOrder &&
                    value >= MinSortOrder && value <= MaxSortOrder)
                {
                    _defaultSortOrder = value;
                    RaisePropertyChanged(nameof(DefaultSortOrder));
                }
            }
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
                    _name = value;
                    RaisePropertyChanged(nameof(Name));
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
                    RaisePropertyChanged(nameof(Race));
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
                    RaisePropertyChanged(nameof(Profession));
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
                    RaisePropertyChanged(nameof(Level));
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
                    RaisePropertyChanged(nameof(BiographyProfession));
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
                    RaisePropertyChanged(nameof(BiographyPersonality));
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
                    RaisePropertyChanged(nameof(BiographyRaceFirst));
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
                    RaisePropertyChanged(nameof(BiographyRaceSecond));
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
                    RaisePropertyChanged(nameof(BiographyRaceThird));
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
                    RaisePropertyChanged(nameof(Order));
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
                    RaisePropertyChanged(nameof(RacialSympathy));
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
                    RaisePropertyChanged(nameof(RetributionAlly));
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
                    RaisePropertyChanged(nameof(GreatestFear));
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
                    RaisePropertyChanged(nameof(PlanOfAttack));
                }
            }
        }

        public bool HasWorldCompletion
        {
            get { return _hasWorldCompletion; }
            set
            {
                _hasWorldCompletion = value;
                RaisePropertyChanged(nameof(HasWorldCompletion));
            }
        }

        /// <summary>
        /// A collection of all the crafting disciplines.
        /// See <see cref="CraftingDiscipline"/>.
        /// </summary>
        public ObservableCollection<CraftingDiscipline> CraftingDisciplines { get; }

        /// <summary>
        /// A collection of the areas this character has information about.
        /// </summary>
        public ObservableCollection<Area> Areas { get; }

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
                    RaisePropertyChanged(nameof(FractalTier));
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
                    RaisePropertyChanged(nameof(Notes));
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
                new CharrElement("DefaultSortOrder", DefaultSortOrder),
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
                new CharrElement("StoryChapters",
                    new CharrElement("Lw2", SerializeActs(Lw2Acts)),
                    new CharrElement("HoT", SerializeActs(HoTActs)),
                    new CharrElement("KotT", SerializeActs(KotTActs)),
                    new CharrElement("Lw3", SerializeActs(Lw3Acts)),
                    new CharrElement("PoF", SerializeActs(PoFActs)),
                    new CharrElement("Lw4", SerializeActs(Lw4Acts)),
                    new CharrElement("Tis", SerializeActs(TisActs))
                ),
                new CharrElement("CraftingDisciplines",
                    from d in CraftingDisciplines
                    select new CharrElement(d.Name,
                        new CharrElement("Level", d.Level)
                    )
                ),
                new CharrElement("HasWorldCompletion", HasWorldCompletion),
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
                ),
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

        private static IEnumerable<CharrElement> SerializeActs(ObservableCollection<Act> acts)
        {
            return from a in acts
                   select new CharrElement("Act",
                       new CharrElement("Name", a.Name),
                       new CharrElement("Chapters",
                           from m in a.Chapters
                           select new CharrElement("Chapter",
                               new CharrElement("Name", m.Name),
                               new CharrElement("Completed", m.ChapterCompleted)
                           )
                       )
                   );
        }

        /// <summary>
        /// IDisposable implementation.
        /// </summary>
        public void Dispose()
        {
            this.PropertyChanged -= _viewModel.MarkFileDirty;
            foreach (var d in CraftingDisciplines)
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
            foreach (var act in Lw2Acts)
            {
                act.PropertyChanged -= _viewModel.MarkFileDirty;
            }
            foreach (var act in HoTActs)
            {
                act.PropertyChanged -= _viewModel.MarkFileDirty;
            }
            foreach (var act in KotTActs)
            {
                act.PropertyChanged -= _viewModel.MarkFileDirty;
            }
            foreach (var act in Lw3Acts)
            {
                act.PropertyChanged -= _viewModel.MarkFileDirty;
            }
            foreach (var act in PoFActs)
            {
                act.PropertyChanged -= _viewModel.MarkFileDirty;
            }
            foreach (var act in Lw4Acts)
            {
                act.PropertyChanged -= _viewModel.MarkFileDirty;
            }
            foreach (var act in TisActs)
            {
                act.PropertyChanged -= _viewModel.MarkFileDirty;
            }
        }

        /// <summary>
        /// Handles adding and removal of PropertyChanged listeners for areas.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">Contains the items added or removed.</param>
        private void Areas_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
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
