using Autofac;

namespace MediaPlayer.Windows
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
        }
    }
}