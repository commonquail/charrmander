using Charrmander.Util;
using System.Collections.ObjectModel;

namespace Charrmander.Model
{
    internal class Act : AbstractNotifier
    {
        public Act(string name, ObservableCollection<Chapter> chapters)
        {
            Name = name;
            Chapters = chapters;
        }

        /// <summary>
        /// The name of this story act.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// This story act's chapters.
        /// </summary>
        public ObservableCollection<Chapter> Chapters { get; }
    }
}
