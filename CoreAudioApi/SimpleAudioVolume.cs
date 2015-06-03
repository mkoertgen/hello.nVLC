// -----------------------------------------
// milligan22963 - implemented to work with nAudio
// 12/2014
// -----------------------------------------

using System;
using System.Runtime.InteropServices;
using CoreAudioApi.Interfaces;

namespace CoreAudioApi
{
    // ReSharper disable UnusedMember.Global
    public class SimpleAudioVolume : IDisposable
    {
        private readonly ISimpleAudioVolume _simpleAudioVolume;

        internal SimpleAudioVolume(ISimpleAudioVolume realSimpleVolume)
        {
            if (realSimpleVolume == null) throw new ArgumentNullException(nameof(realSimpleVolume));
            _simpleAudioVolume = realSimpleVolume;
        }

        public float Volume
        {
            get { return MarshalEx.Get<float>(_simpleAudioVolume.GetMasterVolume); }
            set
            {
                if (value < 0f || value > 1f)
                    throw new ArgumentOutOfRangeException(nameof(value), "Volume must be within 0 and 1");
                    Marshal.ThrowExceptionForHR(_simpleAudioVolume.SetMasterVolume(value, Guid.Empty));
            }
        }

        public bool Mute
        {
            get { return MarshalEx.Get<bool>(_simpleAudioVolume.GetMute); }
            set { Marshal.ThrowExceptionForHR(_simpleAudioVolume.SetMute(value, Guid.Empty)); }
        }

        public void Dispose() { GC.SuppressFinalize(this); }
        ~SimpleAudioVolume() { Dispose(); }
    }
}