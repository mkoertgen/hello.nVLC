using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Declarations;
using Declarations.Events;
using Declarations.Media;
using Declarations.Players;
using Implementation;
using NAudio.CoreAudioApi;

namespace MediaPlayer.Vlc
{
    // ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
    public class VlcPlayer : IMediaPlayer, IDisposable
    {
        private const double RateDelta = 0.25;
        private const double TimeEps = 0.125;
        private const double RateEps = 0.125;
        private const double DefaultVolume = 1.0;
        private const double VolumeEps = 0.05;
        private const double BalanceEps = 0.05;
        private readonly AudioEndpointVolume _audioEndpointVolume;
        private readonly EventHandler<MediaDurationChange> _durationChanged;
        private readonly IMediaPlayerFactory _factory;
        private readonly EventHandler<MediaParseChange> _parsedChanged;
        private readonly IVideoPlayer _player;
        private double _balance = double.MinValue;
        private double _duration;
        private IMedia _media;
        private PlayerState _playerState = PlayerState.Stopped;
        private double _position;
        private double _rate = 1.0;
        private double _restoreVolume = DefaultVolume;
        private Uri _source;
        private double _volume = DefaultVolume;

        public VlcPlayer(IMediaPlayerFactory factory = null, AudioEndpointVolume audioEndpointVolume = null)
        {
            // create the player using the injected factory
            _factory = factory ?? new MediaPlayerFactory();
            _player = _factory.CreatePlayer<IVideoPlayer>();
            _audioEndpointVolume = audioEndpointVolume ?? BalanceExtensions.GetDefaultDevice().AudioEndpointVolume;

            // set default values
            OpenTimeOut = TimeSpan.FromSeconds(10);
            Async = true;
            Error = PlayerError.NoError;

            // cached event handler to avoid leaks during add and remove
            _durationChanged = (s, e) => OnDurationChanged(e.NewDuration);
            _parsedChanged = (s, e) => OnParsedChanged();

            // hook events
            _player.Events.MediaChanged += (s, e) => OnMediaChanged();
            _player.Events.MediaEnded += (s, e) => OnMediaEnded();
            _player.Events.PlayerEncounteredError += (s, e) => OnEncounteredError();
            _player.Events.PlayerLengthChanged += (s, e) => OnDurationChanged(e.NewLength);
            _player.Events.PlayerPaused += (s, e) => OnPaused();
            _player.Events.PlayerPlaying += (s, e) => OnPlaying();
            _player.Events.PlayerPositionChanged += (s, e) => OnPositionChanged(e.NewPosition*Duration);
            _player.Events.PlayerStopped += (s, e) => OnStopped();
        }

        public bool SupportsRate => true;
        public double MaxRate => 4;
        public double MinRate => 0.25;
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private enum PlayerState
        {
            Stopped,
            Playing,
            Paused
        }

        #region Properties

        private TimeSpan OpenTimeOut { get; }

        /// <summary>
        ///     used to make he player for testing synchronously
        /// </summary>
        internal bool Async { private get; set; }

        public IPlayerError Error { get; }

        public bool HasAudio => (_media != null && _media.IsParsed);
        public bool HasDuration => (Duration > 0);

        public bool CanPlay => HasAudio && _playerState != PlayerState.Playing;
        public bool IsPlaying => _playerState == PlayerState.Playing;

        public bool CanPause => HasAudio && _playerState == PlayerState.Playing;
        public bool IsPaused => _playerState == PlayerState.Paused;

        public bool CanStop => HasAudio && !PositionCloseTo(_position, 0.0);
        public bool IsStopped => _playerState == PlayerState.Stopped;

        public bool CanMute => Volume > 0;

        public void Mute()
        {
            _restoreVolume = Volume;
            Volume = 0.0;
        }

        public bool IsMuted => Volume < double.Epsilon;

        public bool CanUnMute => Volume < double.Epsilon;

        public void UnMute()
        {
            Volume = _restoreVolume;
        }

        public bool CanFaster => Source != null && Rate < MaxRate;
        public bool CanSlower => Source != null && Rate > MinRate;
        public bool SupportsBalance => _audioEndpointVolume != null;

        public Uri Source
        {
            get => _source;
            set
            {
                try
                {
                    Open(value);

                    // change source if there are no exceptions during open
                    _source = value;

                    OnPropertyChanged();
                    // ReSharper disable ExplicitCallerInfoArgument
                    OnPropertyChanged(nameof(CanFaster));
                    OnPropertyChanged(nameof(CanSlower));
                    OnPropertyChanged(nameof(CanPlay));
                    OnPropertyChanged(nameof(CanPause));
                    OnPropertyChanged(nameof(CanStop));

                    OnPropertyChanged(nameof(Volume));
                    OnPropertyChanged(nameof(CanMute));
                    OnPropertyChanged(nameof(CanUnMute));
                    // ReSharper restore ExplicitCallerInfoArgument

                    Error.Exception = null;
                }
                catch (MediaNotFoundException ex)
                {
                    Error.Exception = ex;
                }
            }
        }

