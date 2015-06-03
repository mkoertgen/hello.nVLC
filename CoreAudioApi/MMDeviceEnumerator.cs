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
// updated for use in NAudio
// updated for standalone use

using System;
using System.Runtime.InteropServices;
using CoreAudioApi.Interfaces;

namespace CoreAudioApi
{
    public class MMDeviceEnumerator : IDisposable
    {
        private IMMDeviceEnumerator _realEnumerator;

        public MMDeviceEnumerator()
        {
#if !NETFX_CORE
            if (Environment.OSVersion.Version.Major < 6)
            {
                throw new NotSupportedException("This functionality is only supported on Windows Vista or newer.");
            }
#endif
            // ReSharper disable once SuspiciousTypeConversion.Global
            _realEnumerator = new MMDeviceEnumeratorComObject() as IMMDeviceEnumerator;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public MMDeviceCollection EnumerateAudioEndPoints(DataFlow dataFlow, DeviceState dwStateMask)
        {
            IMMDeviceCollection result;
            Marshal.ThrowExceptionForHR(_realEnumerator.EnumAudioEndpoints(dataFlow, dwStateMask, out result));
            return new MMDeviceCollection(result);
        }

        public MMDevice GetDefaultAudioEndpoint(DataFlow dataFlow, Role role)
        {
            IMMDevice device;
            Marshal.ThrowExceptionForHR(_realEnumerator.GetDefaultAudioEndpoint(dataFlow, role, out device));
            return new MMDevice(device);
        }

        public bool HasDefaultAudioEndpoint(DataFlow dataFlow, Role role)
        {
            // ReSharper disable once InconsistentNaming
            const int E_NOTFOUND = unchecked((int) 0x80070490);
            IMMDevice device;
            var hresult = _realEnumerator.GetDefaultAudioEndpoint(dataFlow, role, out device);
            switch (hresult)
            {
                case 0x0:
                    Marshal.ReleaseComObject(device);
                    return true;
                case E_NOTFOUND:
                    return false;
            }
            Marshal.ThrowExceptionForHR(hresult);
            return false;
        }

        public MMDevice GetDevice(string id)
        {
            IMMDevice device;
            Marshal.ThrowExceptionForHR(_realEnumerator.GetDevice(id, out device));
            return new MMDevice(device);
        }

        public int RegisterEndpointNotificationCallback(
            [In] [MarshalAs(UnmanagedType.Interface)] IMMNotificationClient client)
        {
            return _realEnumerator.RegisterEndpointNotificationCallback(client);
        }

        public int UnregisterEndpointNotificationCallback(
            [In] [MarshalAs(UnmanagedType.Interface)] IMMNotificationClient client)
        {
            return _realEnumerator.UnregisterEndpointNotificationCallback(client);
        }

        private void Dispose(bool disposing)
        {
            if (!disposing) return;
            if (_realEnumerator == null) return;
            // although GC would do this for us, we want it done now
            Marshal.ReleaseComObject(_realEnumerator);
            _realEnumerator = null;
        }
    }
}