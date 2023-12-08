using System;
using Newtonsoft.Json;
using System.IO;

namespace VanishBox.Appsettings
{
    public class Settings : ISettings
    {
        /// <summary>
        /// Настройки программы.
        /// </summary>
        public AppSettings AppSettings { get; set; }

        /// <summary>
        /// Сообщение об ошибке.
        /// </summary>
        public string LastError { get; set; }

        /// <summary>
        /// Имя файла с настройками.
        /// </summary>
        private const string _fileName = "appsettings.json";

        /// <summary>
        /// Путь к файлу с настройками.
        /// </summary>
        private string _appSettingsPath;
        public Settings()
        {
            _appSettingsPath = Path.Combine(Directory.GetCurrentDirectory(), _fileName);
            if (!File.Exists(_appSettingsPath))
            {
                AppSettings = new AppSettings();
                return;
            }

            var json = File.ReadAllText(_appSettingsPath);
            AppSettings = JsonConvert.DeserializeObject<AppSettings>(json);
        }

        /// <summary>
        /// Сохранить настройки.
        /// </summary>
        /// <returns></returns>
        public bool Save()
        {
            try
            {
                var json = JsonConvert.SerializeObject(AppSettings, Formatting.Indented);
                File.WriteAllText(_appSettingsPath, json);
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
                return false;
            }

            return true;
        }

        /// <summary>
        /// Очищает все настройки.
        /// </summary
        public void Clear()
        {
            AppSettings.PathSecretKey = string.Empty;
        }
    }
}
