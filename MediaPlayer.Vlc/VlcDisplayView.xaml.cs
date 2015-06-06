using System.Drawing;
using System.Windows.Forms;

namespace MediaPlayer.Vlc
{
    public partial class VlcDisplayView
    {
        public VlcDisplayView()
        {
            InitializeComponent();

            var panel = new Panel {BackColor = Color.Black};
            WindowsFormsHost.Child = panel;

            Loaded += (sender, args) =>
            {
                var viewModel = DataContext as VlcDisplayViewModel;
                if (viewModel != null) viewModel.WindowHandle = panel.Handle;
            };
        }
    }
}