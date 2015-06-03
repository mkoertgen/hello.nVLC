using System;
using System.Runtime.InteropServices;

namespace CoreAudioApi.Interfaces
{
    [Guid("D666063F-1587-4E43-81F1-B948E807363F"),
     InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    // ReSharper disable once InconsistentNaming
    internal interface IMMDevice
    {
        // activationParams is a propvariant
        int Activate(ref Guid id, ClsCtx clsCtx, IntPtr activationParams,
            [MarshalAs(UnmanagedType.IUnknown)] out object interfacePointer);

        int OpenPropertyStore(StorageAccessMode stgmAccess, out IPropertyStore properties);

        int GetId([MarshalAs(UnmanagedType.LPWStr)] out string id);

        int GetState(out DeviceState state);
    }

    static class DeviceExtensions
    {
        public static T Activate<T>(this IMMDevice device, Guid guid) where T : class
        {
            if (device == null) throw new ArgumentNullException(nameof(device));
            object result;
            Marshal.ThrowExceptionForHR(device.Activate(ref guid, ClsCtx.All, IntPtr.Zero, out result));
            return result as T;
        }
    }
}