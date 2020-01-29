using System;
using MediaPlayer;

namespace Hello.nVLC
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

    public class MediaPlayerViewModel<TPlayer, TDisplay> : MediaPlayerViewModel
        where TPlayer : IMediaPlayer
        where TDisplay : IMediaDisplayViewModel
    {
        public MediaPlayerViewModel(TPlayer player, TDisplay display) : base(player, display)
        {
        }
    }
}
