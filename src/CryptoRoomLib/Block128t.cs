using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoRoomLib
{
    /// <summary>
    /// 128 битный тип данных.
    /// </summary>
    public struct Block128t
    {
        /// <summary>
        /// Младший 64 битный блок.
        /// </summary>
        public ulong Low;

        /// <summary>
        /// Старший 64 битный блок.
        /// </summary>
        public ulong Hi;

        public void Copy(ref Block128t data)
        {
            Low = data.Low;
            Hi = data.Hi;
        }

        public void FromArray(byte[] buffer)
        {
            Low = BitConverter.ToUInt64(buffer, 0);
            if (buffer.Length > 8)
            {
                Hi = BitConverter.ToUInt64(buffer, 8);
            }
        }

        /// <summary>
        /// Копирует данные в массив.
        /// </summary>
        /// <param name="buffer"></param>
        public void ToArray(byte[] buffer)
        {
            CopyBlock(ref Low, buffer, 0);
            CopyBlock(ref Hi, buffer, 8);
        }

        private void CopyBlock(ref ulong Dig, byte[] buffer, int pos)
        {
            for (int i = 0; i < 8; i++)
            {
                buffer[pos + i] = GetByte(i, Dig);
            }
        }

        /// <summary>
        /// Возвращает байт из 64 битного(8байтного) числа.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="digit"></param>
        /// <returns></returns>
        private byte GetByte(int index, ulong digit)
        {
            switch (index)
            {
                case 0: return (byte)(digit & 0xFF);
                case 1: return (byte)((digit & 0xFF00) >> 8);
                case 2: return (byte)((digit & 0xFF0000) >> 16);
                case 3: return (byte)((digit & 0xFF000000) >> 24);
                case 4: return (byte)((digit & 0xFF00000000) >> 32);
                case 5: return (byte)((digit & 0xFF0000000000) >> 40);
                case 6: return (byte)((digit & 0xFF000000000000) >> 48);
                case 7: return (byte)((digit & 0xFF00000000000000) >> 56);
                default: return 0;
            }
        }

        /// <summary>
        /// Сравнивает число с собой.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool Compare(Block128t value)
        {
            return value.Low == Low && value.Hi == Hi;
        }
    }
}
