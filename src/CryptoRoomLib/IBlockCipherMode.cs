using CryptoRoomLib.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoRoomLib
{
    /// <summary>
    /// Режим работы блочного шифра.
    /// </summary>
    public interface IBlockCipherMode
    {
        /// <summary>
        /// Сообщение об ошибке.
        /// </summary>
        string LastError { get; set; }

        /// <summary>
        /// Возвращает алгоритм блочного шифра.
        /// </summary>
        ICipherAlgoritm Algoritm { get; }
        
        /// <summary>
        /// Декодирует файл.
        /// </summary>
        /// <param name="cryptfile"></param>
        /// <param name="outfile"></param>
        /// <param name="setMaxBlockCount">Возвращает количество обрабатываемых блоков в файле.</param>
        /// <param name="endIteration">Возвращает номер обработанного блока. Необходим для движения ProgressBar на форме UI.</param>
        /// <param name="setDataSize">Возвращает размер декодируемых данных.</param>
       bool DecryptData(string cryptfile, string outfile, CommonFileInfo info,
            Action<ulong> setDataSize, Action<ulong> setMaxBlockCount, Action<ulong> endIteration);

        /// <summary>
        /// Кодирует данные.
        /// </summary>
        /// <param name="srcfile"></param>
        /// <param name="outfile"></param>
        /// <param name="setMaxBlockCount">Возвращает количество обрабатываемых блоков в файле.</param>
        /// <param name="endIteration">Возвращает номер обработанного блока. Необходим для движения ProgressBar на форме UI.</param>
        /// <param name="setDataSize">Возвращает размер декодируемых данных.</param>
        bool CryptData(string srcfile, string outfile, CommonFileInfo info, Action<ulong> setDataSize,
            Action<ulong> setMaxBlockCount, Action<ulong> endIteration);
    }
}
