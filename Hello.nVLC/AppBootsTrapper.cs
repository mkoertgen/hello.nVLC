using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using Autofac;
using Autofac.Core;
using Caliburn.Micro;
using CoreAudioApi;
using CoreAudioApi.Interfaces;
using MediaPlayer;
using MediaPlayer.Vlc;
using MediaPlayer.Windows;

namespace Hello.nVLC
{
    public class AppBootstrapper : BootstrapperBase
    {
        private IContainer _container;

        public AppBootstrapper()
        {
            Initialize();
        }

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            DisplayRootViewFor<MainViewModel>();
        }

        protected override void Configure()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<WindowManager>().As<IWindowManager>().SingleInstance();
            builder.RegisterType<MainViewModel>().AsSelf().InstancePerLifetimeScope();

            builder.RegisterType<WindowsMediaPlayerViewModel>().As<IPlayerViewModel>().SingleInstance();
            AssemblySource.Instance.Add(typeof(WindowsMediaPlayerViewModel).Assembly);

            // uncomment to use Vlc
            builder.RegisterType<VlcMediaPlayerViewModel>().As<IPlayerViewModel>().SingleInstance();
            new VlcConfiguration().VerifyVlcPresent();
            AssemblySource.Instance.Add(typeof(VlcMediaPlayerViewModel).Assembly);
            // register AudioEndPointVolume
            builder.RegisterInstance(GetDefaultAudioEndpoint().AudioEndpointVolume).SingleInstance();

            _container = builder.Build();
        }

        private static MMDevice GetDefaultAudioEndpoint()
        {
            var deviceEnumerator = new MMDeviceEnumerator();
            //foreach (var d in deviceEnumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active))
            //    Trace.TraceInformation($"device = {d}");
            return deviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
        }

        protected override object GetInstance(Type service, string key)
        {
            if (service == null) throw new ArgumentNullException(nameof(service));

            if (string.IsNullOrWhiteSpace(key))
            {
                object result;
                if (_container.TryResolve(service, out result))
                    return result;
            }
            else
            {
                object result;
                if (_container.TryResolveNamed(key, service, out result))
                    return result;
            }
            throw new DependencyResolutionException(string.Format(CultureInfo.CurrentCulture, "Could not locate any instances of contract {0}.", key ?? service.Name));
        }

        protected override IEnumerable<object> GetAllInstances(Type service)
        {
            return _container.Resolve(typeof(IEnumerable<>).MakeGenericType(new[] { service })) as IEnumerable<object>;
        }

        protected override void BuildUp(object instance)
        {
            _container.InjectProperties(instance);
        }
    }
}