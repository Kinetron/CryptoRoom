using CryptoRoomLib.AsymmetricInformation;
using System;
using CryptoRoomLib.Sign;
using CryptoRoomLib.Models;
using System.Diagnostics;

namespace CryptoRoomLib
{
    /// <summary>
    /// Содержит методы для шифрования файлов.
    /// </summary>
    public class CipherWorker
    {
        /// <summary>
        /// Режим работы блочного шифра.
        /// </summary>
        private readonly IBlockCipherMode _blockCipherMode;

        /// <summary>
        /// Последнее сообщение об ошибке.
        /// </summary>
        public string LastError { get; set; }

        public CipherWorker(IBlockCipherMode blockCipherMode)
        {
            _blockCipherMode = blockCipherMode; 
        }

        /// <summary>
        /// Расшифровывает файл.
        /// </summary>
        /// <param name="setMaxBlockCount">Возвращает количество обрабатываемых блоков в файле.</param>
        /// <param name="endIteration">Возвращает номер обработанного блока. Необходим для движения ProgressBar на форме UI.</param>
        /// <param name="setDataSize">Возвращает размер декодируемых данных.</param>
        /// <returns></returns>
        public bool DecryptingFile(string srcPath, string resultFileName, byte[] privateAsymmetricKey,
            string ecOid, EcPoint ecPublicKey,
            Action<ulong> setDataSize, Action<ulong> setMaxBlockCount, Action<ulong> endIteration,
            Action<string> sendProcessText)
        {
            var commonInfo = ReadFileInfo(srcPath, privateAsymmetricKey);
            if (commonInfo == null) return false;

            sendProcessText("Проверка подписи");
            var signTools = new SignTools();
            if (!signTools.CheckSign(srcPath, commonInfo, ecOid, ecPublicKey))
            {
                LastError = signTools.LastError;
                return false;
            }

            sendProcessText("Расшифровывание файла");
            bool result = _blockCipherMode.DecryptData(srcPath, resultFileName, commonInfo,
                setDataSize, setMaxBlockCount, endIteration);

            sendProcessText("Завершено");
            if (!result)
            {
                LastError = _blockCipherMode.LastError;
                return false;
            }

            return true;
        }

        /// <summary>
        /// Расшифровывает файл. Параллельно выполняя проверку подписи и расшифровывание файла.
        /// </summary>
        /// <param name="setMaxBlockCount">Возвращает количество обрабатываемых блоков в файле.</param>
        /// <param name="endIteration">Возвращает номер обработанного блока. Необходим для движения ProgressBar на форме UI.</param>
        /// <param name="setDataSize">Возвращает размер декодируемых данных.</param>
        /// <returns></returns>
        public bool DecryptingFileParallel(string srcPath, string resultFileName, byte[] privateAsymmetricKey,
            string ecOid, EcPoint ecPublicKey,
            Action<ulong> setDataSize, Action<ulong> setMaxBlockCount, Action<ulong> endIteration,
            Action<string> sendProcessText)
        {
            var commonInfo = ReadFileInfo(srcPath, privateAsymmetricKey);
            if (commonInfo == null) return false;

            string fileName = Path.GetFileName(srcPath);

            var signTask = Task<bool>.Run(() =>
                {
                    return MeasureTime(
                        $"Начата проверка подписи {fileName} ...", 
                        $"Ошибка проверки подписи файла {fileName}",
                            $"Проверка подписи {fileName}  завершена.",
                        () =>
                        {
                            var signTools = new SignTools();
                            if (!signTools.CheckSign(srcPath, commonInfo, ecOid, ecPublicKey))
                            {
                                LastError = signTools.LastError;
                                return signTools.LastError;
                            }

                            return string.Empty;
                        }, 
                        sendProcessText
                    );
                }
            );

            var decryptTask = Task<bool>.Run(() =>
            {
                return MeasureTime(
                    $"Начата расшифровка файла {fileName} ...",
                    $"Ошибка расшифровки файла {fileName}",
                        $"Расшифровка файла {fileName} завершена.",
                    () =>
                    {
                        bool result = _blockCipherMode.DecryptData(srcPath, resultFileName, commonInfo,
                            setDataSize, setMaxBlockCount, endIteration);

                        if (!result)
                        {
                            LastError = _blockCipherMode.LastError;
                            return _blockCipherMode.LastError;
                        }

                        return string.Empty;
                    },
                    sendProcessText
                );
            });
            
            Task.WaitAll(signTask, decryptTask);
          
            return signTask.Result && decryptTask.Result;
        }

