using CryptoRoomLib.Cipher3412.FastConst;
using System;

namespace CryptoRoomLib.Cipher3412
{
    /*
     * Алгоритмы шифрования ГОСТ 34.12.
     */
    public static class Logic3412
    {
        /// <summary>
        /// Выполняет L(линейное преобразование), S(нелинейное преобразование) преобразования.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="output"></param>
        private static void LsTransformation(U128t input, ref U128t output)
        {
            output.Low = 0;
            output.Hi = 0;

            for (byte index = 0; index < 16; ++index)
            {
                output.Low ^= PrecomputedLSTableLeft.Matrix[index, input.GetByte(index)];
                output.Hi ^= PrecomputedLSTableRight.Matrix[index, input.GetByte(index)];
            }
        }

        /// <summary>
        /// Выполняет итерацию сети Фейстеля.
        /// </summary>
        /// <param name="constantIndex"></param>
        /// <param name="leftIndex"></param>
        /// <param name="rigthIndex"></param>
        /// <param name="roudkeys"></param>
        static unsafe void FTransformation(int constantIndex, int leftIndex, int rigthIndex, ulong[] roudkeys)
        {
            fixed (ulong* buff = roudkeys)
            {
                U128t temp;
                U128t temp1 = new U128t();

                temp.Low = *(buff + leftIndex) ^ CipherConst3412.RoundConstantsLeft[constantIndex];
                temp.Hi = *(buff + leftIndex + 1) ^ CipherConst3412.RoundConstantsRight[constantIndex];

                LsTransformation(temp, ref temp1);

                *(buff + rigthIndex) ^= temp1.Low;
                *(buff + rigthIndex+1) ^= temp1.Hi;

                SwapBlocks(roudkeys, leftIndex, rigthIndex);
            }
        }

        /// <summary>
        /// Перестановка блоков в памяти.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="leftIndex"></param>
        /// <param name="rigthIndex"></param>
        private static unsafe void SwapBlocks(ulong[] buffer, int leftIndex, int rigthIndex)
        {
            fixed (ulong* buffPtr = buffer)
            {
                //left[0] = left[0] ^ right[0];
                //left[1] = left[1] ^ right[1];
                *(buffPtr+leftIndex) ^= *(buffPtr+rigthIndex);
                *(buffPtr + leftIndex + 1) ^= *(buffPtr + rigthIndex + 1);

                //right[0] = left[0] ^ right[0];
                //right[1] = left[1] ^ right[1];
                *(buffPtr + rigthIndex) ^= *(buffPtr + leftIndex);
                *(buffPtr + rigthIndex + 1) ^= *(buffPtr + leftIndex + 1);

                //left[0] = left[0] ^ right[0];
                //left[1] = left[1] ^ right[1];
                *(buffPtr + leftIndex) ^= *(buffPtr + rigthIndex);
                *(buffPtr + leftIndex + 1) ^= *(buffPtr + rigthIndex + 1);
            }
        }

        /// <summary>
        /// Выполняет быстрое нелинейное(S) преобразование 16 байтного блока.
        /// </summary>
        /// <param name="data"></param>
        internal static void Stransform(ref U128t data)
        {
            for (int byteIndex = 0; byteIndex < 16; ++byteIndex)
            {
                data.SetByte(byteIndex, CipherConst3412.Pi[data.GetByte(byteIndex)]);
            }
        }

        /// <summary>
        /// Выполняет обратное быстрое нелинейное(S) преобразование 16 байтного блока.
        /// </summary>
        /// <param name="data"></param>
        internal static void InversedStransform(ref U128t data)
        {
            for (int byteIndex = 0; byteIndex < 16; ++byteIndex)
            {
                data.SetByte(byteIndex, CipherConst3412.InversedPi[data.GetByte(byteIndex)]);
            }
        }

        /// <summary>
        /// Инверсное линейное(L), не линейное(S) преобразование. 
        /// </summary>
        internal static void InversedLStransformation(ref U128t input, ref U128t output)
        {
            output.Low = 0;
            output.Hi = 0;

            for (int index = 0; index < 16; ++index)
            {
                output.Low ^= PrecomputedInversedLSTableLeft.Matrix[index, input.GetByte(index)];
                output.Hi ^= PrecomputedInversedLSTableRight.Matrix[index, input.GetByte(index)];
            }
        }

