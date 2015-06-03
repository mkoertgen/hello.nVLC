using System;
using System.Runtime.InteropServices;
using CoreAudioApi.Interfaces;

namespace CoreAudioApi
{
    /// <summary>
    ///     Audio Render Client
    /// </summary>
    public class AudioRenderClient : IDisposable
    {
        private IAudioRenderClient _audioRenderClientInterface;

        internal AudioRenderClient(IAudioRenderClient audioRenderClientInterface)
        {
            _audioRenderClientInterface = audioRenderClientInterface;
        }

        /// <summary>
        ///     Release the COM object
        /// </summary>
        public void Dispose()
        {
            if (_audioRenderClientInterface != null)
            {
                // althugh GC would do this for us, we want it done now
                // to let us reopen WASAPI
                Marshal.ReleaseComObject(_audioRenderClientInterface);
                _audioRenderClientInterface = null;
                GC.SuppressFinalize(this);
            }
        }

        /// <summary>
        ///     Gets a pointer to the buffer
        /// </summary>
        /// <param name="numFramesRequested">Number of frames requested</param>
        /// <returns>Pointer to the buffer</returns>
        public IntPtr GetBuffer(int numFramesRequested)
        {
            IntPtr bufferPointer;
            Marshal.ThrowExceptionForHR(_audioRenderClientInterface.GetBuffer(numFramesRequested, out bufferPointer));
            return bufferPointer;
        }

        /// <summary>
        ///     Release buffer
        /// </summary>
        /// <param name="numFramesWritten">Number of frames written</param>
        /// <param name="bufferFlags">Buffer flags</param>
        public void ReleaseBuffer(int numFramesWritten, AudioClientBufferFlags bufferFlags)
        {
            Marshal.ThrowExceptionForHR(_audioRenderClientInterface.ReleaseBuffer(numFramesWritten, bufferFlags));
        }
    }
}