using System;

namespace MediaPlayer
{
    public static class MediaPlayerExtensions
    {
        public static void TogglePlayPause(this IPlayerViewModel player)
        {
            if (player == null) throw new ArgumentNullException(nameof(player));
            if (player.IsPlaying)
            {
                player.Pause();
            }
            else
            {
                player.Play();
            }
        }

        public static void ToggleMute(this IPlayerViewModel player)
        {
            if (player == null) throw new ArgumentNullException(nameof(player));
            if (player.IsMuted)
            {
                player.UnMute();
            }
            else
            {
                player.Mute();
            }
        }

    }
}