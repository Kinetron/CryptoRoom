using CryptoRoomLib.Cipher3412.FastConst;

namespace CryptoRoomLib.Cipher3412
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
                Test128Type,
                DeployKeyRoundTest,
                GostExampleTest,
                GostWithRoundKeysTest,
                DecryptionTest,
                EncryptionTest,
                ModifyAlg3412MathTest,
                ModifyAlg3412Test
            };

            foreach (var test in tests)
            {
                if(!test()) return false;
            }
            
            return true;
        }

        /// <summary>
        /// Тест развертывания раундовых ключей.
        /// </summary>
        /// <returns></returns>
        private bool DeployKeyRoundTest()
        {
            var roundKeys = new ulong[20];
            var key = new byte[32];
            var etalonResult = new ulong[20];

            //Перебираю все ключи шифрования из тестового набора.
            for (int i = 0; i < TestConst3412.Key.GetLength(0); i++)
            {
                Array.Clear(roundKeys, 0, roundKeys.Length);
                Array.Clear(key, 0, key.Length);

                Buffer.BlockCopy(TestConst3412.Key, 32 * i, key, 0, 32);
                Logic3412.DeploymentEncryptionRoundKeys(key, roundKeys);

                //Получаем результат развертывания для данного ключа.
                Buffer.BlockCopy(TestConst3412.KeyDeploymentResult, i*20*8, etalonResult, 0, 20*8);

                if (!roundKeys.SequenceEqual(etalonResult))
                {
                    Error = $"Bad deploy encrypt test result: key={BitConverter.ToString(key)}";
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Тест на основании значения ключа, текста, и результата приведенного в ГОСТ.
        /// </summary>
        /// <returns></returns>
        private bool GostExampleTest()
        {
            var key = new byte[32];
            var roundKeys = new ulong[20];

            //Используя ключ из примера Гост, разворачиваем раундовые ключи шифрования.
            System.Buffer.BlockCopy(TestConst3412.GostKey, 0, key, 0, 32);
            Logic3412.DeploymentEncryptionRoundKeys(key, roundKeys);

            return GostExampleTest(roundKeys, key);
        }

        /// <summary>
        /// Тест на основании значения ключа, значений итерационных ключей, текста, и результата приведенного в ГОСТ.
        /// </summary>
        /// <returns></returns>
        private bool GostWithRoundKeysTest()
        {
            var key = new byte[32];
            System.Buffer.BlockCopy(TestConst3412.GostKey, 0, key, 0, 32);

            return GostExampleTest(TestConst3412.GostRoundKeys, key);
        }

        /// Тест на основании значения ключа, текста, и результата приведенного в ГОСТ.
        /// </summary>
        /// <returns></returns>
        private bool GostExampleTest(ulong[] roundKeys, byte[] key)
        {
            //Формируем шифротекст.
            U128t data = new U128t();
            var temp = new byte[8];

            System.Buffer.BlockCopy(TestConst3412.GostTextToCipher, 0, temp, 0, 8);
            data.Low = BitConverter.ToUInt64(temp, 0);

            System.Buffer.BlockCopy(TestConst3412.GostTextToCipher, 8, temp, 0, 8);
            data.Hi = BitConverter.ToUInt64(temp, 0);

            //Шифруем блок.
            Logic3412.EncryptBlock(ref data, roundKeys);
           
            if (TestConst3412.GostCipherResult[0] == data.Low && TestConst3412.GostCipherResult[1] == data.Hi)
                return true;

            Error = "Error when check Gost test.";

            return false;
        }

        /// <summary>
        /// Тестирует работу типа данных.
        /// </summary>
        /// <returns></returns>
        private bool Test128Type()
        {
            if (TestConst3412.ShiftTestTable.Length != sizeof(ulong))
            {
                Error = $"Error tests shift operation. Bad test array size.";
                return false;
            }

            U128t t = new U128t();

            //Берем константу для теста, которая будет помещена в разные позиции бинарного представления числа.
            for (int i = 0; i < TestConst3412.ShiftTestTable.Length; i++)
            {
                t.Low = TestConst3412.ShiftTestDigit;
                t.SetByte(i, TestConst3412.ShiftTestLowDigit);

                if (t.Low != TestConst3412.ShiftTestTable[i])
                {
                    Error = $"Error tests shift operation.Iteration={i}";
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Тестирование алгоритма шифрования.
        /// </summary>
        /// <returns></returns>
        private bool EncryptionTest()
        {
            var roundKeys = new ulong[20];
            var key = new byte[32];

            for (int i = 0; i < TestConst3412.EncryptTestKey.GetLength(0); i++)
            {
                Array.Clear(roundKeys, 0, roundKeys.Length);
                Array.Clear(key, 0, key.Length);

                Buffer.BlockCopy(TestConst3412.EncryptTestKey, 32 * i, key, 0, 32);

                Logic3412.DeploymentEncryptionRoundKeys(key, roundKeys);

                U128t data;
                data.Low = TestConst3412.EncryptTestInText[i, 0];
                data.Hi = TestConst3412.EncryptTestInText[i, 1];

                Logic3412.EncryptBlock(ref data, roundKeys);

                if (data.Low != TestConst3412.EncryptTestOutText[i, 0] ||
                    data.Hi != TestConst3412.EncryptTestOutText[i, 1])
                {
                    Error = $"Encrypt test error. Pos={i}";
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Тестирование алгоритма декодирования.
        /// </summary>
        /// <returns></returns>
        private bool DecryptionTest()
        {
            var roundKeys = new ulong[20];
            var key = new byte[32];

            for (int i = 0; i < TestConst3412.DecryptTestKey.GetLength(0); i++)
            {
                Array.Clear(roundKeys, 0, roundKeys.Length);
                Array.Clear(key, 0, key.Length);

                Buffer.BlockCopy(TestConst3412.DecryptTestKey, 32 * i, key, 0, 32);

                Logic3412.DeploymentDecryptionRoundKeys(key, roundKeys);

                U128t data;
                data.Low = TestConst3412.DecryptTestInText[i, 0];
                data.Hi = TestConst3412.DecryptTestInText[i, 1];
                
                Logic3412.DecryptBlock(ref data, roundKeys);

                if (data.Low != TestConst3412.DecryptTestOutText[i, 0] ||
                    data.Hi != TestConst3412.DecryptTestOutText[i, 1])
                {
                    Error = $"Decrypt test error. Pos={i}";
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Тест математических операций измененного алгоритма.
        /// </summary>
        /// <returns></returns>
        private bool ModifyAlg3412MathTest()
        {
            ModifyAlgoritm3412 alg = new ModifyAlgoritm3412();

            //Тест умножения.
            ushort[,] mulConsts = new ushort[16, 3]
            {
                //lhs rhs result
                {0, 0x0094, 0},
                {0, 0x0020, 0},
                {0x0001, 0x0001, 0x0001},
                {0x0001, 0x0094, 0x0094},

                {0x0094, 0x0094, 0x00a4},
                {0x0001, 0x0020, 0x0020},
                {0x0084, 0x0094, 0x00f0},
                {0x0094, 0x0020, 0x00a8},

                {0x0001, 0x0085, 0x0085},
                {0x00dd, 0x0094, 0x0089},
                {0x0084, 0x0020, 0x00ed},
                {0x0094, 0x0085, 0x0064},

                {0x0001, 0x0010, 0x0010},
                {0x0010, 0x0094, 0x0054},
                {0x00dd, 0x0020, 0x009c},
                {0x0084, 0x0085, 0x00e3}
            };

            for (int i = 0; i < mulConsts.GetLength(0); i++)
            {
                var result = alg.Multiply(mulConsts[i,0], mulConsts[i, 1]);

                if (result != mulConsts[i, 2])
                {
                    Error = "ModifyAlg3412MathTest: ошибка метода умножения.";
                    return false;
                }
            }
            
            return true;
        }

        /// <summary>
        /// Тест измененного алгоритма.
        /// </summary>
        /// <returns></returns>
        private bool ModifyAlg3412Test()
        {
            byte[] cipherKey = new byte[32]
            {
                0x6f, 0x1e, 0xa3, 0x4d, 0xe5, 0xf0, 0x6b, 0x84,
                0x59, 0x6e, 0x5e, 0x20, 0xcc, 0x04, 0x32, 0x34,
                0x9e, 0x38, 0xce, 0x5d, 0x08, 0x56, 0xae, 0x35,
                0xd5, 0xec, 0x37, 0x0b, 0x02, 0x4e, 0x16, 0xd6,
            };

            ModifyAlgoritm3412 alg = new ModifyAlgoritm3412();
            alg.DeployDecryptRoundKeys(cipherKey);
            
            byte[,] etalonRoundKeys = new byte[10, 16]
            {
                {
                    0x6f, 0x1e, 0xa3, 0x4d, 0xe5, 0xf0, 0x6b, 0x84,
                    0x59, 0x6e, 0x5e, 0x20, 0xcc, 0x04, 0x32, 0x34,
                },
                {
                    0x9e, 0x38, 0xce, 0x5d, 0x08, 0x56, 0xae, 0x35,
                    0xd5, 0xec, 0x37, 0x0b, 0x02, 0x4e, 0x16, 0xd6,
                },
                {
                    0xfe, 0xac, 0x69, 0xa5, 0xf9, 0xee, 0x6b, 0xba,
                    0x69, 0x3f, 0xa3, 0xe6, 0xcf, 0x72, 0xa1, 0xfa,
                },
                {
                    0x80, 0x44, 0x90, 0x6f, 0x05, 0x91, 0xf4, 0x35,
                    0x39, 0x02, 0x10, 0xaf, 0xaf, 0x54, 0x1c, 0xad,
                },
                {
                    0x53, 0xbc, 0xc9, 0xd3, 0x10, 0xe0, 0x26, 0x4b,
                    0xee, 0x41, 0x0e, 0x8c, 0x1a, 0xee, 0x4e, 0x07,
                },
                {
                    0x52, 0x48, 0xb2, 0xf2, 0x4f, 0x25, 0x2a, 0xa3,
                    0x7b, 0xc6, 0x87, 0xd0, 0x93, 0x1a, 0xdd, 0x7a,
                },
                {
                    0xea, 0xa3, 0x20, 0x23, 0x87, 0x53, 0x71, 0x19,
                    0x03, 0x2a, 0x6f, 0xfd, 0xef, 0xae, 0xcf, 0x5a,
                },
                {
                    0x2a, 0xbc, 0x1f, 0xdf, 0x6c, 0x9f, 0xa3, 0x3c,
                    0x5f, 0x9b, 0xf6, 0x24, 0x8c, 0x01, 0x94, 0x44,
                },
                {
                    0x55, 0x70, 0x54, 0xea, 0x8a, 0x2c, 0x6b, 0x18,
                    0x4b, 0xd5, 0xc9, 0x85, 0x4f, 0x1b, 0xb2, 0xb9,
                },
                {
                    0x7b, 0xe5, 0x05, 0x8d, 0x96, 0x9f, 0xe5, 0xa3,
                    0x69, 0xd0, 0xb8, 0x98, 0xb0, 0xc6, 0x0c, 0x27,
                }
            };
            
            var keys = alg.GetRoundKeys();
            
            for (int i = 0; i < etalonRoundKeys.GetLength(0); i++)
            {
                var buff = new byte[alg.BlockSize];
                Buffer.BlockCopy(etalonRoundKeys,i * alg.BlockSize, buff, 0, alg.BlockSize);

                if (!buff.SequenceEqual(keys[i]))
                {
                    Error = "ModifyAlg3412Test: ошибка развёртывания итерационных ключей";
                    return false;
                }
            }

            byte[] iv = new byte[16]{
                0xdd, 0xef, 0x3f, 0x72, 0xa8, 0xf2, 0x4a, 0x0d,
                0xe9, 0xe2, 0x02, 0x35, 0x04, 0x39, 0x0d, 0x17
            };

            Block128t сBlock = new Block128t(); //Входящий шифротекст.
            сBlock.FromArray(iv);

            Block128t etalonEnc = new Block128t(); //Правильный закодированный блок.
            etalonEnc.FromArray(new byte[]
            {
                0xb7, 0xca, 0x34, 0x37, 0x6e, 0xd3, 0xb3, 0x00,
                0x42, 0x65, 0xd0, 0xe9, 0xbc, 0xdb, 0x87, 0x91,
            });
            
            alg.EncryptBlock(ref сBlock);

            if (!сBlock.Compare(etalonEnc))
            {
                Error = "ModifyAlg3412Test: ошибка кодирования блока";
                return false;
            }
            
            return true;
        }
    }
}
