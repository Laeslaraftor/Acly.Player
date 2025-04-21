using System.ComponentModel;

namespace Acly.Player
{
    /// <summary>
    /// Класс информации о медиа
    /// </summary>
    public partial class MediaItem : IMediaItem
    {
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public string Title
        {
            get => _Title;
            set
            {
                if (_Title != value)
                {
                    _Title = value;
                    InvokePropertyChanged(nameof(value));
                }
            }
        }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public string Artist
        {
            get => _Artist;
            set
            {
                if (_Artist != value)
                {
                    _Artist = value;
                    InvokePropertyChanged(nameof(Artist));
                }
            }
        }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public TimeSpan Duration
        {
            get => _Duration;
            set
            {
                if (_Duration != value)
                {
                    _Duration = value;
                    InvokePropertyChanged(nameof(Duration));
                }
            }
        }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public string AudioUrl
        {
            get => _AudioUrl;
            set
            {
                if (_AudioUrl != value)
                {
                    _AudioUrl = value;
                    InvokePropertyChanged(nameof(AudioUrl));
                }
            }
        }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public string? ImageUrl
        {
            get => _ImageUrl;
            set
            {
                if (_ImageUrl != value)
                {
                    _ImageUrl = value;
                    InvokePropertyChanged(nameof(ImageUrl));
                }
            }
        }

        private string _Title = DefaultTitle;
        private string _Artist = DefaultTitle;
        private TimeSpan _Duration;
        private string _AudioUrl = string.Empty;
        private string? _ImageUrl;

        #region Управление

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

            return ImageUrl;
        }

        #endregion

        #region События

        /// <summary>
        /// Вызвать событие изменения поля
        /// </summary>
        /// <param name="PropertyName">Название изменённого поля</param>
        protected void InvokePropertyChanged(string PropertyName)
        {
            PropertyChanged?.Invoke(this, new(PropertyName));
        }

        /// <summary>
        /// <inheritdoc cref="PropertyChangedEventHandler"/>
        /// </summary>
        /// <param name="sender"><inheritdoc cref="PropertyChangedEventHandler"/></param>
        /// <param name="e"><inheritdoc cref="PropertyChangedEventHandler"/></param>
        protected virtual void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
        }

        #endregion

        #region Константы

        /// <summary>
        /// Значение названия аудиоэлемента по умолчанию
        /// </summary>
        public const string DefaultTitle = "Без названия";
        /// <summary>
        /// Значение артиста аудиоэлемента по умолчанию
        /// </summary>
        public const string DefaultArtist = "Неизвестен";

        #endregion
    }
}
