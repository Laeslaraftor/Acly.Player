namespace Acly.Player.Android
{
    /// <summary>
    /// Класс настроек уведомления
    /// </summary>
    public class PlayerNotificationSettings
    {
        /// <summary>
        /// Идентификатор уведомления
        /// </summary>
        public int Id { get; set; } = 101;
        /// <summary>
        /// Тег медиасессии
        /// </summary>
        public string SessionTag { get; set; } = "AclyPlayer";
        /// <summary>
        /// Идентификатор канала уведомления
        /// </summary>
        public string ChannelId { get; set; } = "AclyPlayerNotificationChannel";
        /// <summary>
        /// Название канала уведомления (отображается в настройках)
        /// </summary>
        public string ChannelName { get; set; } = "Acly.Player";
        /// <summary>
        /// Описание канала уведомления (отображается в настройках)
        /// </summary>
        public string ChannelDescription { get; set; } = "Управление музыкой";
    }
}
