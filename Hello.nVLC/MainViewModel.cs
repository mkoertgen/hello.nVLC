using System;
using System.Diagnostics;
using System.Windows;
using Caliburn.Micro;
using MediaPlayer;
using Microsoft.VisualBasic;
using Microsoft.Win32;

namespace Hello.nVLC
{
    public class MainViewModel : Screen
    {
        public MainViewModel(IMediaPlayerViewModel player)
        {
            Player = player ?? throw new ArgumentNullException(nameof(player));

            // ReSharper disable once DoNotCallOverridableMethodsInConstructor
            DisplayName = "hello.nVLC";
        }

        public IMediaPlayerViewModel Player { get; }
        // ReSharper disable once UnusedMember.Global
        public void OpenFile()
        {
            var dlg = new OpenFileDialog();
            var res = dlg.ShowDialog();

            if (res.HasValue && res.Value)
                OpenUrl(dlg.FileName);
        }

        // ReSharper disable once UnusedMember.Global
        public void OpenUrl()
        {
            var url = Interaction.InputBox("Url", "Open Url",
                "https://archive.org/download/Wildlife_20160527/Wildlife.mp4");
            if (!string.IsNullOrEmpty(url))
                OpenUrl(url);
        }

        private void OpenUrl(string url)
        {
            try
            {
                Player.Player.Source = new Uri(url);
            }
            catch (Exception ex)
            {
                Trace.TraceWarning("Could not open url: {0}", ex);
                MessageBox.Show("Could not open url: " + ex.Message);
            }
        }
    }
}