using System;

namespace MediaPlayer
{
    public class MediaPlayerViewModel : IMediaPlayerViewModel
    {
        public MediaPlayerViewModel(IMediaPlayer player, IMediaDisplayViewModel display)
        {
            Player = player ?? throw new ArgumentNullException(nameof(player));
            Display = display ?? throw new ArgumentNullException(nameof(display));
        }

        public IMediaPlayer Player { get; }
        public IMediaDisplayViewModel Display { get; }
    }
}