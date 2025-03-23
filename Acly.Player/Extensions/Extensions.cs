#if ANDROID
using Acly.Requests;
using Android.Graphics;
using Android.Support.V4.Media;
using System.Diagnostics;
#elif WINDOWS
using Acly.Player.Server;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;
using Windows.Storage;
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
        public static RandomAccessStreamReference? GetImage(this IMediaItem MediaItem, IPlayer Player)
        {
            if (MediaItem.ImageUrl == null)
            {
                ImagesServer.Start();
                RandomAccessStreamReference.CreateFromUri(ImagesServer.GetEmptyLink());
                return null;
            }

            Uri? FileUri = null;

            if (File.Exists(MediaItem.AudioUrl))
            {
                ImagesServer.Start();
                var ImageToken = ImagesServer.AddImage(Player, MediaItem.ImageUrl);
                FileUri = ImagesServer.GetLink(ImageToken);
            }

            FileUri ??= new(MediaItem.ImageUrl, UriKind.Absolute);

            return RandomAccessStreamReference.CreateFromUri(FileUri);
        }

        private static string GetFileName(this string FilePath)
        {
            FilePath = FilePath.Replace(@"\", "/");
            var Parts = FilePath.Split('/');

            if (Parts.Length > 0)
            {
                return Parts[^1];
            }

            return string.Empty;
        }

#endif
    }
}
