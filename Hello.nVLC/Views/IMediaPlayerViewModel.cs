using MediaPlayer;

namespace Hello.nVLC
{
    public interface IMediaPlayerViewModel
    {
        IMediaPlayer Player { get; }
        IMediaDisplayViewModel Display { get; }
    }
}
