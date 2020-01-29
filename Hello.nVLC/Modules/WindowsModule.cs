using Autofac;
using Caliburn.Micro;
using MediaPlayer.Windows;

namespace Hello.nVLC.Modules
{
    public class WindowsModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<WindowsMediaPlayer>().AsSelf().SingleInstance();
            builder.RegisterType<WindowsDisplayViewModel>().AsSelf();
            AssemblySource.Instance.Add(typeof (WindowsDisplayViewModel).Assembly);

            builder.RegisterType<MediaPlayerViewModel<WindowsMediaPlayer, WindowsDisplayViewModel>>()
                .As<IMediaPlayerViewModel>();
        }
    }
}
