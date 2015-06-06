using Autofac;
using Caliburn.Micro;
using MediaPlayer;
using MediaPlayer.Windows;

namespace Hello.nVLC
{
    public class WindowsModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<WindowsMediaPlayer>()
                .AsSelf()
                .As<IMediaPlayer>()
                .SingleInstance();

            builder.RegisterType<WindowsDisplayViewModel>().As<IMediaDisplayViewModel>();
            AssemblySource.Instance.Add(typeof (WindowsDisplayViewModel).Assembly);
        }
    }
}