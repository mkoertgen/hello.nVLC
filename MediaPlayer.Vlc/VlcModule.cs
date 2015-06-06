using Autofac;
using Caliburn.Micro;
using NAudio.CoreAudioApi;

namespace MediaPlayer.Vlc
{
    //[Export(typeof(IModule))]
    public class VlcModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            // uncomment to use Vlc
            builder.RegisterType<VlcPlayerViewModel>().As<IMediaPlayer>().SingleInstance();
            new VlcConfiguration().VerifyVlcPresent();
            AssemblySource.Instance.Add(typeof(VlcPlayerViewModel).Assembly);
            // register AudioEndPointVolume
            builder.RegisterInstance(GetDefaultAudioEndpoint().AudioEndpointVolume).SingleInstance();
        }

        private static MMDevice GetDefaultAudioEndpoint()
        {
            var deviceEnumerator = new MMDeviceEnumerator();
            //foreach (var d in deviceEnumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active))
            //    Trace.TraceInformation($"device = {d}");
            return deviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
        }
    }
}