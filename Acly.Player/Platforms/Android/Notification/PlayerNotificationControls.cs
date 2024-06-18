using Android.Content;
using Android.Support.V4.Media.Session;
using Android.Views;

namespace Acly.Player.Android
{
    public static partial class PlayerNotification
    {
        public class Callback : MediaSessionCompat.Callback
        {
            private Callback()
            {
            }

            #region Callback

            public override void OnSeekTo(long pos)
            {
                SeekRequest?.Invoke(TimeSpan.FromMilliseconds(pos));
            }
            public override void OnPause()
            {
                PauseRequest?.Invoke();
            }
            public override void OnPlay()
            {
                PlayRequest?.Invoke();
            }

            public override bool OnMediaButtonEvent(Intent? mediaButtonEvent)
            {
                KeyEvent? Event = (KeyEvent?)mediaButtonEvent?.GetParcelableExtra(Intent.ExtraKeyEvent);

                if (Event?.Action == KeyEventActions.Up || Event == null)
                {
                    return false;
                }
                if (Event.KeyCode == Keycode.MediaPause)
                {
                    OnPause();
                }
                else if (Event.KeyCode == Keycode.MediaPlay)
                {
                    OnPlay();
                }
                else if (Event.KeyCode == Keycode.MediaNext)
                {
                    ;
                    SkipToNextRequest?.Invoke();
                }
                else if (Event.KeyCode == Keycode.MediaPrevious)
                {
                    SkipToPreviousRequest?.Invoke();
                }
                else if (Event.KeyCode == Keycode.MediaStop)
                {
                    StopRequest?.Invoke();
                }

                return false;
            }

            #endregion

            //--

            #region Статика

            public static readonly Callback Instance = new();

            #endregion
        }
    }
}