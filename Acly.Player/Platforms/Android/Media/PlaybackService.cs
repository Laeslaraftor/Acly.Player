using Android.Support.V4.Media.Session;
using AndroidX.Media.Session;
using MediaSession = Android.Media.Session.MediaSession;
using AndroidX.Core.App;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Content.PM;
using static Android.Text.Style.TtsSpan;

namespace Acly.Player.Android.Media
{
    [Service(ForegroundServiceType = ForegroundService.TypeMediaPlayback, Exported = false), IntentFilter(["androidx.media2.session.MediaSessionService"])]
    public class PlaybackService : AndroidX.Media2.Session.MediaSessionService
    {
        private static PlaybackService? _Current;
        private static IMediaItem? _LastItem;
        private static Bitmap? _LastItemImage;

        private MediaSession? _MediaSession;
        private MediaSessionCompat? _MediaCompat;

        #region Инициализация

        public override void OnCreate()
        {
            base.OnCreate();

            _Current = this;
            _MediaCompat = new(this, PlayerNotification.Settings.SessionTag);
            _MediaSession = (MediaSession?)_MediaCompat.MediaSession;

            _MediaCompat.SetFlags(MediaSessionCompat.FlagHandlesQueueCommands);
            _MediaCompat.SetCallback(PlayerNotification.CallbackInstance);

            _MediaSession.Active = false;
            _MediaCompat.Active = false;

            MediaButtonReceiver.HandleIntent(_MediaCompat, new(this, typeof(PlaybackService)));

            MediaBrowserImplementation.Current.SessionToken = _MediaCompat.SessionToken;

            StartForeground(PlayerNotification.Settings.Id, BuildNotification(BuildMediaNotification(_MediaCompat, null, PlayerNotification.FirstAvailablePlayer)));
        }
        public override void OnDestroy()
        {
            base.OnDestroy();

            _MediaSession?.Release();
            _MediaCompat?.Release();

            StopForeground(StopForegroundFlags.Remove);
        }

        #endregion

        #region Уведомление

        public static async void CreateNotification(IMediaItem? Item, ISimplePlayer Player)
        {
            if (_Current == null)
            {
                return;
            }

            bool HaveItem = Player.SourceSetted;

            _Current._MediaSession.Active = HaveItem;
            _Current._MediaCompat.Active = HaveItem;

            NotificationCompat.Builder? Notification = BuildMediaNotification(_Current._MediaCompat, Item, Player);

            if (Item == null || Item?.ImageUrl == null)
            {
                BuildNotification(Notification);
                return;
            }

            Bitmap? Image = _LastItemImage;

            if (_LastItem != Item)
            {
                Image = await Item.GetImage();
            }
            if (Image != null)
            {
                Notification?.SetLargeIcon(Image);
            }

            BuildNotification(Notification);

            _LastItem = Item;
            _LastItemImage = Image;
        }
        private static void CreateChannel()
        {
            NotificationChannel Channel = new(PlayerNotification.Settings.ChannelId, PlayerNotification.Settings.ChannelName, NotificationImportance.High);
            Channel.Description = PlayerNotification.Settings.ChannelDescription;

            if (PlayerNotification.Activity != null)
            {
                NotificationManager? Manager = NotificationManager.FromContext(PlayerNotification.Activity);
                Manager.CreateNotificationChannel(Channel);
            }
        }

        private static void ApplyCustomActions(NotificationCompat.Builder Builder, int State)
        {
            PendingIntent? Stop = CreatePendingIntent(PlaybackStateCompat.ActionStop);
            PendingIntent? PlayPause = CreatePendingIntent(PlaybackStateCompat.ActionPlayPause);
            PendingIntent? Previous = CreatePendingIntent(PlaybackStateCompat.ActionSkipToPrevious);
            PendingIntent? Next = CreatePendingIntent(PlaybackStateCompat.ActionSkipToNext);

            Builder.AddAction(PlayerNotification.Style.StopIcon, PlayerNotification.Style.StopActionName, Stop);
            Builder.AddAction(PlayerNotification.Style.BackwardIcon, PlayerNotification.Style.BackwardActionName, Previous);

            if (State == PlaybackStateCompat.StatePlaying)
            {
                Builder.AddAction(PlayerNotification.Style.PauseIcon, PlayerNotification.Style.PauseActionName, PlayPause);
            }
            else
            {
                Builder.AddAction(PlayerNotification.Style.PlayIcon, PlayerNotification.Style.PlayActionName, PlayPause);
            }

            Builder.AddAction(PlayerNotification.Style.ForwardIcon, PlayerNotification.Style.ForwardActionName, Next);
        }

