using Autofac;
using Caliburn.Micro;
using MediaPlayer.Vlc;

namespace Hello.nVLC.Modules
{
    public class VlcModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            if (!new VlcConfiguration().IsVlcPresent.Value)
                return;

            builder.RegisterType<VlcPlayer>().AsSelf().SingleInstance();
            builder.RegisterType<VlcDisplayViewModel>().AsSelf();

            AssemblySource.Instance.Add(typeof (VlcDisplayViewModel).Assembly);
            builder.RegisterType<MediaPlayerViewModel<VlcPlayer, VlcDisplayViewModel>>()
                .As<IMediaPlayerViewModel>();
        }
    }
}
