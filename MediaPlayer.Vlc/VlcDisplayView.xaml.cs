using System.Drawing;
using System.Windows.Forms;

namespace MediaPlayer.Vlc
{
    // ReSharper disable once UnusedMember.Global
    public partial class VlcDisplayView
    {
        public VlcDisplayView()
        {
            InitializeComponent();

            var panel = new Panel {BackColor = Color.Black};
            WindowsFormsHost.Child = panel;

            Loaded += (sender, args) =>
            {
                if (DataContext is VlcDisplayViewModel viewModel) viewModel.WindowHandle = panel.Handle;
            };
        }
    }
}
