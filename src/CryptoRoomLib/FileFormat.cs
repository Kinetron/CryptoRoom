using CryptoRoomLib.AsymmetricInformation;
using CryptoRoomLib.Models;
using System.Text;
using static System.Reflection.Metadata.BlobBuilder;

namespace CryptoRoomLib
{
    /// <summary>
    /// Разметка кодируемых/декодируемых файлов.
    /// </summary>
    internal static class FileFormat
    {
        /*
         * Формат заголовка файла:
         * [7 байт версия][Заголовок 47 байт][ХЭШ ГОСТ 34.11 сообщения идущего за ним 64байт][Размер блока шифрованных данных 8байт]
	        7 + 47 + 64 = 118
        */

        /// <summary>
        /// Позиция в файле с которой начинается кодируемая информация.
        /// В начале идет определение размера блока данных.
        /// </summary>
        internal static readonly int BeginDataBlock = 118;

        /// <summary>
        /// ...[Размер блока шифрованных данных 8байт]
        /// Размер блока содержащего информацию о длине кодируемых данных(байт).
        /// </summary>
        internal static readonly int DataSizeInfo = 8;

        /// <summary>
        /// Длина начального текста.
        /// </summary>
        internal const int FirstTextLen = 7;

        /// <summary>
        /// Первые 7 байт содержащих что то вроде версии ключа.
        /// </summary>
        internal static readonly byte[] FirstText = new byte[FirstTextLen] { 0xf9, 0xc5, 0xa8, 0xd3, 0x47, 0xb6, 0x3a };

        /// <summary>
        /// Длина текста названия программы, используемое при создании шифруемых файлов.
        /// </summary>
        internal static readonly int ProgramTextForFileLen = 47;

        /// <summary>
        /// Название программы, используемое при создании шифруемых файлов.
        /// </summary>
        public static string ProgramTextForFile
        {
            get
            {
                string text = "I'll always have a little red rose in my heart ";
                if (text.Length != ProgramTextForFileLen)
                {
                    throw new ArgumentException("Bad string size.", nameof(ProgramTextForFile));
                }
                return text;
            }
        }

        /// <summary>
        /// Размер блока – хеш всего файла.
        /// </summary>
        internal static readonly int HashDataBlockLen = 64;

        /*
        * Формат заголовка файла шифруемого файла:
        * [7 байт версия][Заголовок 46 байт][ХЭШ ГОСТ 34.11 сообщения идущего за ним 64байт] далее идет [Размер блока шифрованных данных 8байт]
           7 + 46 + 64 = 117
        */

        /// <summary>
        /// Позиция в файле с которой начинается кодируемая информация.
        /// В начале идет определение размера блока данных.
        /// </summary>
        internal static readonly int CryptFileHeadLen = FirstTextLen + ProgramTextForFileLen + HashDataBlockLen;
        
        /// <summary>
        /// Размер блока содержащего начальный вектор(байт).
        /// </summary>
        internal static readonly int IvSize = 32;

        /// <summary>
        /// Размер блока содержащего контрольную сумму.
        /// </summary>
        internal static readonly int EndInfoSize = 118;

        /// <summary>
        /// Размер заголовка данных ассиметричной системы.
        /// Заголовок 5 байт [тип][длина]
        /// </summary>
        internal static readonly int AsymmetricHeadSize = 5;
        
        /// <summary>
        /// Последнее сообщение об ошибке.
        /// </summary>
        public static string LastError { get; set; }
        
        /// <summary>
        /// Считывает из файла размер блока шифрованных данных.
        /// </summary>
        /// <param name="inFile"></param>
        /// <returns></returns>
        internal static ulong ReadDataSize(FileStream inFile)
        {
            //Устанавливаю текущую позицию(считываемый файл) на начало определения размера блока данных
            inFile.Position = FileFormat.BeginDataBlock;

            //Считывание размера блока данных
            byte[] arrLen = new byte[DataSizeInfo];

            inFile.Read(arrLen, 0, DataSizeInfo);

            //Получаю размер всего блока данных.
           return BitConverter.ToUInt64(arrLen, 0);
        }

