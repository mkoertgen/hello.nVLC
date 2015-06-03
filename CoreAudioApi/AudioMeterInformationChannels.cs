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
    // ReSharper disable UnusedMember.Global
    public class AudioMeterInformationChannels
    {
        private readonly IAudioMeterInformation _audioMeterInformation;

        internal AudioMeterInformationChannels(IAudioMeterInformation parent)
        {
            if (parent == null) throw new ArgumentNullException(nameof(parent));
            _audioMeterInformation = parent;
        }

        public int Count => MarshalEx.Get<int>(_audioMeterInformation.GetMeteringChannelCount);

        public float this[int index]
        {
            get
            {
                var peakValues = new float[Count];
                var Params = GCHandle.Alloc(peakValues, GCHandleType.Pinned);
                Marshal.ThrowExceptionForHR(_audioMeterInformation.GetChannelsPeakValues(peakValues.Length,
                    Params.AddrOfPinnedObject()));
                Params.Free();
                return peakValues[index];
            }
        }
    }
}