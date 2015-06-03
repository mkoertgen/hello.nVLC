using System;
using System.Runtime.InteropServices;
using CoreAudioApi.Interfaces;

namespace CoreAudioApi
{
    // ReSharper disable UnusedMember.Global
    public class AudioCaptureClient : IDisposable
    {
        private IAudioCaptureClient _audioCaptureClientInterface;

        internal AudioCaptureClient(IAudioCaptureClient audioCaptureClientInterface)
        {
            _audioCaptureClientInterface = audioCaptureClientInterface;
        }

        public void Dispose()
        {
            if (_audioCaptureClientInterface == null) return;
            // althugh GC would do this for us, we want it done now
            // to let us reopen WASAPI
            Marshal.ReleaseComObject(_audioCaptureClientInterface);
            _audioCaptureClientInterface = null;
            GC.SuppressFinalize(this);
        }

        public IntPtr GetBuffer(
            out int numFramesToRead,
            out AudioClientBufferFlags bufferFlags,
            out long devicePosition,
            out long qpcPosition)
        {
            IntPtr bufferPointer;
            Marshal.ThrowExceptionForHR(_audioCaptureClientInterface.GetBuffer(out bufferPointer, out numFramesToRead,
                out bufferFlags, out devicePosition, out qpcPosition));
            return bufferPointer;
        }

        public IntPtr GetBuffer(
            out int numFramesToRead,
            out AudioClientBufferFlags bufferFlags)
        {
            IntPtr bufferPointer;
            long devicePosition;
            long qpcPosition;
            Marshal.ThrowExceptionForHR(_audioCaptureClientInterface.GetBuffer(out bufferPointer, out numFramesToRead,
                out bufferFlags, out devicePosition, out qpcPosition));
            return bufferPointer;
        }

        public int GetNextPacketSize()
        {
            int numFramesInNextPacket;
            Marshal.ThrowExceptionForHR(_audioCaptureClientInterface.GetNextPacketSize(out numFramesInNextPacket));
            return numFramesInNextPacket;
        }

        public void ReleaseBuffer(int numFramesWritten)
        {
            Marshal.ThrowExceptionForHR(_audioCaptureClientInterface.ReleaseBuffer(numFramesWritten));
        }
    }
}