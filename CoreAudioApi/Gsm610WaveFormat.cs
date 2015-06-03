using System.IO;
using System.Runtime.InteropServices;

namespace CoreAudioApi
{
    /// <summary>
    ///     GSM 610
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    public class Gsm610WaveFormat : WaveFormat
    {
        /// <summary>
        ///     Creates a GSM 610 WaveFormat
        ///     For now hardcoded to 13kbps
        /// </summary>
        public Gsm610WaveFormat()
        {
            waveFormatTag = WaveFormatEncoding.Gsm610;
            channels = 1;
            averageBytesPerSecond = 1625;
            bitsPerSample = 0; // must be zero
            blockAlign = 65;
            sampleRate = 8000;

            extraSize = 2;
            SamplesPerBlock = 320;
        }

        /// <summary>
        ///     Samples per block
        /// </summary>
        public short SamplesPerBlock { get; }

        /// <summary>
        ///     Writes this structure to a BinaryWriter
        /// </summary>
        public override void Serialize(BinaryWriter writer)
        {
            base.Serialize(writer);
            writer.Write(SamplesPerBlock);
        }
    }
}