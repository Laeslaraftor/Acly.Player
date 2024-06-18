#if ANDROID
using Android.Graphics;
using Android.Support.V4.Media;
#elif WINDOWS
using Windows.Storage.Streams;
#endif

namespace Acly.Player
{
    /// <summary>
    /// Интерфейс медиа предмета
    /// </summary>
    public interface IMediaItem : IDisposable
    {
        /// <summary>
        /// Название
        /// </summary>
        public string Title { get; }
        /// <summary>
        /// Исполнитель
        /// </summary>
        public string Artist { get; }
        /// <summary>
        /// Продолжительность аудиофайла
        /// </summary>
        public TimeSpan Duration { get; set; }
        /// <summary>
        /// Адрес или путь к аудиофайлу
        /// </summary>
        public string AudioUrl { get; }
        /// <summary>
        /// Обложка (необязательно)
        /// </summary>
        public string? ImageUrl { get; }

#if ANDROID
        /// <summary>
        /// Получить обложку
        /// </summary>
        /// <returns>Картинка обложки</returns>
        public Task<Bitmap?> GetImage();
        /// <summary>
        /// Конвертировать в метаданные
        /// </summary>
        /// <returns>Метаданные</returns>
        public MediaMetadataCompat? ToMetadata();
#elif WINDOWS
        /// <summary>
        /// Получить обложку
        /// </summary>
        /// <returns>Картинка обложки</returns>
        public RandomAccessStreamReference? GetImage();
#endif

        /// <summary>
        /// Получить картинку обложки
        /// </summary>
        /// <returns>Картинка обложки</returns>
        public ImageSource? GetImageSource();
    }
}
