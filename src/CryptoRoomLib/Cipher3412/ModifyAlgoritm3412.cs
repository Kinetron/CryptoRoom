namespace CryptoRoomLib.Cipher3412
{
    /// <summary>
    /// Модифицированный алгоритм  ГОСТ 34.12-2015.
    /// </summary>
    internal class ModifyAlgoritm3412 : ICipherAlgoritm
    {
        /// <summary>
        /// Алгоритм был инициализирован.
        /// </summary>
        private bool _isInit = false;
        
        /// <summary>
        /// Константы для нелинейного преобразования.
        /// </summary>
        private byte[] NonlinearTransformPerm = new byte[]
        {
            252, 238, 221, 17, 207, 110, 49, 22, 251, 196,
            250, 218, 35, 197, 4, 77, 233, 119, 240, 219,
            147, 46, 153, 186, 23, 54, 241, 187, 20, 205,
            95, 193, 249, 24, 101, 90, 226, 92, 239, 33,
            129, 28, 60, 66, 139, 1, 142, 79, 5, 132, 2,
            174, 227, 106, 143, 160, 6, 11, 237, 152, 127,
            212, 211, 31, 235, 52, 44, 81, 234, 200, 72,
            171, 242, 42, 104, 162, 253, 58, 206, 204, 181,
            112, 14, 86, 8, 12, 118, 18, 191, 114, 19, 71,
            156, 183, 93, 135, 21, 161, 150, 41, 16, 123,
            154, 199, 243, 145, 120, 111, 157, 158, 178, 177,
            50, 117, 25, 61, 255, 53, 138, 126, 109, 84,
            198, 128, 195, 189, 13, 87, 223, 245, 36, 169,
            62, 168, 67, 201, 215, 121, 214, 246, 124, 34,
            185, 3, 224, 15, 236, 222, 122, 148, 176, 188,
            220, 232, 40, 80, 78, 51, 10, 74, 167, 151, 96,
            115, 30, 0, 98, 68, 26, 184, 56, 130, 100, 159,
            38, 65, 173, 69, 70, 146, 39, 94, 85, 47, 140,
            163, 165, 125, 105, 213, 149, 59, 7, 88, 179,
            64, 134, 172, 29, 247, 48, 55, 107, 228, 136,
            217, 231, 137, 225, 27, 131, 73, 76, 63, 248,
            254, 141, 83, 170, 144, 202, 216, 133, 97, 32,
            113, 103, 164, 45, 43, 9, 91, 203, 155, 37,
            208, 190, 229, 108, 82, 89, 166, 116, 210, 230,
            244, 180, 192, 209, 102, 175, 194, 57, 75, 99,
            182
        };

        private byte[] LinearTransformCoeff = new byte[]{
            148, 32, 133, 16, 194, 192, 1, 251, 1, 192,
            194, 16, 133, 32, 148, 1
        };

        /// <summary>
        /// Раундовые ключи для преобразований.
        /// </summary>
        private Dictionary<byte, byte> DirectPermutation = new Dictionary<byte, byte>();

        private Dictionary<byte, byte> InversePermutation = new Dictionary<byte, byte>();

        /// <summary>
        /// Итерационные константы.
        /// </summary>
        private List<byte[]> IterationConstants = new List<byte[]>();

        /// <summary>
        /// Константа для быстрого умножения.
        /// </summary>
        private const ushort LinearTransformModulus = 0x1C3;

        /// <summary>
        /// Раундовые ключи.
        /// </summary>
        private List<byte[]> Keys = new List<byte[]>();

        public int KeySize { get; }
        public int BlockSize
        {
            get => 16;
        }

        /// <summary>
        /// Возвращает раундовые ключи.
        /// </summary>
        /// <returns></returns>
        public List<byte[]> GetRoundKeys()
        {
            return Keys;
        }

        public void DeployDecryptRoundKeys(byte[] key)
        {
            if (!_isInit)
            {
                InitPerms();
                InitConsts();

                _isInit = true;
            }

            for (int i = 0; i < BlockSize; i++)
            {
                Keys.Add(new byte[BlockSize]);
            }

            byte[] key0 = new byte[BlockSize];
            byte[] key1 = new byte[BlockSize];

            Buffer.BlockCopy(key, 0, key0,0, BlockSize);
            Buffer.BlockCopy(key, BlockSize, key1, 0, BlockSize);

            Keys[0] = key0;
            Keys[1] = key1;

            for (int i = 0; i < 4; i++)
            {
                var u1 = 2 * i + 2;
                var u2 = 2 * i + 3;

                KeyDerivation(
                        Keys[2 * i],
                        Keys[2 * i + 1],
                        Keys[2 * i + 2],
                        Keys[2 * i + 3],
                        i
                    );
            }
        }

        public void DecryptBlock(ref Block128t block)
        {
            throw new NotImplementedException();
        }

        public void EncryptBlock(ref Block128t block)
        {
            Block128t key0 = new Block128t();
            key0.FromArray(Keys[0]);

            block.Low ^= key0.Low;
            block.Hi ^= key0.Hi;

            byte[] buffer = new byte[BlockSize];
            block.ToArray(buffer);

            for (int i = 1; i < 10; i++)
            {
                NonlinearTransformDirect(buffer);
                IterationLinearTransformDirect(buffer);
                Xor(buffer, Keys[i], buffer);
            }

            block.FromArray(buffer);
        }

        public void DeployСryptRoundKeys(byte[] key)
        {
            DeployDecryptRoundKeys(key);
        }

        /// <summary>
        /// Заполняет словари с перестановками.
        /// </summary>
        private void InitPerms()
        {
            //Заполняю перестановки 
            for (int i = 0; i < 256; i++)
            {
                DirectPermutation.Add((byte)i, NonlinearTransformPerm[i]);
                
                //Обратная перестановка
                InversePermutation.Add(NonlinearTransformPerm[i], (byte)i);
            }
        }

        public void InitConsts()
        {
            for (byte i = 1; i <= 32; i++)
            {
                byte[] buffer = new byte[16];
                buffer[BlockSize - 1] = i;

                IterationLinearTransformDirect(buffer);
                IterationConstants.Add(buffer);
            }
        }
        
        private void IterationLinearTransformDirect(byte[] target)
        {
            for (int i = 0; i < BlockSize; i++) LinearTransformDirect(target);
        }

        public void LinearTransformDirect(byte[] target)
        {
            ushort buffer = LinearTransform(target);

            for (int i = BlockSize - 1; i > 0; i--)
            {
                target[i] = target[i - 1];
            }

            target[0] = (byte)buffer;
        }

        /// <summary>
        /// Линейное, прямое преобразование.
        /// </summary>
        private ushort LinearTransform(byte[] target)
        {
            ushort result = 0;
            for (int i = 0; i < BlockSize; i++)
            {
                result ^= Multiply(target[i], LinearTransformCoeff[i]);
            }
            
            return result;
        }

        /// <summary>
        /// Умножение для линейного преобразования.
        /// Все вычисления ведутся в фактор-кольце GL(2)[x]/p(x), где p(x) = x^8 + x^7 + x^6 + x + 1.
        /// В первом цикле производится перемножение многочленов,
        /// заданных своими коэффициентами из GL(2). Во втором же цикле пошагово вычисляется значение по модулю p(x).
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public ushort Multiply(ushort lhs, ushort rhs)
        {
            ushort result = 0;
            ushort modulus = LinearTransformModulus << 7;

            for (ushort detecter = 0x1; detecter != 0x100; detecter <<= 1, lhs <<= 1)
            {
                if ((rhs & detecter) != 0) result ^= lhs;
            }

            for (ushort detecter = 0x8000; detecter != 0x80; detecter >>= 1, modulus >>= 1)
            {
                if ((result & detecter) != 0) result ^= modulus;
            }

            return result;
        }

        private void KeyDerivation(byte[] k1, byte[] k2, byte[] k3, byte[] k4, int ipair)
        {
            if (!k1.Equals(k3)) Buffer.BlockCopy(k1, 0, k3, 0, BlockSize);
            if (!k2.Equals(k4)) Buffer.BlockCopy(k2, 0, k4, 0, BlockSize);

            for (int i = 0; i < 8; i++)
            {
                KeysTransform(k3, k4, ipair * 8 + i);
            }
        }

        private void KeysTransform(byte[] k1, byte[] k2, int iconst)
        {
            byte[] buffer = new byte[BlockSize];

            Buffer.BlockCopy(k1, 0, buffer, 0, BlockSize);

            Xor(k1, IterationConstants[iconst], k1);

            NonlinearTransformDirect(k1);
            IterationLinearTransformDirect(k1);
            Xor(k1, k2, k1);

            Buffer.BlockCopy(buffer, 0, k2, 0, BlockSize);
        }

        private void Xor(byte[] lhs, byte[] rhs, byte[] result)
        {
            for (int i = 0; i < BlockSize; i++)
            {
                result[i] = (byte)(lhs[i] ^ rhs[i]);
            }
        }

        private void NonlinearTransformDirect(byte[] target)
        {
            for (int i = 0; i < BlockSize; i++)
            {
                target[i] = DirectPermutation[target[i]];
            }
        }
    }
}
