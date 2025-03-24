using Acly.Player.Android;
using Acly.Player.Spectrum;
using Acly.Tokens;
using Android.Media;
using Android.OS;
using Acly.Platforms;

namespace Acly.Player
{
    /// <summary>
    /// Плеер для Android
    /// </summary>
    [SimplePlayerImplementation(RuntimePlatform.Android)]
    public class CrossPlayer : CrossPlayerBase
    {
        /// <summary>
        /// Создать новый экземпляр плеера для Android
        /// </summary>
        public CrossPlayer()
        {
            _Player = new();

            if (AudioAnalyzer.TryCreate(_Player, true, true, out _Analyzer))
            {
                _Analyzer.Capacity = 1024;
            }

            _Player.Prepared += OnPlayerPrepared;
            _Player.Completion += OnSourceCompleted;

            PlayerNotification.PlayRequest += Play;
            PlayerNotification.PauseRequest += Pause;
            PlayerNotification.StopRequest += Stop;
            PlayerNotification.SkipToNextRequest += InvokeSkippedToNextEvent;
            PlayerNotification.SkipToPreviousRequest += InvokeSkippedToPreviousEvent;
            PlayerNotification.SeekRequest += OnSeekRequest;
            PlayerNotification.AddPlayer(this);

            _Timer.Post(OnTimerTick);
        }
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
            get => TimeSpan.FromMilliseconds(_Player.CurrentPosition);
            set => _Player.SeekTo(Convert.ToInt32(value.TotalMilliseconds));
        }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override TimeSpan Duration => _CurrentDuration;
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override bool SourceSetted => _SourceSetted;
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override float Speed
        {
            get
            {
                try
                {
                    return _Player.PlaybackParams.Pitch;
                }
                catch
                {
                    return 0;
                }
            }
            set => _Player.PlaybackParams.SetPitch(value);
        }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override float Volume
        {
            get => _Volume;
            set
            {
                _Player.SetVolume(value, value);
                _Volume = value;
            }
        }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override bool Loop
        {
            get => _Player.Looping;
            set => _Player.Looping = value;
        }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override SimplePlayerState State
        {
            get => base.State;
            protected set
            {
                base.State = value;
                UpdateNotification();
            }
        }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override int CaptureDataSize
        {
            get
            {
                if (_Analyzer != null)
                {
                    return _Analyzer.Capacity;
                }

                return 0;
            }
            set
            {
                if (_Analyzer != null)
                {
                    _Analyzer.Capacity = value;
                }
            }
        }

        private readonly MediaPlayer _Player;
        private readonly AudioAnalyzer? _Analyzer;
        private TimeSpan _CurrentDuration;
        private float _Volume = 1;
        private Handler? _Timer = new();
        private bool _DisableCompletedEvent;
        private Token? _SourceSettingToken;
        private bool _SourceSetted;

        #region Установка

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="Data"><inheritdoc/></param>
        public override Task SetSource(byte[] Data)
        {
            if (SourceSetted)
            {
                _Player.Reset();
            }

            SetSourceLoop(async () =>
            {
                return await TrySetSourceAndPrepare(Data);
            });

            InvokeSourceChangedEvent();

            return Task.CompletedTask;
        }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="SourceStream"><inheritdoc/></param>
        public override async Task SetSource(System.IO.Stream SourceStream)
        {
            using MemoryStream Memory = new();
            await SourceStream.CopyToAsync(Memory);

            byte[] Data = Memory.ToArray();

            await SetSource(Data);
        }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="SourceUrl"><inheritdoc/></param>
        public override async Task SetSource(string SourceUrl)
        {
            await SetSource(new MediaItem
            {
                AudioUrl = SourceUrl
            });
        }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="Item"><inheritdoc/></param>
        public override Task SetSource(IMediaItem Item)
        {
            Source = Item;
            UpdateNotification();

            if (SourceSetted)
            {
                _Player.Reset();
            }

            SetSourceLoop(async () =>
            {
                return await TrySetSourceAndPrepare(Item.AudioUrl);
            });

            InvokeSourceChangedEvent();

            return Task.CompletedTask;
        }

        private async void SetSourceLoop(Func<Task<bool>> SettingMethod)
        {
            Token SettingToken = new();
            _DisableCompletedEvent = true;
            _SourceSettingToken = SettingToken;

            while (!await SettingMethod() && _SourceSettingToken == SettingToken)
            {
            }
            if (_SourceSettingToken != SettingToken)
            {
                return;
            }

            _DisableCompletedEvent = false;
        }

        #endregion

