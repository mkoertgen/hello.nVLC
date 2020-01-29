using Autofac;
using Caliburn.Micro;
using MediaPlayer.NAudio;

namespace Hello.nVLC.Modules
{
    public class NAudioModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<NAudioPlayer>().AsSelf().SingleInstance();
            builder.RegisterType<NAudioDisplayViewModel>().AsSelf();
            AssemblySource.Instance.Add(typeof (NAudioDisplayViewModel).Assembly);

            builder.RegisterType<MediaPlayerViewModel<NAudioPlayer, NAudioDisplayViewModel>>()
                .As<IMediaPlayerViewModel>();
        }
    }
}
