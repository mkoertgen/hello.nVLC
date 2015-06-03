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
// this version modified for NAudio from Ray Molenkamp's original

using System;
using System.Runtime.InteropServices;
using CoreAudioApi.Interfaces;

namespace CoreAudioApi
{
    // ReSharper disable MemberCanBePrivate.Global
    // ReSharper disable UnusedMember.Global
    public class PropertyStore
    {
        private readonly IPropertyStore _storeInterface;

        internal PropertyStore(IPropertyStore store)
        {
            if (store == null) throw new ArgumentNullException(nameof(store));
            _storeInterface = store;
        }

        public int Count => MarshalEx.Get<int>(_storeInterface.GetCount);

        public PropertyStoreProperty this[int index]
        {
            get
            {
                PropVariant result;
                var key = Get(index);
                Marshal.ThrowExceptionForHR(_storeInterface.GetValue(ref key, out result));
                return new PropertyStoreProperty(key, result);
            }
        }

        public PropertyStoreProperty this[PropertyKey key]
        {
            get
            {
                for (var i = 0; i < Count; i++)
                {
                    var ikey = Get(i);
                    if ((ikey.FormatId != key.FormatId) || (ikey.PropertyId != key.PropertyId)) continue;
                    PropVariant result;
                    Marshal.ThrowExceptionForHR(_storeInterface.GetValue(ref ikey, out result));
                    return new PropertyStoreProperty(ikey, result);
                }
                return null;
            }
        }

        public bool Contains(PropertyKey key)
        {
            for (var i = 0; i < Count; i++)
            {
                var ikey = Get(i);
                if ((ikey.FormatId == key.FormatId) && (ikey.PropertyId == key.PropertyId))
                    return true;
            }
            return false;
        }

        public PropertyKey Get(int index)
        {
            PropertyKey key;
            Marshal.ThrowExceptionForHR(_storeInterface.GetAt(index, out key));
            return key;
        }

        public PropVariant GetValue(int index)
        {
            PropVariant result;
            var key = Get(index);
            Marshal.ThrowExceptionForHR(_storeInterface.GetValue(ref key, out result));
            return result;
        }

        public T Get<T>(PropertyKey key, T defaultValue)
        {
            if (Contains(key)) return (T) this[key].Value;
            return defaultValue;
        }
    }
}