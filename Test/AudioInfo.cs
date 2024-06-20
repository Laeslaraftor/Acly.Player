using Acly.Player;

namespace Test
{
    public class AudioInfo : MediaItem
    {
        public string FullName => Artist + " - " + Title;
        public ImageSource? Image => GetImageSource();
    }
}
