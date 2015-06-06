using Autofac;
using Caliburn.Micro;
using MediaPlayer;
using MediaPlayer.NAudio;

namespace Hello.nVLC
{
    public class NAudioModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<NAudioPlayer>()
                .AsSelf()
                .As<IMediaPlayer>()
                .SingleInstance();
            builder.RegisterType<NAudioDisplayViewModel>().As<IMediaDisplayViewModel>().SingleInstance();
            AssemblySource.Instance.Add(typeof (NAudioDisplayViewModel).Assembly);
        }
    }
}