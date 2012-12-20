using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Xml.Linq;
using Charrmander.Util;
using Charrmander.ViewModel;

namespace Charrmander.Model
{
    class Character : AbstractNotifier, IDisposable
    {
        private IViewModel _viewModel;

        private string _name;
        private string _race;
        private string _profession;

        private ObservableCollection<CraftingDiscipline> _craftingDisciplines;
        private ObservableCollection<Area> _areas;

        /// <summary>
        /// Creates a new character for the specified view model.
        /// </summary>
        /// <param name="vm">The view model housing this character.</param>
        public Character(IViewModel vm)
        {
            _viewModel = vm;
            this.PropertyChanged += _viewModel.MarkFileDirty;
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
                if (value != _race)
                {
                    _race = value;
                    RaisePropertyChanged("Race");
                }
            }
        }

        /// <summary>
        /// The characater's profession. Values are not validated.
        /// </summary>
        public string Profession
        {
            get { return _profession; }
            set
            {
                if (value != _profession)
                {
                    _profession = value;
                    RaisePropertyChanged("Profession");
                }
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
                        new CharrElement("CraftingDisciplines",
                            from d in CraftingDisciplines
                            select new CharrElement(d.Name,
                                new CharrElement("Level", d.Level)
                            )
                        ),
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
                        ) : null)
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
            foreach (Area a in Areas)
            {
                a.PropertyChanged -= _viewModel.MarkFileDirty;
            }
            Areas.CollectionChanged -= Areas_CollectionChanged;
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
