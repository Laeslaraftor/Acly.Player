using Acly.Player;
#if WINDOWS
using Acly.Player.Windows;
#endif

namespace Test;

public partial class MiniPlayer : ContentView
{
	public MiniPlayer()
	{
		InitializeComponent();
    }
    public MiniPlayer(IMediaItem Item) : this()
    {
        InputBlock.BindingContext = Item;
    }

    public event Action<MiniPlayer>? Removed;

    private CrossPlayer? _Player;
#if WINDOWS
    private SpectrumDrawable _SpectrumDrawable = new();
#endif

    #region Плеер

    private async void InitPlayer(AudioInfo Item)
    {
        if (_Player != null)
        {
            return;
        }

        InputBlock.IsVisible = false;
        PlayerBlock.IsVisible = true;
        PlayerBlock.BindingContext = Item;

        _Player = new()
        {
            AutoPlay = true
        };
        _Player.PositionChanged += OnPositionChanged;
        _Player.StateChanged += OnStateChanged;
        _Player.SkippedToNext += OnSkippedToNext;
        _Player.SkippedToPrevious += OnSkippedToPrevious;
        RemoteControlsEnabledCheckbox.IsChecked = _Player.RemoteControls.IsEnabled;

        await _Player.SetSource(Item);
    }
    private void RemovePlayer()
    {
        if (_Player == null)
        {
            return;
        }

        InputBlock.IsVisible = true;
        PlayerBlock.IsVisible = false;

        _Player.PositionChanged -= OnPositionChanged;
        _Player.StateChanged -= OnStateChanged;
        _Player.SkippedToNext -= OnSkippedToNext;
        _Player.SkippedToPrevious -= OnSkippedToPrevious;
        _Player.Release();
        _Player = null;
    }

    #endregion

    #region События

    private void OnRemoveButtonClicked(object sender, EventArgs e)
    {
        RemovePlayer();
        Removed?.Invoke(this);
    }
    private void OnStartButtonClicked(object sender, EventArgs e)
    {
        AudioInfo Item = new()
        {
            AudioUrl = InputBlockAudio.Text.Trim()
        };
        string Title = InputBlockTitle.Text.Trim();
        string Artist = InputBlockArtist.Text.Trim();
        string? Image = InputBlockImage.Text?.Trim();

        if (Title.Length > 0)
        {
            Item.Title = Title;
        }
        if (Artist.Length > 0)
        {
            Item.Artist = Artist;
        }
        if (Image != null && Image.Length > 0)
        {
            Item.ImageUrl = Image;
        }

        InitPlayer(Item);
    }

    private void OnClearButtonClicked(object sender, EventArgs e)
    {
        RemovePlayer();
    }

    private void OnSkipToPreviousButtonClicked(object sender, EventArgs e)
    {
        OnSkippedToPrevious(_Player);
    }
    private void OnSkipToNextButtonClicked(object sender, EventArgs e)
    {
        OnSkippedToNext(_Player);
    }
    private void OnChangeStateButtonClicked(object sender, EventArgs e)
    {
        if (_Player.IsPlaying)
        {
            _Player.Pause();
            return;
        }

        _Player.Play();
    }

    #endregion

    #region События плеера

    private void OnSkippedToPrevious(IPlayer Player)
    {
        MainPage.Alert("Фокус", "Назад", "Замётано");
    }
    private void OnSkippedToNext(IPlayer Player)
    {
        MainPage.Alert("Фокус", "Далее", "Замётано");
    }

    private void OnStateChanged(ISimplePlayer Player, SimplePlayerState State)
    {
        Dispatcher.Dispatch(() =>
        {
            if (State == SimplePlayerState.Playing)
            {
                PlayerBlockSwitchStateBtn.Text = "Пауза";
            }
            else if (State == SimplePlayerState.Paused)
            {
                PlayerBlockSwitchStateBtn.Text = "Воспроизвести";
            }
            else
            {
                PlayerBlockSwitchStateBtn.Text = "Остановлено";
            }
        });
    }
    private void OnPositionChanged(IPlayer Player, TimeSpan Position)
    {
#pragma warning disable CS8602 // Разыменование вероятной пустой ссылки.
        TimeSpan Duration = _Player.Duration;
#pragma warning restore CS8602

        PlayerBlockPosition.Text = Position.ToStringFormat();
        PlayerBlockDuration.Text = Duration.ToStringFormat();
        PlayerBlockPositionSlider.Maximum = Duration.TotalSeconds;
        PlayerBlockPositionSliderUser.Maximum = Duration.TotalSeconds;
        PlayerBlockPositionSlider.Value = Position.TotalSeconds;

#if WINDOWS
        _SpectrumDrawable.Fft = _Player.GetSpectrumData(6);
        Visualizer.Drawable = null;
        Visualizer.Drawable = _SpectrumDrawable;
#endif
    }
    private void OnPlayerPositionSliderValueChanged(object sender, ValueChangedEventArgs e)
    {
        if (_Player == null)
        {
            return;
        }

        TimeSpan Position = TimeSpan.FromSeconds(e.NewValue);
        _Player.Position = Position;
    }

    private void OnCheckBoxCheckedPreviousChanged(object sender, CheckedChangedEventArgs e)
    {
        if (_Player != null)
        {
            _Player.RemoteControls.CanSkipToPrevious = e.Value;
        }
    }
    private void OnCheckBoxCheckedNextChanged(object sender, CheckedChangedEventArgs e)
    {
        if (_Player != null)
        {
            _Player.RemoteControls.CanSkipToNext = e.Value;
        }
    }
    private void OnCheckBoxIsEnabledCheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        if (_Player != null)
        {
            _Player.RemoteControls.IsEnabled = e.Value;
        }
    }

    #endregion
}