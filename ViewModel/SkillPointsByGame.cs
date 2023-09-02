using Charrmander.Util;

namespace Charrmander.ViewModel
{
    internal class SkillPointsByGame : AbstractNotifier
    {
        private int _core;
        public int Core
        {
            get => _core;

            set
            {
                if (_core != value)
                {
                    _core = value;
                    RaisePropertyChanged(nameof(Core));
                }
            }
        }

        private int _hot;
        public int Hot
        {
            get => _hot;
            set
            {
                if (_hot != value)
                {
                    _hot = value;
                    RaisePropertyChanged(nameof(Hot));
                }
            }
        }

        private int _pof;
        public int Pof
        {
            get => _pof;
            set
            {
                if (_pof != value)
                {
                    _pof = value;
                    RaisePropertyChanged(nameof(Pof));
                }
            }
        }

        private int _eod;
        public int Eod
        {
            get => _eod;
            set
            {
                if (_eod != value)
                {
                    _eod = value;
                    RaisePropertyChanged(nameof(Eod));
                }
            }
        }

        private int _soto;
        public int Soto
        {
            get => _soto;
            set
            {
                if (_soto != value)
                {
                    _soto = value;
                    RaisePropertyChanged(nameof(Soto));
                }
            }
        }
    }
}
