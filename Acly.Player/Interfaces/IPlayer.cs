namespace Acly.Player
{
    /// <summary>
    /// Интерфейс плеера, расширяющий <see cref="ISimplePlayer{T}"/>
    /// </summary>
    public interface IPlayer : ISimplePlayer<IMediaItem>
    {
        /// <summary>
        /// Вызывается при пропуски до следующей песни
        /// </summary>
        public event PlayerEvent? SkippedToNext;
        /// <summary>
        /// Вызывается при перемотки до предыдущей песни
        /// </summary>
        public event PlayerEvent? SkippedToPrevious;
        /// <summary>
        /// Вызывается при очистке экземпляра плеера
        /// </summary>
        public event DisposePlayer? Disposed;
        /// <summary>
        /// Вызывается при изменении текущей проигрываемой позиции
        /// </summary>
        public event PlayerTimeEvent? PositionChanged;

        /// <summary>
        /// Информация о текущем аудио
        /// </summary>
        public IMediaItem? Source { get; }
        /// <summary>
        /// Интервал обновления текущей позиции проигрывания аудиофайла
        /// </summary>
        public TimeSpan PositionUpdateInterval { get; set; }
        /// <summary>
        /// Размер захватываемых данных для визуализации. Значение должно быть результатом 2 в N степени.
        /// </summary>
        public int CaptureDataSize { get; set; }
        /// <summary>
        /// Настройки удалённого управления плеером
        /// </summary>
        public IPlayerRemoteControls RemoteControls { get; }

        #region Дополнительно

        /// <summary>
        /// Получить данные для визуализации аудиофайла.
        /// </summary>
        /// <param name="Size">Размер данных</param>
        /// <returns>Данные для визуализации аудиофайла</returns>
        public float[] GetWaveformData(int Size);

        #endregion

        #region Делегаты

        /// <summary>
        /// Очистить плеер
        /// </summary>
        /// <param name="Player">Плеер</param>
        public delegate void DisposePlayer(IPlayer Player);

        #endregion
    }
}
