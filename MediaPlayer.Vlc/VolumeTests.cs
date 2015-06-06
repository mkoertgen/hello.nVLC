using System;
using System.Diagnostics;
using System.Threading;
using FluentAssertions;
using NAudio.CoreAudioApi;
using NAudio.CoreAudioApi.Interfaces;
using NUnit.Framework;

namespace MediaPlayer.Vlc
{
    [TestFixture]
    class VolumeTests
    {
        private readonly MMDevice _device = BalanceExtensions.GetDefaultDevice();

        [Test, Explicit]
        public void Can_Mute()
        {
            var volume = _device.AudioEndpointVolume;

            volume.MasterVolumeLevelScalar.Should().BeInRange(0, 1);

            volume.VolumeRange.MinDecibels.Should().BeInRange(-140, -40);
            volume.VolumeRange.MaxDecibels.Should().Be(0);
            volume.MasterVolumeLevel.Should()
                .BeInRange(volume.VolumeRange.MinDecibels, volume.VolumeRange.MaxDecibels);

            volume.Mute = true;
            Thread.Sleep(500);
            volume.Mute = false;
        }

        [Test, Explicit]
        public void EndpointBalanceTest()
        {
            var volume = _device.AudioEndpointVolume;

            var duration = TimeSpan.FromSeconds(5);
            const int n = 50;

            var dt = (int) (duration.TotalMilliseconds/n);
            for (var i = 1; i <= n; i++)
            {
                var balance = (float) Math.Sin(2*i*Math.PI/n);
                volume.SetBalance(balance);

                Thread.Sleep(dt);

                var eps = 1e-6f/Math.Max(1e-6f, volume.MasterVolumeLevelScalar);
                volume.GetBalance().Should().BeApproximately(balance, eps);
            }
        }

        [Test, Explicit]
        public void SessionTest()
        {
            var sessionManager = _device.AudioSessionManager;
            sessionManager.RefreshSessions();
            //sessionManager.AudioSessionControl.State.Should().Be(AudioSessionState.AudioSessionStateInactive);

            for (var i = 0; i < sessionManager.Sessions.Count; i++)
            {
                var session = sessionManager.Sessions[i];

                if (session.IsSystemSoundsSession) continue;
                if (session.State != AudioSessionState.AudioSessionStateActive) continue;

                session.DisplayName.Should().BeNullOrWhiteSpace();
                var process = Process.GetProcessById((int) session.GetProcessID);
                var title = process.MainWindowTitle;
                Trace.TraceInformation($"session: {title} ({session.GetProcessID})");

                //session.s
                //session.SimpleAudioVolume.Mute = true;
                //Thread.Sleep(1000);
                //session.SimpleAudioVolume.Mute = false;
            }
        }
    }
}