        #region Управление

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override void Pause()
        {
            State = SimplePlayerState.Paused;

            _Player.Pause();
        }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override void Play()
        {
            State = SimplePlayerState.Playing;

            _Player.Start();
        }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override void Stop()
        {
            State = SimplePlayerState.Stopped;

            _Player.Stop();
            UpdateNotification();
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override void Release()
        {
            _Timer?.Dispose();
            _Analyzer?.Dispose();
            _Player.Release();

            _Timer = null;
            _Player.Prepared -= OnPlayerPrepared;
            _Player.Completion -= OnSourceCompleted;
            PlayerNotification.PlayRequest -= Play;
            PlayerNotification.PauseRequest -= Pause;
            PlayerNotification.StopRequest -= Stop;
            PlayerNotification.SkipToNextRequest -= InvokeSkippedToNextEvent;
            PlayerNotification.SkipToPreviousRequest -= InvokeSkippedToPreviousEvent;
            PlayerNotification.SeekRequest -= OnSeekRequest;

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
            if (_Analyzer == null)
            {
                return new float[Size];
            }
            if (Size >= CaptureDataSize)
            {
                throw new ArgumentException("Запрошено больше данных чем захватывается!");
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
            if (_Analyzer == null)
            {
                return new float[Size];
            }
            if (Size >= CaptureDataSize)
            {
                throw new ArgumentException("Запрошено больше данных чем захватывается!");
            }

            return _Analyzer.GetWaveformData(Size);
        }

        private async Task<bool> TrySetSourceAndPrepare(byte[] Data)
        {
            return await TryPerform(() =>
            {
                _Player.SetDataSource(new MediaData(Data));
                _Player.Prepare();
            });
        }
        private async Task<bool> TrySetSourceAndPrepare(string AudioUrl)
        {
            return await TryPerform(() =>
            {
                _Player.SetDataSource(AudioUrl);
                _Player.Prepare();
            });
        }
        /// <summary>
        /// прохерачивалка
        /// </summary>
        /// <param name="Action">действие, которое нужно прохерачить</param>
        /// <returns>успешно ли прохерачилось</returns>
        private async Task<bool> TryPerform(Action Action)
        {
            bool Result = true;

            await Task.Run(() =>
            {
                try
                {
                    Action?.Invoke();
                }
                catch
                {
                    Result = false;
                }
            });

            return Result;
        }

        #endregion

        #region Уведомление

        private void UpdateNotification()
        {
            PlayerNotification.Update(this, Source);
        }

        #endregion

        #region События

        private new void InvokeSourceChangedEvent()
        {
            _SourceSetted = true;
            base.InvokeSourceChangedEvent();
        }

        private void OnPlayerPrepared(object? sender, EventArgs e)
        {
            State = SimplePlayerState.Paused;
            _CurrentDuration = TimeSpan.FromMilliseconds(_Player.Duration);

            if (Source != null && Source.Duration == TimeSpan.Zero)
            {
                Source.Duration = Duration;
            }

            UpdateNotification();

            if (AutoPlay)
            {
                Play();
            }
        }
        private void OnSourceCompleted(object? sender, EventArgs e)
        {
            if (_DisableCompletedEvent)
            {
                return;
            }

            InvokeSourceEndedEvent();
        }
        private void OnSeekRequest(TimeSpan Position)
        {
            this.Position = Position;
        }

        private void OnTimerTick()
        {
            if (SourceSetted && State != SimplePlayerState.Stopped)
            {
                InvokePositionChangedEvent(Position);
            }

            _Timer?.PostDelayed(OnTimerTick, (long)PositionUpdateInterval.TotalMilliseconds);
        }

        #endregion

        //--

        #region Статика

        /// <summary>
        /// Инициализировать уведомление плеера
        /// </summary>
        /// <param name="Activity">Главный экземпляр Activity Android приложения</param>
        /// <param name="Style">Настройки внешнего вида уведомления</param>
        /// <param name="Settings">Настройки уведомления</param>
        public static void InitNotification(MauiAppCompatActivity Activity, PlayerNotificationStyle Style, PlayerNotificationSettings Settings)
        {
            if (PlayerNotification.ServicesStarted)
            {
                return;
            }

            PlayerNotification.Init(Activity, Style, Settings);
        }
        /// <summary>
        /// Инициализировать уведомление плеера
        /// </summary>
        /// <param name="Activity">Главный экземпляр Activity Android приложения</param>
        public static void InitNotification(MauiAppCompatActivity Activity)
        {
            InitNotification(Activity, new(), new());
        }

        #endregion
    }
}
