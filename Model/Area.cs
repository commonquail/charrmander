using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.ComponentModel;
using Charrmander.Util;

namespace Charrmander.Model
{
    class Area : AbstractNotifier
    {
        private string _heartsCompleted;
        private string _waypointsCompleted;
        private string _poisCompleted;
        private string _skillsCompleted;
        private string _vistasCompleted;

        public string Name { get; set; }

        public string Hearts
        {
            get { return GetCompletionItem(_heartsCompleted); }

            set
            {
                if (value != _heartsCompleted && !string.IsNullOrWhiteSpace(value))
                {
                    _heartsCompleted = value.Trim();
                    OnPropertyChanged("Hearts");
                }
            }
        }

        public string Waypoints
        {
            get { return GetCompletionItem(_waypointsCompleted); }

            set
            {
                if (value != _waypointsCompleted && !string.IsNullOrWhiteSpace(value))
                {
                    _waypointsCompleted = value.Trim();
                    OnPropertyChanged("Waypoints");
                }
            }
        }

        public string PoIs
        {
            get { return GetCompletionItem(_poisCompleted); }
            set
            {
                if (value != _poisCompleted && !string.IsNullOrWhiteSpace(value))
                {
                    _poisCompleted = value.Trim();
                    OnPropertyChanged("PoIs");
                }
            }
        }

        public string Skills
        {
            get { return GetCompletionItem(_skillsCompleted); }
            set
            {
                if (value != _skillsCompleted && !string.IsNullOrWhiteSpace(value))
                {
                    _skillsCompleted = value.Trim();
                    OnPropertyChanged("Skills");
                }
            }
        }

        public string Vistas
        {
            get { return GetCompletionItem(_vistasCompleted); }
            set
            {
                if (value != _vistasCompleted && !string.IsNullOrWhiteSpace(value))
                {
                    _vistasCompleted = value.Trim();
                    OnPropertyChanged("Vistas");
                }
            }
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
    }
}
