using CryptoRoomLib.Models;

namespace CryptoRoomLib.CipherMode3413
{
    /// <summary>
    /// Реализация режима работы блочного шифра - режим простой замены с зацеплением,
    /// согласно ГОСТ 34.13-2015
    /// (Cipher Block Chaining, СВС)
    /// </summary>
    public class ModeCBC : IBlockCipherMode
    {
        private readonly ICipherAlgoritm _algoritm;

        /// <summary>
        /// Сообщение об ошибке.
        /// </summary>
        public string LastError { get; set; }

        public ICipherAlgoritm Algoritm
        {
            get
            {
                return _algoritm;
            }
        }

        public ModeCBC(ICipherAlgoritm algoritm)
        {
            _algoritm = algoritm;
        }

        /// <summary>
        /// Итерация в режиме простой замены(ГОСТп5.4стр16)
        /// P-открытый текст,MSB-значением n разрядов регистра сдвига с большими номерами,
        /// C-результирующий шифртекст
        ///
        /// part 16 байт - Блок сообщения подлежащего шифрованию
        /// MSB 16 байт - значением n разрядов регистра сдвига с большими номерами
        /// C_block[16] - результирующий шифротекст
        /// </summary>
        internal void DeсryptIterationCBC(ref Block128t part, Block128t msb, ref Block128t cBlock)
        {
            _algoritm.DecryptBlock(ref cBlock); //Расшифровываю блок.

            //Складываю  блоки.
            //Использование двух строк вместо вызова метода(XorBlocks) ускоряет работу.
            part.Low = cBlock.Low ^ msb.Low;
            part.Hi = cBlock.Hi ^ msb.Hi;
        }

        /// <summary>
        /// Итерация в режиме простой замены(ГОСТп5.4стр16)
        /// P-открытый текст,MSB-значением n разрядов регистра сдвига с большими номерами,
        /// C-результирующий шифртекст
        ///
        /// part 16 байт - Блок сообщения подлежащего шифрованию
        /// MSB 16 байт - значением n разрядов регистра сдвига с большими номерами
        /// C_block[16] - результирующий шифротекст
        /// </summary>
        internal void CryptIterationCBC(ref Block128t part, Block128t lsb, ref Block128t cBlock)
        {
            //Складываю  блоки.
            cBlock.Low = part.Low ^ lsb.Low;
            cBlock.Hi = part.Hi ^ lsb.Hi;

            _algoritm.EncryptBlock(ref cBlock); //Шифрую блок.
        }

        /// <summary>
        /// Складывает блоки длиной 16 байт по модулю 2. Метод используется только для тестов.
        /// </summary>
        /// <param name="block1"></param>
        /// <param name="block2"></param>
        /// <param name="result"></param>
        internal void XorBlocks(ref Block128t block1, ref Block128t block2, ref Block128t result)
        {
            result.Low = block1.Low ^ block2.Low;
            result.Hi = block1.Hi ^ block2.Hi;
        }

        /// <summary>
        /// Выполняет одну итерацию сцепки блоков.
        /// </summary>
        internal void IterationCBC(ref Register256t register, ref Block128t tmpBlock, ref Block128t lsb, ref Block128t сBlock, ref Block128t pBlock)
        {
            tmpBlock.Copy(ref сBlock); //Копирую С, так как оно измениться.
            lsb = register.LSB; //Получаю младшие байты LSB из регистра
            DeсryptIterationCBC(ref pBlock, lsb, ref сBlock); //Расшифровываю

            //Быстрый сдвиг.
            register.RightShift(); //Сдвигаю регистр R вправо на 16 байт, старшие ячейки заполняю значением шифротекста 

            //Заполнение старших бит значением блока шифротекста
            register.MSB = tmpBlock;
        }
        
