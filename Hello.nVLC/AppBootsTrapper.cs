using System.Windows;
using Autofac;
using Caliburn.Micro;
using Caliburn.Micro.Autofac;
using MediaPlayer;
using MediaPlayer.Windows;

namespace Hello.nVLC
{
    public class AppBootstrapper : AutofacBootstrapper<MainViewModel>
    {
        public AppBootstrapper()
        {
            Initialize();
        }

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            DisplayRootViewFor<MainViewModel>();
        }

        protected override void ConfigureBootstrapper()
        {
            base.ConfigureBootstrapper();
            EnforceNamespaceConvention = false;
        }

        protected override void ConfigureContainer(ContainerBuilder builder)
        {
            builder.RegisterType<MainViewModel>().AsSelf().InstancePerLifetimeScope();

            builder.RegisterType<MediaPlayerViewModel>().As<IMediaPlayerViewModel>().SingleInstance();

            builder.RegisterModule<WindowsModule>();
            AssemblySource.Instance.Add(typeof(WindowsModule).Assembly);

            //builder.RegisterModule<VlcModule>();
            //AssemblySource.Instance.Add(typeof(VlcModule).Assembly);

            //builder.RegisterModule<NAudioModule>();
            //AssemblySource.Instance.Add(typeof(NAudioModule).Assembly);
        }
    }
}