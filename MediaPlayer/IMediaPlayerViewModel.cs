namespace MediaPlayer
{
    public interface IMediaPlayerViewModel
    {
        IMediaPlayer Player { get; }
        IMediaDisplayViewModel Display { get; }
    }
}