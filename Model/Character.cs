using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.ComponentModel;
using System.Collections.ObjectModel;
using Charrmander.Util;

namespace Charrmander.Model
{
    class Character : AbstractNotifier
    {
        private string _name;
        private string _profession;

        public string Name
        {
            get { return _name; }
            set
            {
                if (value != _name)
                {
                    _name = value;
                    RaisePropertyChanged("Name");
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

        public Character()
        {
            Areas = new ObservableCollection<Area>();
        }
    }
}
