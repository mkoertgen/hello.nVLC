using System;
using System.Runtime.InteropServices;
using CoreAudioApi.Interfaces;

namespace CoreAudioApi
{
    // ReSharper disable UnusedMember.Global
    // ReSharper disable MemberCanBePrivate.Global
    public class AudioStreamVolume : IDisposable
    {
        private IAudioStreamVolume _audioStreamVolumeInterface;

        internal AudioStreamVolume(IAudioStreamVolume audioStreamVolumeInterface)
        {
            if (audioStreamVolumeInterface == null) throw new ArgumentNullException(nameof(audioStreamVolumeInterface));
            _audioStreamVolumeInterface = audioStreamVolumeInterface;
        }

        public int ChannelCount
        {
            get
            {
                unchecked
                {
                    return (int)MarshalEx.Get<uint>(_audioStreamVolumeInterface.GetChannelCount);
                }
            }
        }

        // ReSharper disable once UnusedParameter.Local
        private void CheckChannelIndex(int channelIndex, string parameter)
        {
            var channelCount = ChannelCount;
            if (channelIndex >= channelCount)
            {
                throw new ArgumentOutOfRangeException(parameter,
                    "You must supply a valid channel index < current count of channels: " + channelCount);
            }
        }

        public float[] GetAllVolumes()
        {
            uint channels;
            Marshal.ThrowExceptionForHR(_audioStreamVolumeInterface.GetChannelCount(out channels));
            var levels = new float[channels];
            Marshal.ThrowExceptionForHR(_audioStreamVolumeInterface.GetAllVolumes(channels, levels));
            return levels;
        }

        public float GetChannelVolume(int channelIndex)
        {
            CheckChannelIndex(channelIndex, "channelIndex");

            uint index;
            float level;
            unchecked { index = (uint) channelIndex; }
            Marshal.ThrowExceptionForHR(_audioStreamVolumeInterface.GetChannelVolume(index, out level));
            return level;
        }

        public void SetAllVolumes(float[] levels)
        {
            // Make friendly Net exceptions for common problems:
            var channelCount = ChannelCount;
            if (levels == null)
                throw new ArgumentNullException(nameof(levels));

            if (levels.Length != channelCount)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(levels),
                    $"{nameof(SetAllVolumes)} MUST be supplied with a volume level for ALL channels. The AudioStream has {channelCount} channels and you supplied {levels.Length} channels.");
            }

            for (var i = 0; i < levels.Length; i++)
            {
                var level = levels[i];
                if (level < 0.0f)
                    throw new ArgumentOutOfRangeException(nameof(levels),
                        "All volumes must be between 0.0 and 1.0. Invalid volume at index: " + i);
                if (level > 1.0f)
                    throw new ArgumentOutOfRangeException(nameof(levels),
                        "All volumes must be between 0.0 and 1.0. Invalid volume at index: " + i);
            }
            unchecked
            {
                Marshal.ThrowExceptionForHR(_audioStreamVolumeInterface.SetAllVoumes((uint) channelCount, levels));
            }
        }

        public void SetChannelVolume(int index, float level)
        {
            CheckChannelIndex(index, "index");

            if (level < 0.0f)
                throw new ArgumentOutOfRangeException(nameof(level), "Volume must be between 0.0 and 1.0");
            if (level > 1.0f)
                throw new ArgumentOutOfRangeException(nameof(level), "Volume must be between 0.0 and 1.0");
            unchecked
            {
                Marshal.ThrowExceptionForHR(_audioStreamVolumeInterface.SetChannelVolume((uint) index, level));
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!disposing) return;
            if (_audioStreamVolumeInterface == null) return;
            // although GC would do this for us, we want it done now
            Marshal.ReleaseComObject(_audioStreamVolumeInterface);
            _audioStreamVolumeInterface = null;
        }
    }
}