        private static NotificationCompat.Builder? BuildMediaNotification(MediaSessionCompat MediaCompat, IMediaItem? Item, ISimplePlayer? Player)
        {
            if (MediaCompat == null || PlayerNotification.Activity == null)
            {
                return null;
            }

            CreateChannel();

            int PlayerState = PlayerStateToInt(Player);
            PlaybackStateCompat.Builder StateBuilder = new();
            AndroidX.Media.App.NotificationCompat.MediaStyle Style = new();
            NotificationCompat.Builder Notification = new(PlayerNotification.Activity, PlayerNotification.Settings.ChannelId);

            Intent MainIntent = new(PlayerNotification.Activity, PlayerNotification.Activity.GetType());
            MainIntent.SetFlags(ActivityFlags.ClearTop | ActivityFlags.SingleTop);
            PendingIntent? MainPending = PendingIntent.GetActivity(PlayerNotification.Activity, 0, MainIntent, PendingIntentFlags.Immutable);

            long Actions = PlaybackStateCompat.ActionSeekTo
                | PlaybackStateCompat.ActionPlayPause
                | PlaybackStateCompat.ActionSkipToPrevious
                | PlaybackStateCompat.ActionSkipToNext
                | PlaybackStateCompat.ActionRewind
                | PlaybackStateCompat.ActionFastForward;
            long PlayerPosition = 0;
            float PlayerSpeed = 1;

            if (Player != null)
            {
                PlayerPosition = Convert.ToInt64(Player.Position.TotalMilliseconds);
                PlayerSpeed = Player.Speed;
            }

            StateBuilder.SetState(PlayerState, PlayerPosition, PlayerSpeed);
            StateBuilder.SetActions(Actions);
            StateBuilder.SetActiveQueueItemId(1);
            StateBuilder.SetBufferedPosition(100);

            ApplyCustomActions(Notification, PlayerState);

            if (Item != null)
            {
                MediaCompat.SetMetadata(Item.ToMetadata());
            }

            MediaCompat.SetPlaybackState(StateBuilder.Build());
            Style.SetMediaSession(MediaCompat.SessionToken);
            Style.SetShowActionsInCompactView(1, 2, 3);
            Style.SetShowCancelButton(true);

            Notification.SetStyle(Style);
            Notification.SetUsesChronometer(true);
            Notification.SetSmallIcon(PlayerNotification.Style.ApplicationIcon);
            Notification.SetWhen(0);
            Notification.SetOngoing(true);
            Notification.SetContentIntent(MainPending);

            return Notification;
        }
        private static Notification? BuildNotification(NotificationCompat.Builder? Builder)
        {
            if (Builder == null)
            {
                return null;
            }

            Notification Result = Builder.Build();
            _Current?.StartForeground(PlayerNotification.Settings.Id, Result);

            return Result;
        }

        private static int PlayerStateToInt(ISimplePlayer? Player)
        {
            if (Player?.State == SimplePlayerState.Stopped || Player == null)
            {
                return PlaybackStateCompat.StateStopped;
            }
            else if (Player.State == SimplePlayerState.Playing)
            {
                return PlaybackStateCompat.StatePlaying;
            }

            return PlaybackStateCompat.StatePaused;
        }
        private static PendingIntent? CreatePendingIntent(long Action)
        {
            return MediaButtonReceiver.BuildMediaButtonPendingIntent(PlayerNotification.Activity, Action);
        }

        #endregion

        #region Другое

        public override AndroidX.Media2.Session.MediaSession? OnGetSession(AndroidX.Media2.Session.MediaSession.ControllerInfo p0)
        {
            return null;
        }

        #endregion
    }
}
