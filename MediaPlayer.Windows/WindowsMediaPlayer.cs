using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace MediaPlayer.Windows
{
    // ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
    public class WindowsMediaPlayer : IMediaPlayer
    {
        private const double RateEpsilon = 0.125;
        private readonly System.Windows.Media.MediaPlayer _player = new System.Windows.Media.MediaPlayer();
        private readonly DispatcherTimer _positionTimer = new DispatcherTimer(DispatcherPriority.ApplicationIdle);
        private bool _isPaused;
        private bool _isPlaying;
        private bool _isStopped;
        private double _rate = 1.0;
        private double _restoreVolume = 0.5;

        public WindowsMediaPlayer()
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

        public DrawingBrush VideoBrush { get; }
        public IntPtr WindowHandle { get; set; }
        public bool SupportsRate => true;
        public double MaxRate => 4;
        public double MinRate => 0.25;
        public double Duration => HasDuration ? _player.NaturalDuration.TimeSpan.TotalSeconds : 0.0;
        public bool HasDuration => _player.NaturalDuration.HasTimeSpan;

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
                OnPropertyChanged(nameof(CanMute));
                OnPropertyChanged(nameof(CanUnMute));
                OnPropertyChanged(nameof(IsMuted));
                // ReSharper restore ExplicitCallerInfoArgument
            }
        }

        public bool SupportsBalance => true;

        public double Balance
        {
            get { return _player.Balance; }
            set
            {
                _player.Balance = value;
                OnPropertyChanged();
            }
        }

        public void Faster()
        {
            Rate += 0.25;
        }

        public bool CanFaster => Source != null && Rate < MaxRate;

        public void Slower()
        {
            Rate -= 0.25;
        }

        public bool CanSlower => Source != null && Rate > MinRate;

        public double Rate
        {
            get { return _rate; }
            set
            {
                var newValue = Math.Max(MinRate, Math.Min(MaxRate, value));
                if (CloseTo(_rate, newValue, RateEpsilon)) return;
                _player.SpeedRatio = (float) newValue;
                _rate = newValue;
                OnPropertyChanged();
                // ReSharper disable ExplicitCallerInfoArgument
                OnPropertyChanged(nameof(CanFaster));
                OnPropertyChanged(nameof(CanSlower));
                // ReSharper restore ExplicitCallerInfoArgument
            }
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

        public IPlayerError Error { get; }

        public void Play()
        {
            _player.Play();
            _positionTimer.Start();

            IsPlaying = true;
            IsPaused = false;
            IsStopped = false;
        }

        public bool CanPlay => Source != null && !IsPlaying;

        public bool IsPlaying
        {
            get { return _isPlaying; }
            private set
            {
                if (_isPlaying == value) return;
                _isPlaying = value;
                OnPropertyChanged();
                // ReSharper disable once ExplicitCallerInfoArgument
                OnPropertyChanged(nameof(CanPlay));
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

        public bool CanPause => _player.CanPause;

        public bool IsPaused
        {
            get { return _isPaused; }
            private set
            {
                if (_isPaused == value) return;
                _isPaused = value;
                OnPropertyChanged();
                // ReSharper disable once ExplicitCallerInfoArgument
                OnPropertyChanged(nameof(CanPause));
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

        public bool CanStop => IsPlaying || IsPaused;

        public bool IsStopped
        {
            get { return _isStopped; }
            private set
            {
                _isStopped = value;
                OnPropertyChanged();
                // ReSharper disable once ExplicitCallerInfoArgument
                OnPropertyChanged(nameof(CanStop));
            }
        }

        public bool CanMute => Volume > 0;

        public void Mute()
        {
            _restoreVolume = Volume;
            Volume = 0.0;
        }

        public bool IsMuted => CanUnMute;
        public bool CanUnMute => Volume < double.Epsilon;

        public void UnMute()
        {
            Volume = _restoreVolume;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void PlayerMediaOpened(object sender, EventArgs e)
        {
            Error.Exception = null;

            // ReSharper disable ExplicitCallerInfoArgument
            OnPropertyChanged(nameof(Source));
            OnPropertyChanged(nameof(Duration));
            OnPropertyChanged(nameof(HasDuration));
            OnPropertyChanged(nameof(CanPlay));
            OnPropertyChanged(nameof(CanPause));
            OnPropertyChanged(nameof(CanStop));
            OnPropertyChanged(nameof(CanFaster));
            OnPropertyChanged(nameof(CanSlower));
            // ReSharper restore ExplicitCallerInfoArgument
        }

        private static bool CloseTo(double a, double b, double epsilon)
        {
            return Math.Abs(a - b) <= epsilon;
        }

        private void VerifyUri(Uri uri)
        {
            try
            {
                uri?.VerifyUriExists();
                Error.Exception = null;
            }
            catch (Exception ex)
            {
                Error.Exception = new MediaException("Could not open audio", ex);
            }
        }

        [DebuggerStepThrough]
        private void PositionTimerTick(object sender, EventArgs e)
        {
            if (!IsPlaying) return;
            // ReSharper disable once ExplicitCallerInfoArgument
            OnPropertyChanged(nameof(Position));
        }

        private void HandleMediaFailedException(Exception exception)
        {
            Error.Exception = new MediaException("A media exception occurred", exception);
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}