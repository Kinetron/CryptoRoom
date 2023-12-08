using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoRoomLib.Models
{
    /// <summary>
    /// Основные данные файла.
    /// </summary>
    public class CommonFileInfo
    {
        /// <summary>
        /// Длина файла.
        /// </summary>
        public ulong FileLength { get; set; }

        /// <summary>
        /// Размер блока шифрованных данных (идущим за заголовком).
        /// </summary>
        public ulong UserDataSize { get; set; }

        /// <summary>
        /// Позиция начала шифрованных данных в файле.
        /// </summary>
        public int BeginDataPosition { get; set; }

        /// <summary>
        /// Cеансовый ключ.
        /// </summary>
        public byte[] SessionKey { get; set; }

        /// <summary>
        /// Шифрованный сеансовый ключ.
        /// </summary>
        public byte[] CryptedSessionKey { get; set; }

        /// <summary>
        /// Начальный вектор.
        /// </summary>
        public byte[] Iv { get; set; }

        /// <summary>
        /// Заголовок файла.
        /// </summary>
        public byte[] FileHead { get; set; }

        /// <summary>
        /// Бинарные данные, похожие на ASN1 формат (любое число блоков), содержащие дополнительную информацию в файле. 
        /// </summary>
        public byte[] BlockData { get; set; }

        /// <summary>
        /// Вектор подписи R.
        /// </summary>
        public byte[] VectorR { get; set;}

        /// <summary>
        /// Вектор подписи S.
        /// </summary>
        public byte[] VectorS { get; set; }

        /// <summary>
        /// Идентификатор (в БД системы), лица подписавшего файл.
        /// </summary>
        public byte[] SignerIdentity { get; set; }

        /// <summary>
        /// Позиция в файле начала блока подписи
        /// </summary>
        public long BeginSignBlockPosition { get; set; }
    }
}
