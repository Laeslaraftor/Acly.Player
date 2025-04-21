using System.ComponentModel;
using System.Windows.Input;

namespace Acly.Player
{
    /// <summary>
    /// Интерфейс удалённого управления плеером
    /// </summary>
    public interface IPlayerRemoteControls : INotifyPropertyChanged, IDisposable
    {
        /// <summary>
        /// Событие перемотки до следующего аудиофайла
        /// </summary>
        public event EventHandler? SkippedToNext;
        /// <summary>
        /// Событие перемотки до предыдущего аудиофайла
        /// </summary>
        public event EventHandler? SkippedToPrevious;
        /// <summary>
        /// Событие изменения доступности перемотки до следующего аудиофайла
        /// </summary>
        public event EventHandler<bool>? CanSkipToNextChanged;
        /// <summary>
        /// Событие изменения доступности перемотки до предыдущего аудиофайла
        /// </summary>
        public event EventHandler<bool>? CanSkipToPreviousChanged;

        /// <summary>
        /// Очищен ли экземпляр
        /// </summary>
        public bool IsDisposed { get; }
        /// <summary>
        /// Включено ли удалённое управление
        /// </summary>
        public bool IsEnabled { get; set; }
        /// <summary>
        /// Плеер к которому принадлежит текущий экземпляр удалённого управления
        /// </summary>
        public IPlayer Player { get; }
        /// <summary>
        /// Команда пропуска до следующего аудиофайла
        /// </summary>
        public ICommand? SkipToNextCommand { get; set; }
        /// <summary>
        /// Параметр команды пропуска до следующего аудиофайла (<see cref="SkipToNextCommandParameter"/>)
        /// </summary>
        public object? SkipToNextCommandParameter { get; set; }
        /// <summary>
        /// Команда перемотки до предыдущего аудиофайла
        /// </summary>
        public ICommand? SkipToPreviousCommand { get; set; }
        /// <summary>
        /// Параметр команды перемотки до предыдущего аудиофайла (<see cref="SkipToPreviousCommand"/>)
        /// </summary>
        public object? SkipToPreviousCommandParameter { get; set; }
        /// <summary>
        /// Можно ли пропустить до следующего аудиофайла
        /// </summary>
        public bool CanSkipToNext { get; set; }
        /// <summary>
        /// Можно ли перемотать до предыдущего аудиофайла
        /// </summary>
        public bool CanSkipToPrevious { get; set; }
    }
}
