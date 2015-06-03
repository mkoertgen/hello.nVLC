using System;
using System.Runtime.InteropServices;
using CoreAudioApi.Interfaces;

namespace CoreAudioApi
{
    // ReSharper disable UnusedMember.Global
    // ReSharper disable MemberCanBePrivate.Global
    public class AudioClient : IDisposable
    {
        private static readonly Guid Guid = new Guid("1CB9AD4C-DBFA-4c32-B178-C2F568A703B2");

        private AudioCaptureClient _audioCaptureClient;
        private IAudioClient _audioClientInterface;
        private AudioClockClient _audioClockClient;
        private AudioRenderClient _audioRenderClient;
        private AudioStreamVolume _audioStreamVolume;
        private WaveFormat _mixFormat;
        private AudioClientShareMode _shareMode;

        private AudioClient(IAudioClient audioClientInterface)
        {
            if (audioClientInterface == null) throw new ArgumentNullException(nameof(audioClientInterface));
            _audioClientInterface = audioClientInterface;
        }

        internal AudioClient(IMMDevice device) : this(device.Activate<IAudioClient>(Guid))
        {}

        public WaveFormat MixFormat
        {
            get
            {
                if (_mixFormat != null) return _mixFormat;
                var waveFormatPointer = MarshalEx.Get<IntPtr>(_audioClientInterface.GetMixFormat);
                var waveFormat = WaveFormat.MarshalFromPtr(waveFormatPointer);
                Marshal.FreeCoTaskMem(waveFormatPointer);
                _mixFormat = waveFormat;
                return _mixFormat;
            }
        }

        public int BufferSize => (int)MarshalEx.Get<uint>(_audioClientInterface.GetBufferSize);
        public long StreamLatency => _audioClientInterface.GetStreamLatency();
        public int CurrentPadding => MarshalEx.Get<int>(_audioClientInterface.GetCurrentPadding);
        public long DefaultDevicePeriod => MarshalEx.Get<long, long>(_audioClientInterface.GetDevicePeriod).Item1;
        public long MinimumDevicePeriod => MarshalEx.Get<long, long>(_audioClientInterface.GetDevicePeriod).Item2;

        // TODO: GetService:
        // IID_IAudioSessionControl
        // IID_IChannelAudioVolume
        // IID_ISimpleAudioVolume

        public AudioStreamVolume AudioStreamVolume
        {
            get
            {
                if (_shareMode == AudioClientShareMode.Exclusive)
                    throw new InvalidOperationException("AudioStreamVolume is ONLY supported for shared audio streams.");

                if (_audioStreamVolume != null) return _audioStreamVolume;

                object audioStreamVolumeInterface;
                var audioStreamVolumeGuid = new Guid("93014887-242D-4068-8A15-CF5E93B90FE3");
                Marshal.ThrowExceptionForHR(_audioClientInterface.GetService(audioStreamVolumeGuid,
                    out audioStreamVolumeInterface));
                _audioStreamVolume = new AudioStreamVolume((IAudioStreamVolume) audioStreamVolumeInterface);
                return _audioStreamVolume;
            }
        }

        public AudioClockClient AudioClockClient
        {
            get
            {
                if (_audioClockClient != null) return _audioClockClient;
                object audioClockClientInterface;
                var audioClockClientGuid = new Guid("CD63314F-3FBA-4a1b-812C-EF96358728E7");
                Marshal.ThrowExceptionForHR(_audioClientInterface.GetService(audioClockClientGuid,
                    out audioClockClientInterface));
                _audioClockClient = new AudioClockClient((IAudioClock) audioClockClientInterface);
                return _audioClockClient;
            }
        }

        public AudioRenderClient AudioRenderClient
        {
            get
            {
                if (_audioRenderClient != null) return _audioRenderClient;
                object audioRenderClientInterface;
                var audioRenderClientGuid = new Guid("F294ACFC-3146-4483-A7BF-ADDCA7C260E2");
                Marshal.ThrowExceptionForHR(_audioClientInterface.GetService(audioRenderClientGuid,
                    out audioRenderClientInterface));
                _audioRenderClient = new AudioRenderClient((IAudioRenderClient) audioRenderClientInterface);
                return _audioRenderClient;
            }
        }

        public AudioCaptureClient AudioCaptureClient
        {
            get
            {
                if (_audioCaptureClient != null) return _audioCaptureClient;
                object audioCaptureClientInterface;
                var audioCaptureClientGuid = new Guid("c8adbd64-e71e-48a0-a4de-185c395cd317");
                Marshal.ThrowExceptionForHR(_audioClientInterface.GetService(audioCaptureClientGuid,
                    out audioCaptureClientInterface));
                _audioCaptureClient = new AudioCaptureClient((IAudioCaptureClient) audioCaptureClientInterface);
                return _audioCaptureClient;
            }
        }

        public void Dispose()
        {
            if (_audioClientInterface == null) return;

            if (_audioClockClient != null)
            {
                _audioClockClient.Dispose();
                _audioClockClient = null;
            }

            if (_audioRenderClient != null)
            {
                _audioRenderClient.Dispose();
                _audioRenderClient = null;
            }

            if (_audioCaptureClient != null)
            {
                _audioCaptureClient.Dispose();
                _audioCaptureClient = null;
            }

            if (_audioStreamVolume != null)
            {
                _audioStreamVolume.Dispose();
                _audioStreamVolume = null;
            }

            Marshal.ReleaseComObject(_audioClientInterface);
            _audioClientInterface = null;

            GC.SuppressFinalize(this);
        }

        public void Initialize(AudioClientShareMode shareMode,
            AudioClientStreamFlags streamFlags,
            long bufferDuration,
            long periodicity,
            WaveFormat waveFormat,
            Guid audioSessionGuid)
        {
            _shareMode = shareMode;
            var hresult = _audioClientInterface.Initialize(shareMode, streamFlags, bufferDuration, periodicity,
                waveFormat, ref audioSessionGuid);
            Marshal.ThrowExceptionForHR(hresult);
            // may have changed the mix format so reset it
            _mixFormat = null;
        }

        public bool IsFormatSupported(AudioClientShareMode shareMode,
            WaveFormat desiredFormat)
        {
            WaveFormatExtensible closestMatchFormat;
            return IsFormatSupported(shareMode, desiredFormat, out closestMatchFormat);
        }

        public bool IsFormatSupported(AudioClientShareMode shareMode, WaveFormat desiredFormat,
            out WaveFormatExtensible closestMatchFormat)
        {
            var hresult = _audioClientInterface.IsFormatSupported(shareMode, desiredFormat, out closestMatchFormat);
            // S_OK is 0, S_FALSE = 1
            switch (hresult)
            {
                case 0:
                    // directly supported
                    return true;
                case 1:
                    return false;
                case (int) AudioClientErrors.UnsupportedFormat:
                    return false;
            }
            Marshal.ThrowExceptionForHR(hresult);
            // shouldn't get here
            throw new NotSupportedException("Unknown hresult " + hresult);
        }

        public void Start() { _audioClientInterface.Start(); }
        public void Stop() { _audioClientInterface.Stop(); }
        public void SetEventHandle(IntPtr eventWaitHandle) { _audioClientInterface.SetEventHandle(eventWaitHandle); }
        public void Reset() { _audioClientInterface.Reset(); }
    }
}