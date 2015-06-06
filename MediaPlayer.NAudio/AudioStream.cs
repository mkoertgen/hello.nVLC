using System;
using System.IO;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace MediaPlayer.NAudio
{
    /// <summary>
    /// Simply a copy & paste from NAudios <see cref="AudioFileReader"/> so we can override the 
    /// logic for creating reader streams and enable uri support.
    /// Since most members in <see cref="AudioFileReader"/> are private we cannot subclass but
    /// must re-implement via copy & paste.
    /// </summary>
    class AudioStream : WaveStream, ISampleProvider
    {
        private WaveStream _readerStream; // the waveStream which we will use for all positioning
        private readonly SampleChannel _sampleChannel; // sample provider that gives us most stuff we need
        private readonly int _destBytesPerSample;
        private readonly int _sourceBytesPerSample;
        private readonly object _lockObject;

        /// <summary>
        /// Initializes a new instance of AudioStream
        /// </summary>
        /// <param name="source">The media source to open</param>
        public AudioStream(Uri source)
        {
            _lockObject = new object();
            // ReSharper disable once DoNotCallOverridableMethodsInConstructor
            CreateReaderStream(source);
            _sourceBytesPerSample = (_readerStream.WaveFormat.BitsPerSample / 8) * _readerStream.WaveFormat.Channels;
            _sampleChannel = new SampleChannel(_readerStream, false);
            _destBytesPerSample = 4*_sampleChannel.WaveFormat.Channels;
            Length = SourceToDest(_readerStream.Length);
        }

        /// <summary>
        /// Creates the reader stream, supporting all filetypes in the core NAudio library,
        /// and ensuring we are in PCM format.
        /// </summary>
        /// <param name="source">Source uri</param>
        protected virtual void CreateReaderStream(Uri source)
        {
            var fileName = source.GetFileName(true);
            if (!string.IsNullOrEmpty(fileName))
                _readerStream = CreateReaderStream(fileName);
            else
                // for remote uris fall back to media foundation reader, see if that can play it
                _readerStream = new MediaFoundationReader(source.ToString());
        }

        private static WaveStream CreateReaderStream(string fileName)
        {
            var ext = Path.GetExtension(fileName).ToLowerInvariant();
            switch(ext)
            {
                case ".wav": return CreatePcmStream(fileName);
                case ".mp3": return new Mp3FileReader(fileName);
                case ".aiff": return new AiffFileReader(fileName);
                default:
                    // fall back to media foundation reader, see if that can play it
                    return  new MediaFoundationReader(fileName);
            }
        }

        private static WaveStream CreatePcmStream(string fileName)
        {
            WaveStream stream = new WaveFileReader(fileName);
            if (stream.WaveFormat.Encoding != WaveFormatEncoding.Pcm &&
                stream.WaveFormat.Encoding != WaveFormatEncoding.IeeeFloat)
            {
                stream = WaveFormatConversionStream.CreatePcmStream(stream);
                stream = new BlockAlignReductionStream(stream);
                return stream;
            }
            return stream;
        }

        /// <summary>
        /// WaveFormat of this stream
        /// </summary>
        public override WaveFormat WaveFormat => _sampleChannel.WaveFormat;

        /// <summary>
        /// Length of this stream (in bytes)
        /// </summary>
        public override long Length { get; }

        /// <summary>
        /// Position of this stream (in bytes)
        /// </summary>
        public override long Position
        {
            get { return SourceToDest(_readerStream.Position); }
            set { lock (_lockObject) { _readerStream.Position = DestToSource(value); }  }
        }

        /// <summary>
        /// Reads from this wave stream
        /// </summary>
        /// <param name="buffer">Audio buffer</param>
        /// <param name="offset">Offset into buffer</param>
        /// <param name="count">Number of bytes required</param>
        /// <returns>Number of bytes read</returns>
        public override int Read(byte[] buffer, int offset, int count)
        {
            var waveBuffer = new WaveBuffer(buffer);
            var samplesRequired = count / 4;
            var samplesRead = Read(waveBuffer.FloatBuffer, offset / 4, samplesRequired);
            return samplesRead * 4;
        }

        /// <summary>
        /// Reads audio from this sample provider
        /// </summary>
        /// <param name="buffer">Sample buffer</param>
        /// <param name="offset">Offset into sample buffer</param>
        /// <param name="count">Number of samples required</param>
        /// <returns>Number of samples read</returns>
        public int Read(float[] buffer, int offset, int count)
        {
            lock (_lockObject)
            {
                return _sampleChannel.Read(buffer, offset, count);
            }
        }

        /// <summary>
        /// Gets or Sets the Volume of this AudioFileReader. 1.0f is full volume
        /// </summary>
        public float Volume
        {
            get { return _sampleChannel.Volume; }
            set { _sampleChannel.Volume = value; } 
        }

        private long SourceToDest(long sourceBytes)
        {
            return _destBytesPerSample * (sourceBytes / _sourceBytesPerSample);
        }

        private long DestToSource(long destBytes)
        {
            return _sourceBytesPerSample * (destBytes / _destBytesPerSample);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _readerStream.Dispose();
                _readerStream = null;
            }
            base.Dispose(disposing);
        }
    }
}