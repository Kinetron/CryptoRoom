using System.Text;

namespace CryptoRoomLib.Sign
{
    /// <summary>
    /// Цифровая подпись.
    /// </summary>
    internal class Signature
    {
        /// <summary>
        /// Вектор подписи r.
        /// </summary>
        public BigInteger R { get; set; }

        /// <summary>
        /// Вектор подписи s.
        /// </summary>
        public BigInteger S { get; set; }

        /// <summary>
        /// Размер числа Q на основе которого создана подпись, байт(256 / 512).
        /// </summary>
        public long Qlength;

        /// <summary>
        /// Вектор подписи r. 16 ричное представление.
        /// </summary>
        public string VectorR
        {
            get
            {
                return Padding(R.ToHexString(), (int)Qlength / 4).ToLower();
            }
        }

        /// <summary>
        /// Вектор подписи s. 16 ричное представление.
        /// </summary>
        public string VectorS
        {
            get
            {
                return Padding(S.ToHexString(), (int)Qlength / 4).ToLower();
            }
        }

        /// <summary>
        /// Дополняем подпись нулями слева до длины size, где size - длина модуля в битах.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        private string Padding(string input, int size)
        {
            StringBuilder sb = new StringBuilder(input);
            if (input.Length < size)
            {
                do
                {
                    sb.Insert(0, "0");
                } while (sb.Length < size);
            }

            return sb.ToString();
        }
    }
}
