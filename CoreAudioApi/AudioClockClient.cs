using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using CoreAudioApi.Interfaces;

namespace CoreAudioApi
{
    // ReSharper disable MemberCanBePrivate.Global
    // ReSharper disable UnusedMember.Global
    public class AudioClockClient : IDisposable
    {
        private IAudioClock _audioClockClientInterface;

        internal AudioClockClient(IAudioClock audioClockClientInterface)
        {
            _audioClockClientInterface = audioClockClientInterface;
        }

        public int Characteristics => (int)MarshalEx.Get<uint>(_audioClockClientInterface.GetCharacteristics);
        public ulong Frequency => MarshalEx.Get<ulong>(_audioClockClientInterface.GetFrequency);

        public ulong AdjustedPosition
        {
            get
            {
                // figure out ticks per byte (for later)
                var byteLatency = (TimeSpan.TicksPerSecond/Frequency);

                ulong pos, qpos;
                var cnt = 0;
                while (!GetPosition(out pos, out qpos))
                {
                    if (++cnt == 5)
                    {
                        // we've tried too many times, so now we have to just run with what we have...
                        break;
                    }
                }

                if (!Stopwatch.IsHighResolution) return pos;

                // cool, we can adjust our position appropriately
                // get the current qpc count (in ticks)
                var qposNow = (ulong) ((Stopwatch.GetTimestamp()*10000000M)/Stopwatch.Frequency);

                // find out how many ticks has passed since the device reported the position
                var qposDiff = (qposNow - qpos)/100;

                // find out how many byte would have played in that time span
                var bytes = qposDiff/byteLatency;

                // add it to the position
                pos += bytes;
                return pos;
            }
        }

        public bool CanAdjustPosition => Stopwatch.IsHighResolution;

        public void Dispose()
        {
            if (_audioClockClientInterface == null) return;
            // althugh GC would do this for us, we want it done now
            // to let us reopen WASAPI
            Marshal.ReleaseComObject(_audioClockClientInterface);
            _audioClockClientInterface = null;
            GC.SuppressFinalize(this);
        }

        public bool GetPosition(out ulong position, out ulong qpcPosition)
        {
            var hr = _audioClockClientInterface.GetPosition(out position, out qpcPosition);
            if (hr == -1) return false;
            Marshal.ThrowExceptionForHR(hr);
            return true;
        }
    }
}