#if ANDROID
using Android.Graphics;
using Android.Support.V4.Media;
using Acly.Requests;
using System.Diagnostics;
#elif WINDOWS
using Windows.Storage.Streams;
#endif

namespace Acly.Player
{
    /// <summary>
    /// Класс информации о медиа
    /// </summary>
    public class MediaItem : IMediaItem
    {
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public string Title { get; set; } = "Без названия";
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public string Artist { get; set; } = "Неизвестен";
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public TimeSpan Duration { get; set; } = TimeSpan.Zero;
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public string AudioUrl { get; set; } = "";
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public string? ImageUrl { get; set; }

#if ANDROID

        private Bitmap? _Image;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns><inheritdoc/></returns>
        public async Task<Bitmap?> GetImage()
        {
            if (ImageUrl == null)
            {
                return null;
            }
            if (_Image != null)
            {
                return _Image;
            }

            try
            {
                byte[] Data = await Ajax.GetBytes(ImageUrl);
                _Image = BitmapFactory.DecodeByteArray(Data, 0, Data.Length);

                return _Image;
            }
            catch (Exception Error)
            {
                Debug.WriteLine(Error.Message);
            }

            return null;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns><inheritdoc/></returns>
        public MediaMetadataCompat? ToMetadata()
        {
            return new MediaMetadataCompat.Builder()
                ?.PutString(MediaMetadataCompat.MetadataKeyTitle, Title)
                ?.PutString(MediaMetadataCompat.MetadataKeyArtist, Artist)
                ?.PutLong(MediaMetadataCompat.MetadataKeyDuration, Convert.ToInt64(Duration.TotalMilliseconds))
                ?.PutString(MediaMetadataCompat.MetadataKeyArtUri, AudioUrl)
                ?.Build();
        }

#elif WINDOWS

        private RandomAccessStreamReference? _Image;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns><inheritdoc/></returns>
        public RandomAccessStreamReference? GetImage()
        {
            if (ImageUrl == null)
            {
                return null;
            }
            if (_Image != null)
            {
                return _Image;
            }

            _Image = RandomAccessStreamReference.CreateFromUri(new(ImageUrl, UriKind.Absolute));

            return _Image;
        }

#endif

        private ImageSource? _ImageSource;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns><inheritdoc/></returns>
        public ImageSource? GetImageSource()
        {
            if (ImageUrl == null)
            {
                return null;
            }

            _ImageSource = ImageSource.FromUri(new(ImageUrl, UriKind.Absolute));

            return _ImageSource;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public void Dispose()
        {
#if WINDOWS || ANDROID
            _Image = null;
#endif
        }
    }
}
