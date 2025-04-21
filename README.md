# Acly.Player
**Плеер для Android и Windows с возможностью удалённого управления**

![Windows](https://raw.githubusercontent.com/Laeslaraftor/Acly.Player/master/Acly.Player/Preview/Win.jpg)
![Android](https://raw.githubusercontent.com/Laeslaraftor/Acly.Player/master/Acly.Player/Preview/Android.jpg)

Чтобы получить экземпляр плеера необходимо написать следующее:

```c#
CrossPlayer player = new CrossPlayer();
// или
IPlayer player = new CrossPlayer();
```

# Настройка для Android

Для того чтобы управлять плеером через уведомление надо сначала инициализировать MediaSessionService и MediaBrowserService.
Звучит сложно, но для этого надо всего лишь вызвать 1 метод:

```c#
CrossPlayer.InitNotification(MauiAppCompatActivity);
```

Лучше всего его вызывать в методе `OnCreate(Bundle?)` класса `MainActivity` (или любого другого унаследованного от `MauiAppCompatActivity`)
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

Так же этот метод имеет перегрузку, с помощью которой можно настроить уведомление под себя. 

```c#
CrossPlayer.InitNotification(MauiAppCompatActivity, PlayerNotificationStyle, PlayerNotificationSettings);
```

В `PlayerNotificationStyle` передаются настройки внешнего вида уведомления - иконки и надписи, а в `PlayerNotificationSettings` общие настройки.
В общих настройках указываются параметры канала, создаваемого для уведомления, идентификатор уведомления и тег сессии.

Для работы методов `GetSpectrumData` и `GetWaveformData` на Android необходимо дать некоторые разрешения. 
Если вам эти методы не нужны, то можно не запрашивать разрешения. 
В любом случае, если вы попытаетесь вызвать эти методы, не дав разрешения, то они просто вернут пустой массив.
Также при неудачной инициализации визуализатора на Android в консоли будет выведено сообщение с информацией - это сообщение можно проигнорировать, если вам не нужна визуализация.
Для работы этих методов необходимо указать следующие разрешения:

```
android.permission.RECORD_AUDIO
android.permission.MODIFY_AUDIO_SETTINGS
```

# Настройка для Windows

Настройка для Windows не требуется

# Использование

Плеер предлагает следующие события:

```c#
event SimplePlayerStateEvent? StateChanged; // Вызывается при изменении состояния плеера
event SimplePlayerEvent? SourceChanged; // Вызывается при изменении источника
event SimplePlayerEvent? SourceEnded; // Вызывается при окончании источника
event PlayerTimeEvent? PositionChanged; // Вызывается при изменении текущего времени проигрывания
event PlayerEvent? SkippedToNext; // Вызывается при пропуске до следующей песни
event PlayerEvent? SkippedToPrevious; // Вызывается при перемотки до предыдущей песни
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
IMediaItem Source { get; } // Информация о текущем аудио
int CaptureDataSize { get; set; } // Размер захыватываемых данных для визуализации (FFT, Waveform)
IPlayerRemoteControls RemoteControls { get; } // Настройки удалённого управления
```

> Для настройки поведения удалённого управления плеера используйте `RemoteControls`. 
Подробнее об этом можно прочитать [здесь](https://github.com/Laeslaraftor/Acly.Player/blob/master/Acly.Player/RemoteControls.md)

Управление плеером:

```c#
void Pause(); // Пауза
void Play(); // Воспроизвести
void Stop(); // Остановить
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