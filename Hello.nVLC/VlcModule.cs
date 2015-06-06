using Autofac;
using Caliburn.Micro;
using MediaPlayer;
using MediaPlayer.Vlc;

namespace Hello.nVLC
{
    public class VlcModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<VlcPlayer>()
                .AsSelf()
                .As<IMediaPlayer>()
                .SingleInstance();
            builder.RegisterType<VlcDisplayViewModel>().As<IMediaDisplayViewModel>().SingleInstance();
            builder.RegisterInstance(VlcPlayer.GetVolumeEndpoint()).SingleInstance();
            AssemblySource.Instance.Add(typeof (VlcDisplayViewModel).Assembly);

            new VlcConfiguration().VerifyVlcPresent();
        }
    }
}