using CryptoRoomLib.Models;
using CryptoRoomLib.Sign;

namespace CryptoRoomLib;

public interface ICipherWorker
{
	/// <summary>
	/// Последнее сообщение об ошибке.
	/// </summary>
	string LastError { get; set; }

	/// <summary>
	/// Расшифровывает файл.
	/// </summary>
	/// <param name="setMaxBlockCount">Возвращает количество обрабатываемых блоков в файле.</param>
	/// <param name="endIteration">Возвращает номер обработанного блока. Необходим для движения ProgressBar на форме UI.</param>
	/// <param name="setDataSize">Возвращает размер декодируемых данных.</param>
	/// <returns></returns>
	bool DecryptingFile(string srcPath, string resultFileName, byte[] privateAsymmetricKey,
		string ecOid, EcPoint ecPublicKey,
		Action<ulong> setDataSize, Action<ulong> setMaxBlockCount, Action<ulong> endIteration,
		Action<string> sendProcessText);

	/// <summary>
	/// Расшифровывает файл. Параллельно выполняя проверку подписи и расшифровывание файла.
	/// </summary>
	/// <param name="setMaxBlockCount">Возвращает количество обрабатываемых блоков в файле.</param>
	/// <param name="endIteration">Возвращает номер обработанного блока. Необходим для движения ProgressBar на форме UI.</param>
	/// <param name="setDataSize">Возвращает размер декодируемых данных.</param>
	/// <returns></returns>
	bool DecryptingFileParallel(string srcPath, string resultFileName, byte[] privateAsymmetricKey,
		string ecOid, EcPoint ecPublicKey, byte[] signPrivateKey,
		Action<ulong> setDataSize, Action<ulong> setMaxBlockCount, Action<ulong> endIteration,
		Action<string> sendProcessText);

	/// <summary>
	/// Выполняет функцию, замеряя время выполнения.
	/// </summary>
	/// <param name="beginMessage"></param>
	/// <param name="errorMessage"></param>
	/// <param name="success"></param>
	/// <param name="func"></param>
	/// <param name="sendProcessText"></param>
	/// <returns></returns>
	bool MeasureTime(string beginMessage, string errorMessage, string success, Func<string> func, Action<string> sendProcessText);

	/// <summary>
	/// Считывает из файла основные данные.
	/// </summary>
	/// <returns></returns>
	CommonFileInfo ReadFileInfo(string srcPath, byte[] privateAsymmetricKey);

	/// <summary>
	/// Шифрует файл.
	/// </summary>
	/// <param name="setMaxBlockCount">Возвращает количество обрабатываемых блоков в файле.</param>
	/// <param name="endIteration">Возвращает номер обработанного блока. Необходим для движения ProgressBar на форме UI.</param>
	/// <param name="ecOid">Идентификатор эллиптической кривой.</param>
	/// <param name="setDataSize">Возвращает размер декодируемых данных.</param>
	/// <returns></returns>
	bool CryptingFile(string srcPath, string resultFileName, byte[] publicAsymmetricKey, string ecOid, byte[] signPrivateKey, EcPoint ecPublicKey,
		Action<ulong> setDataSize, Action<ulong> setMaxBlockCount,
		Action<ulong> endIteration, Action<string> sendMessage);
}