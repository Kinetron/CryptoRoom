namespace VanishBox.Appsettings
{
    public interface ISettings
    {
        /// <summary>
        /// Сообщение об ошибке.
        /// </summary>
        public string LastError { get; set; }

        /// <summary>
        /// Настройки программы.
        /// </summary>
        AppSettings AppSettings { get; set; }

        /// <summary>
        /// Сохранить настройки.
        /// </summary>
        /// <returns></returns>
        bool Save();

        /// <summary>
        /// Очищает все настройки.
        /// </summary>
        void Clear();
    }
}
