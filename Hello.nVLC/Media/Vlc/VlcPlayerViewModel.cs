using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;
using Declarations;
using Declarations.Events;
using Declarations.Media;
using Declarations.Players;
using Implementation;

namespace Hello.nVLC.Media.Vlc
{
    // ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
    public class VlcPlayerViewModel : Screen, IPlayerViewModel, IDisposable
    {
        private const double MaxSpeedRatioInternal = 4;
        private const double MinSpeedRatioInternal = 0.25;
        private const double TimeEpsilon = 0.25;
        private const double RateEpsilon = MinSpeedRatioInternal / 2;

        public double MaxSpeedRatio { get { return MaxSpeedRatioInternal; } }
        public double MinSpeedRatio { get { return MinSpeedRatioInternal; } }

        private readonly IMediaPlayerFactory _factory;
        private readonly IVideoPlayer _player; // supports speedratio
        private readonly EventHandler<MediaDurationChange> _durationChanged;
        private readonly EventHandler<MediaParseChange> _parsedChanged;

        private double _position;
        private double _duration;
        private double _speedRatio = 1.0;
        private double _restoreVolume = 0.5;
        private IMedia _media;
        private Uri _source;

        private PlayerState _playerState = PlayerState.Stopped;
        private enum PlayerState { Stopped, Playing, Paused }

        public VlcPlayerViewModel(IMediaPlayerFactory factory = null)
        {
            // create the player using the injected factory
            _factory = factory ?? new MediaPlayerFactory();
            _player = _factory.CreatePlayer<IVideoPlayer>();

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
            _player.Events.PlayerPositionChanged += (s, e) => OnPositionChanged(e.NewPosition * Duration);
            _player.Events.PlayerStopped += (s, e) => OnStopped();
        }

        #region Properties

        public TimeSpan OpenTimeOut { get; set; }

        /// <summary>
        /// used to make he player for testing syncronously
        /// </summary>
        internal bool Async { private get; set; }

        public IPlayerError Error { get; private set; }

        public bool HasAudio { get { return (_media != null && _media.IsParsed); } }
        public bool HasDuration { get { return (Duration > 0); } }

        public bool CanPlay { get { return HasAudio && _playerState != PlayerState.Playing; } }
        public bool IsPlaying { get { return _playerState == PlayerState.Playing; } }

        public bool CanPause { get { return HasAudio && _playerState == PlayerState.Playing; } }
        public bool IsPaused { get { return _playerState == PlayerState.Paused; } }

        public bool CanStop { get { return HasAudio && !PositionCloseTo(_position, 0.0); } }
        public bool IsStopped { get { return _playerState == PlayerState.Stopped; } }

        public bool CanMute { get { return Volume > 0; } }
        public void Mute() { _restoreVolume = Volume; Volume = 0.0; }
        public bool IsMuted { get { return Volume < double.Epsilon; } }

        public bool CanUnMute { get { return Volume < double.Epsilon; } }
        public void UnMute() { Volume = _restoreVolume; }

        public bool CanIncreaseSpeed { get { return Source != null && SpeedRatio < MaxSpeedRatio; } }
        public bool CanDecreaseSpeed { get { return Source != null && SpeedRatio > MinSpeedRatio; } }

