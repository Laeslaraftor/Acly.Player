using Acly.Player.Android;
using Acly.Tokens;
using Android.Media;
using Android.OS;

namespace Acly.Player
{
    /// <summary>
    /// Плеер для Android
    /// </summary>
    public class CrossPlayer : IPlayer
    {
        /// <summary>
        /// Создать новый экземпляр плеера для Android
        /// </summary>
        public CrossPlayer()
        {
            _Player = new();
            _Player.Prepared += OnPlayerPrepared;
            _Player.Completion += OnSourceCompleted;

            PlayerNotification.PlayRequest += Play;
            PlayerNotification.PauseRequest += Pause;
            PlayerNotification.StopRequest += Stop;
            PlayerNotification.SkipToNextRequest += SkipToNext;
            PlayerNotification.SkipToPreviousRequest += SkipToPrevious;
            PlayerNotification.SeekRequest += OnSeekRequest;
            PlayerNotification.AddPlayer(this);

            _Timer.Post(OnTimerTick);
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
            get => TimeSpan.FromMilliseconds(_Player.CurrentPosition);
            set => _Player.SeekTo(Convert.ToInt32(value.TotalMilliseconds));
        }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public TimeSpan Duration { get; private set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public float Speed
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
        public float Volume
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
        public bool Loop
        {
            get => _Player.Looping;
            set => _Player.Looping = value;
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
            get => _State;
            private set
            {
                _State = value;
                UpdateNotification();
                StateChanged?.Invoke(value);
            }
        }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public IMediaItem? Source { get; private set; }

        private readonly MediaPlayer _Player;
        private SimplePlayerState _State = SimplePlayerState.Stopped;
        private float _Volume = 1;
        private Handler? _Timer = new();
        private bool _DisableCompletedEvent;
        private Token? _SourceSettingToken;

        #region Установка

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="Data"><inheritdoc/></param>
        public Task SetSource(byte[] Data)
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
        public async Task SetSource(System.IO.Stream SourceStream)
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
        public async Task SetSource(string SourceUrl)
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
        public Task SetSource(IMediaItem Item)
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
        public void Pause()
        {
            State = SimplePlayerState.Paused;

            _Player.Pause();
        }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public void Play()
        {
            State = SimplePlayerState.Playing;

            _Player.Start();
        }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public void SwitchState()
        {
            if (State == SimplePlayerState.Stopped)
            {
                return;
            }
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
        public void Stop()
        {
            State = SimplePlayerState.Stopped;

            _Player.Stop();
            UpdateNotification();
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public void Release()
        {
            _Timer?.Dispose();
            _Player.Release();

            _Timer = null;
            PlayerNotification.PlayRequest -= Play;
            PlayerNotification.PauseRequest -= Pause;
            PlayerNotification.StopRequest -= Stop;
            PlayerNotification.SkipToNextRequest -= SkipToNext;
            PlayerNotification.SkipToPreviousRequest -= SkipToPrevious;
            PlayerNotification.SeekRequest -= OnSeekRequest;

            Disposed?.Invoke(this);
        }

        private void SkipToNext()
        {
            SkippedToNext?.Invoke();
        }
        private void SkipToPrevious()
        {
            SkippedToPrevious?.Invoke();
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

        private void InvokeSourceChangedEvent()
        {
            SourceSetted = true;

            SourceChanged?.Invoke();
        }

        private void OnPlayerPrepared(object? sender, EventArgs e)
        {
            State = SimplePlayerState.Paused;
            Duration = TimeSpan.FromMilliseconds(_Player.Duration);

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

            SourceEnded?.Invoke();
        }
        private void OnSeekRequest(TimeSpan Position)
        {
            this.Position = Position;
        }

        private void OnTimerTick()
        {
            if (SourceSetted && State != SimplePlayerState.Stopped)
            {
                PositionChanged?.Invoke(Position);
            }

            _Timer?.PostDelayed(OnTimerTick, 400);
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
