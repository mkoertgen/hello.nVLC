using System;

namespace MediaPlayer.Vlc
{
    public class VlcDisplayViewModel : IMediaDisplayViewModel
    {
        private readonly VlcPlayer _player;

        public VlcDisplayViewModel(VlcPlayer player)
        {
            _player = player ?? throw new ArgumentNullException(nameof(player));
        }

        public IntPtr WindowHandle
        {
            get => _player.WindowHandle;
            set => _player.WindowHandle = value;
        }
    }
}