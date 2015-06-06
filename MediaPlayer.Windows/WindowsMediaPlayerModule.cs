using Autofac;
using Caliburn.Micro;

namespace MediaPlayer.Windows
{
    public class WindowsMediaPlayerModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            // uncomment to use Vlc
            builder.RegisterType<WindowsMediaPlayer>().As<IMediaPlayer>().SingleInstance();
            AssemblySource.Instance.Add(typeof(WindowsMediaPlayer).Assembly);
        }
    }
}