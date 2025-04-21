using System.ComponentModel;

namespace Acly.Player
{
    /// <summary>
    /// Интерфейс медиа предмета
    /// </summary>
    public interface IMediaItem : INotifyPropertyChanged
    {
        /// <summary>
        /// Название
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// Исполнитель
        /// </summary>
        public string Artist { get; set; }
        /// <summary>
        /// Продолжительность аудиофайла
        /// </summary>
        public TimeSpan Duration { get; set; }
        /// <summary>
        /// Адрес или путь к аудиофайлу
        /// </summary>
        public string AudioUrl { get; set; }
        /// <summary>
        /// Обложка (необязательно)
        /// </summary>
        public string? ImageUrl { get; set; }

        /// <summary>
        /// Получить картинку обложки
        /// </summary>
        /// <returns>Картинка обложки</returns>
        public ImageSource? GetImageSource();
    }
}
