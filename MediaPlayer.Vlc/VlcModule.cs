using Autofac;
using NAudio.CoreAudioApi;

namespace MediaPlayer.Vlc
{
    public class VlcModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<VlcPlayerViewModel>().As<IMediaPlayer>().SingleInstance();
            builder.RegisterType<VlcDisplayViewModel>().As<IMediaDisplayViewModel>().SingleInstance();

            builder.RegisterInstance(GetVolumeEndpoint()).SingleInstance();

            new VlcConfiguration().VerifyVlcPresent();
        }

        private static AudioEndpointVolume GetVolumeEndpoint()
        {
            return new MMDeviceEnumerator()
                .GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia)
                .AudioEndpointVolume;
        }
    }
}