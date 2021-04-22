using Charrmander.Util;

namespace Charrmander.Model
{
    class Dungeon : AbstractNotifier
    {
        private bool _storyCompleted = false;

        /// <summary>
        /// Creates a new <c>Area</c> with the specified name.
        /// </summary>
        /// <param name="name">The name of the area.</param>
        public Dungeon(string name, string storyLevel)
        {
            Name = name;
            StoryLevel = storyLevel;
        }

        /// <summary>
        /// The name of this dungeon.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// This dungeon's minimum story level.
        /// </summary>
        public string StoryLevel { get; private set; }

        /// <summary>
        /// The state of completion of this dungeon's story mode. <c>True</c>
        /// if completed, <c>False</c> otherwise.
        /// </summary>
        public bool StoryCompleted
        {
            get { return _storyCompleted; }
            set
            {
                if (value != _storyCompleted)
                {
                    _storyCompleted = value;
                    RaisePropertyChanged(nameof(StoryCompleted));
                }
            }
        }
    }
}
