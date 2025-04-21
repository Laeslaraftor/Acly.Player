using Acly.Player.Android.Media;
using Android;
using Android.Content;
using AndroidX.Core.Content;
using Android.Content.PM;

namespace Acly.Player.Android
{
    public static partial class PlayerNotification
    {
        public static event Action? PauseRequest;
        public static event Action? PlayRequest;
        public static event Action? StopRequest;
        public static event Action? SkipToNextRequest;
        public static event Action? SkipToPreviousRequest;
        public static event SeekTo? SeekRequest;

        public static MauiAppCompatActivity? Activity { get; private set; }
        public static CrossPlayer? FirstAvailablePlayer
        {
            get
            {
                if (_Players.Count == 0)
                {
                    return null;
                }

                return _Players[0];
            }
        }
        public static IPlayer? LastPlayerNotification { get; private set; }
        public static int PlayersCount => _Players.Count;

        public static PlayerNotificationStyle Style { get; private set; } = new();
        public static PlayerNotificationSettings Settings { get; private set; } = new();
        public static bool ServicesStarted { get; private set; }
        public static Callback CallbackInstance => Callback.Instance; 

        private static Intent? _BrowserService;
        private static readonly List<CrossPlayer> _Players = [];
        private static bool _Initialized;

        #region Установка

        public static void Init(MauiAppCompatActivity Activity, PlayerNotificationStyle Style, PlayerNotificationSettings Settings)
        {
            if (_Initialized)
            {
                throw new InvalidOperationException("Уведомление уже инициализировано");
            }

            ArgumentNullException.ThrowIfNull(Style, nameof(Style));
            ArgumentNullException.ThrowIfNull(Settings, nameof(Settings));

            _Initialized = true;

            PlayerNotification.Activity = Activity;
            PlayerNotification.Style = Style;
            PlayerNotification.Settings = Settings;
        }

        #endregion

        #region Управление

        public static void AddPlayer(CrossPlayer Player)
        {
            _Players.Add(Player);
            Player.Disposed += OnPlayerDisposed;
        }

        #endregion

        #region Уведомление

        public static void Update(IPlayer Player, AndroidMediaItem? Item, bool CanSkipToNext, bool CanSkipToPrevious)
        {
            if (!ServicesStarted)
            {
                if (Activity != null)
                {
                    StartServices(Activity);
                }

                return;
            }

            PlaybackService.CreateNotification(Item, Player, CanSkipToNext, CanSkipToPrevious);
            LastPlayerNotification = Player;
        }

        #endregion

        #region Сервисы

        private static void StartServices(MauiAppCompatActivity Activity)
        {
            if (ServicesStarted)
            {
                return;
            }
            if (ContextCompat.CheckSelfPermission(Activity, Manifest.Permission.ForegroundService) == Permission.Denied)
            {
                throw new PermissionNotGrantedException(Manifest.Permission.ForegroundService);
            }

            ServicesStarted = true;
            _BrowserService = new(Activity, typeof(MediaBrowserImplementation));

            Activity.StartService(_BrowserService);
        }
        public static void Stop()
        {
            if (!ServicesStarted || Activity == null)
            {
                return;
            }

            Activity.StopService(_BrowserService);

            ServicesStarted = false;
            _BrowserService = null;
            LastPlayerNotification = null;
        }

        #endregion

        #region Делегаты

        public delegate void SeekTo(TimeSpan Position);

        #endregion

        #region События

        private static void OnPlayerDisposed(IPlayer Player)
        {
            _Players.Remove((CrossPlayer)Player);
            Player.Disposed -= OnPlayerDisposed;

            Stop();
        }

        #endregion
    }
}
