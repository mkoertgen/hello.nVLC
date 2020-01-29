using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;

namespace Hello.nVLC
{
    public class MainViewModel : Conductor<PlayerTabViewModel>.Collection.OneActive
    {
        private bool _pauseOnSwitch;

        public MainViewModel(IEnumerable<IMediaPlayerViewModel> players)
        {
            Items.AddRange(players.Select(ToTab));
            ActiveItem = Items.FirstOrDefault();

            // ReSharper disable once VirtualMemberCallInConstructor
            DisplayName = typeof(MainViewModel).Assembly.GetName().Name;
        }
        public bool PauseOnSwitch
        {
            get => _pauseOnSwitch;
            set
            {
                if (value == _pauseOnSwitch) return;
                _pauseOnSwitch = value;
                NotifyOfPropertyChange();
            }
        }

        protected override void ChangeActiveItem(PlayerTabViewModel newItem, bool closePrevious)
        {
            if (PauseOnSwitch) ActiveItem?.Player.Player.Pause();
            base.ChangeActiveItem(newItem, closePrevious);
        }

        private static PlayerTabViewModel ToTab(IMediaPlayerViewModel player)
        {
            return new PlayerTabViewModel(player);
        }
    }
}
