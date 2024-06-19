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
    }
}
