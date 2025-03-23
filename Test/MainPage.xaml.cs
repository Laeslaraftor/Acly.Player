
using Acly.Player;
using System.Diagnostics;

namespace Test
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            _Current = this;
        }

        private readonly List<AudioInfo> _Presets = [
            new() {
                Title = "Deja Vu",
                Artist = "Yves V, INNA, Janieck",
                AudioUrl = "https://acly.ru/resourse/user-music/e9e53e45dd01dfecebed2ee730c26aa2Yves-V,-INNA,-Janieck-Deja-Vu.mp3",
                ImageUrl = "https://e-cdns-images.dzcdn.net/images/cover/02cb3d9fb950ecd7f2023e9d4ab04864/500x500-000000-80-0-0.jpg"
            },
            new() {
                Title = "Poison",
                Artist = "Bhaskar",
                AudioUrl = "https://acly.ru/resourse/user-music/1d4b3d17657dd1641683c6c542e1be88Bhaskar-Poison.mp3",
                ImageUrl = "https://cdns-images.dzcdn.net/images/cover/baaf513b65d140007c30b4ed59140d1e/500x500-000000-80-0-0.jpg"
            },
            new() {
                Title = "Not Gonna Get Us",
                Artist = "Harddope, Nito-Onna",
                AudioUrl = "https://acly.ru/resourse/user-music/770b59d4614d4f19b93295d87afc296dHarddope,-Nito-Onna-Not-Gonna-Get-Us.mp3",
                ImageUrl = "https://e-cdns-images.dzcdn.net/images/cover/bee450fc529e5ecff9f5a9ea9baa6cc6/500x500-000000-80-0-0.jpg"
            },
            new() {
                Title = "Castle",
                Artist = "Clarx, Harddope",
                AudioUrl = "https://acly.ru/resourse/user-music/0e6791e757d2046d0fcd81ea7fb42d32Clarx,-Harddope-Castle.mp3",
                ImageUrl = "https://cdns-images.dzcdn.net/images/cover/c19ce14b22aaa8784b53cb0554278eb9/500x500-000000-80-0-0.jpg"
            },
            new() {
                Title = "Living Life, in the Night",
                Artist = "Neptunica, Matthew Clanton",
                AudioUrl = @"C:\Users\Mdely\OneDrive\Documents\Openbeat\Songs\cnHxQYSgpkX5\audio.ogg",
                ImageUrl = @"C:\Users\Mdely\OneDrive\Documents\Openbeat\Songs\cnHxQYSgpkX5\cover.jpg"
            },
            new()
        ];

        #region Список

        private AudioInfo? FindInfoByFullName(string FullName)
        {
            foreach (var Info in _Presets)
            {
                if (Info.FullName == FullName)
                {
                    return Info;
                }
            }

            return null;
        }

        private void AddPlayer(string Preset)
        {
            AudioInfo? Info = FindInfoByFullName(Preset);
            Info ??= new();

            MiniPlayer Player = new(Info);
            Player.Removed += OnPlayerRemoved;

            PlayersList.Children.Add(Player);
        }

        #endregion

        #region События

        private void OnAddButtonClicked(object sender, EventArgs e)
        {
            PresetsBlock.IsVisible = true;
            PresetsBlockList.ItemsSource = _Presets;
        }
        private void OnPresetButtonClicked(object sender, EventArgs e)
        {
            AddPlayer(((Button)sender).Text);
            OnClosePresetsBlockButtonClicked(sender, e);
        }
        private void OnClosePresetsBlockButtonClicked(object sender, EventArgs e)
        {
            PresetsBlock.IsVisible = false;
        }

        private void OnPlayerRemoved(MiniPlayer Player)
        {
            Player.Removed += OnPlayerRemoved;
            PlayersList.Children.Remove(Player);
        }

        #endregion

        #region Статика

        private static MainPage? _Current;

        public static async void Alert(string Title, string Text, string ButtonText)
        {
            if (_Current == null)
            {
                return;
            }

            await _Current.DisplayAlert(Title, Text, ButtonText);
        }

        #endregion
    }

}
