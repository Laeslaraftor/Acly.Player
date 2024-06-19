#if ANDROID
using Acly.Requests;
using Android.Graphics;
using Android.Support.V4.Media;
using System.Diagnostics;
#elif WINDOWS
using Windows.Storage.Streams;
#endif

namespace Acly.Player
{
    /// <summary>
    /// Класс с методами расширения для <see cref="IMediaItem"/>
    /// </summary>
    public static class Extensions
    {
#if ANDROID

        /// <summary>
        /// Получить обложку
        /// </summary>
        /// <returns>Картинка обложки</returns>
        public static async Task<Bitmap?> GetImage(this IMediaItem MediaItem)
        {
            if (MediaItem.ImageUrl == null)
            {
                return null;
            }

            try
            {
                byte[] Data = await Ajax.GetBytes(MediaItem.ImageUrl);
                return BitmapFactory.DecodeByteArray(Data, 0, Data.Length);
            }
            catch (Exception Error)
            {
                Debug.WriteLine(Error.Message);
            }

            return null;
        }

        /// <summary>
        /// Конвертировать в метаданные
        /// </summary>
        /// <returns>Метаданные</returns>
        public static MediaMetadataCompat? ToMetadata(this IMediaItem MediaItem)
        {
            return new MediaMetadataCompat.Builder()
                ?.PutString(MediaMetadataCompat.MetadataKeyTitle, MediaItem.Title)
                ?.PutString(MediaMetadataCompat.MetadataKeyArtist, MediaItem.Artist)
                ?.PutLong(MediaMetadataCompat.MetadataKeyDuration, Convert.ToInt64(MediaItem.Duration.TotalMilliseconds))
                ?.PutString(MediaMetadataCompat.MetadataKeyArtUri, MediaItem.AudioUrl)
                ?.Build();
        }

#elif WINDOWS

        /// <summary>
        /// Получить обложку
        /// </summary>
        /// <returns>Картинка обложки</returns>
        public static RandomAccessStreamReference? GetImage(this IMediaItem MediaItem)
        {
            if (MediaItem.ImageUrl == null)
            {
                return null;
            }

            return RandomAccessStreamReference.CreateFromUri(new(MediaItem.ImageUrl, UriKind.Absolute));
        }

#endif
    }
}
