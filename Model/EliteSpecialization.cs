using Charrmander.Util;

namespace Charrmander.Model
{
    internal class EliteSpecialization : AbstractNotifier
    {
        public EliteSpecialization(string name)
        {
            Name = name;
        }

        public string Name { get; }

        private bool _unlocked;
        public bool Unlocked
        {
            get => _unlocked;
            set
            {
                if (value != _unlocked)
                {
                    _unlocked = value;
                    RaisePropertyChanged(nameof(Unlocked));
                }
            }
        }
    }
}