        public Uri Source
        {
            get { return _source; }
            set
            {
                try
                {
                    Open(value);

                    // change souce if there are no exceptions during open
                    _source = value;

                    NotifyOfPropertyChange();
                    // ReSharper disable ExplicitCallerInfoArgument
                    NotifyOfPropertyChange("CanIncreaseSpeed");
                    NotifyOfPropertyChange("CanDecreaseSpeed");
                    NotifyOfPropertyChange("CanPlay");
                    NotifyOfPropertyChange("CanPause");
                    NotifyOfPropertyChange("CanStop");
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
        /// duration in 
        /// </summary>
        public double Duration
        {
            get { return _duration; }
            set
            {
                if (CloseTo(_duration, value, TimeEpsilon)) return;
                _duration = value;

                // ReSharper disable ExplicitCallerInfoArgument
                NotifyOfPropertyChange();
                NotifyOfPropertyChange("HasDuration");
                // ReSharper restore ExplicitCallerInfoArgument
            }
        }

        /// <summary>
        /// position in seconds
        /// </summary>
        public double Position
        {
            get { return _position; }
            set
            {
                // when playing this raises PositionChanged
                if (!PositionCloseTo(_player.Position * Duration, value))
                    _player.Position = (float)(value / Duration);

                // when not playing we need to raise it ourselves
                if (!IsPlaying)
                    OnPositionChanged(value);

                OnStateChanged();
            }
        }

        /// <summary>
        /// volume between 0 and 1.
        /// </summary>
        /// <remarks>internally the volume is an integer between 0 and 100 (pecent)</remarks>
        public double Volume
        {
            get { return (_player.Volume / 100.0); }
            set
            {
                var newVolume = (int)Math.Round(value * 100);
                if (_player.Volume == newVolume) return;
                _player.Volume = newVolume;

                // ReSharper disable ExplicitCallerInfoArgument
                NotifyOfPropertyChange();
                NotifyOfPropertyChange("CanMute");
                NotifyOfPropertyChange("CanUnMute");
                NotifyOfPropertyChange("IsMuted");
                // ReSharper restore ExplicitCallerInfoArgument
            }
        }

        /// <summary>
        /// balance setter is not implemented yet and getter always returns 0.
        /// </summary>
        public double Balance
        {
            get { return 0.0; }
            set
            {
                // TODO: How to implement balance with VLC?
            }
        }

        public IntPtr WindowHandle
        {
            get { return _player.WindowHandle; }
            set { _player.WindowHandle = value; }
        }

        /// <summary>
        /// speed of the audio. 1 means "normal speed"
        /// </summary>
        /// <remarks>this needs video player</remarks>
        public double SpeedRatio
        {
            get { return _speedRatio; }
            set
            {
                var newValue = Math.Max(MinSpeedRatio, Math.Min(MaxSpeedRatio, value));
                if (CloseTo(_speedRatio, newValue, RateEpsilon)) return;
                _player.PlaybackRate = (float)newValue;
                _speedRatio = newValue;

                // ReSharper disable ExplicitCallerInfoArgument
                NotifyOfPropertyChange();
                NotifyOfPropertyChange("CanIncreaseSpeed");
                NotifyOfPropertyChange("CanDecreaseSpeed");
                // ReSharper restore ExplicitCallerInfoArgument
            }
        }

        #endregion

        #region Methods

        protected override void OnViewAttached(object view, object context)
        {
            var playerView = view as VlcPlayerView;
            if (playerView == null) return;
            _player.WindowHandle = playerView.WindowHandle;
        }

        public void IncreaseSpeed() { SpeedRatio += 0.25; }
        public void DecreaseSpeed() { SpeedRatio -= 0.25; }

        public void Play() { PlayWithTask(); }

        /// <summary>
        /// play media and return a task -so it can be tested
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
                EventHandler eventHandler = (sender, e) => waitHandle.Set();

                // register on stop, execute STOP and wait for finish
                _player.Events.PlayerStopped += eventHandler;
                _player.Stop();
                waitHandle.WaitOne(1000);
                _player.Events.PlayerStopped -= eventHandler;

                // register on play, execute PLAY and wait for finish
                _player.Events.PlayerPlaying += eventHandler;
                _player.Play();
                waitHandle.WaitOne(1000);
                _player.Events.PlayerPlaying -= eventHandler;

                // now we set the position so nothing is going to change this afterwards asynchronously
                Trace.WriteLine(string.Format("PLAY setting position to: {0}", currentPosition));
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
            NotifyOfPropertyChange("Position");
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
            NotifyOfPropertyChange("HasAudio");
            OnStateChanged();
        }

        private void OnDurationChanged(long newLength)
        {
            var durationInSeconds = (newLength / 1000.0);
            Duration = durationInSeconds;
            OnStateChanged();
        }

        private void OnPositionChanged(double value)
        {
            if (PositionCloseTo(_position, value)) return;

            _position = value;
            // ReSharper disable ExplicitCallerInfoArgument
            NotifyOfPropertyChange("Position");
            // in case we just started, we need to enforce an "CanStop" update, because this depends on a position change
            NotifyOfPropertyChange("CanStop");
            // ReSharper restore ExplicitCallerInfoArgument
        }

        private void OnStateChanged()
        {
            // ReSharper disable ExplicitCallerInfoArgument
            NotifyOfPropertyChange("IsPlaying");
            NotifyOfPropertyChange("IsPaused");
            NotifyOfPropertyChange("IsStopped");
            NotifyOfPropertyChange("CanPlay");
            NotifyOfPropertyChange("CanPause");
            NotifyOfPropertyChange("CanStop");
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
                SpeedRatio = 1.0;
            }
            catch (Exception ex)
            {
                Trace.TraceWarning("Could not open source \"{0}\": {1}", source, ex);
                throw new MediaNotFoundException(String.Format(CultureInfo.CurrentCulture, "Could not open audio \"{0}\"", source), ex);
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
            NotifyOfPropertyChange("HasAudio");
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
            return CloseTo(a, b, Math.Max(TimeEpsilon * SpeedRatio, 0.5));
        }

        private static bool CloseTo(double a, double b, double epsilon)
        {
            return Math.Abs(a - b) <= epsilon;
        }

        #endregion

        #region IDisposable implementation

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                DisposeMedia();
                _factory.Dispose();
                _player.Dispose();
            }
        }

        public void Dispose()
        {

            Dispose(true);

            GC.SuppressFinalize(this);

        }

        // Disposable types implement a finalizer.
        ~VlcPlayerViewModel()
        {
            Dispose(false);
        }

        #endregion
    }
}
