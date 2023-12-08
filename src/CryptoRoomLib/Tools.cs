using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CryptoRoomLib
{
    /// <summary>
    /// Содержит набор инструментов для отладки крипто алгоритмов.
    /// </summary>
    public static class Tools
    {
        /// <summary>
        /// Сохраняет массив 64 битных чисел в файл. Формирует табличку из 8 колонок.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="array"></param>
        public static void ULongArrayToFile(string name, ulong[] array)
        {
            int outSize = array.Length * 8;
            byte[] byteArray = new byte[outSize];
            Buffer.BlockCopy(array, 0, byteArray, 0, outSize);

            ByteArrayToFile(name, byteArray);
        }

        /// <summary>
        /// Сохраняет массив 8 битных значений в файл. Формирует табличку из 8 колонок.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="array"></param>
        public static void ByteArrayToFile(string name, byte[] array)
        {
            string hexValue = BitConverter.ToString(array);

            string[] str = hexValue.Split('-'); //По умолчанию  BitConverter ставит '-'.

            var sB = new StringBuilder();
            int count8 = 0;
            foreach (var item in str)
            {
                if (count8 > 7) //Формирую разбивку на блоки по 8 столбцов.
                {
                    sB.Append(Environment.NewLine);
                    count8 = 0;
                }

                sB.Append($"0x{item.ToLower()}, ");
                count8++;
            }

            File.WriteAllText(name, sB.ToString());
        }

        /// <summary>
        /// Преобразовывает 3х мерную([16, 256, 8])  8 битную 
        ///матрицу к двумерной 64битной([16, 256]). Результат работы сохраняется в файл 
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="fileName"></param>
        public static void Conver8bitMatrixTo64(byte[,,] matrix, string fileName)
        {
            int byteSize = sizeof(ulong);

            byte[] tmpArr = new byte[byteSize];

            var sB = new StringBuilder();
            sB.Append("{");
            sB.Append(Environment.NewLine);

            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                sB.Append("{");

                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    for (int k = 0; k < byteSize; k++)
                    {
                        tmpArr[k] = matrix[i, j, k];
                    }

                    var val64 = BitConverter.ToUInt64(tmpArr);
                    sB.Append(string.Format("0x{0:X}, ", val64).ToLower());
                    sB.Append(Environment.NewLine);
                }

                sB.Append("}, ");
                sB.Append(Environment.NewLine);
            }

            sB.Append(Environment.NewLine);
            sB.Append("}");

            File.WriteAllText(fileName, sB.ToString());
        }
    }
}
