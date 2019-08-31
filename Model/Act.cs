using Charrmander.Util;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Charrmander.Model
{
    class Act : AbstractNotifier
    {
        public Act(string name, ObservableCollection<Chapter> chapters)
        {
            Name = name;
            Chapters = chapters;
        }

        /// <summary>
        /// The name of this story act.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// This story act's chapters.
        /// </summary>
        public ObservableCollection<Chapter> Chapters { get; private set; }
    }
}