        /// <summary>
        ///     duration in seconds
        /// </summary>
        public double Duration
        {
            get => _duration;
            set
            {
                if (CloseTo(_duration, value, TimeEps)) return;
                _duration = value;

                OnPropertyChanged();
                // ReSharper disable once ExplicitCallerInfoArgument
                OnPropertyChanged(nameof(HasDuration));
            }
        }

        /// <summary>
        ///     position in seconds
        /// </summary>
        public double Position
        {
            get => _position;
            set
            {
                // when playing this raises PositionChanged
                if (!PositionCloseTo(_player.Position*Duration, value))
                    _player.Position = (float) (value/Duration);

                // when not playing we need to raise it ourselves
                if (!IsPlaying)
                    OnPositionChanged(value);

                OnStateChanged();
            }
        }

        /// <summary>
        ///     volume between 0 and 1.
        /// </summary>
        /// <remarks>internally the volume is an integer between 0 and 100 (percent)</remarks>
        public double Volume
        {
            get
            {
                var playerVolume = _player.Volume;
                var playerInitialized = playerVolume >= 0; // not -1
                if (playerInitialized)
                {
                    var newVolume = (int) Math.Round(_volume*100);
                    if (newVolume != playerVolume)
                        _player.Volume = newVolume;
                }
                return _volume;
            }
            set
            {
                if (Math.Abs(_volume - value) < VolumeEps) return;
                _volume = value;

                var newVolume = (int) Math.Round(value*100);
                if (_player.Volume != newVolume)
                    _player.Volume = newVolume;

                OnPropertyChanged();
                // ReSharper disable ExplicitCallerInfoArgument
                OnPropertyChanged(nameof(CanMute));
                OnPropertyChanged(nameof(CanUnMute));
                OnPropertyChanged(nameof(IsMuted));
                // ReSharper restore ExplicitCallerInfoArgument
            }
        }

        /// <summary>
        ///     balance setter is not implemented yet and getter always returns 0.
        /// </summary>
        public double Balance
        {
            get
            {
                if (_balance < -1.0)
                    _balance = _audioEndpointVolume?.GetBalance() ?? 0.0;
                return _balance;
            }
            set
            {
                if (Math.Abs(_balance - value) < BalanceEps) return;
                _balance = value;
                _audioEndpointVolume?.SetBalance((float) _balance);
                OnPropertyChanged();
            }
        }

        public IntPtr WindowHandle
        {
            get => _player.WindowHandle;
            set => _player.WindowHandle = value;
        }

        /// <summary>
        ///     speed of the audio. 1 means "normal speed"
        /// </summary>
        /// <remarks>this needs video player</remarks>
        public double Rate
        {
            get => _rate;
            set
            {
                var newValue = Math.Max(MinRate, Math.Min(MaxRate, value));
                if (CloseTo(_rate, newValue, RateEps)) return;
                _player.PlaybackRate = (float) newValue;
                _rate = newValue;

                OnPropertyChanged();
                // ReSharper disable ExplicitCallerInfoArgument
                OnPropertyChanged(nameof(CanFaster));
                OnPropertyChanged(nameof(CanSlower));
                // ReSharper restore ExplicitCallerInfoArgument
            }
        }

        #endregion

        #region Methods

        public void Faster()
        {
            Rate += RateDelta;
        }

        public void Slower()
        {
            Rate -= RateDelta;
        }

        public void Play()
        {
            PlayWithTask();
        }

        /// <summary>
        ///     play media and return a task -so it can be tested
        /// </summary>
        internal Task PlayWithTask()
        {
            // I am using AutoResetEvent and add a temporary event handler to wait
            // until the method was successfully executed on VLC.
            // cf: http://stackoverflow.com/questions/1246153/which-methods-can-be-used-to-make-thread-wait-for-an-event-and-then-continue-its
            return Task.Run(() =>
            {
                // when not playing, weird vlc rewinds to 0.0
                // so we hack the position by saving & restoring
                var currentPosition = _position;

                var waitHandle = new AutoResetEvent(false);
                // ReSharper disable once AccessToDisposedClosure
                void EventHandler(object sender, EventArgs e) => waitHandle.Set();

                // register on stop, execute STOP and wait for finish
                _player.Events.PlayerStopped += EventHandler;
                _player.Stop();
                waitHandle.WaitOne(1000);
                _player.Events.PlayerStopped -= EventHandler;

                // register on play, execute PLAY and wait for finish
                _player.Events.PlayerPlaying += EventHandler;
                _player.Play();
                waitHandle.WaitOne(1000);
                _player.Events.PlayerPlaying -= EventHandler;

                // now we set the position so nothing is going to change this afterwards asynchronously
                Position = currentPosition;

                waitHandle.Dispose();
            });
        }

