using System;
using NAudio.CoreAudioApi;

namespace MediaPlayer.Vlc
{
    public static class BalanceExtensions
    {
        private const int LeftChan = 0;
        private const int RightChan = 1;
        private static MMDevice _device;

        public static MMDevice GetDefaultDevice()
        {
            return _device ??= new MMDeviceEnumerator()
                .GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
        }

        public static float GetBalance(this AudioEndpointVolume volume)
        {
            VerifyChannels(volume);

            var masterVol = Math.Max(1e-6f, volume.MasterVolumeLevelScalar);

            var leftVol = volume.Channels[LeftChan].VolumeLevelScalar;
            var rightVol = volume.Channels[RightChan].VolumeLevelScalar;

            var balance = (rightVol - leftVol)/masterVol;
            return balance;
        }

        public static void SetBalance(this AudioEndpointVolume volume, float balance)
        {
            VerifyChannels(volume);

            var safeBalance = Math.Max(-1, Math.Min(1, balance));
            var masterVol = volume.MasterVolumeLevelScalar;
            var rightVol = 1f + Math.Min(0f, safeBalance);
            var leftVol = 1f - Math.Max(0f, safeBalance);

            volume.Channels[LeftChan].VolumeLevelScalar = leftVol*masterVol;
            volume.Channels[RightChan].VolumeLevelScalar = rightVol*masterVol;
        }

        public static AudioEndpointVolume GetDefaultVolumeEndpoint()
        {
            return GetDefaultDevice()
                .AudioEndpointVolume;
        }

        private static void VerifyChannels(AudioEndpointVolume volume)
        {
            if (volume == null) throw new ArgumentNullException(nameof(volume));
            if (volume.Channels.Count < 2)
                throw new InvalidOperationException(
                    "The specified audio endpoint does not expose left/right volume channels");
        }
    }
}
