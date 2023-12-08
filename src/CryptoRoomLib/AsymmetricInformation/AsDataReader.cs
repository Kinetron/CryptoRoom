namespace CryptoRoomLib.AsymmetricInformation
{
    /// <summary>
    /// Читает данные ассиметричной системы шифрования из файла.
    /// </summary>
    internal class AsDataReader
    {
        /// <summary>
        /// Номер байта в заголовке который передает тип блока.
        /// Заголовок содержит 5 байт - [тип][длина]
        /// </summary>
        private readonly int AsymmetricPosInHeadType = 0;

        /// <summary>
        /// Сообщение об ошибке.
        /// </summary>
        public string Error { get; private set; }

        /// <summary>
        /// Позиция в файле начала блока подписи.
        /// </summary>
        public long BeginSignBlockPosition { get; private set; }

        /// <summary>
        /// Считанные блоки данных
        /// </summary>
        public List<AsBlockData> Blocks;

        public AsDataReader()
        {
            Blocks = new List<AsBlockData>();
        }

        public void Read(FileStream inFile, ulong dataLen)
        {
            //Вычисляю позицию в которой заканчиваются шифрованные данные
            dataLen += (ulong)(FileFormat.BeginDataBlock + FileFormat.DataSizeInfo); //Позиция конца блока данных

            //Устанавливаю текущую позицию на начало блока данных.
            inFile.Position = (long)dataLen;

            byte[] title = new byte[FileFormat.AsymmetricHeadSize]; //Заголовок 5 байт [тип][длина] 

            int blockLen = 0; //Длина блока данных ассиметричной системы.
            
            //Файл может содержать произвольное количество блоков данных.
            while (inFile.Position < inFile.Length)
            {
                inFile.Read(title, 0, FileFormat.AsymmetricHeadSize);

                //Читаю блок данных.
                blockLen = DecodeAssymetricalDataLen(title);
                AsBlockData block = new AsBlockData();
                block.Type = (AsBlockDataTypes)title[AsymmetricPosInHeadType];
                block.Data = new byte[blockLen];

                //Позиция в файле начала блока данных.
                if (block.Type == AsBlockDataTypes.VectorR) BeginSignBlockPosition = inFile.Position - title.Length;

                inFile.Read(block.Data, 0, blockLen);
                Blocks.Add(block);
            }
        }

        /// <summary>
        /// Получаю длину блока ассиметричных данных.
        /// </summary>
        /// <param name="asTitle"></param>
        /// <returns></returns>
        private static int DecodeAssymetricalDataLen(byte[] asTitle)
        {
            //Заголовок 5 байт [тип][длина] 
            return BitConverter.ToInt32(asTitle, 1);
        }

        /// <summary>
        /// Проверяет наличие всех необходимых блоков в файле.
        /// </summary>
        /// <returns></returns>
        public bool CheckAll()
        {
            List<Func<bool>> checks = new List<Func<bool>>()
            {
                HasSign,
                HasSignKeyIndex
            };

            foreach (var check in checks)
            {
                if (!check()) return false;
            }

            return true;
        }

        /// <summary>
        /// Проверяет наличие цифровой подписи в файле.
        /// </summary>
        /// <returns></returns>
        private bool HasSign()
        {
            var cntR = Blocks.Where(x => x.Type == AsBlockDataTypes.VectorR).Count();
            var cntS = Blocks.Where(x => x.Type == AsBlockDataTypes.VectorS).Count();

            if (cntR != 1 || cntS != 1)
            {
                Error = "Ошибка Пр0:В файле отсутствуют блоки подписи.";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Проверка наличия информации об открытом ключе подписанта.
        /// </summary>
        /// <returns></returns>
        private bool HasSignKeyIndex()
        {
            var cnt = Blocks.Where(x => x.Type == AsBlockDataTypes.SignKeyIndex).Count();

            //На данный момент файл можно подписать только одной подписью.
            if (cnt != 1)
            {
                Error = "Ошибка Пр1:В файле отсутствуют сведения об открытом ключе проверки подписи.";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Возвращает шифрованный сеансовый ключ. Если ошибка-возвращает null.
        /// </summary>
        /// <returns></returns>
        public byte[] GetCryptedSessionKey()
        {
            var keyData = Blocks.Where(x => x.Type == AsBlockDataTypes.CryptSessionKey);

            if (!keyData.Any())
            {
                Error = "Ошибка AC2: Отсутствует сеансовый ключ.";
                return null;
            }

            if (keyData.Count() > 1)
            {
                Error = $"Ошибка AC3: В файле несколько {keyData.Count()} блоков данных о сеансовом ключе.";
                return null;
            }

            return keyData.First().Data;
        }

        /// <summary>
        /// Возвращает вектор подписи R.
        /// </summary>
        /// <returns></returns>
        public byte[] GetVectorR()
        {
            return Blocks.Where(x => x.Type == AsBlockDataTypes.VectorR).First().Data;
        }

        /// <summary>
        /// Возвращает вектор подписи S.
        /// </summary>
        /// <returns></returns>
        public byte[] GetVectorS()
        {
            return Blocks.Where(x => x.Type == AsBlockDataTypes.VectorS).First().Data;
        }
    }
}
