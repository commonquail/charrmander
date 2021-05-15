using Charrmander.Util;

namespace Charrmander.Model
{
    class CraftingDiscipline : AbstractNotifier
    {
        private string _name = string.Empty;
        private int _level = CraftingDiscipline.MinLevel;
        private int _maxLevel = 500;

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
                    RaisePropertyChanged(nameof(Name));
                }
            }
        }

        /// <summary>
        /// The level of this crafting discipline, from 0 to MaxLevel.
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
        /// The max level of this crafting discipline.
        /// </summary>
        public int MaxLevel
        {
            get { return _maxLevel; }
            set
            {
                if (value != _maxLevel && value >= MinLevel)
                {
                    _maxLevel = value;
                    RaisePropertyChanged(nameof(MaxLevel));
                }
            }
        }
    }
}
