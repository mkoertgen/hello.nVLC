using System;
using System.Runtime.InteropServices;
using CoreAudioApi.Interfaces;

namespace CoreAudioApi
{
    public class SessionCollection
    {
        private readonly IAudioSessionEnumerator _audioSessionEnumerator;

        internal SessionCollection(IAudioSessionEnumerator realEnumerator)
        {
            if (realEnumerator == null) throw new ArgumentNullException(nameof(realEnumerator));
            _audioSessionEnumerator = realEnumerator;
        }

        public AudioSessionControl this[int index]
        {
            get
            {
                IAudioSessionControl result;
                Marshal.ThrowExceptionForHR(_audioSessionEnumerator.GetSession(index, out result));
                return new AudioSessionControl(result);
            }
        }

        public int Count => MarshalEx.Get<int>(_audioSessionEnumerator.GetCount);
    }
}