/*
  LICENSE
  -------
  Copyright (C) 2007 Ray Molenkamp

  This source code is provided 'as-is', without any express or implied
  warranty.  In no event will the authors be held liable for any damages
  arising from the use of this source code or the software it produces.

  Permission is granted to anyone to use this source code for any purpose,
  including commercial applications, and to alter it and redistribute it
  freely, subject to the following restrictions:

  1. The origin of this source code must not be misrepresented; you must not
     claim that you wrote the original source code.  If you use this source code
     in a product, an acknowledgment in the product documentation would be
     appreciated but is not required.
  2. Altered source versions must be plainly marked as such, and must not be
     misrepresented as being the original source code.
  3. This notice may not be removed or altered from any source distribution.
*/

using System;
using System.Runtime.InteropServices;
using CoreAudioApi.Interfaces;

namespace CoreAudioApi
{
    // ReSharper disable MemberCanBePrivate.Global
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    // ReSharper disable UnusedMember.Global
    public class AudioEndpointVolume : IDisposable
    {
        private static readonly Guid Guid = new Guid("5CDF2C82-841E-4546-9722-0CF74078229A");
        private readonly IAudioEndpointVolume _audioEndPointVolume;
        private AudioEndpointVolumeCallback _callBack;

        internal AudioEndpointVolume(IMMDevice device) : this(device.Activate<IAudioEndpointVolume>(Guid))
        {}

        private AudioEndpointVolume(IAudioEndpointVolume realEndpointVolume)
        {
            if (realEndpointVolume == null) throw new ArgumentNullException(nameof(realEndpointVolume));

            _audioEndPointVolume = realEndpointVolume;
            Channels = new AudioEndpointVolumeChannels(_audioEndPointVolume);
            StepInformation = new AudioEndpointVolumeStepInformation(_audioEndPointVolume);
            var hardwareSupp = MarshalEx.Get<uint>(_audioEndPointVolume.QueryHardwareSupport);
            HardwareSupport = (EEndpointHardwareSupport) hardwareSupp;
            VolumeRange = new AudioEndpointVolumeVolumeRange(_audioEndPointVolume);
            _callBack = new AudioEndpointVolumeCallback(this);
            Marshal.ThrowExceptionForHR(_audioEndPointVolume.RegisterControlChangeNotify(_callBack));
        }

        public AudioEndpointVolumeVolumeRange VolumeRange { get; }
        public EEndpointHardwareSupport HardwareSupport { get; }
        public AudioEndpointVolumeStepInformation StepInformation { get; }
        public AudioEndpointVolumeChannels Channels { get; }

        public float MasterVolumeLevel
        {
            get { return MarshalEx.Get<float>(_audioEndPointVolume.GetMasterVolumeLevel); }
            set { Marshal.ThrowExceptionForHR(_audioEndPointVolume.SetMasterVolumeLevel(value, Guid.Empty)); }
        }

        public float MasterVolumeLevelScalar
        {
            get { return MarshalEx.Get<float>(_audioEndPointVolume.GetMasterVolumeLevelScalar); }
            set { Marshal.ThrowExceptionForHR(_audioEndPointVolume.SetMasterVolumeLevelScalar(value, Guid.Empty)); }
        }

        public bool Mute
        {
            get { return MarshalEx.Get<bool>(_audioEndPointVolume.GetMute); }
            set { Marshal.ThrowExceptionForHR(_audioEndPointVolume.SetMute(value, Guid.Empty)); }
        }

        // ReSharper disable once EventNeverSubscribedTo.Global
        public event AudioEndpointVolumeNotificationDelegate OnVolumeNotification;

        public void VolumeStepUp() { Marshal.ThrowExceptionForHR(_audioEndPointVolume.VolumeStepUp(Guid.Empty)); }
        public void VolumeStepDown() { Marshal.ThrowExceptionForHR(_audioEndPointVolume.VolumeStepDown(Guid.Empty)); }

        internal void FireNotification(AudioVolumeNotificationData notificationData)
        {
            OnVolumeNotification?.Invoke(notificationData);
        }

        public void Dispose()
        {
            if (_callBack != null)
            {
                Marshal.ThrowExceptionForHR(_audioEndPointVolume.UnregisterControlChangeNotify(_callBack));
                _callBack = null;
            }
            GC.SuppressFinalize(this);
        }

        ~AudioEndpointVolume() { Dispose(); }
    }
}