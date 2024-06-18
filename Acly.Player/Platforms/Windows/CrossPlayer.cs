using Acly.Player.Windows;
using Windows.Media.Playback;

namespace Acly.Player
{
    /// <summary>
    /// Плеер для Windows
    /// </summary>
    public class CrossPlayer : IPlayer
    {
        /// <summary>
        /// Создать новый экземпляр плеера для Windows
        /// </summary>
        public CrossPlayer()
        {
            _Player = new();
            _Player.MediaEnded += (p, obj) =>
            {
                SourceEnded?.Invoke();
            };
            _Player.MediaOpened += (p, obj) =>
            {
                SourceChanged?.Invoke();
            };
            _Player.CurrentStateChanged += (p, obj) =>
            {
                StateChanged?.Invoke(State);
            };

            _Dispatcher = Dispatcher.GetForCurrentThread();
            _Controls = new(this, _Player, _Dispatcher);
            _Controls.SkipToNextRequest += OnSkipToNextRequest;
            _Controls.SkipToPreviousRequest += OnSkipToPreviousRequest;

            _PositionTimer = new()
            {
                Interval = 400,
                AutoReset = true
            };
            _PositionTimer.Elapsed += OnPositionTimerElapsed;

            _PositionTimer.Start();
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public event Action<SimplePlayerState>? StateChanged;
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public event Action? SourceChanged;
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public event Action? SourceEnded;
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public event Action<TimeSpan>? PositionChanged;
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public event Action? SkippedToNext;
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public event Action? SkippedToPrevious;
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public event IPlayer.DisposePlayer? Disposed;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public TimeSpan Position
        {
            get => _Player.Position;
            set => _Player.Position = value;
        }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public TimeSpan Duration => _Player.NaturalDuration;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public float Speed
        {
            get => (float)_Player.PlaybackRate;
            set => _Player.PlaybackRate = (double)value;
        }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public float Volume
        {
            get => (float)_Player.Volume;
            set => _Player.Volume = (double)value;
        }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public bool Loop
        {
            get => _Player.IsLoopingEnabled;
            set => _Player.IsLoopingEnabled = value;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public bool IsPlaying => State == SimplePlayerState.Playing;
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public bool SourceSetted { get; private set; }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public bool AutoPlay { get; set; }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public SimplePlayerState State
        {
            get
            {
                SimplePlayerState result = SimplePlayerState.Stopped;

                switch (_Player.CurrentState)
                {
                    case MediaPlayerState.Playing:
                        result = SimplePlayerState.Playing;
                        break;
                    case MediaPlayerState.Paused:
                        result = SimplePlayerState.Paused;
                        break;
                }

                return result;
            }
        }

        private readonly MediaPlayer _Player;
        private readonly PlayerTransportControls _Controls;
        private readonly System.Timers.Timer _PositionTimer;
        private readonly IDispatcher? _Dispatcher;

        #region Установка

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="Data"><inheritdoc/></param>
        public async Task SetSource(byte[] Data)
        {
            Stream DataStream = Data.ToStream();
            await SetSource(DataStream);
        }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="SourceStream"><inheritdoc/></param>
        public Task SetSource(Stream SourceStream)
        {
            _Player.SetStreamSource(SourceStream.AsRandomAccessStream());
            _Controls.SetMediaItem(new MediaItem());

            return Task.CompletedTask;
        }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="SourceUrl"><inheritdoc/></param>
        public Task SetSource(string SourceUrl)
        {
            SourceSetted = true;

            _Player.SetUriSource(new(SourceUrl, UriKind.Absolute));
            _Controls.SetMediaItem(new MediaItem());

            if (AutoPlay)
            {
                Play();
            }

            return Task.CompletedTask;
        }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="Item"><inheritdoc/></param>
        public async Task SetSource(IMediaItem Item)
        {
            await SetSource(Item.AudioUrl);
            _Controls.SetMediaItem(Item);
        }

        #endregion

        #region Управление

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public void Pause()
        {
            _Player.Pause();
        }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public void Play()
        {
            _Player.Play();
        }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public void Stop()
        {
            Pause();
        }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public void SwitchState()
        {
            if (IsPlaying)
            {
                Pause();
                return;
            }

            Play();
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public void Release()
        {
            _PositionTimer.Enabled = false;
            _PositionTimer.Stop();
            _PositionTimer.Dispose();

            _Player.Dispose();
            _Controls.Dispose();

            Disposed?.Invoke(this);
        }

        #endregion

        #region Дополнительно

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="Size"><inheritdoc/></param>
        /// <param name="Window"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        /// <exception cref="NotImplementedException"></exception>
        public float[] GetSpectrumData(int Size, SpectrumWindow Window = SpectrumWindow.Rectangular)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="Size"><inheritdoc/></param>
        /// <param name="SmoothAmount"><inheritdoc/></param>
        /// <param name="Window"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        /// <exception cref="NotImplementedException"></exception>
        public float[] GetSpectrumData(int Size, int SmoothAmount, SpectrumWindow Window = SpectrumWindow.Rectangular)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region События

        private async void OnPositionTimerElapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            if (!IsPlaying || _Dispatcher == null)
            {
                return;
            }

            await _Dispatcher.DispatchAsync(() =>
            {
                PositionChanged?.Invoke(Position);
            });
        }

        private void OnSkipToPreviousRequest()
        {
            SkippedToPrevious?.Invoke();
        }
        private void OnSkipToNextRequest()
        {
            SkippedToNext?.Invoke();
        }

        #endregion
    }
}
