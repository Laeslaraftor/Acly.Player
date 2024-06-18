namespace Acly.Player.Android
{
    /// <summary>
    /// Класс настроек внешнего вида уведомления
    /// </summary>
    public class PlayerNotificationStyle
    {
        /// <summary>
        /// Название приложения
        /// </summary>
        public string ApplicationName { get; set; } = "Acly.Player";
        /// <summary>
        /// Название действия перемотки до предыдущей песни
        /// </summary>
        public string BackwardActionName { get; set; } = "Предыдущая песня";
        /// <summary>
        /// Название действия перемотки до следующей песни
        /// </summary>
        public string ForwardActionName { get; set; } = "Следующая песня";
        /// <summary>
        /// Название действия паузы
        /// </summary>
        public string PauseActionName { get; set; } = "Пауза";
        /// <summary>
        /// Название действия возобновления
        /// </summary>
        public string PlayActionName { get; set; } = "Воспроизвести";
        /// <summary>
        /// Название действия остановки
        /// </summary>
        public string StopActionName { get; set; } = "Выключить";

        /// <summary>
        /// Иконка приложения
        /// </summary>
        public int ApplicationIcon { get; set; } = Resource.Drawable.acly_letter;
        /// <summary>
        /// Иконка кнопки перемотки до предыдущей песни
        /// </summary>
        public int BackwardIcon { get; set; } = Resource.Drawable.ic_skip_back;
        /// <summary>
        /// Иконка кнопки перемотки до следующей песни
        /// </summary>
        public int ForwardIcon { get; set; } = Resource.Drawable.ic_skip_forward;
        /// <summary>
        /// Иконка кнопки паузы
        /// </summary>
        public int PauseIcon { get; set; } = Resource.Drawable.ic_pause;
        /// <summary>
        /// Иконка кнопки возобновления
        /// </summary>
        public int PlayIcon { get; set; } = Resource.Drawable.ic_play;
        /// <summary>
        /// Иконка кнопки остановки
        /// </summary>
        public int StopIcon { get; set; } = Resource.Drawable.ic_stop;
    }
}
