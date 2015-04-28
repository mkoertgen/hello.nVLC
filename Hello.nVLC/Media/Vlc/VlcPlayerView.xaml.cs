using System;

namespace Hello.nVLC.Media.Vlc
{
    public partial class VlcPlayerView
    {
        public VlcPlayerView()
        {
            InitializeComponent();

            var panel = new System.Windows.Forms.Panel();
            WindowsFormsHost.Child = panel;
            WindowHandle = panel.Handle;
        }

        public IntPtr WindowHandle { get; private set; }
    }
}
