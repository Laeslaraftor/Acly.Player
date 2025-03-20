namespace Acly.Player
{
    /// <summary>
    /// Базовый класс кроссплатформенного плеера
    /// </summary>
    public abstract class CrossPlayerBase : IPlayer
    {
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public event PlayerEvent? SkippedToNext;
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public event PlayerEvent? SkippedToPrevious;
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
        public abstract TimeSpan Position { get; set; }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public abstract TimeSpan Duration { get; }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public abstract float Speed { get; set; }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public abstract float Volume { get; set; }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public virtual bool Loop { get; set; }
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
                    StateChanged?.Invoke(this, value);
                }
            }
        }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public virtual bool IsPlaying => State == SimplePlayerState.Playing;
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public virtual bool SourceSetted => Source != null;
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public virtual bool AutoPlay { get; set; }
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
                    InvokeSourceChangedEvent();
                }
            }
        }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public virtual TimeSpan PositionUpdateInterval { get; set; } = TimeSpan.FromSeconds(0.05);
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public abstract int CaptureDataSize { get; set; }

        private SimplePlayerState _State = SimplePlayerState.Stopped;
        private IMediaItem? _Source;

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
        public abstract void Release();

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
        /// Вызвать событие очистки плеера
        /// </summary>
        protected void InvokeDisposedEvent()
        {
            Disposed?.Invoke(this);
        }
        /// <summary>
        /// Вызвать событие пропуска до следующего аудиофайла
        /// </summary>
        protected void InvokeSkippedToNextEvent()
        {
            SkippedToNext?.Invoke(this);
        }
        /// <summary>
        /// Вызвать событие пропуска до предыдущего аудиофайла
        /// </summary>
        protected void InvokeSkippedToPreviousEvent()
        {
            SkippedToPrevious?.Invoke(this);
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
        }

        #endregion
    }
}
