using CryptoRoomLib.KeyGenerator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoRoomLib
{
    /// <summary>
    /// Сервис для работы с секретным ключом.
    /// </summary>
    public interface IKeyService
    {
        /// <summary>
        /// Возвращает закрытый ключ ассиметричной системы шифрования.
        /// </summary>
        /// <returns></returns>
        byte[] GetPrivateAsymmetricKey();


        /// <summary>
        /// Возвращает открытый ключ ассиметричной системы шифрования.
        /// </summary>
        /// <returns></returns>
        byte[] GetPublicAsymmetricKey();
    }
}
