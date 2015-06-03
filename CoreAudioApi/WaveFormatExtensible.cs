using System;
using System.IO;
using System.Runtime.InteropServices;

namespace CoreAudioApi
{
    /// <summary>
    ///     WaveFormatExtensible
    ///     http://www.microsoft.com/whdc/device/audio/multichaud.mspx
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 2)]
    public class WaveFormatExtensible : WaveFormat
    {
        private readonly short wValidBitsPerSample; // bits of precision, or is wSamplesPerBlock if wBitsPerSample==0
        private readonly int dwChannelMask; // which channels are present in stream
        private Guid subFormat;

        /// <summary>
        ///     Parameterless constructor for marshalling
        /// </summary>
        private WaveFormatExtensible()
        {
        }

        /// <summary>
        ///     Creates a new WaveFormatExtensible for PCM or IEEE
        /// </summary>
        public WaveFormatExtensible(int rate, int bits, int channels)
            : base(rate, bits, channels)
        {
            waveFormatTag = WaveFormatEncoding.Extensible;
            extraSize = 22;
            wValidBitsPerSample = (short) bits;
            for (var n = 0; n < channels; n++)
                dwChannelMask |= (1 << n);

            subFormat = bits == 32 ? AudioMediaSubtypes.MEDIASUBTYPE_IEEE_FLOAT : AudioMediaSubtypes.MEDIASUBTYPE_PCM;
        }

        /// <summary>
        ///     WaveFormatExtensible for PCM or floating point can be awkward to work with
        ///     This creates a regular WaveFormat structure representing the same audio format
        /// </summary>
        /// <returns></returns>
        public WaveFormat ToStandardWaveFormat()
        {
            if (subFormat == AudioMediaSubtypes.MEDIASUBTYPE_IEEE_FLOAT && bitsPerSample == 32)
                return CreateIeeeFloatWaveFormat(sampleRate, channels);
            if (subFormat == AudioMediaSubtypes.MEDIASUBTYPE_PCM)
                return new WaveFormat(sampleRate, bitsPerSample, channels);
            throw new InvalidOperationException("Not a recognised PCM or IEEE float format");
        }

        /// <summary>
        ///     SubFormat (may be one of AudioMediaSubtypes)
        /// </summary>
        public Guid SubFormat => subFormat;

        /// <summary>
        ///     Serialize
        /// </summary>
        /// <param name="writer"></param>
        public override void Serialize(BinaryWriter writer)
        {
            base.Serialize(writer);
            writer.Write(wValidBitsPerSample);
            writer.Write(dwChannelMask);
            var guid = subFormat.ToByteArray();
            writer.Write(guid, 0, guid.Length);
        }

        /// <summary>
        ///     String representation
        /// </summary>
        public override string ToString()
        {
            return
                $"{base.ToString()} wBitsPerSample:{wValidBitsPerSample} dwChannelMask:{dwChannelMask} subFormat:{subFormat} extraSize:{extraSize}";
        }
    }
}