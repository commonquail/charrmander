using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.ComponentModel;
using System.Collections.ObjectModel;
using Charrmander.Util;
using System.Xml.Linq;
using Charrmander.Properties;
using System.Xml;
using System.Xml.Schema;

namespace Charrmander.Model
{
    class Character : AbstractNotifier
    {
        private string _name;
        private string _race;
        private string _profession;

        private ObservableCollection<CraftingDiscipline> _craftingDisciplines;

        public Character()
        {
            Areas = new ObservableCollection<Area>();
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
                }
                return _craftingDisciplines;
            }
            set
            {
                if (value != _craftingDisciplines)
                {
                    _craftingDisciplines = value;
                }
            }
        }

        /// <summary>
        /// A collection of the areas this character has information about.
        /// </summary>
        public ObservableCollection<Area> Areas { get; set; }

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
                            select new CharrElement("CraftingDiscipline",
                                new CharrElement("Name", d.Name),
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
    }
}
