using System.Runtime.InteropServices;

namespace CoreAudioApi.Interfaces
{
    [Guid("1BE09788-6894-4089-8586-9A2A6C265AC5"),
     InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    // ReSharper disable once InconsistentNaming
    internal interface IMMEndpoint
    {
        int GetDataFlow(out DataFlow dataFlow);
    }
}