        public void Pause()
        {
            _player.Pause();
        }

        public void Stop()
        {
            _player.Stop();
            _position = 0.0;
            _player.Position = 0.0F;
            // ReSharper disable once ExplicitCallerInfoArgument
            OnPropertyChanged(nameof(Position));
            OnStateChanged();
        }

        #endregion

        #region Events

        private void OnMediaChanged()
        {
            OnStateChanged();
        }

        private void OnPlaying()
        {
            _playerState = PlayerState.Playing;
            OnStateChanged();
        }

        private void OnPaused()
        {
            _playerState = PlayerState.Paused;
            OnStateChanged();
        }

        private void OnMediaEnded()
        {
            // do the same as "paused" would be pressed
            _playerState = PlayerState.Paused;
            OnStateChanged();
        }

        private void OnParsedChanged()
        {
            // ReSharper disable once ExplicitCallerInfoArgument
            OnPropertyChanged(nameof(HasAudio));
            OnStateChanged();
        }

        private void OnDurationChanged(long newLength)
        {
            var durationInSeconds = (newLength/1000.0);
            Duration = durationInSeconds;
            OnStateChanged();
        }

        private void OnPositionChanged(double value)
        {
            if (PositionCloseTo(_position, value)) return;

            _position = value;
            // ReSharper disable ExplicitCallerInfoArgument
            OnPropertyChanged(nameof(Position));
            // in case we just started, we need to enforce an "CanStop" update, because this depends on a position change
            OnPropertyChanged(nameof(CanStop));
            // ReSharper restore ExplicitCallerInfoArgument
        }

        private void OnStateChanged()
        {
            // ReSharper disable ExplicitCallerInfoArgument
            OnPropertyChanged(nameof(IsPlaying));
            OnPropertyChanged(nameof(IsPaused));
            OnPropertyChanged(nameof(IsStopped));
            OnPropertyChanged(nameof(CanPlay));
            OnPropertyChanged(nameof(CanPause));
            OnPropertyChanged(nameof(CanStop));
            OnPropertyChanged(nameof(Volume));
            // ReSharper restore ExplicitCallerInfoArgument
        }

        private void OnStopped()
        {
            _playerState = PlayerState.Stopped;
            OnStateChanged();
        }

        private void OnEncounteredError()
        {
            var exception = new MediaException("A VLC exception occurred");

            Trace.TraceError(exception.ToString());
            Error.Exception = exception;
        }

        #endregion

        #region private helper

        private void Open(Uri source)
        {
            DisposeMedia();
            if (source == null) return;

            try
            {
                source.VerifyUriExists(OpenTimeOut);

                _media = _factory.CreateMedia<IMedia>(source.ToString());
                HookMediaEvents(true);
                _media.Parse(Async);
                _player.Open(_media);

                Position = 0.0;
                Rate = 1.0;
            }
            catch (Exception ex)
            {
                Trace.TraceWarning("Could not open source \"{0}\": {1}", source, ex);
                throw new MediaNotFoundException(
                    string.Format(CultureInfo.CurrentCulture, "Could not open audio \"{0}\"", source), ex);
            }
        }

        private void DisposeMedia()
        {
            if (_media == null) return;
            if (_player.IsPlaying) _player.Stop();

            HookMediaEvents(false);
            _media.Dispose();
            _media = null;

            // ReSharper disable once ExplicitCallerInfoArgument
            OnPropertyChanged(nameof(HasAudio));
            Duration = 0.0;
            Position = 0.0;
        }

        private void HookMediaEvents(bool hook)
        {
            if (hook)
            {
                _media.Events.DurationChanged += _durationChanged;
                _media.Events.ParsedChanged += _parsedChanged;
            }
            else
            {
                _media.Events.DurationChanged -= _durationChanged;
                _media.Events.ParsedChanged -= _parsedChanged;
            }
        }

        private bool PositionCloseTo(double a, double b)
        {
            // just to make sure we get close enough at high speed (streaming and variable length issues)
            return CloseTo(a, b, Math.Max(TimeEps*Rate, TimeEps));
        }

        private static bool CloseTo(double a, double b, double epsilon)
        {
            return Math.Abs(a - b) <= epsilon;
        }

        #endregion

        #region IDisposable implementation

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;
            DisposeMedia();
            _factory.Dispose();
            _player.Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Disposable types implement a finalizer.
        ~VlcPlayer()
        {
            Dispose(false);
        }

        #endregion
    }
}
