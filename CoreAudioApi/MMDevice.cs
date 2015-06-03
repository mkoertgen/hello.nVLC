using System;
using System.Runtime.InteropServices;
using CoreAudioApi.Interfaces;

namespace CoreAudioApi
{
    // ReSharper disable once InconsistentNaming
    // ReSharper disable UnusedMember.Global
    // ReSharper disable MemberCanBePrivate.Global
    // ReSharper disable SuspiciousTypeConversion.Global
    public class MMDevice
    {
        private readonly IMMDevice _deviceInterface;
        private AudioEndpointVolume _audioEndpointVolume;
        private AudioMeterInformation _audioMeterInformation;
        private AudioSessionManager _audioSessionManager;
        private PropertyStore _propertyStore;

        internal MMDevice(IMMDevice realDevice)
        {
            if (realDevice == null) throw new ArgumentNullException(nameof(realDevice));
            _deviceInterface = realDevice;
        }

        public AudioClient AudioClient => new AudioClient(_deviceInterface);
        public AudioMeterInformation AudioMeterInformation => _audioMeterInformation ?? (_audioMeterInformation = new AudioMeterInformation(_deviceInterface));
        public AudioEndpointVolume AudioEndpointVolume => _audioEndpointVolume ?? (_audioEndpointVolume = new AudioEndpointVolume(_deviceInterface));
        public AudioSessionManager AudioSessionManager => _audioSessionManager ?? (_audioSessionManager = new AudioSessionManager(_deviceInterface));
        public PropertyStore Properties => _propertyStore ?? (_propertyStore = GetPropertyInformation());

        public string FriendlyName => Properties.Get(PropertyKeys.PkeyDeviceFriendlyName, "Unknown");
        public string DeviceFriendlyName => Properties.Get(PropertyKeys.PkeyDeviceInterfaceFriendlyName, "Unknown");
        public string IconPath => Properties.Get(PropertyKeys.PkeyDeviceIconPath, "Unknown");

        public string Id => MarshalEx.Get<string>(_deviceInterface.GetId);
        public DataFlow DataFlow => MarshalEx.Get<DataFlow>(((IMMEndpoint) _deviceInterface).GetDataFlow);
        public DeviceState State => MarshalEx.Get<DeviceState>(_deviceInterface.GetState);

        private PropertyStore GetPropertyInformation()
        {
            IPropertyStore propstore;
            Marshal.ThrowExceptionForHR(_deviceInterface.OpenPropertyStore(StorageAccessMode.Read, out propstore));
            return new PropertyStore(propstore);
        }

        public override string ToString() { return FriendlyName; }
    }
}