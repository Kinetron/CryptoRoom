using CryptoRoomLib.AsymmetricInformation;
using CryptoRoomLib.Cipher3412;
using System.Security.Cryptography;
using CryptoRoomLib.Models;

namespace CryptoRoomLib.CipherMode3413
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

        /// <summary>
        /// Общий метод тестирования всего алгоритма.
        /// </summary>
        /// <returns></returns>
        public bool RunTests()
        {
            List<Func<bool>> tests = new List<Func<bool>>
            {
                XorBlocksTest,
                DeсryptIterationCBC,
                GostDecryptCbc,
                //DecryptData,
                TestCfbEncrypt
            };

            foreach (var test in tests)
            {
                if (!test()) return false;
            }

            return true;
        }

        /// <summary>
        /// Тестирование метода сложения блоков длиной 16 байт по модулю 2.
        /// </summary>
        /// <returns></returns>
        private bool XorBlocksTest()
        {
            ModeCBC cbc = new ModeCBC(new CipherAlgoritm3412());

            Block128t cBlock;
            cBlock.Low = 0x14353cca5619e7bd;
            cBlock.Hi = 0xe6b24748662b9dc1;
            
            Block128t msb;
            msb.Low = 0x14353cca5642174c;
            msb.Hi = 0xe6b24748662b9dc1;

            Block128t part;
            part.Low = 0x00000000005bf0f1;
            part.Hi = 0x0000000000000000;

            Block128t result = new Block128t();

            cbc.XorBlocks(ref cBlock, ref msb, ref result);

            if (part.Low != result.Low || part.Hi != result.Hi)
            {
                Error = "Error test CipherMode3413 XorBlocks.";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Тестирование итерации в режиме простой замены.
        /// </summary>
        /// <returns></returns>
        private bool DeсryptIterationCBC()
        {
            ICipherAlgoritm algoritm = new CipherAlgoritm3412();
            ModeCBC cbc = new ModeCBC(algoritm);

            Block128t cBlock;
            cBlock.Low = 0xda3ecc31a05c9124;
            cBlock.Hi = 0x04139dc14ab5b347;
            
            algoritm.DeployDecryptRoundKeys(TestConst3413.Key);
            
            Block128t msb;
            msb.Low = 0x14353cca5642174c;
            msb.Hi = 0xe6b24748662b9dc1;

            Block128t result = new Block128t();

            cbc.DeсryptIterationCBC(ref result, msb, ref cBlock);

            //Правильный результат шифрования.
            Block128t etalon;
            etalon.Low = 0x00000000005bf0f1;
            etalon.Hi = 0x0000000000000000;

            if (etalon.Low != result.Low || etalon.Hi != result.Hi)
            {
                Error = "Error test CipherMode3413 DeсryptIterationCBC.";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Тест режима работы блочного шифра на основание таблицы из ГОСТ.
        /// А.2.4 Режим простой замены с зацеплением
        /// </summary>
        /// <returns></returns>
        bool GostDecryptCbc()
        {
            Register256t register = new Register256t();// Регистр размером m = kn =  2*16
            Block128t lsb = new Block128t();//значением n разрядов регистра сдвига с большими номерами

            Block128t сBlock = new Block128t(); //Входящий шифротекст.
            Block128t pBlock = new Block128t(); //Исходящий декодированный текст.
            Block128t tmpBlock = new Block128t(); //Хранит временные данные

            //Данные из примера.
            var gostKey = TestConst3413.ExampleA2_4_key;
            var gostIv = TestConst3413.ExampleA2_4_iv;
            var cryptedText = TestConst3413.ExampleA2_4_cryptedText;
            var text = TestConst3413.ExampleA2_4_text;
            
            ICipherAlgoritm algoritm = new CipherAlgoritm3412();
            ModeCBC cbc = new ModeCBC(algoritm);

            algoritm.DeployDecryptRoundKeys(gostKey);

            //Заполнение регистра данными IV
            register.FromArray(gostIv);

            int testWordSize = cryptedText.GetLength(1);
            byte[] buffer = new byte[testWordSize]; 

            for (int i = 0; i < cryptedText.GetLength(0); i++)
            {
                Buffer.BlockCopy(cryptedText, i * testWordSize,
                    buffer, 0, testWordSize);

                сBlock.FromArray(buffer);
                cbc.IterationCBC(ref register, ref tmpBlock, ref lsb, ref сBlock, ref pBlock); //Расшифровываю

                Block128t etalon = new Block128t(); //Эталон из примеров.

                Buffer.BlockCopy(text, i * testWordSize,
                    buffer, 0, testWordSize);
                
                etalon.FromArray(buffer);

                if (!etalon.Compare(pBlock))
                {
                    Error = $"Error test CipherMode3413 GostDecryptCbc. Pos={i}";
                    return false;
                }
            }
            
            return true;
        }
        
        /// <summary>
        /// Тестирование режима шифрования CFB(режим обратной связи по шифротексту).
        /// </summary>
        /// <returns></returns>
        private bool TestCfbEncrypt()
        {
            //Ключ шифрования.
            byte[] cipherKey = new byte[]
            {
                0x6f, 0x1e, 0xa3, 0x4d, 0xe5, 0xf0, 0x6b, 0x84,
                0x59, 0x6e, 0x5e, 0x20, 0xcc, 0x04, 0x32, 0x34,
                0x9e, 0x38, 0xce, 0x5d, 0x08, 0x56, 0xae, 0x35,
                0xd5, 0xec, 0x37, 0x0b, 0x02, 0x4e, 0x16, 0xd6,
            };

            //Начальный вектор.
            byte[] iv = new byte[]
            {
                0xdd, 0xef, 0x3f, 0x72, 0xa8, 0xf2, 0x4a, 0x0d,
                0xe9, 0xe2, 0x02, 0x35, 0x04, 0x39, 0x0d, 0x17
            };

            //Данные для шифрования.
            byte[] toCrypt = new byte[]
            {
                0x58, 0x76, 0xdd, 0xf5, 0x96, 0xfa, 0xdd, 0xe3,
                0x32, 0x0f, 0xa9, 0xa4, 0xf9, 0x7a, 0xd5, 0x86,
                0x13, 0x0e, 0xc1, 0xa1, 0x69, 0x54, 0x07, 0x3c,
                0x93, 0xfe, 0x12, 0xa6, 0xbb, 0xc5, 0x4e, 0x59,
                0x24, 0x0b, 0x23, 0x09, 0x07, 0x10, 0x02, 0x20,
                0x2f, 0xd0, 0x05, 0x04, 0x1f, 0xf6, 0x5b, 0x25,
                0x89, 0xbb, 0x80, 0x2a, 0xfc, 0xf2, 0xf3, 0x4e,
                0x05, 0xe5, 0x6b, 0x94, 0x2b, 0xa8, 0xdb, 0xde,
            };

            ICipherAlgoritm algoritm = new ModifyAlgoritm3412();
            algoritm.DeployDecryptRoundKeys(cipherKey);
            ModeCFB cfb = new ModeCFB(algoritm);

            //Правильный результат кодирования.
            byte[] etalonEncResult = new byte[64]
            {
                0xef, 0xbc, 0xe9, 0xc2, 0xf8, 0x29, 0x6e, 0xe3,
                0x70, 0x6a, 0x79, 0x4d, 0x45, 0xa1, 0x52, 0x17,
                0x9f, 0x93, 0xec, 0xbe, 0xb1, 0x86, 0xd5, 0x93,
                0x95, 0xea, 0x3f, 0x8b, 0x23, 0xb9, 0x35, 0x3f,
                0x45, 0xa1, 0x92, 0xbe, 0xd5, 0xa7, 0x36, 0xd6,
                0xb4, 0x50, 0x31, 0xbf, 0xbd, 0xcb, 0xff, 0xa8,
                0xa3, 0x66, 0x46, 0xf9, 0xd6, 0x2d, 0xf0, 0x59,
                0xe9, 0xc6, 0x2d, 0xca, 0x32, 0x19, 0xad, 0x84
            };

            cfb.CfbEncrypt(toCrypt, iv);

            if (!etalonEncResult.SequenceEqual(toCrypt))
            {
                Error = "TestCfbEncrypt: результат кодирования не совпал с эталонным результатом";
                return false;
            }

            return true;
        }
    }
}
