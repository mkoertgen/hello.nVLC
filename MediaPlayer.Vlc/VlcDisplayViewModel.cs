using System;

namespace MediaPlayer.Vlc
{
    public class VlcDisplayViewModel : IMediaDisplayViewModel
    {
        private readonly VlcPlayerViewModel _player;

        public IntPtr WindowHandle
        {
            get { return _player.WindowHandle; }
            set { _player.WindowHandle = value; }
        }

        public VlcDisplayViewModel(VlcPlayerViewModel player)
        {
            if (player == null) throw new ArgumentNullException(nameof(player));
            _player = player;
        }
    }
}
