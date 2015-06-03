using System.Windows.Forms;

namespace MediaPlayer.Vlc
{
    public partial class VlcMediaPlayerView
    {
        public VlcMediaPlayerView()
        {
            InitializeComponent();

            var panel = new Panel();
            WindowsFormsHost.Child = panel;

            Loaded += (sender, args) =>
            {
                var viewModel = DataContext as VlcMediaPlayerViewModel;
                if (viewModel != null) viewModel.WindowHandle = panel.Handle;
            };
        }
    }
}
