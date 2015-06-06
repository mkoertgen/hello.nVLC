using System;

namespace MediaPlayer
{
    public class MediaPlayerViewModel : IMediaPlayerViewModel
    {
        public MediaPlayerViewModel(IMediaPlayer player, IMediaDisplayViewModel display)
        {
            if (player == null) throw new ArgumentNullException(nameof(player));
            if (display == null) throw new ArgumentNullException(nameof(display));
            Player = player;
            Display = display;
        }

        public IMediaPlayer Player { get; }
        public IMediaDisplayViewModel Display { get; }
    }
}