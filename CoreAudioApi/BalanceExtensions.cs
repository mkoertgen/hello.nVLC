using System;

namespace CoreAudioApi
{
    public static class BalanceExtensions
    {
        // left=0, right=1
        private const int LeftChan = 0;
        private const int RightChan = 1;

        /// <summary>
        /// Gets the ratio of volume across the left and right speakers in a range between -1 (left) and 1 (right). The default is 0 (center).
        /// </summary>
        /// <param name="volume">The audio volume endpoint</param>
        /// <returns></returns>
        public static float GetBalance(this AudioEndpointVolume volume)
        {
            VerifyChannels(volume);

            var masterVol = volume.MasterVolumeLevelScalar;
            var leftVol = volume.Channels[LeftChan].VolumeLevelScalar;
            var rightVol = volume.Channels[RightChan].VolumeLevelScalar;

            var balance = (rightVol - leftVol) / masterVol;
            return balance;
        }

        public static void SetBalance(this AudioEndpointVolume volume, float balance)
        {
            var masterVol = volume.MasterVolumeLevelScalar;
            var rightVol = 1f + Math.Min(0f, balance);
            var leftVol = 1f - Math.Max(0f, balance);

            volume.Channels[LeftChan].VolumeLevelScalar = leftVol * masterVol;
            volume.Channels[RightChan].VolumeLevelScalar = rightVol * masterVol;
        }

        private static void VerifyChannels(AudioEndpointVolume volume)
        {
            if (volume == null) throw new ArgumentNullException(nameof(volume));
            if (volume.Channels.Count < 2)
                throw new InvalidOperationException("The specified audio endpoint does not expose left/right volume channels");
        }
    }
}