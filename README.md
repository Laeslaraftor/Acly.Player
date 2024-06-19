# Acly.Player
**Плеер для Android и Windows с возможностью удалённого управления**

![Windows](https://raw.githubusercontent.com/Laeslaraftor/Acly.Player/master/Acly.Player/Preview/Win.jpg)
![Android](https://raw.githubusercontent.com/Laeslaraftor/Acly.Player/master/Acly.Player/Preview/Android.jpg)

Чтобы получить экзепляр плеера необходимо написать следующее:

```c#
CrossPlayer player = new CrossPlayer();
IPlayer player = new CrossPlayer(); //альтернативный вариант
```

# Настройка для Android

Для того чтобы управлять плеером через уведомление надо сначала инициализировать MediaSessionService и MediaBrowserService.
Звучит сложно, но для этого надо всего лишь вызвать 1 метод:

```c#
CrossPlayer.InitNotification(MauiAppCompatActivity);
```

Лучше всего его вызывать в методе `OnCreate(Bundle?)` класса `MainActivity` (или любого другого унаследнованного от `MauiAppCompatActivity`)
Для примера:

```c#
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        CrossPlayer.InitNotification(this);
    }
}
```

Так же этот метот имеет перегрузку, с помощью которой можно настроить уведомление под себя. 

```c#
CrossPlayer.InitNotification(MauiAppCompatActivity, PlayerNotificationStyle, PlayerNotificationSettings);
```

В `PlayerNotificationStyle` передаются настройки внешнего вида уведомления - иконки и надписи, а в `PlayerNotificationSettings` общие настройки.
В общих настройках указываются параметры канала, создаваемого для уведомления, идентификатор уведомления и тег сессии.

# Настройка для Windows

Настройка для Windows не требуется

# Использование

Плеер предлагает следующие события:

```c#
event Action<SimplePlayerState>? StateChanged; // Вызывается при изменении состояния плеера
event Action? SourceChanged; // Вызывается при изменении источника
event Action? SourceEnded; // Вызывается при окончании источника
event Action<TimeSpan>? PositionChanged; // Вызывается при изменении текущего времени проигрывания
event Action? SkippedToNext; // Вызывается при пропуске до следующей песни
event Action? SkippedToPrevious; // Вызывается при перемотки до предыдущей песни
event IPlayer.DisposePlayer? Disposed; // Вызывается при очистке
```

Поля:

```c#
TimeSpan Position { get; set; } // Текущее время проигрывания
TimeSpan Duration { get; } // Продолжительность аудио
float Speed { get; set; } // Скорость проигрывания
float Volume { get; set; } // Громкость плеера
bool Loop { get; set; } // Повторение аудио
bool IsPlaying { get; } // Проигрывается ли сейчас аудио
bool SourceSetted { get; } // Установлено ли аудио
bool AutoPlay { get; set; } // Автоматически проигрывать после смены аудио
SimplePlayerState State { get; } // Состояние плеера
```

Управление плеером:

```c#
void Pause(); // Пауза
void Play(); // Воспроизвести
void Stop(); // Остановить
void SwitchState(); // Пауза / воспроизести

void Release(); // Очистить
```

# Установка аудио

Для установки аудио доступны следующие методы:

```c#
Task SetSource(byte[] Data);
Task SetSource(Stream SourceStream);
Task SetSource(string SourceUrl);
Task SetSource(IMediaItem Item);
```

Последний метод устанавливает источник аудио и данные о медиа, которые будут отображаться при удалённом управлении плеером:

```c#
public interface IMediaItem
{
    public string Title { get; set; }
    public string Artist { get; set; }
    public TimeSpan Duration { get; set; }
    public string AudioUrl { get; set; }
    public string? ImageUrl { get; set; }
}
```