        /// <summary>
        /// Выполняет функцию, замеряя время выполнения.
        /// </summary>
        /// <param name="beginMessage"></param>
        /// <param name="errorMessage"></param>
        /// <param name="success"></param>
        /// <param name="func"></param>
        /// <param name="sendProcessText"></param>
        /// <returns></returns>
        private bool MeasureTime(string beginMessage, string errorMessage, string success, Func<string> func, Action<string> sendProcessText)
        {
            sendProcessText(beginMessage);

            Stopwatch stopwatch = new Stopwatch(); //Измерение времени операции.
            stopwatch.Start();
            var error = func();
            stopwatch.Stop();

            if (!string.IsNullOrEmpty(error))
            {
                sendProcessText($"{errorMessage} : {error}");
                return false;
            }
            
            TimeSpan time = TimeSpan.FromMilliseconds(stopwatch.ElapsedMilliseconds);
            sendProcessText($"{success} Затрачено {String.Format("{0:hh\\:mm\\:ss\\:fff}", time)} .");
            
            return true;
        }

        /// <summary>
        /// Считывает из файла основные данные.
        /// </summary>
        /// <returns></returns>
        private CommonFileInfo ReadFileInfo(string srcPath, byte[] privateAsymmetricKey)
        {
            var commonInfo = FileFormat.ReadFileInfo(srcPath);
            if (commonInfo == null)
            {
                LastError = FileFormat.LastError;
                return null;
            }

            KeyDecoder kd = new KeyDecoder();
            byte[] decryptKey;

            var decryptResult = kd.DecryptSessionKey(privateAsymmetricKey, commonInfo.CryptedSessionKey,
                out decryptKey);

            commonInfo.SessionKey = decryptKey;

            //В реальном коде вывести сообщение об ошибке.
            if (!decryptResult)
            {
                LastError = kd.Error;
                return null;
            }

            return commonInfo;
        }

        /// <summary>
        /// Шифрует файл.
        /// </summary>
        /// <param name="setMaxBlockCount">Возвращает количество обрабатываемых блоков в файле.</param>
        /// <param name="endIteration">Возвращает номер обработанного блока. Необходим для движения ProgressBar на форме UI.</param>
        /// <param name="ecOid">Идентификатор эллиптической кривой.</param>
        /// <param name="setDataSize">Возвращает размер декодируемых данных.</param>
        /// <returns></returns>
        public bool CryptingFile(string srcPath, string resultFileName, byte[] publicAsymmetricKey, string ecOid, byte[] signPrivateKey, EcPoint ecPublicKey,
            Action<ulong> setDataSize, Action<ulong> setMaxBlockCount,
            Action<ulong> endIteration, Action<string> sendMessage)
        {
            string fileName = Path.GetFileName(srcPath);
            sendMessage($"Шифрование файла {fileName}...");

            CommonFileInfo info = new CommonFileInfo();

            FileInfo fi = new FileInfo(srcPath);
            info.FileLength = (ulong)fi.Length; //Размер исходного файла в байтах.
            
            info.FileHead = FileFormat.CreateCryptFileTitle(info.FileLength, _blockCipherMode.Algoritm);
            info.SessionKey = CipherTools.GenerateRand(32); //Формирую случайное число размером 32байта, которое является сеансовым ключом.
            KeyDecoder kd = new KeyDecoder();
            byte[] cryptKey;

            var encryptResult = kd.EncryptSessionKey(publicAsymmetricKey, info.SessionKey, out cryptKey);
            if (!encryptResult)
            {
                LastError = kd.Error;
                return false;
            }

            //Сведения о шифрованном сеансовом ключе, в виде блоков ASN1.
            var asWriter = new AsDataWriter();
            asWriter.AddRsaHash(new byte[37]);
            asWriter.AddCryptedBlockKey(cryptKey);

            info.BlockData = asWriter.GetData();


            bool result = _blockCipherMode.CryptData(srcPath, resultFileName, info, 
                setDataSize, setMaxBlockCount, endIteration);

            if (!result)
            {
                LastError = _blockCipherMode.LastError;
                return false;
            }

            sendMessage($"Шифрование файла {fileName} завершено.");

            sendMessage($"Подпись файла {fileName} ...");
            var signTools = new SignTools();
            if (!signTools.SignFile(resultFileName, sendMessage, ecOid, signPrivateKey, ecPublicKey))
            {
                LastError = signTools.LastError;
                return false;
            }
            sendMessage($"Подпись файла {fileName} завершена.");

            return true;
        }
    }
}
