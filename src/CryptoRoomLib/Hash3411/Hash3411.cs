namespace CryptoRoomLib.Hash3411
{
    /// <summary>
    /// Класс содержащий методы для генерации хеш функции согласно ГОСТ 34.11-2012
    /// </summary>
    internal class Hash3411
    {
        /// <summary>
        /// Размер выходного блока хеш функции.
        /// </summary>
        public static int Hash512Size = 64;

        /// <summary>
        /// Размер выходного блока хеш функции.
        /// </summary>
        public static int Hash256Size = 32;

        /// <summary>
        /// Складывает(по модулю 2) два числа.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="result"></param>
        private void AddXor512(byte[] a, byte[] b, byte[] result)
        {
            for (int i = 0; i < 64; i++)
            {
                result[i] = (byte)(a[i] ^ b[i]);
            }
        }

        /// <summary>
        /// S-преобразование. Функция S является обычной функцией подстановки.
        /// Каждый байт из 512-битной входной последовательности заменяется соответствующим байтом из таблицы подстановок π
        /// </summary>
        /// <param name="data"></param>
        private void Stransform(byte[] data)
        {
            for (int i = 0; i < 64; i++)
            {
                data[i] = FastConst.Sbox[data[i]];
            }
        }

        /// <summary>
        /// P-преобразование. Функция перестановки. Для каждой пары байт из входной последовательности происходит замена одного байта другим.
        /// </summary>
        /// <param name="data"></param>
        private void Ptransform(byte[] data)
        {
            byte[] temp = new byte[64];

            for (int i = 0; i < 64; i++)
            {
                temp[i] = data[FastConst.Tau[i]];
            }

            temp.CopyTo(data, 0);
        }

        /// <summary>
        /// L-преобразование. Представляет собой умножение 64-битного входного вектора на бинарную матрицу A размерами 64x64.
        /// </summary>
        /// <param name="data"></param>
        private void Ltransform(byte[] data)
        {
            ulong val = 0;
            int k = 0;

            for (int i = 0; i < 8; i++)
            {
                val = 0;

                for (k = 0; k < 8; k++)
                {
                    for (int j = 0; j < 8; j++)
                    {
                        if ((data[i * 8 + k] & (1 << (7 - j))) != 0)
                        {
                            val ^= FastConst.MatrixMixColumns[k * 8 + j];
                        }
                    }
                }

                for (k = 0; k < 8; k++)
                {
                    data[i * 8 + k] = (byte)((val & ((ulong)0xFF << (7 - k) * 8)) >> (7 - k) * 8);
                }
            }
        }

        /// <summary>
        /// Функция KeySchedule(K, i), отвечающая за формирование временного ключа K на каждом раунде функции E(K, m);
        /// </summary>
        /// <param name="K"></param>
        /// <param name="i"></param>
        void KeySchedule(byte[] K, int i)
        {
            byte[] constArr = new byte[64];
            var t = FastConst.KeySchedule.GetLength(1) * i;
            Buffer.BlockCopy(FastConst.KeySchedule, FastConst.KeySchedule.GetLength(1) * i, constArr, 0, 64);

            AddXor512(K, constArr, K);

            Stransform(K);
            Ptransform(K);
            Ltransform(K);
        }

        void Efunc(byte[] K, byte[] m, byte[] state)
        {
            AddXor512(m, K, state);

            for (int i = 0; i < 12; i++)
            {
                Stransform(state);
                Ptransform(state);
                Ltransform(state);

                KeySchedule(K, i);
                AddXor512(state, K, state);
            }
        }

        /// <summary>
        /// Функция сжатия.g(N, m, h)
        /// </summary>
        /// <param name="N"></param>
        /// <param name="h"></param>
        /// <param name="m"></param>
        private void СompresFunc(byte[] N, byte[] h, byte[] m)
        {
            byte[] temp = new byte[64];
            byte[] K = new byte[64];

            AddXor512(N, h, K);

            Stransform(K);
            Ptransform(K);
            Ltransform(K);

            Efunc(K, m, temp);

            AddXor512(temp, h, temp);
            AddXor512(temp, m, h);
        }

        private void AddModulo512(byte[] a, byte[] b, byte[] result)
        {
            int i = 0, t = 0;

            for (i = 63; i >= 0; i--)
            {
                t = a[i] + b[i] + (t >> 8);
                result[i] = (byte)(t & 0xFF);
            }
        }
        
        /// <summary>
        /// Хэш функция.
        /// </summary>
        /// <param name="IV"></param>
        /// <param name="message"></param>
        /// <param name="result"></param>
        private void HashAny(byte[] IV, byte[] message, byte[] result)
        {
            byte[] v512 = new byte[64]{
                0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
                0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
                0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
                0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x02,0x00
            }; 

            byte[] v0 = new byte[64];
            byte[] Sigma = new byte[64];
            byte[] N = new byte[64];

            byte[] temp = new byte[64];

            int len = message.Length;

            //Кратно 64.
            int multipleOf64 = len % 64 == 0 ? 0 : 1;

            //Шаг 2
            while (len >= 64)
            {
                Buffer.BlockCopy(message, len - 63 - multipleOf64, temp, 0, 64);
                
                СompresFunc(N, IV, temp);
                AddModulo512(N, v512, N);
                AddModulo512(Sigma, temp, Sigma);

                len -= 64;
            }

            Array.Clear(temp);
            
            Buffer.BlockCopy(message, 0, temp, 63 - len + multipleOf64, len);

            //Шаг 3
            temp[63 - len] |= (byte)(1 << (len * 8 & 0x7));

            СompresFunc(N, IV, temp);

            v512[63] = (byte)((len * 8) & 0xFF);
            v512[62] = (byte)((len * 8) >> 8);

            AddModulo512(N, v512, N);
            AddModulo512(Sigma, temp, Sigma);

            СompresFunc(v0, IV, N);
            СompresFunc(v0, IV, Sigma);

            Buffer.BlockCopy(IV, 0, result, 0, result.Length);
        }

        /// <summary>
        /// Хеш функция с длиной выхода 512 бит.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="result"></param>
        public void Hash512(byte[] message, byte[] result)
        {
            byte[] IV = new byte[64];
            HashAny(IV, message, result);
        }

        /// <summary>
        ///  Хеш функция с длиной выхода 256 бит.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="result"></param>
        public void Hash256(byte[] message, byte[] result)
        {
            byte[] IV = new byte[64]
            {
                0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01,
                0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01,
                0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01,
                0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01
            };

            HashAny(IV, message, result);
        }
    }
}