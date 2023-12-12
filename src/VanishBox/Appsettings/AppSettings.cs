using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VanishBox.Appsettings
{
    public class AppSettings
    {
        public string PathSecretKey { get; set; }

        /// <summary>
        /// Алгоритм шифрования.
        /// </summary>
        public string СipherAlgoritm { get; set; } = "RsaAesSha256";
    }
}
