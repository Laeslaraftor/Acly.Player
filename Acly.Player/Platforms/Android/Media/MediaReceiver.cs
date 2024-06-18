using Android.App;
using Android.Content;
using AndroidX.Media.Session;

namespace Acly.Player.Android.Media
{
    [BroadcastReceiver(Exported = true), IntentFilter([Intent.ActionMediaButton])]
    public class MediaReceiver : MediaButtonReceiver
    {
    }
}
