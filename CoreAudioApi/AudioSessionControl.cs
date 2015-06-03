// -----------------------------------------
// milligan22963 - implemented to work with nAudio
// 12/2014
// -----------------------------------------

// extracted from NAudio

using System;
using System.Runtime.InteropServices;
using CoreAudioApi.Interfaces;

namespace CoreAudioApi
{
    // ReSharper disable UnusedMember.Global
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    // ReSharper disable MemberCanBePrivate.Global
    public class AudioSessionControl : IDisposable
    {
        private readonly IAudioSessionControl _audioSessionControlInterface;
        private readonly IAudioSessionControl2 _audioSessionControlInterface2;
        private AudioSessionEventsCallback _audioSessionEventCallback;

        public AudioSessionControl(IAudioSessionControl audioSessionControl)
        {
            _audioSessionControlInterface = audioSessionControl;
            _audioSessionControlInterface2 = audioSessionControl as IAudioSessionControl2;

            // ReSharper disable SuspiciousTypeConversion.Global
            var meters = _audioSessionControlInterface as IAudioMeterInformation;
            var volume = _audioSessionControlInterface as ISimpleAudioVolume;
            if (meters != null)
                AudioMeterInformation = new AudioMeterInformation(meters);
            if (volume != null)
                SimpleAudioVolume = new SimpleAudioVolume(volume);
        }

        public AudioMeterInformation AudioMeterInformation { get; }

        public SimpleAudioVolume SimpleAudioVolume { get; }

        public AudioSessionState State => MarshalEx.Get<AudioSessionState>(_audioSessionControlInterface.GetState);

        public string DisplayName
        {
            get { return MarshalEx.Get(_audioSessionControlInterface.GetDisplayName, string.Empty); }
            set
            {
                if (value != string.Empty)
                    Marshal.ThrowExceptionForHR(_audioSessionControlInterface.SetDisplayName(value, Guid.Empty));
            }
        }

        public string IconPath
        {
            get { return MarshalEx.Get(_audioSessionControlInterface.GetIconPath, string.Empty); }
            set
            {
                if (value != string.Empty)
                    Marshal.ThrowExceptionForHR(_audioSessionControlInterface.SetIconPath(value, Guid.Empty));
            }
        }

        public string GetSessionIdentifier
        {
            get
            {
                VerifyOs();
                return MarshalEx.Get<string>(_audioSessionControlInterface2.GetSessionIdentifier);
            }
        }

        public string GetSessionInstanceIdentifier
        {
            get
            {
                VerifyOs();
                return MarshalEx.Get<string>(_audioSessionControlInterface2.GetSessionInstanceIdentifier);
            }
        }

        public uint GetProcessId
        {
            get
            {
                VerifyOs();
                return MarshalEx.Get<uint>(_audioSessionControlInterface2.GetProcessId);
            }
        }

        public bool IsSystemSoundsSession
        {
            get
            {
                VerifyOs();
                return (_audioSessionControlInterface2.IsSystemSoundsSession() == 0);
            }
        }

        private void VerifyOs()
        {
            if (_audioSessionControlInterface2 == null)
                throw new InvalidOperationException("Not supported on this version of Windows");
        }

        public Guid GetGroupingParam()
        {
            return MarshalEx.Get(_audioSessionControlInterface.GetGroupingParam, Guid.Empty);
        }

        public void SetGroupingParam(Guid groupingId, Guid context)
        {
            Marshal.ThrowExceptionForHR(_audioSessionControlInterface.SetGroupingParam(groupingId, context));
        }

        public void RegisterEventClient(IAudioSessionEventsHandler eventClient)
        {
            // we could have an array or list of listeners if we like
            _audioSessionEventCallback = new AudioSessionEventsCallback(eventClient);
            Marshal.ThrowExceptionForHR(
                _audioSessionControlInterface.RegisterAudioSessionNotification(_audioSessionEventCallback));
        }

        public void UnRegisterEventClient()
        {
            // if one is registered, let it go
            if (_audioSessionEventCallback == null) return;
            Marshal.ThrowExceptionForHR(
                _audioSessionControlInterface.UnregisterAudioSessionNotification(_audioSessionEventCallback));
        }

        public void Dispose()
        {
            if (_audioSessionEventCallback != null)
                Marshal.ThrowExceptionForHR(
                    _audioSessionControlInterface.UnregisterAudioSessionNotification(_audioSessionEventCallback));
            GC.SuppressFinalize(this);
        }

        ~AudioSessionControl() { Dispose(); }
    }
}