        /// <summary>
        /// Декодирует данные.
        /// </summary>
        /// <param name="cryptfile"></param>
        /// <param name="outfile"></param>
        /// <param name="setMaxBlockCount">Возвращает количество обрабатываемых блоков в файле.</param>
        /// <param name="endIteration">Возвращает номер обработанного блока. Необходим для движения ProgressBar на форме UI.</param>
        /// <param name="setDataSize">Возвращает размер декодируемых данных.</param>
        public bool DecryptData(string cryptfile, string outfile, CommonFileInfo info,
            Action<ulong> setDataSize ,Action<ulong> setMaxBlockCount, Action<ulong> endIteration)
        {
            Register256t register = new Register256t();// Регистр размером m = kn =  2*16
            Block128t lsb = new Block128t();//значением n разрядов регистра сдвига с большими номерами

            Block128t сBlock = new Block128t(); //Входящий шифротекст.
            Block128t pBlock = new Block128t(); //Исходящий декодированный текст.
            Block128t tmpBlock =  new Block128t(); //Хранит временные данные
            
            byte[] readBuffer = new byte[_algoritm.BlockSize];
            byte[] writeBuffer = new byte[_algoritm.BlockSize];

            Span<byte> buffSpan = new Span<byte>(readBuffer);
            Span<byte> writeSpan = new Span<byte>(writeBuffer);
            
            //Добавить проверку на out of range.

            using (FileStream outFile = new FileStream(outfile, FileMode.Create, FileAccess.Write))
            using (FileStream inFile = new FileStream(cryptfile, FileMode.Open, FileAccess.Read))
            {
                _algoritm.DeployDecryptRoundKeys(info.SessionKey);

                //Читаем с места где содержатся данные.
                inFile.Position = info.BeginDataPosition;
                inFile.Read(buffSpan);
                сBlock.FromArray(readBuffer);

                register.FromArray(info.Iv); //Заполнение регистра данными IV
                IterationCBC(ref register, ref tmpBlock, ref lsb, ref сBlock, ref pBlock);

                //Первый блок является длиной данных-вынести в логику формирования файла.
                ulong dataLen = pBlock.Low;//Длина блока данных в байтах. 
                ulong blockCount = dataLen / (ulong)_algoritm.BlockSize; //Количество блоков в сообщении

                setDataSize(dataLen);
                setMaxBlockCount(blockCount);

                //Цикл расшифровывания блоков	
                ulong blockNum = 0;
                for (; blockNum < blockCount; blockNum++)
                {
                    inFile.Read(buffSpan);
                    сBlock.FromArray(readBuffer);

                    IterationCBC(ref register, ref tmpBlock, ref lsb, ref сBlock, ref pBlock);

                    pBlock.ToArray(writeBuffer);
                    outFile.Write(writeSpan);

                    endIteration(blockNum);
                }

                //Кратна ли длина сообщения размеру блока шифра?
                int rLen = (int)(dataLen % (ulong)_algoritm.BlockSize);

                //Длина сообщения не кратна размеру блока шифра
                if (rLen != 0)
                {
                    //Считываю очередной блок
                    inFile.Read(buffSpan);
                    сBlock.FromArray(readBuffer);

                    IterationCBC(ref register, ref tmpBlock, ref lsb, ref сBlock, ref pBlock);

                    outFile.Write(writeBuffer,  0, rLen);

                    endIteration(blockNum);
                }

                inFile.Close();
                outFile.Close();
            }

            return true;
        }

