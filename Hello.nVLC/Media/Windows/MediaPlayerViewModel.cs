using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace Hello.nVLC.Media.Windows
{
    // ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
    public class MediaPlayerViewModel : IPlayerViewModel
    {
        private const double MaxSpeedRatioInternal = 4;
        private const double MinSpeedRatioInternal = 0.25;

        public double MaxSpeedRatio { get { return MaxSpeedRatioInternal; } }
        public double MinSpeedRatio { get { return MinSpeedRatioInternal; } }

        public DrawingBrush VideoBrush { get; private set; }
        
        private readonly MediaPlayer _player = new MediaPlayer();
        private readonly DispatcherTimer _positionTimer = new DispatcherTimer(DispatcherPriority.ApplicationIdle);
        private bool _isPlaying;
        private bool _isPaused;
        private bool _isStopped;
        private double _restoreVolume = 0.5;
        private double _speedRatio = 1.0;

        public MediaPlayerViewModel()
        {
            _player.MediaOpened += PlayerMediaOpened;

            _player.MediaEnded += (s, e) => Stop();
            _player.MediaFailed += (s, e) => HandleMediaFailedException(e.ErrorException);

            _positionTimer.Interval = TimeSpan.FromMilliseconds(250);
            _positionTimer.Tick += PositionTimerTick;

            Error = PlayerError.NoError;

            var videoDrawing = new VideoDrawing {Player = _player, Rect = new Rect(0, 0, 1, 1)};
            VideoBrush = new DrawingBrush(videoDrawing);

        }

        void PlayerMediaOpened(object sender, EventArgs e)
        {
            Error.Exception = null;

            // ReSharper disable ExplicitCallerInfoArgument
            OnPropertyChanged("Source");
            OnPropertyChanged("Duration");
            OnPropertyChanged("HasDuration");
            OnPropertyChanged("CanPlay");
            OnPropertyChanged("CanPause");
            OnPropertyChanged("CanStop");
            OnPropertyChanged("CanIncreaseSpeed");
            OnPropertyChanged("CanDecreaseSpeed");
            // ReSharper restore ExplicitCallerInfoArgument
        }

        public double Duration
        {
            get { return HasDuration ? _player.NaturalDuration.TimeSpan.TotalSeconds : 0.0; }
        }

        public bool HasDuration
        {
            get { return _player.NaturalDuration.HasTimeSpan; }
        }

        public double Position
        {
            get { return _player.Position.TotalSeconds; }
            set
            {
                _player.Position = TimeSpan.FromSeconds(value);
                OnPropertyChanged();
            }
        }

        public double Volume
        {
            get { return _player.Volume; }
            set
            {
                _player.Volume = value;
                OnPropertyChanged();
                // ReSharper disable ExplicitCallerInfoArgument
                OnPropertyChanged("CanMute");
                OnPropertyChanged("CanUnMute");
                OnPropertyChanged("IsMuted");
                // ReSharper restore ExplicitCallerInfoArgument
            }
        }

        public double Balance
        {
            get { return _player.Balance; }
            set
            {
                _player.Balance = value;
                OnPropertyChanged();
            }
        }

        public IntPtr WindowHandle { get; set; }

        private const double MaxRate = 4;
        private const double MinRate = 1 / MaxRate;
        private const double RateEpsilon = MinRate / 2;

        // this needs video player!
        public void IncreaseSpeed() { SpeedRatio += 0.25; }
        public bool CanIncreaseSpeed { get { return Source != null && SpeedRatio < MaxSpeedRatio; } }

        public void DecreaseSpeed() { SpeedRatio -= 0.25; }
        public bool CanDecreaseSpeed { get { return Source != null && SpeedRatio > MinSpeedRatio; } }

        public double SpeedRatio
        {
            get { return _speedRatio; }
            set
            {
                var newValue = Math.Max(MinRate, Math.Min(MaxRate, value));
                if (CloseTo(_speedRatio, newValue, RateEpsilon)) return;
                _player.SpeedRatio = (float)newValue;
                _speedRatio = newValue;
                OnPropertyChanged();
                // ReSharper disable ExplicitCallerInfoArgument
                OnPropertyChanged("CanIncreaseSpeed");
                OnPropertyChanged("CanDecreaseSpeed");
                // ReSharper restore ExplicitCallerInfoArgument
            }
        }

        private static bool CloseTo(double a, double b, double epsilon)
        {
            return Math.Abs(a - b) <= epsilon;
        }

        public Uri Source
        {
            get { return _player.Source; }
            set
            {
                VerifyUri(value);
                _player.Open(value);
                Stop();
                OnPropertyChanged();
            }
        }

        private void VerifyUri(Uri uri)
        {
            try
            {
                if (uri != null) uri.VerifyUriExists();
                Error.Exception = null;
            }
            catch (Exception ex) { Error.Exception = new MediaException("Could not open audio", ex); }
        }

        public IPlayerError Error { get; private set; }

        public void Play()
        {
            _player.Play();
            _positionTimer.Start();

            IsPlaying = true;
            IsPaused = false;
            IsStopped = false;
        }

        public bool CanPlay
        {
            get { return Source != null && !IsPlaying; }
        }

        public bool IsPlaying
        {
            get { return _isPlaying; }
            private set
            {
                if (_isPlaying == value) return;
                _isPlaying = value;
                OnPropertyChanged();
                // ReSharper disable once ExplicitCallerInfoArgument
                OnPropertyChanged("CanPlay");
            }
        }

        public void Pause()
        {
            _player.Pause();
            _positionTimer.Stop();

            IsPlaying = false;
            IsPaused = true;
            IsStopped = false;
        }

        public bool CanPause { get { return _player.CanPause; } }

        public bool IsPaused
        {
            get { return _isPaused; }
            private set
            {
                if (_isPaused == value) return;
                _isPaused = value;
                OnPropertyChanged();
                // ReSharper disable once ExplicitCallerInfoArgument
                OnPropertyChanged("CanPause");
            }
        }

        public void Stop()
        {
            _player.Stop();
            _positionTimer.Stop();

            IsPlaying = false;
            IsPaused = false;
            IsStopped = true;

            Position = 0.0;
        }

        public bool CanStop
        {
            get { return IsPlaying || IsPaused; }
        }

        public bool IsStopped
        {
            get { return _isStopped; }
            private set
            {
                _isStopped = value;
                OnPropertyChanged();
                // ReSharper disable once ExplicitCallerInfoArgument
                OnPropertyChanged("CanStop");
            }
        }

        public bool CanMute { get { return Volume > 0; } }
        public void Mute() { _restoreVolume = Volume; Volume = 0.0; }
        public bool IsMuted { get { return CanUnMute; } }
        public bool CanUnMute { get { return Volume < double.Epsilon; } }
        public void UnMute() { Volume = _restoreVolume; }

        [DebuggerStepThrough]
        private void PositionTimerTick(object sender, EventArgs e)
        {
            if (!IsPlaying) return;
            // ReSharper disable once ExplicitCallerInfoArgument
            OnPropertyChanged("Position");
        }

        private void HandleMediaFailedException(Exception exception)
        {
            Error.Exception = new MediaException("A media exception occurred", exception);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