        /// <summary>
        /// Считывает начальный вектор из файла.
        /// </summary>
        internal static byte[] ReadIV(FileStream inFile, ulong dataLen)
        {
            //Вычисляю позицию в которой заканчиваются данные и начинается iv
            dataLen = dataLen + (ulong)(EndInfoSize + DataSizeInfo - IvSize);
            inFile.Position = (long)dataLen;

            byte[] iv = new byte[IvSize];
            inFile.Read(iv, 0, IvSize);

            return iv;
        }

        /// <summary>
        /// Формирует заголовок шифруемого файла.
        /// </summary>
        /// <returns></returns>
        internal static byte[] CreateCryptFileTitle(ulong fileLen, ICipherAlgoritm algoritm)
        {
            byte[] fileTitle = new byte[CryptFileHeadLen + DataSizeInfo];

            //Формирую заголовок файла [7-байт служебной информации]
            Buffer.BlockCopy(FirstText, 0, fileTitle,0, FirstText.Length);

            //Название программы [Заголовок 47байт]
            byte[] programText = Encoding.ASCII.GetBytes(ProgramTextForFile);
            Buffer.BlockCopy(programText, 0, fileTitle, FirstText.Length, programText.Length);

            //Хеш файла[64]. В текущей версии не считается – так как есть подпись.
            byte[] hash = new byte[HashDataBlockLen];
            Buffer.BlockCopy(hash, 0, fileTitle, FirstText.Length + programText.Length, hash.Length);

            int curentArrayPos = FirstText.Length + programText.Length + HashDataBlockLen;

            //Преобразовывает длину блока данных в массив из 8 байт.

            //Модификация размера. 
            ulong tail = fileLen % (ulong)algoritm.BlockSize;
            if (tail == 0) fileLen += (ulong)algoritm.BlockSize + (ulong)IvSize; //Если размер файла кратен 16: 16 байт длина, 32 iv
            else
            {
                fileLen -= tail;
                //Сообщение дополняется блоком 16 байт  если его длина не кратна 16.
                fileLen += (ulong)algoritm.BlockSize * 2 + (ulong)IvSize;
            }
            
            byte[] fileLenInfo = BitConverter.GetBytes(fileLen);
            Buffer.BlockCopy(fileLenInfo, 0, fileTitle, curentArrayPos, fileLenInfo.Length);

            return fileTitle;
        }

        /// <summary>
        /// Получает основную информацию из файла.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static CommonFileInfo ReadFileInfo(string fileName)
        {
            using (FileStream inFile = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                CommonFileInfo info = new CommonFileInfo();
                info.UserDataSize = FileFormat.ReadDataSize(inFile); //Считывает из файла размер блока шифрованных данных.

                //Чтение данных ассиметричной системы. Получение сессионного ключа.
                var asymmetricData = new AsDataReader();
                asymmetricData.Read(inFile, info.UserDataSize);
                if (!asymmetricData.CheckAll())
                {
                    LastError = asymmetricData.Error;
                    return null;
                }

                info.CryptedSessionKey = asymmetricData.GetCryptedSessionKey();
                info.VectorR = asymmetricData.GetVectorR();
                info.VectorS = asymmetricData.GetVectorS();
                info.BeginSignBlockPosition = asymmetricData.BeginSignBlockPosition;

                if (info.CryptedSessionKey == null)
                {
                    LastError = asymmetricData.Error;
                    return null;
                }
                
                info.Iv = FileFormat.ReadIV(inFile, info.UserDataSize); //Считывает значение вектора iv.
                info.BeginDataPosition = FileFormat.BeginDataBlock + FileFormat.DataSizeInfo;

                return info;
            }
        }
    }
}
