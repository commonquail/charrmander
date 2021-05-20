using Charrmander.Util;
using System.Collections.Generic;

namespace Charrmander.Model
{
    internal class Act : AbstractNotifier
    {
        public Act(string name, IReadOnlyList<Chapter> chapters)
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
        public IReadOnlyList<Chapter> Chapters { get; }
    }
}
