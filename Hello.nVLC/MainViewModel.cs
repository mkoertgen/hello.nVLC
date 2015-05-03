using System;
using System.Diagnostics;
using System.Windows;
using Caliburn.Micro;
using Hello.nVLC.Media;
using Microsoft.VisualBasic;
using Microsoft.Win32;

namespace Hello.nVLC
{
    public class MainViewModel : Screen
    {
        public MainViewModel(IPlayerViewModel player)
        {
            if (player == null) throw new ArgumentNullException("player");
            Player = player;

            // ReSharper disable once DoNotCallOverridableMethodsInConstructor
            DisplayName = "hello.nVLC";
        }

        public IPlayerViewModel Player { get; private set; }

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
            var url = Interaction.InputBox("Url", "Open Url", String.Empty);
            if (!String.IsNullOrEmpty(url))
                OpenUrl(url);
        }

        private void OpenUrl(string url)
        {
            try
            {
                Player.Source = new Uri(url);
            }
            catch (Exception ex)
            {
                Trace.TraceWarning("Could not open url: {0}", ex);
                MessageBox.Show("Could not open url: " + ex.Message);
            }
        }
    }
}