using System;
using System.Windows.Media;

namespace MediaPlayer.Windows
{
    public class WindowsDisplayViewModel : IMediaDisplayViewModel
    {
        public WindowsDisplayViewModel(WindowsMediaPlayer player)
        {
            if (player == null) throw new ArgumentNullException(nameof(player));
            VideoBrush = player.VideoBrush;
        }

        public DrawingBrush VideoBrush { get; }
    }
}