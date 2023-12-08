namespace CryptoRoomLib.Cipher3412
{
    /// <summary>
    /// 128 битный тип.
    /// Внимание! Использование повторяющегося кода-без цепочки вызовов, и разбивки на мелкие методы,
    /// позволяет повысить производительность операций.
    /// Не пытайтесь вынести повторяющийся код в отдельные методы-это снизит производительность.
    /// </summary>
    struct U128t
    {
        public U128t(ulong lowBlock, ulong hiBlock)
        {
            Low = lowBlock;
            Hi = hiBlock;
        }
        
        /// <summary>
        /// Младший 64 битный блок.
        /// </summary>
        public ulong Low;

        /// <summary>
        /// Старший 64 битный блок.
        /// </summary>
        public ulong Hi;

        /// <summary>
        /// Возвращает байт из указанной позиции в 128битном(16байтном) числе.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public byte GetByte(int index)
        {
            if (index < 8)
            {
                return GetByte(index, Low);
            }
            else
            {
                return GetByte(index - 8, Hi);
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
        /// Устанавливает байт в указанную позицию 16 байтного числа.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value"></param>
        public void SetByte(int index, byte value)
        {
            if (index < 8)
            {
                SetByte(index, ref Low, ref value);
            }
            else
            {
                SetByte(index - 8, ref Hi, ref value);
            }
        }

        /// <summary>
        /// Устанавливает байт в указанную позицию числа.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="digit"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private void SetByte(int index, ref ulong digit, ref byte value)
        {
            switch (index)
            {
                case 0: digit &= 0xFFFFFFFFFFFFFF00; digit |= (ulong)(value); break;
                case 1: digit &= 0xFFFFFFFFFFFF00FF; digit |= ((ulong)value) << 8; break;
                case 2: digit &= 0xFFFFFFFFFF00FFFF; digit |= ((ulong)value) << 16; break;
                case 3: digit &= 0xFFFFFFFF00FFFFFF; digit |= ((ulong)value) << 24; break;
                case 4: digit &= 0xFFFFFF00FFFFFFFF; digit |= ((ulong)value) << 32; break;
                case 5: digit &= 0xFFFF00FFFFFFFFFF; digit |= ((ulong)value) << 40; break;
                case 6: digit &= 0xFF00FFFFFFFFFFFF; digit |= ((ulong)value) << 48; break;
                case 7: digit &= 0x00FFFFFFFFFFFFFF; digit |= ((ulong)value) << 56; break;

                default: return;
            }
        }
    }
}