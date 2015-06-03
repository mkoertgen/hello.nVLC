using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace CoreAudioApi
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 2)]
    public class WaveFormat
    {
        protected WaveFormatEncoding waveFormatTag;
        protected short channels;
        protected int sampleRate;
        protected int averageBytesPerSecond;
        protected short blockAlign;
        protected short bitsPerSample;
        protected short extraSize;

        public WaveFormat() : this(44100, 16, 2)
        {
        }

        public WaveFormat(int sampleRate, int channels)
            : this(sampleRate, 16, channels)
        {
        }

        public int ConvertLatencyToByteSize(int milliseconds)
        {
            var bytes = (int) ((AverageBytesPerSecond/1000.0)*milliseconds);
            if ((bytes%BlockAlign) != 0)
            {
                // Return the upper BlockAligned
                bytes = bytes + BlockAlign - (bytes%BlockAlign);
            }
            return bytes;
        }

        public static WaveFormat CreateCustomFormat(WaveFormatEncoding tag, int sampleRate, int channels,
            int averageBytesPerSecond, int blockAlign, int bitsPerSample)
        {
            return new WaveFormat
            {
                waveFormatTag = tag,
                channels = (short) channels,
                sampleRate = sampleRate,
                averageBytesPerSecond = averageBytesPerSecond,
                blockAlign = (short) blockAlign,
                bitsPerSample = (short) bitsPerSample,
                extraSize = 0
            };
        }

        public static WaveFormat CreateALawFormat(int sampleRate, int channels)
        {
            return CreateCustomFormat(WaveFormatEncoding.ALaw, sampleRate, channels, sampleRate*channels, channels, 8);
        }

        public static WaveFormat CreateMuLawFormat(int sampleRate, int channels)
        {
            return CreateCustomFormat(WaveFormatEncoding.MuLaw, sampleRate, channels, sampleRate*channels, channels, 8);
        }

        public WaveFormat(int rate, int bits, int channels)
        {
            if (channels < 1)
                throw new ArgumentOutOfRangeException(nameof(channels), "Channels must be 1 or greater");
            // minimum 16 bytes, sometimes 18 for PCM
            waveFormatTag = WaveFormatEncoding.Pcm;
            this.channels = (short) channels;
            sampleRate = rate;
            bitsPerSample = (short) bits;
            extraSize = 0;

            blockAlign = (short) (channels*(bits/8));
            averageBytesPerSecond = sampleRate*blockAlign;
        }

        public static WaveFormat CreateIeeeFloatWaveFormat(int sampleRate, int channels)
        {
            var wf = new WaveFormat
            {
                waveFormatTag = WaveFormatEncoding.IeeeFloat,
                channels = (short) channels,
                bitsPerSample = 32,
                sampleRate = sampleRate,
                blockAlign = (short) (4*channels),
                extraSize = 0
            };
            wf.averageBytesPerSecond = sampleRate*wf.blockAlign;
            return wf;
        }

        public static WaveFormat MarshalFromPtr(IntPtr pointer)
        {
            var waveFormat = (WaveFormat) Marshal.PtrToStructure(pointer, typeof (WaveFormat));
            switch (waveFormat.Encoding)
            {
                case WaveFormatEncoding.Pcm:
                    // can't rely on extra size even being there for PCM so blank it to avoid reading
                    // corrupt data
                    waveFormat.extraSize = 0;
                    break;
                case WaveFormatEncoding.Extensible:
                    waveFormat = (WaveFormatExtensible) Marshal.PtrToStructure(pointer, typeof (WaveFormatExtensible));
                    break;
                case WaveFormatEncoding.Adpcm:
                    waveFormat = (AdpcmWaveFormat) Marshal.PtrToStructure(pointer, typeof (AdpcmWaveFormat));
                    break;
                case WaveFormatEncoding.Gsm610:
                    waveFormat = (Gsm610WaveFormat) Marshal.PtrToStructure(pointer, typeof (Gsm610WaveFormat));
                    break;
                default:
                    if (waveFormat.ExtraSize > 0)
                    {
                        waveFormat = (WaveFormatExtraData) Marshal.PtrToStructure(pointer, typeof (WaveFormatExtraData));
                    }
                    break;
            }
            return waveFormat;
        }

        /// <summary>
        ///     Helper function to marshal WaveFormat to an IntPtr
        /// </summary>
        /// <param name="format">WaveFormat</param>
        /// <returns>IntPtr to WaveFormat structure (needs to be freed by callee)</returns>
        public static IntPtr MarshalToPtr(WaveFormat format)
        {
            var formatSize = Marshal.SizeOf(format);
            var formatPointer = Marshal.AllocHGlobal(formatSize);
            Marshal.StructureToPtr(format, formatPointer, false);
            return formatPointer;
        }

        /// <summary>
        ///     Reads in a WaveFormat (with extra data) from a fmt chunk (chunk identifier and
        ///     length should already have been read)
        /// </summary>
        /// <param name="br">Binary reader</param>
        /// <param name="formatChunkLength">Format chunk length</param>
        /// <returns>A WaveFormatExtraData</returns>
        public static WaveFormat FromFormatChunk(BinaryReader br, int formatChunkLength)
        {
            var waveFormat = new WaveFormatExtraData();
            waveFormat.ReadWaveFormat(br, formatChunkLength);
            waveFormat.ReadExtraData(br);
            return waveFormat;
        }

        private void ReadWaveFormat(BinaryReader br, int formatChunkLength)
        {
            if (formatChunkLength < 16)
                throw new InvalidDataException("Invalid WaveFormat Structure");
            waveFormatTag = (WaveFormatEncoding) br.ReadUInt16();
            channels = br.ReadInt16();
            sampleRate = br.ReadInt32();
            averageBytesPerSecond = br.ReadInt32();
            blockAlign = br.ReadInt16();
            bitsPerSample = br.ReadInt16();
            if (formatChunkLength > 16)
            {
                extraSize = br.ReadInt16();
                if (extraSize != formatChunkLength - 18)
                {
                    Debug.WriteLine("Format chunk mismatch");
                    extraSize = (short) (formatChunkLength - 18);
                }
            }
        }

        public WaveFormat(BinaryReader br)
        {
            var formatChunkLength = br.ReadInt32();
            ReadWaveFormat(br, formatChunkLength);
        }

        /// <summary>
        ///     Reports this WaveFormat as a string
        /// </summary>
        /// <returns>String describing the wave format</returns>
        public override string ToString()
        {
            switch (waveFormatTag)
            {
                case WaveFormatEncoding.Pcm:
                case WaveFormatEncoding.Extensible:
                    // extensible just has some extra bits after the PCM header
                    return $"{bitsPerSample} bit PCM: {sampleRate/1000}kHz {channels} channels";
                default:
                    return waveFormatTag.ToString();
            }
        }

        /// <summary>
        ///     Compares with another WaveFormat object
        /// </summary>
        /// <param name="obj">Object to compare to</param>
        /// <returns>True if the objects are the same</returns>
        public override bool Equals(object obj)
        {
            var other = obj as WaveFormat;
            if (other == null) return false;
            return waveFormatTag == other.waveFormatTag &&
                   channels == other.channels &&
                   sampleRate == other.sampleRate &&
                   averageBytesPerSecond == other.averageBytesPerSecond &&
                   blockAlign == other.blockAlign &&
                   bitsPerSample == other.bitsPerSample;
        }

        /// <summary>
        ///     Provides a Hashcode for this WaveFormat
        /// </summary>
        /// <returns>A hashcode</returns>
        public override int GetHashCode()
        {
            return (int) waveFormatTag ^
                   channels ^
                   sampleRate ^
                   averageBytesPerSecond ^
                   blockAlign ^
                   bitsPerSample;
        }

        public WaveFormatEncoding Encoding => waveFormatTag;

        public virtual void Serialize(BinaryWriter writer)
        {
            writer.Write(18 + extraSize); // wave format length
            writer.Write((short) Encoding);
            writer.Write((short) Channels);
            writer.Write(SampleRate);
            writer.Write(AverageBytesPerSecond);
            writer.Write((short) BlockAlign);
            writer.Write((short) BitsPerSample);
            writer.Write(extraSize);
        }

        public int Channels => channels;
        public int SampleRate => sampleRate;
        public int AverageBytesPerSecond => averageBytesPerSecond;
        public virtual int BlockAlign => blockAlign;
        public int BitsPerSample => bitsPerSample;
        public int ExtraSize => extraSize;
    }
}