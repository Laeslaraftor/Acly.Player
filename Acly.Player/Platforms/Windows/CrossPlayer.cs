using Acly.Player.Spectrum;
using Acly.Player.Windows;
using Windows.Media.Playback;
using Acly.Platforms;

namespace Acly.Player
{
    /// <summary>
    /// Плеер для Windows
    /// </summary>
    [SimplePlayerImplementation(RuntimePlatform.Windows)]
    public class CrossPlayer : CrossPlayerBase
    {
        /// <summary>
        /// Создать новый экземпляр плеера для Windows
        /// </summary>
        public CrossPlayer()
        {
            _Analyzer = new()
            {
                Capacity = 1024
            };
            _Player = new();
            _Player.MediaEnded += OnPlayerMediaEnded;
            _Player.CurrentStateChanged += OnPlayerCurrentStateChanged;

            _Dispatcher = Dispatcher.GetForCurrentThread();
            _Controls = new(this, _Player, _Dispatcher);
            _Controls.SkipToNextRequest += InvokeSkippedToNextEvent;
            _Controls.SkipToPreviousRequest += InvokeSkippedToPreviousEvent;

            _PositionTimer = new()
            {
                Interval = PositionUpdateInterval.TotalMilliseconds,
                AutoReset = true
            };
            _PositionTimer.Elapsed += OnPositionTimerElapsed;
            _PositionTimer.Start();

            _Analyzer.Start();
        }
        /// <summary>
        /// Очистка!
        /// </summary>
        ~CrossPlayer()
        {
            try
            {
                Release();
            }
            catch
            {
            }
        }        

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override TimeSpan Position
        {
            get => _Player.Position;
            set => _Player.Position = value;
        }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override TimeSpan Duration => _Player.NaturalDuration;
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override float Speed
        {
            get => (float)_Player.PlaybackRate;
            set => _Player.PlaybackRate = (double)value;
        }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override float Volume
        {
            get => (float)_Player.Volume;
            set => _Player.Volume = (double)value;
        }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override bool Loop
        {
            get => _Player.IsLoopingEnabled;
            set => _Player.IsLoopingEnabled = value;
        }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override TimeSpan PositionUpdateInterval 
        { 
            get => base.PositionUpdateInterval; 
            set
            {
                base.PositionUpdateInterval = value;
                _PositionTimer.Interval = value.TotalMilliseconds;
            } 
        }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override int CaptureDataSize
        {
            get => _Analyzer.Capacity;
            set => _Analyzer.Capacity = value;
        }

        private readonly MediaPlayer _Player;
        private readonly PlayerTransportControls _Controls;
        private readonly AudioAnalyzer _Analyzer;
        private readonly System.Timers.Timer _PositionTimer;
        private readonly IDispatcher? _Dispatcher;

        #region Установка

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="Data"><inheritdoc/></param>
        public override async Task SetSource(byte[] Data)
        {
            Stream DataStream = Data.ToStream();
            await SetSource(DataStream);
        }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="SourceStream"><inheritdoc/></param>
        public override Task SetSource(Stream SourceStream)
        {
            Source = new MediaItem();

            _Player.SetStreamSource(SourceStream.AsRandomAccessStream());
            _Controls.SetMediaItem(Source);

            return Task.CompletedTask;
        }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="SourceUrl"><inheritdoc/></param>
        public override Task SetSource(string SourceUrl)
        {
            Source = new MediaItem();

            _Player.SetUriSource(new(SourceUrl, UriKind.Absolute));
            _Controls.SetMediaItem(Source);

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
        public override async Task SetSource(IMediaItem Item)
        {
            Source = Item;

            await SetSource(Item.AudioUrl);
            _Controls.SetMediaItem(Item);
        }

        #endregion

        #region Управление

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override void Pause()
        {
            _Player.Pause();
        }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override void Play()
        {
            _Player.Play();
        }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override void Stop()
        {
            Pause();
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override void Release()
        {
            _Player.MediaEnded -= OnPlayerMediaEnded;
            _Player.CurrentStateChanged -= OnPlayerCurrentStateChanged;
            _PositionTimer.Elapsed -= OnPositionTimerElapsed;
            _Controls.SkipToNextRequest -= InvokeSkippedToNextEvent;
            _Controls.SkipToPreviousRequest -= InvokeSkippedToPreviousEvent;

            _PositionTimer.Enabled = false;
            _PositionTimer.Stop();
            _PositionTimer.Dispose();
            _Analyzer.Dispose();

            _Player.Dispose();
            _Controls.Dispose();

            InvokeDisposedEvent();
        }

        #endregion

        #region Дополнительно

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="Size"><inheritdoc/></param>
        /// <param name="SmoothAmount"><inheritdoc/></param>
        /// <param name="Window"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        /// <exception cref="NotImplementedException"></exception>
        public override float[] GetSpectrumData(int Size, int SmoothAmount, SpectrumWindow Window = SpectrumWindow.Rectangular)
        {
            if (Size >= 512)
            {
                throw new ArgumentException("Максимальный доступный для текущей платформы размер - 512");
            }

            float[] Result = _Analyzer.GetSpectrumData(Size);
            
            if (SmoothAmount > 0)
            {
                Result = ArrayWork.Smooth(Result, SmoothAmount);
            }

            return Result;
        }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="Size"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        public override float[] GetWaveformData(int Size)
        {
            _Analyzer.AudioFile = Source?.AudioUrl;
            return _Analyzer.GetWaveformData(Size);
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
                InvokePositionChangedEvent(Position);
            });
        }

        private void OnPlayerMediaEnded(MediaPlayer sender, object args)
        {
            InvokeSourceEndedEvent();
        }
        private void OnPlayerCurrentStateChanged(MediaPlayer sender, object args)
        {
            State = MediaToSimpleState(_Player.CurrentState);
        }

        #endregion

        #region Статика

        private static SimplePlayerState MediaToSimpleState(MediaPlayerState State)
        {
            SimplePlayerState result = SimplePlayerState.Stopped;

            switch (State)
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

        #endregion
    }
}
