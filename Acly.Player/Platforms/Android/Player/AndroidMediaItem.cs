using Android.Graphics;
using System.ComponentModel;

namespace Acly.Player.Android
{
    public class AndroidMediaItem : IMediaItem, IDisposable
    {
        public AndroidMediaItem(IMediaItem Original)
        {
            this.Original = Original;

            Original.PropertyChanged += OnOriginalPropertyChanged;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public bool IsDisposed
        {
            get => _IsDisposed;
            private set
            {
                if (_IsDisposed != value)
                {
                    _IsDisposed = value;
                    OnOriginalPropertyChanged(this, new(nameof(IsDisposed)));
                }
            }
        }
        public IMediaItem Original { get; }
        public Bitmap? Image
        {
            get => _Image;
            private set
            {
                if (_Image != value)
                {
                    _Image = value;
                    OnOriginalPropertyChanged(this, new(nameof(Image)));
                }
            }
        }
        public string Title
        {
            get => Original.Title;
            set => Original.Title = value;
        }
        public string Artist
        {
            get => Original.Artist;
            set => Original.Artist = value;
        }
        public TimeSpan Duration
        {
            get => Original.Duration;
            set => Original.Duration = value;
        }
        public string AudioUrl
        {
            get => Original.AudioUrl;
            set => Original.AudioUrl = value;
        }
        public string? ImageUrl
        {
            get => Original.ImageUrl;
            set => Original.ImageUrl = value;
        }

        private bool _IsDisposed;
        private Bitmap? _Image;
        private string? _LastImageUrl;

        #region Управление

        public async void UpdateImage()
        {
            if (_LastImageUrl == ImageUrl || IsDisposed)
            {
                return;
            }
            if (ImageUrl == null)
            {
                _Image = null;
                _LastImageUrl = null;

                return;
            }

            Image?.Dispose();
            _LastImageUrl = ImageUrl;
            Image = await this.GetImage();
        }

        public void Dispose()
        {
            if (IsDisposed)
            {
                return;
            }

            IsDisposed = true;
            Image?.Dispose();
            Image = null;
            _LastImageUrl = null;

            GC.SuppressFinalize(this);
        }

        public ImageSource? GetImageSource()
        {
            return Original.GetImageSource();
        }

        #endregion

        #region События

        private void OnOriginalPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }

        #endregion
    }
}
