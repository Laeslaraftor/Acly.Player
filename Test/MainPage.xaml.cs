using Acly.Player;
using System.Diagnostics;
using System.Timers;

namespace Test
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();

            _Current = this;

            _Player = new CrossPlayer()
            {
                AutoPlay = true
            };
            _Player.SourceChanged += OnSourceChanged;
            songTime.ValueChanged += SongTime_ValueChanged;

            _Player.SkippedToNext += _Player_SkippedToNext;
            _Player.SkippedToPrevious += _Player_SkippedToPrevious;
        }

        private async void _Player_SkippedToPrevious()
        {
            await Alert("То сё", "Назад", "Да");
        }

        private async void _Player_SkippedToNext()
        {
            await Alert("То сё", "Вперёд", "Да");
        }

        private static MainPage _Current;
        private IPlayer _Player;
        private string? url = null;

        private void Timer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            //try
            //{
            //    Dispatcher.Dispatch(() =>
            //    {
            //        songTime.Value = _Player.Position.TotalSeconds;
            //    });
            //}
            //catch (Exception Error)
            //{
            //    Debug.WriteLine(Error.Message);
            //}
        }
        private void SongTime_ValueChanged(object? sender, ValueChangedEventArgs e)
        {
            _Player.Position = TimeSpan.FromSeconds(songTime.Value);
        }
        private async void OnSourceChanged()
        {
            await Task.Delay(1000);

            try
            {
                Dispatcher.Dispatch(() =>
                {
                    songTime.Maximum = _Player.Duration.TotalSeconds;
                });
            }
            catch (Exception Error)
            {
                Debug.WriteLine(Error.Message);
            }
        }

        private void songControl_Clicked(object sender, EventArgs e)
        {
            if (url == songUrl.Text)
            {
                if (_Player.IsPlaying)
                {
                    _Player.Pause();
                }
                else
                {
                    _Player.Play();
                }

                return;
            }

            UpdateSource();
        }
        private async void UpdateSource()
        {
            url = songUrl.Text.Trim();
            string imageUrl = songImage.Text.Trim();

            await _Player.SetSource(new MediaItem
            {
                Title = songName.Text.Trim(),
                Artist = songArtist.Text.Trim(),
                AudioUrl = url,
                ImageUrl = imageUrl
            });

            songImagePreview.Source = ImageSource.FromUri(new(imageUrl, UriKind.Absolute));
        }

        private void ContentPage_Loaded(object sender, EventArgs e)
        {
            try
            {
                System.Timers.Timer timer = new(TimeSpan.FromSeconds(0.5))
                {
                    AutoReset = true,
                    Enabled = true
                };

                timer.Elapsed += Timer_Elapsed;
                timer.Start();
            }
            catch (Exception Error)
            {
                Debug.WriteLine(Error.Message);
            }
        }

        public static async Task Alert(string Title, string Text, string Button)
        {
            await _Current.DisplayAlert(Title, Text, Button);
        }
    }

}
