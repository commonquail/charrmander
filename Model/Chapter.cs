using Charrmander.Util;

namespace Charrmander.Model
{
    internal class Chapter : AbstractNotifier
    {
        private bool _completed = false;

        public Chapter(string name)
        {
            Name = name;
        }

        /// <summary>
        /// The name of this story chapter.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The state of completion of this story chapter.
        /// <c>True</c> if completed, <c>False</c> otherwise.
        /// </summary>
        public bool ChapterCompleted
        {
            get { return _completed; }
            set
            {
                if (value != _completed)
                {
                    _completed = value;
                    RaisePropertyChanged(nameof(ChapterCompleted));
                }
            }
        }
    }
}
