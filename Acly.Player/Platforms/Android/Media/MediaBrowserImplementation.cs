using Android.Content;
using Android.OS;
using Android.Support.V4.Media;
using AndroidX.Media;
using Android.App;
using Java.Util;

namespace Acly.Player.Android.Media
{
    [Service(Exported = true), IntentFilter(["android.media.browse.MediaBrowserService"])]
    public class MediaBrowserImplementation : MediaBrowserServiceCompat
    {
        public static MediaBrowserImplementation? Current { get; private set; }

        private static Intent? _PlaybackService;

        public override void OnCreate()
        {
            base.OnCreate();

            Current = this;
            _PlaybackService = new(this, typeof(PlaybackService));

            StartForegroundService(_PlaybackService);
        }
        public override void OnDestroy()
        {
            base.OnDestroy();

            StopForeground(StopForegroundFlags.Remove);
            Current = null;

            if (_PlaybackService != null)
            {
                StopService(_PlaybackService);
            }

            _PlaybackService = null;
        }

        #region Неважности

        public override BrowserRoot? OnGetRoot(string clientPackageName, int clientUid, Bundle? rootHints)
        {
            return new("root", null);
        }
        public override void OnLoadChildren(string parentId, Result result)
        {
            ArrayList list = new();
            MediaBrowserCompat.MediaItem item = new(new MediaDescriptionCompat.Builder()
                    .SetMediaId("mediaId")
                    .SetDescription("descriptions")
                    .SetTitle("title")
                    .SetSubtitle("subtitle")
                    .Build(), MediaBrowserCompat.MediaItem.FlagPlayable);

            list.Add(item);
            result.SendResult(list);
        }

        #endregion
    }
}
