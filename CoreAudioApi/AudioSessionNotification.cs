using System.Runtime.InteropServices;
using CoreAudioApi.Interfaces;

namespace CoreAudioApi
{
    internal class AudioSessionNotification : IAudioSessionNotification
    {
        private readonly AudioSessionManager _parent;

        internal AudioSessionNotification(AudioSessionManager parent)
        {
            _parent = parent;
        }

        [PreserveSig]
        public int OnSessionCreated(IAudioSessionControl newSession)
        {
            _parent.FireSessionCreated(newSession);
            return 0;
        }
    }
}