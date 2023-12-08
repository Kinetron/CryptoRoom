using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CryptoRoomLib.Models;

namespace CryptoRoomLib
{
    /// <summary>
    /// Настройки библиотеки.
    /// </summary>
    internal static class SettingsLib
    {
        public static readonly Settings Settings;

        static SettingsLib()
        {
            using (StreamReader reader = new("KeyGenData.json"))
            {
                var json = reader.ReadToEnd();
                Settings = JsonConvert.DeserializeObject<Settings>(json);
            }
        }
    }
}
