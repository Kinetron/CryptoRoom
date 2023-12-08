using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VanishBox
{
    public interface ICipherService
    {
        /// <summary>
        /// Сообщение об ошибке.
        /// </summary>
        public string LastError { get; set; }

        /// <summary>
        /// Проверяет пароль для ключа.
        /// </summary>
        /// <returns></returns>
        bool CheckPassword(string password, string pathToKey);

        /// <summary>
        /// Шифрует/расшифровывает файлы.
        /// </summary>
        /// <param name="paths"></param>
        /// <param name="direction"></param>
        /// <param name="sendInfo"></param>
        /// <param name="progressIteration"></param>
        /// <param name="textIteration"></param>
        /// <returns></returns>
        public bool RunOperation(string[] paths, bool direction, Action<string> sendInfo,
            Action<int> progressIteration, Action<string> textIteration);
    }
}
