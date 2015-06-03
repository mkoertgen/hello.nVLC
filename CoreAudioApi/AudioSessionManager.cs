// -----------------------------------------
// milligan22963 - implemented to work with nAudio
// 12/2014
// -----------------------------------------

using System;
using System.Runtime.InteropServices;
using CoreAudioApi.Interfaces;

namespace CoreAudioApi
{
    // ReSharper disable MemberCanBePrivate.Global
    // ReSharper disable UnusedMember.Global
    public class AudioSessionManager
    {
        public delegate void SessionCreatedDelegate(object sender, IAudioSessionControl newSession);

        private static readonly Guid Guid = new Guid("BFA971F1-4D5E-40BB-935E-967039BFBEE4");
        private readonly IAudioSessionManager _audioSessionInterface;
        private readonly IAudioSessionManager2 _audioSessionInterface2;
        private AudioSessionControl _audioSessionControl;
        private AudioSessionNotification _audioSessionNotification;
        private SimpleAudioVolume _simpleAudioVolume;

        internal AudioSessionManager(IMMDevice device) : this(device.Activate<IAudioSessionManager2>(Guid))
        {}

        private AudioSessionManager(IAudioSessionManager audioSessionManager)
        {
            if (audioSessionManager == null) throw new ArgumentNullException(nameof(audioSessionManager));
            _audioSessionInterface = audioSessionManager;
            _audioSessionInterface2 = audioSessionManager as IAudioSessionManager2;

            RefreshSessions();
        }

        public SimpleAudioVolume SimpleAudioVolume
        {
            get
            {
                if (_simpleAudioVolume != null) return _simpleAudioVolume;
                ISimpleAudioVolume simpleAudioInterface;
                _audioSessionInterface.GetSimpleAudioVolume(Guid.Empty, 0, out simpleAudioInterface);
                _simpleAudioVolume = new SimpleAudioVolume(simpleAudioInterface);
                return _simpleAudioVolume;
            }
        }

        public AudioSessionControl AudioSessionControl
        {
            get
            {
                if (_audioSessionControl != null) return _audioSessionControl;
                IAudioSessionControl audioSessionControlInterface;
                _audioSessionInterface.GetAudioSessionControl(Guid.Empty, 0, out audioSessionControlInterface);
                _audioSessionControl = new AudioSessionControl(audioSessionControlInterface);
                return _audioSessionControl;
            }
        }

        public SessionCollection Sessions { get; private set; }

        // ReSharper disable once EventNeverSubscribedTo.Global
        public event SessionCreatedDelegate OnSessionCreated;

        internal void FireSessionCreated(IAudioSessionControl newSession)
        {
            OnSessionCreated?.Invoke(this, newSession);
        }

        public void RefreshSessions()
        {
            UnregisterNotifications();

            if (_audioSessionInterface2 == null) return;
            IAudioSessionEnumerator sessionEnum;
            Marshal.ThrowExceptionForHR(_audioSessionInterface2.GetSessionEnumerator(out sessionEnum));
            Sessions = new SessionCollection(sessionEnum);

            _audioSessionNotification = new AudioSessionNotification(this);
            Marshal.ThrowExceptionForHR(
                _audioSessionInterface2.RegisterSessionNotification(_audioSessionNotification));
        }

        public void Dispose()
        {
            UnregisterNotifications();
            GC.SuppressFinalize(this);
        }

        private void UnregisterNotifications()
        {
            if (Sessions != null)
                Sessions = null;

            if (_audioSessionNotification != null)
                Marshal.ThrowExceptionForHR(
                    _audioSessionInterface2.UnregisterSessionNotification(_audioSessionNotification));
        }

        ~AudioSessionManager() { Dispose(); }
    }
}