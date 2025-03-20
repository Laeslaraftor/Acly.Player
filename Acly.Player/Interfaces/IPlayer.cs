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

        #region Делегаты

        /// <summary>
        /// Очистить плеер
        /// </summary>
        /// <param name="Player">Плеер</param>
        public delegate void DisposePlayer(IPlayer Player);

        #endregion
    }
}
