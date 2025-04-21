using System.ComponentModel;

namespace Acly.Player
{
    /// <summary>
    /// Базовый класс кроссплатформенного плеера
    /// </summary>
    public abstract class CrossPlayerBase : IPlayer, IDisposable, INotifyPropertyChanged
    {
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public event PlayerEvent? SkippedToNext
        {
            add
            {
                if (!IsDisposed && value != null)
                {
                    _SkipToNextHandlers.Add(value);
                }
            }
            remove
            {
                if (!IsDisposed && value != null)
                {
                    _SkipToNextHandlers.Remove(value);
                }
            }
        }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public event PlayerEvent? SkippedToPrevious
        {
            add
            {
                if (!IsDisposed && value != null)
                {
                    _SkipToPreviousHandlers.Add(value);
                }
            }
            remove
            {
                if (!IsDisposed && value != null)
                {
                    _SkipToPreviousHandlers.Remove(value);
                }
            }
        }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public event IPlayer.DisposePlayer? Disposed;
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public event SimplePlayerStateEvent? StateChanged;
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public event SimplePlayerEvent? SourceChanged;
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public event SimplePlayerEvent? SourceEnded;
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public event PlayerTimeEvent? PositionChanged;
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Очищен ли плеер
        /// </summary>
        public bool IsDisposed
        {
            get => _IsDisposed;
            private set
            {
                if (_IsDisposed != value)
                {
                    _IsDisposed = value;
                    InvokePropertyChangedEvent(nameof(IsDisposed));
                }
            }
        }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public abstract TimeSpan Position { get; set; }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public virtual TimeSpan Duration
        {
            get => _Duration;
            protected set
            {
                if (_Duration != value)
                {
                    _Duration = value;
                    InvokePropertyChangedEvent(nameof(Duration));
                }
            }
        }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public virtual float Speed
        {
            get => _Speed;
            set
            {
                if (_Speed != value)
                {
                    _Speed = value;
                    InvokePropertyChangedEvent(nameof(Speed));
                }
            }
        }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public virtual float Volume
        {
            get => _Volume;
            set
            {
                if (_Volume != value)
                {
                    InvokePropertyChangedEvent(nameof(Volume));
                }
            }
        }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public virtual bool Loop
        {
            get => _Loop;
            set
            {
                if (_Loop != value)
                {
                    _Loop = value;
                    InvokePropertyChangedEvent(nameof(Loop));
                }
            }
        }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public virtual SimplePlayerState State
        {
            get => _State;
            protected set
            {
                if (_State != value)
                {
                    _State = value;
                    IsPlaying = value == SimplePlayerState.Playing;
                    StateChanged?.Invoke(this, value);
                    InvokePropertyChangedEvent(nameof(State));
                }
            }
        }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public virtual bool IsPlaying
        {
            get => _IsPlaying;
            private set
            {
                if (_IsPlaying != value)
                {
                    _IsPlaying = value;
                    InvokePropertyChangedEvent(nameof(IsPlaying));
                }
            }
        }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public virtual bool SourceSetted
        {
            get => _SourceSetted;
            private set
            {
                if (_SourceSetted != value)
                {
                    _SourceSetted = value;
                    InvokePropertyChangedEvent(nameof(SourceSetted));
                }
            }
        }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public virtual bool AutoPlay
        {
            get => _AutoPlay;
            set
            {
                if (_AutoPlay != value)
                {
                    _AutoPlay = value;
                    InvokePropertyChangedEvent(nameof(AutoPlay));
                }
            }
        }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public virtual IMediaItem? Source
        {
            get => _Source;
            protected set
            {
                if (_Source != value)
                {
                    _Source = value;
                    SourceSetted = value != null;
                    InvokeSourceChangedEvent();
                    InvokePropertyChangedEvent(nameof(Source));
                }
            }
        }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public virtual TimeSpan PositionUpdateInterval
        {
            get => _PositionUpdateInterval;
            set
            {
                if (_PositionUpdateInterval != value)
                {
                    _PositionUpdateInterval = value;
                    InvokePropertyChangedEvent(nameof(PositionUpdateInterval));
                }
            }
        }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public abstract int CaptureDataSize { get; set; }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public IPlayerRemoteControls RemoteControls => InternalRemoteControls;

        private InternalPlayerRemoteControls InternalRemoteControls
        {
            get
            {
                if (_InternalRemoteControls == null)
                {
                    _InternalRemoteControls = new(this);
                    _InternalRemoteControls.SkippedToNext += OnSkippedToNext;
                    _InternalRemoteControls.SkippedToPrevious += OnSkippedToPrevious;
                    _InternalRemoteControls.CanSkipToNextChanged += OnCanSkipToNextChanged;
                    _InternalRemoteControls.CanSkipToPreviousChanged += OnCanSkipToPreviousChanged;
                    _InternalRemoteControls.PropertyChanged += OnRemoteControlsPropertyChanged;
                }
                
                return _InternalRemoteControls;
            }
        }

        private bool _IsDisposed;
        private bool _IsPlaying;
        private bool _SourceSetted;
        private bool _AutoPlay;
        private bool _Loop;
        private float _Speed = 1;
        private float _Volume = 1;
        private TimeSpan _PositionUpdateInterval = TimeSpan.FromSeconds(0.05);
        private SimplePlayerState _State = SimplePlayerState.Stopped;
        private IMediaItem? _Source;
        private TimeSpan _Duration;
        private InternalPlayerRemoteControls? _InternalRemoteControls;
        private List<PlayerEvent> _SkipToNextHandlers = [];
        private List<PlayerEvent> _SkipToPreviousHandlers = [];

