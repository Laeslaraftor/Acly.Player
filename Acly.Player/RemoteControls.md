# Настройка поведения удалённого управления

За настройку отвечает внутренняя реализация интерфейса `IPlayerRemoteControls`.
Этот интерфейс содержит следующие события:

```c#
event EventHandler? SkippedToNext; // Запрошен пропуск до следующего аудиофайла
event EventHandler? SkippedToPrevious; // Запрошена перемотка до предыдущего аудиофайла
EventHandler<bool>? CanSkipToNextChanged; // Доступность пропуска до следующего аудиофайла изменилась
EventHandler<bool>? CanSkipToPreviousChanged; // Доступность перемотки до предыдущего аудиофайла изменилась
```

И поля:

```C#
bool IsDisposed { get; } // Очищен ли плеер
bool IsEnabled { get; set; } // Включено ли удалённое управление
IPlayer Player { get; } // Плеер, к которому относится текущий экземпляр настроек
ICommand? SkipToNextCommand { get; set; } // Команда пропуска до следующего аудиофайла
object? SkipToNextCommandParameter { get; set; } // Параметр команды пропуска до следующего аудиофайла
ICommand? SkipToPreviousCommand { get; set; } // Команда перемотки до прошлого аудиофайла
object? SkipToPreviousCommandParameter { get; set; } // Параметр команды перемотки до прошлого аудиофайла
bool CanSkipToNext { get; set; } // Доступен ли пропуск до следующего аудиофайла
bool CanSkipToPrevious { get; set; } // Доступна ли перемотка до предыдущего аудиофайла
```

Если `IsEnabled` равен `false`, то удалённое управление будет отключено. 
По умолчанию на Windows это поле равно `true`, а вот для Android всё сложнее...
Так как Android приложения могут иметь только одно уведомление от `MediaSession` (вроде как),
то в первом экземпляре плеера поле `IsEnabled` будет иметь значение `true`, а вот последующие - `false`.
Но если вы перед тем как создать второй экземпляр плеера, очистите плеер, вызовом `Dispose`, то `IsEnabled` будет равен `true`.
Конечно же, вам никто не запрещает вручную включить удалённое управление сразу на нескольких плеерах, 
но тогда оно будет кучеряво работать.

Доступность перемоток определяется благодаря полям `CanSkipToNext` и `CanSkipToPrevious`, 
а также соответствующим командам действий.

## Примеры

Вот так выглядит управление с отключенной возможностью пропуска:

![Windows](https://raw.githubusercontent.com/Laeslaraftor/Acly.Player/master/Acly.Player/Preview/AndroidEmptyControls.jpg)
![Android](https://raw.githubusercontent.com/Laeslaraftor/Acly.Player/master/Acly.Player/Preview/WindowsEmptyControls.jpg)


А вот так выглядит управление с возможностью перематывать только до следующего аудиофайла:

![Windows](https://raw.githubusercontent.com/Laeslaraftor/Acly.Player/master/Acly.Player/Preview/AndroidForwardControls.jpg)
![Android](https://raw.githubusercontent.com/Laeslaraftor/Acly.Player/master/Acly.Player/Preview/WindowsForwardControls.jpg)


> Для Android можно изменить иконки уведомления. Об говорилось [здесь](https://github.com/Laeslaraftor/Acly.Player/blob/master/README.md)