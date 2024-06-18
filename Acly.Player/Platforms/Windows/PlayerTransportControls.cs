using System.Diagnostics;
using Windows.Media;
using Windows.Media.Playback;

namespace Acly.Player.Windows
{
    /// <summary>
    /// Класс для удалённого управление плеером Windows
    /// </summary>
    public class PlayerTransportControls : IDisposable
    {
        /// <summary>
        /// Создать новый экземпляр класса для удалённого управление плеером Windows
        /// </summary>
        /// <param name="Player">Плеер Windows</param>
        /// <param name="SystemPlayer"><see cref="MediaPlayer"/> Windows</param>
        /// <param name="Dispatcher">Диспатчер плеер</param>
        /// <exception cref="PlatformNotSupportedException"></exception>
        public PlayerTransportControls(IPlayer Player, MediaPlayer SystemPlayer, IDispatcher? Dispatcher)
        {
#if !WINDOWS
            throw new PlatformNotSupportedException("Доступно только для Windows");
#endif

            ArgumentNullException.ThrowIfNull(Player, nameof(Player));
            ArgumentNullException.ThrowIfNull(Dispatcher, nameof(Dispatcher));

            SystemPlayer.CommandManager.IsEnabled = false;

            _Dispatcher = Dispatcher;
            _SystemPlayer = SystemPlayer;
            _Controls = SystemPlayer.SystemMediaTransportControls;
            _Player = Player;
            _Controls.IsChannelDownEnabled = false;
            _Controls.IsChannelUpEnabled = false;
            _Controls.IsRewindEnabled = false;
            _Controls.IsRecordEnabled = false;
            _Controls.IsFastForwardEnabled = false;
            _Controls.IsPlayEnabled = true;
            _Controls.IsPauseEnabled = true;
            _Controls.IsStopEnabled = true;
            _Controls.IsNextEnabled = true;
            _Controls.IsPreviousEnabled = true;
            _Controls.IsEnabled = true;
            Updater.Type = MediaPlaybackType.Music;

            SystemPlayer.CurrentStateChanged += OnCurrentStateChanged;

            _Controls.ButtonPressed += OnButtonPressed;
            _Controls.AutoRepeatModeChangeRequested += OnAutoRepeatModeChangeRequested;
            _Controls.PlaybackPositionChangeRequested += OnPlaybackPositionChangeRequested;
            _Controls.PlaybackRateChangeRequested += OnPlaybackRateChangeRequested;
        }

        /// <summary>
        /// Вызывается при запросе пропуска до следующей песни
        /// </summary>
        public event Action? SkipToNextRequest;
        /// <summary>
        /// Вызывается при запросе перемотки до предыдущей песни
        /// </summary>
        public event Action? SkipToPreviousRequest;

        private readonly IDispatcher _Dispatcher;
        private readonly IPlayer _Player;
        private readonly MediaPlayer _SystemPlayer;
        private readonly SystemMediaTransportControls _Controls;
        private SystemMediaTransportControlsDisplayUpdater Updater => _Controls.DisplayUpdater;

        #region Управление

        /// <summary>
        /// Установить информацию о медиа
        /// </summary>
        /// <param name="Item"></param>
        public void SetMediaItem(IMediaItem Item)
        {
            Updater.MusicProperties.Title = Item.Title;
            Updater.MusicProperties.Artist = Item.Artist;
            Updater.Thumbnail = Item.GetImage();

            Updater.Update();
        } 

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public void Dispose()
        {
            _SystemPlayer.CurrentStateChanged -= OnCurrentStateChanged;

            _Controls.ButtonPressed -= OnButtonPressed;
            _Controls.AutoRepeatModeChangeRequested -= OnAutoRepeatModeChangeRequested;
            _Controls.PlaybackPositionChangeRequested -= OnPlaybackPositionChangeRequested;
            _Controls.PlaybackRateChangeRequested -= OnPlaybackRateChangeRequested;
        }

        #endregion

        #region События

        private void OnCurrentStateChanged(MediaPlayer sender, object args)
        {
            switch (sender.CurrentState)
            {
                case MediaPlayerState.Playing:
                    _Controls.PlaybackStatus = MediaPlaybackStatus.Playing;
                    break;
                case MediaPlayerState.Paused:
                    _Controls.PlaybackStatus = MediaPlaybackStatus.Paused;
                    break;
                case MediaPlayerState.Stopped:
                    _Controls.PlaybackStatus = MediaPlaybackStatus.Stopped;
                    break;
                case MediaPlayerState.Closed:
                    _Controls.PlaybackStatus = MediaPlaybackStatus.Closed;
                    break;
            }
        }

        private void OnPlaybackRateChangeRequested(SystemMediaTransportControls sender, PlaybackRateChangeRequestedEventArgs args)
        {
            _Player.Speed = (float)args.RequestedPlaybackRate;
        }
        private void OnPlaybackPositionChangeRequested(SystemMediaTransportControls sender, PlaybackPositionChangeRequestedEventArgs args)
        {
            _Player.Position = args.RequestedPlaybackPosition;
        }
        private void OnAutoRepeatModeChangeRequested(SystemMediaTransportControls sender, AutoRepeatModeChangeRequestedEventArgs args)
        {
            _Player.Loop = args.RequestedAutoRepeatMode != MediaPlaybackAutoRepeatMode.None;
        }
        private async void OnButtonPressed(SystemMediaTransportControls sender, SystemMediaTransportControlsButtonPressedEventArgs args)
        {
            await _Dispatcher.DispatchAsync(() =>
            {
                Debug.WriteLine(args.Button);

                if (args.Button == SystemMediaTransportControlsButton.Next)
                {
                    SkipToNextRequest?.Invoke();
                }
                else if (args.Button == SystemMediaTransportControlsButton.Previous)
                {
                    SkipToPreviousRequest?.Invoke();
                }
                else if (args.Button == SystemMediaTransportControlsButton.Pause)
                {
                    _Player.Pause();
                }
                else if (args.Button == SystemMediaTransportControlsButton.Play)
                {
                    _Player.Play();
                }
                else if (args.Button == SystemMediaTransportControlsButton.Stop)
                {
                    _Player.Stop();
                }
            });
        }

        #endregion
    }
}
