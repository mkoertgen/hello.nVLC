using System;

namespace MediaPlayer.NAudio
{
    public class NAudioDisplayViewModel : IMediaDisplayViewModel, IDisposable
    {
        private readonly NAudioPlayer _player;
        private bool _isDisposed;

        public NAudioDisplayViewModel(NAudioPlayer player)
        {
            if (player == null) throw new ArgumentNullException(nameof(player));
            _player = player;
            _player.MaxVolume += OnMaxVolume;
        }

        public void Dispose()
        {
            if (_isDisposed) return;
            _player.MaxVolume -= OnMaxVolume;
            _isDisposed = true;
        }

        private void OnMaxVolume(object sender, VolumeEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}