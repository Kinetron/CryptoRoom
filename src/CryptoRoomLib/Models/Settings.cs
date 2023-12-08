using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoRoomLib.Models
{
    /// <summary>
    /// Настройки библиотеки.
    /// </summary>
    internal class Settings
    {
        /// <summary>
        /// Версия  ключа
        /// </summary>
        public string KeyVersion { get; set; }

        /// <summary>
        /// Версия программы генератора ключа
        /// </summary>
        public string KeyGenVersion { get; set; }

        /// <summary>
        /// Наименование организации.
        /// </summary>
        public string OrgName { get; set; }

        /// <summary>
        /// Код организации.
        /// </summary>
        public string OrgCode { get; set; }

        /// <summary>
        /// Подразделение.
        /// </summary>
        public string Department { get; set; }

        /// <summary>
        /// Телефон.
        /// </summary>
        public string PhoneNumber { get; set; }

        /// <summary>
        /// Фамилия лица создавшего ключ.
        /// </summary>
        public string Familia { get; set; }

        /// <summary>
        /// Имя лица создавшего ключ.
        /// </summary>
        public string Imia { get; set; }

        /// <summary>
        /// Отчество лица создавшего ключ.
        /// </summary>
        public string Otchestvo { get; set; }
    }
}