        #region Установка

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="Data"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        public abstract Task SetSource(IMediaItem Data);
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="Data"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        public abstract Task SetSource(byte[] Data);
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="SourceStream"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        public abstract Task SetSource(Stream SourceStream);
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="SourceUrl"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        public abstract Task SetSource(string SourceUrl);

        #endregion

        #region Управление

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public abstract void Pause();
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public abstract void Play();
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public abstract void Stop();

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        [Obsolete("Надо использовать " + nameof(Dispose))]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void Release() => Dispose();

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public void Dispose()
        {
            if (IsDisposed)
            {
                return;
            }

            IsDisposed = true;

            Dispose(true);
            Disposed?.Invoke(this);

            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// <inheritdoc cref="Dispose()"/>
        /// </summary>
        /// <param name="Disposing">Мегаочистка</param>
        protected virtual void Dispose(bool Disposing)
        {
            if (!Disposing)
            {
                return;
            }

            RemoteControls.Dispose();
            RemoteControls.SkippedToNext -= OnSkippedToNext;
            RemoteControls.SkippedToPrevious -= OnSkippedToPrevious;
            RemoteControls.CanSkipToNextChanged -= OnCanSkipToNextChanged;
            RemoteControls.CanSkipToPreviousChanged -= OnCanSkipToPreviousChanged;
            RemoteControls.PropertyChanged -= OnRemoteControlsPropertyChanged;
        }

        #endregion

        #region Дополнительно

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="Size"><inheritdoc/></param>
        /// <param name="Window"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        public virtual float[] GetSpectrumData(int Size, SpectrumWindow Window = SpectrumWindow.Rectangular)
        {
            return GetSpectrumData(Size, 0, Window);
        }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="Size"><inheritdoc/></param>
        /// <param name="SmoothAmount"><inheritdoc/></param>
        /// <param name="Window"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        public abstract float[] GetSpectrumData(int Size, int SmoothAmount, SpectrumWindow Window = SpectrumWindow.Rectangular);

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="Size"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        public abstract float[] GetWaveformData(int Size);

        #endregion

        #region События

        /// <summary>
        /// Вызвать событие пропуска до следующего аудиофайла
        /// </summary>
        protected void InvokeSkippedToNextEvent()
        {
            InternalRemoteControls.SkipToNext();
        }
        /// <summary>
        /// Вызвать событие пропуска до предыдущего аудиофайла
        /// </summary>
        protected void InvokeSkippedToPreviousEvent()
        {
            InternalRemoteControls.SkipToPrevious();
        }
        /// <summary>
        /// Вызвать событие окончания проигрывания аудиофайла
        /// </summary>
        protected void InvokeSourceEndedEvent()
        {
            SourceEnded?.Invoke(this);
        }
        /// <summary>
        /// Вызвать событие изменения источника аудио
        /// </summary>
        protected void InvokeSourceChangedEvent()
        {
            SourceChanged?.Invoke(this);
        }
        /// <summary>
        /// Вызвать событие изменения текущей проигрываемой позиции
        /// </summary>
        /// <param name="Value">Текущая проигрываемая позиция</param>
        protected void InvokePositionChangedEvent(TimeSpan Value)
        {
            PositionChanged?.Invoke(this, Value);
            InvokePropertyChangedEvent(nameof(Position));
        }

        /// <summary>
        /// Вызвать событие изменения поля
        /// </summary>
        /// <param name="PropertyName">Изменённое поле</param>
        protected void InvokePropertyChangedEvent(string PropertyName)
        {
            PropertyChanged?.Invoke(this, new(PropertyName));
        }

        /// <summary>
        /// Событие изменения доступности пропуска до следующего аудиофайла
        /// </summary>
        /// <param name="sender">Отправитель</param>
        /// <param name="e">Доступен ли пропуск до следующего аудиофайла</param>
        protected virtual void OnCanSkipToNextChanged(object? sender, bool e)
        {
        }
        /// <summary>
        /// Событие изменения доступности перемотки до предыдущего аудиофайла
        /// </summary>
        /// <param name="sender">Отправитель</param>
        /// <param name="e">Доступна ли перемотка до предыдущего аудиофайла</param>
        protected virtual void OnCanSkipToPreviousChanged(object? sender, bool e)
        {
        }
        /// <summary>
        /// Событие изменения поля удалённого управления плеером
        /// </summary>
        /// <param name="sender">Отправитель</param>
        /// <param name="e">Информация</param>
        protected virtual void OnRemoteControlsPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            InvokePropertyChangedEvent(nameof(RemoteControls));
        }

        private void OnSkippedToPrevious(object? sender, EventArgs e)
        {
            InvokeHandlers(_SkipToPreviousHandlers);
        }
        private void OnSkippedToNext(object? sender, EventArgs e)
        {
            InvokeHandlers(_SkipToNextHandlers);
        }

        private void InvokeHandlers(IEnumerable<PlayerEvent> Handlers)
        {
            foreach (var Handler in Handlers)
            {
                Handler(this);
            }
        }

        #endregion
    }
}
