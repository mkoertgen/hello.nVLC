using System.Runtime.InteropServices;

namespace CoreAudioApi.Interfaces
{
    [Guid("0BD7A1BE-7A1A-44DB-8397-CC5392387B5E"),
     InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    // ReSharper disable once InconsistentNaming
    internal interface IMMDeviceCollection
    {
        int GetCount(out int numDevices);
        // ReSharper disable once UnusedMethodReturnValue.Global
        int Item(int deviceNumber, out IMMDevice device);
    }
}