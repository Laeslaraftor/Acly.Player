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
        public event Action? SkippedToNext;
        /// <summary>
        /// Вызывается при перемотки до предыдущей песни
        /// </summary>
        public event Action? SkippedToPrevious;
        /// <summary>
        /// Вызывается при очистке экземпляра плеера
        /// </summary>
        public event DisposePlayer? Disposed;

        /// <summary>
        /// Информация о текущем аудио
        /// </summary>
        public IMediaItem? Source { get; }

        #region Управление

        /// <summary>
        /// Возобновить / пауза
        /// </summary>
        public void SwitchState();

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
