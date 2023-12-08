using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoRoomLib.Hash3411
{
    /// <summary>
    /// Набор встроенных тестов для проверки правильности алгоритма.
    /// </summary>
    public class SelfTests
    {
        /// <summary>
        /// Сообщение об ошибке.
        /// </summary>
        public string Error { get; private set; }

        public SelfTests()
        {
            Error = string.Empty;
        }

        /// <summary>
        /// Общий метод тестирования всего алгоритма 34.12.
        /// </summary>
        /// <returns></returns>
        public bool RunTests()
        {
            List<Func<bool>> tests = new List<Func<bool>>
            {
                TestAlgoritms
            };

            foreach (var test in tests)
            {
                if (!test()) return false;
            }

            return true;
        }

        /// <summary>
        /// Тестирует правильность алгоритмов хеширования.
        /// </summary>
        /// <returns></returns>
        private bool TestAlgoritms()
        {
            int size256 = Hash3411.Hash256Size;
            int size512 = Hash3411.Hash512Size;
            
            byte[] h512 = new byte[size512];
            byte[] h256 = new byte[size256];

            Hash3411 hasher = new Hash3411();

            for (int i = 0; i < TestConst.TestMessage.Length; i++)
            {
                hasher.Hash512(TestConst.TestMessage[i], h512);

                byte[] etalon = new byte[size512];
                Buffer.BlockCopy(TestConst.Hash512, TestConst.Hash512.GetLength(1) * i, etalon, 0, size512);

                if (!etalon.SequenceEqual(h512))
                {
                    Error = "Ошибка проверки алгоритма Hash3411 512";
                    return false;
                }

                hasher.Hash256(TestConst.TestMessage[i], h256);
                byte[] etalon256 = new byte[size256];
                Buffer.BlockCopy(TestConst.Hash256, TestConst.Hash256.GetLength(1) * i, etalon256, 0, size256);

                if (!etalon256.SequenceEqual(h256))
                {
                    Error = "Ошибка проверки алгоритма Hash3411 256";
                    return false;
                }
            }

            return true;
        }
    }
}
