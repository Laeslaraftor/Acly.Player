namespace Acly.Player.Android
{
    /// <summary>
    /// Исключение о не выданном разрешении 
    /// </summary>
    public sealed class PermissionNotGrantedException : Exception
    {
        /// <summary>
        /// Создать новый экземпляр исключения
        /// </summary>
        /// <param name="Permission">Не выданное разрешение</param>
        public PermissionNotGrantedException(string Permission) : base(string.Format(_Message, Permission))
        {
        }

        private const string _Message = "Разрешение \"{0}\" не было дано. Необходимо указать его в манифесте.";
    }
}
