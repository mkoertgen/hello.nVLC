using System;

namespace MediaPlayer.Vlc
{
    public class VlcDisplayViewModel : IMediaDisplayViewModel
    {
        private readonly VlcPlayer _player;

        public VlcDisplayViewModel(VlcPlayer player)
        {
            if (player == null) throw new ArgumentNullException(nameof(player));
            _player = player;
        }

        public IntPtr WindowHandle
        {
            get { return _player.WindowHandle; }
            set { _player.WindowHandle = value; }
        }
    }
}