        /// <summary>
        /// Кодирует данные.
        /// </summary>
        /// <param name="srcfile"></param>
        /// <param name="outfile"></param>
        /// <param name="setMaxBlockCount">Возвращает количество обрабатываемых блоков в файле.</param>
        /// <param name="endIteration">Возвращает номер обработанного блока. Необходим для движения ProgressBar на форме UI.</param>
        /// <param name="setDataSize">Возвращает размер декодируемых данных.</param>
        public bool CryptData(string srcfile, string outfile, CommonFileInfo info, Action<ulong> setDataSize,
            Action<ulong> setMaxBlockCount, Action<ulong> endIteration)
        {
            ulong srcFileLen = info.FileLength; //Размер исходного файла в байтах

            //Нeт механизма обработки файлов менее BlockSize. При желании можно сделать.
            if (srcFileLen < (ulong)_algoritm.BlockSize)
            {
                LastError = $"Файл {Path.GetFileName(srcfile)} имеет размер менее {_algoritm.BlockSize} байт. Шифрование файла невозможно.";
                return false;
            }
            
            byte[] iv = CipherTools.GenerateRand(32); //Формирую случайный начальный вектор(32байта).
           
            _algoritm.DeployСryptRoundKeys(info.SessionKey);

            Register256t register = new Register256t();// Регистр размером m = kn =  2*16
            Block128t lsb = new Block128t();//значением n разрядов регистра сдвига с большими номерами

            Block128t cBlock = new Block128t(); //Входящий шифротекст.
            Block128t pBlock = new Block128t(); //Исходящий декодированный текст.
            Block128t tmpBlock = new Block128t(); //Хранит временные данные
            
            register.FromArray(iv); //Заполнение регистра данными IV
            
            //Шифрование размера файла
            byte[] fileLen = BitConverter.GetBytes(srcFileLen);
            tmpBlock.FromArray(fileLen);

            lsb = register.LSB;//Получаю младшие байты LSB из регистра
            CryptIterationCBC(ref tmpBlock, lsb, ref cBlock);
            
            register.RightShift(); //Сдвигаю регистр R влево на 16 байт.

            //Заполнение старших бит значением блока шифротекста
            register.MSB = cBlock;


            byte[] readBuffer = new byte[_algoritm.BlockSize];
            byte[] writeBuffer = new byte[_algoritm.BlockSize];

            Span<byte> readSpan = new Span<byte>(readBuffer);
            Span<byte> writeSpan = new Span<byte>(writeBuffer);

            using (FileStream outFile = new FileStream(outfile, FileMode.Create, FileAccess.Write))
            using (FileStream inFile = new FileStream(srcfile, FileMode.Open, FileAccess.Read))
            {
                outFile.Write(info.FileHead);

                ulong blockCount = srcFileLen / (ulong)_algoritm.BlockSize; //Количество блоков в сообщении

                setDataSize(srcFileLen);
                setMaxBlockCount(blockCount);

                cBlock.ToArray(writeBuffer);
                outFile.Write(writeSpan);
               
                //Цикл шифровывания блоков	
                ulong blockNum = 0;
                for (; blockNum < blockCount; blockNum++)
                {
                    inFile.Read(readSpan);
                    tmpBlock.FromArray(readBuffer);

                    lsb = register.LSB;//Получаю младшие байты LSB из регистра
                    CryptIterationCBC(ref tmpBlock, lsb, ref cBlock);
                    register.RightShift(); //Сдвигаю регистр R влево на 16 байт.

                    //Заполнение старших бит значением блока шифротекста
                    register.MSB = cBlock;
                    
                    cBlock.ToArray(writeBuffer);
                    outFile.Write(writeSpan);

                    endIteration(blockNum);
                }
                
                //Кратна ли длина сообщения размеру блока шифра?
                int rLen = (int)(srcFileLen % (ulong)_algoritm.BlockSize);

                //Длина сообщения не кратна размеру блока шифра
                if (rLen != 0)
                {
                    //Считываю очередной блок
                    inFile.Read(readSpan);
                    tmpBlock.FromArray(readBuffer);

                    PaddingMessage(rLen, readBuffer);
                    
                    lsb = register.LSB;//Получаю младшие байты LSB из регистра
                    CryptIterationCBC(ref tmpBlock, lsb, ref cBlock);
                    register.RightShift(); //Сдвигаю регистр R влево на 16 байт.

                    //Заполнение старших бит значением блока шифротекста
                    register.MSB = cBlock;

                    cBlock.ToArray(writeBuffer);
                    outFile.Write(writeBuffer);

                    endIteration(blockNum);
                }
                
                outFile.Write(iv);
                outFile.Write(info.BlockData);
                outFile.Close();
            }

            return true;
        }

        /// <summary>
        /// Дополняю сообщение согласно ГОСТ 34.13-2015 4.1.2 Процедура 2.
        /// </summary>
        private void PaddingMessage(int tailLen, byte[] buffer)
        {
            //Ситуация когда длина оставшегося сообщения =15
            if (tailLen == 15)
            {
                buffer[15] = 1; //Дополняю блок 1
                return;
            }

            //Сообщение не равно 15
            buffer[tailLen] = 1;

            for (int i = tailLen + 1; i < 16; i++)
            {
                buffer[i] = 0;
            }
        }
    }
}
