using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Charrmander.Util;

namespace Charrmander.Model
{
    class CraftingDiscipline : AbstractNotifier
    {
        private string _name;
        private int _level = CraftingDiscipline.MinLevel;

        public const int MaxLevel = 400;
        public const int MinLevel = 0;

        /// <summary>
        /// The name of this crafting discipline.
        /// </summary>
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

        /// <summary>
        /// The level of this crafting discipline, from 0 to 400. Limits are
        /// not enforced.
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
    }
}
