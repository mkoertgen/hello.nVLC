using System;
using System.ComponentModel;

namespace MediaPlayer
{
    public interface IMediaPlayer : INotifyPropertyChanged
    {
        /// <summary> Gets the maximum speed ratio. </summary>
        double MaxSpeedRatio { get; }

        /// <summary> Gets the minimum speed ratio. </summary>
        double MinSpeedRatio { get; }

        /// <summary>Gets the media error.</summary>
        /// <returns>The media error.</returns>
        IPlayerError Error { get; }

        /// <summary>Plays media from the current System.Windows.Media.MediaPlayer.Position.</summary>
        void Play();
        /// <summary>Gets a value indicating whether the media can be played.</summary>
        /// <returns>true if the media can be played; otherwise, false.</returns>
        bool CanPlay { get; }
        /// <summary>Gets a value that indicates whether the media is playing.</summary>
        /// <returns>true if the media is playing; otherwise, false.</returns>
        bool IsPlaying { get; }

        /// <summary>Pauses media playback.</summary>
        void Pause();
        /// <summary>Gets a value indicating whether the media can be paused.</summary>
        /// <returns>true if the media can be paused; otherwise, false.</returns>
        bool CanPause { get; }
        /// <summary>Gets a value that indicates whether the media is paused.</summary>
        /// <returns>true if the media is paused; otherwise, false.</returns>
        bool IsPaused { get; }

        /// <summary>Stops media playback.</summary>
        void Stop();
        /// <summary>Gets a value indicating whether the media can be stopped.</summary>
        /// <returns>true if the media can be stopped; otherwise, false.</returns>
        bool CanStop { get; }
        /// <summary>Gets a value that indicates whether the media is stopped.</summary>
        /// <returns>true if the media is stopped; otherwise, false.</returns>
        bool IsStopped { get; }

        /// <summary>Gets a value indicating whether the media can be muted.</summary>
        /// <returns>true if the media can be muted; otherwise, false.</returns>
        bool CanMute { get; }
        /// <summary>Mutes the media, i.e. sets the playback volume to 0.</summary>
        void Mute();
        /// <summary>Gets a value that indicates whether the media is muted.</summary>
        /// <returns>true if the media is muted; otherwise, false.</returns>
        bool IsMuted { get; }

        /// <summary>Gets a value indicating whether the media can be unmuted.</summary>
        /// <returns>true if the media can be unmuted; otherwise, false.</returns>
        bool CanUnMute { get; }
        /// <summary>Unmutes the media, i.e. restores the playback volume to the volume before muting.</summary>
        void UnMute();

        /// <summary>Gets the media System.Uri.</summary>
        /// <returns>The current media System.Uri.</returns>
        Uri Source { get; set; }

        /// <summary>Gets or sets the current position of the media.</summary>
        /// <returns>The current position of the media (in seconds).</returns>
        double Position { get; set; }

        /// <summary>Gets the duration of the media.</summary>
        /// <returns>The duration of the media (in seconds). O if unknown like for e.g. continuous network streams.</returns>
        double Duration { get; }

        /// <summary>Gets a value that indicates whether the media has a known duration.</summary>
        /// <returns>true if the media has a known duration; otherwise, false.</returns>
        bool HasDuration { get; }

        /// <summary>Gets or sets the ratio of speed that media is played at.</summary>
        /// <returns>The ratio of speed that media is played back represented by a value between 0 and the largest double value. The default is 1.0.</returns>
        double SpeedRatio { get; set; }

        /// <summary>increase speed by the given value. wrapper around <see cref="SpeedRatio"/>.</summary>
        void IncreaseSpeed();
        /// <summary> Gets a value indicating whether this instance can increase speed by. </summary>
        /// <value> <c>true</c> if this instance can increase speed by; otherwise, <c>false</c>. </value>
        bool CanIncreaseSpeed { get; }

        /// <summary>decrease speed by the given value. wrapper around <see cref="SpeedRatio"/>.</summary>
        void DecreaseSpeed();
        /// <summary> Gets a value indicating whether this instance can decrease speed by. </summary>
        /// <value> <c>true</c> if this instance can decrease speed by; otherwise, <c>false</c>. </value>
        bool CanDecreaseSpeed { get; }

        /// <summary>Gets or sets the media's volume.</summary>
        /// <returns>The media's volume represented on a linear scale between 0 and 1. The default is 0.5.</returns>
        double Volume { get; set; }

        /// <summary>Gets or sets the balance between the left and right speaker volumes.</summary>
        /// <returns>The ratio of volume across the left and right speakers in a range between -1 and 1. The default is 0.</returns>
        double Balance { get; set; }
    }
}