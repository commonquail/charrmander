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

        public Character()
        {
            Areas = new ObservableCollection<Area>();
        }

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

        public ObservableCollection<Area> Areas { get; set; }

        public CharrElement ToXML()
        {
            return new CharrElement("Character",
                        new CharrElement("Name", Name),
                        new CharrElement("Race", Race),
                        new CharrElement("Profession", Profession),
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
