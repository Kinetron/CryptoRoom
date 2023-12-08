using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoRoomLib
{
    /// <summary>
    /// Содержит вспомогательные инструменты для работы алгоритмов шифрования.
    /// </summary>
    internal class CipherTools
    {
        /// <summary>
        /// Генерирует случайное число, пригодное для криптографии.
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public static byte[] GenerateRand(int length)
        {
            byte[] data = new byte[length];

            System.Security.Cryptography.RandomNumberGenerator.Create().GetBytes(data);

            return data;
        }

        /// <summary>
        /// Дополняет слева массив нулем, до кратной двум длины.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static byte[] PaddingLeft(byte[] data)
        {
            if (data.Length % 2 == 0)
            {
                return data;
            }

            byte[] padding = new byte[data.Length + 1];
            Buffer.BlockCopy(data, 0, padding, 1, data.Length);
            
            return padding;
        }
    }
}
