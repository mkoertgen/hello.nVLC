using System;
using System.Runtime.InteropServices;

namespace CoreAudioApi
{
    static class MarshalEx
    {
        public delegate int FuncOut<T>(out T value);
        public delegate int FuncOut<T1,T2>(out T1 value1, out T2 value2);

        public static T Get<T>(FuncOut<T> getter, T defaultValue = default(T))
        {
            // ReSharper disable once RedundantAssignment
            var result = defaultValue;
            Marshal.ThrowExceptionForHR(getter(out result));
            return result;
        }

        public static Tuple<T1,T2> Get<T1,T2>(FuncOut<T1,T2> getter)
        {
            T1 result1;
            T2 result2;
            Marshal.ThrowExceptionForHR(getter(out result1, out result2));
            return new Tuple<T1, T2>(result1, result2);
        }
    }
}