        /// <summary>
        /// Формирую раундовые ключи шифрования(160 байт, 20(64-bit) чисел).
        /// </summary>
        /// <param name="key"></param>
        internal static void DeploymentEncryptionRoundKeys(byte[] key, ulong[] roundKeys)
        {
            //Копирую мастер ключ.
            Buffer.BlockCopy(key, 0, roundKeys, 0, 32);

            for (int nextKeyIndex = 2, constantIndex = 0; nextKeyIndex != 10; nextKeyIndex += 2)
            {
                //Переделать на Buffer.BlockCopy.
                Array.Copy(roundKeys, 2 * (nextKeyIndex - 2), roundKeys,
                2 * (nextKeyIndex), 4);

                for (int feistelRoundIndex = 0; feistelRoundIndex < 8; ++feistelRoundIndex)
                {
                    //Выполняет итерацию сети Фейстеля.
                    FTransformation(constantIndex++, 2 * (nextKeyIndex), 
                        2 * (nextKeyIndex + 1), roundKeys);
                }
            }
        }

        /// <summary>
        /// Формирую раундовые ключи декодирования(160 байт, 20(64-bit) чисел).
        /// </summary>
        /// <param name="key"></param>
        /// <param name="roundKeys"></param>
        internal static unsafe void DeploymentDecryptionRoundKeys(byte[] key, ulong[] roundKeys)
        {
            DeploymentEncryptionRoundKeys(key, roundKeys);
            U128t cache;
            U128t temp = new U128t();

            fixed (ulong* roundKeysPtr = roundKeys)
            {
                for (int roundKeyIndex = 1; roundKeyIndex <= 8; ++roundKeyIndex)
                {
                    cache.Low = *(roundKeysPtr + 2 * roundKeyIndex);
                    cache.Hi = *(roundKeysPtr + 2 * roundKeyIndex + 1);

                    Stransform(ref cache);
                    
                    InversedLStransformation(ref cache, ref temp);

                    *(roundKeysPtr + 2 * roundKeyIndex) = temp.Low;
                    *(roundKeysPtr + 2 * roundKeyIndex + 1) = temp.Hi;
                }
            }
        }

        /// <summary>
        /// Шифрует блок.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="roundKeys"></param>
        internal static unsafe void EncryptBlock(ref U128t data, ulong[] roundKeys)
        {
            int round = 0;
            U128t cache = new U128t();
            
            fixed (ulong* roundPtr = roundKeys)
            {
                for (; round < 10 - 1; ++round)
                {
                    cache.Low = data.Low ^ *(roundPtr + 2 * round);
                    cache.Hi = data.Hi ^ *(roundPtr + 2 * round + 1);

                    LsTransformation(cache, ref data);
                }

                data.Low ^= *(roundPtr + 2 * round);
                data.Hi ^= *(roundPtr + 2 * round + 1);
            }
        }

        internal static unsafe void DecryptBlock(ref U128t data, ulong[] roundKeys)
        {
            U128t cache = new U128t();
            int round = 10 - 1;

            fixed (ulong* roundPtr = roundKeys)
            {
                data.Low ^= *(roundPtr + 2 * round);
                data.Hi ^= *(roundPtr + 2 * round + 1);
                --round;

                Stransform(ref data);
                InversedLStransformation(ref data, ref cache);
                InversedLStransformation(ref cache, ref data);

                cache.Low = data.Low ^ *(roundPtr + 2 * round);
                cache.Hi = data.Hi ^ *(roundPtr + 2 * round + 1);
                --round;

                for (; round > 0; --round)
                {
                    InversedLStransformation(ref cache, ref data);
                    cache.Low = data.Low ^ *(roundPtr + 2 * round);
                    cache.Hi = data.Hi ^ *(roundPtr + 2 * round + 1);
                }

                InversedStransform(ref cache);
                data.Low = cache.Low ^ *(roundPtr + 2 * round);
                data.Hi = cache.Hi ^ *(roundPtr + 2 * round + 1);
            }
        }
    }
}
