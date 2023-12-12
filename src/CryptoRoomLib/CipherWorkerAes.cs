using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using CryptoRoomLib.AsymmetricInformation;
using CryptoRoomLib.Models;
using CryptoRoomLib.Sign;

namespace CryptoRoomLib
{
	/// <summary>
	/// Алгоритм aes с внутренними механизмами сцепления блоков.
	/// </summary>
	public class CipherWorkerAes : ICipherWorker
	{
		private readonly ICipherAlgoritm _algoritm;
		public string LastError { get; set; }

		/// <summary>
		/// Размер блока, содержащего информацию о размере закодированного файла. 8Байт.
		/// </summary>
		private const int FileSizeBlockLen = 8;
		
		/// <summary>
		/// Размер блока шифра aes. 128 байт, изменен быть не может.
		/// </summary>
		private const int aesBlockSize = 16;

		private const int bufferSize = 32768;

		public CipherWorkerAes(ICipherAlgoritm algoritm)
		{
			_algoritm = algoritm;
		}

		public bool DecryptingFile(string srcPath, string resultFileName, byte[] privateAsymmetricKey, string ecOid,
			EcPoint ecPublicKey, Action<ulong> setDataSize, Action<ulong> setMaxBlockCount, Action<ulong> endIteration, Action<string> sendProcessText)
		{
			throw new NotImplementedException();
		}
		private void CopyStream(Stream input, Stream output, long bytes, Action<ulong> progress)
		{
			byte[] buffer = new byte[bufferSize];
			int read;
			ulong writeBytes = 0;

			while (bytes > 0 &&
			       (read = input.Read(buffer, 0, (int)Math.Min(buffer.Length, bytes))) > 0)
			{
				output.Write(buffer, 0, read);
				bytes -= read;
				writeBytes += (ulong)read;
				progress(writeBytes);
			}
		}

		public bool DecryptingFileParallel(string srcPath, string resultFileName, byte[] privateAsymmetricKey, string ecOid,
			EcPoint ecPublicKey, byte[] signPrivateKey, Action<ulong> setDataSize, Action<ulong> setMaxBlockCount, Action<ulong> endIteration, Action<string> sendProcessText)
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
						HmacSha256 sign = new HmacSha256(); 
						if (!sign.VerifyFile(srcPath, signPrivateKey, commonInfo.HmacSha256Hash))
						{
							return "Неверная подпись файла";
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

						//Согласно формату файла iv 32 байта. Но размер блока шифра 128.
						byte[] iv = new byte[aesBlockSize];
						Buffer.BlockCopy(commonInfo.Iv, 0, iv, 0, iv.Length);

						using (var aes = Aes.Create())
						{
							using (FileStream inFile = new FileStream(srcPath, FileMode.Open, FileAccess.Read))
							{
								//Читаем с места где содержатся данные.
								inFile.Position = commonInfo.BeginDataPosition;

								byte[] readBuffer = new byte[FileFormat.DataSizeInfo];
								Span<byte> buffSpan = new Span<byte>(readBuffer);
								inFile.Read(buffSpan);

								//Размер рельного файла.
								long fileLength = BitConverter.ToInt64(readBuffer);

								aes.Padding = PaddingMode.PKCS7;
								aes.Mode = CipherMode.CBC;
								aes.Key = commonInfo.SessionKey;
								aes.BlockSize = 128;
								aes.IV = iv;

								using (FileStream outFile = new FileStream(resultFileName, FileMode.Create, FileAccess.Write))
								using (var cryptoStream =
								       new CryptoStream(outFile, aes.CreateDecryptor(), CryptoStreamMode.Write))
								{
									//Для текущих настроек padding aes справедлив этот алгоритм.
									long rLen = fileLength % aesBlockSize;
									if (rLen == 0)
									{
										fileLength += aesBlockSize;
									}
									else
									{
										fileLength += aesBlockSize - rLen;
									}

									CopyStream(inFile, cryptoStream, fileLength, endIteration);
								}
							}
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
		public bool MeasureTime(string beginMessage, string errorMessage, string success, Func<string> func, Action<string> sendProcessText)
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

		public CommonFileInfo ReadFileInfo(string srcPath, byte[] privateAsymmetricKey)
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

		public bool CryptingFile(string srcPath, string resultFileName, byte[] publicAsymmetricKey, string ecOid,
			byte[] signPrivateKey, EcPoint ecPublicKey, Action<ulong> setDataSize, Action<ulong> setMaxBlockCount, Action<ulong> endIteration,
			Action<string> sendMessage)
		{
			string fileName = Path.GetFileName(srcPath);
			sendMessage($"Шифрование файла {fileName}...");
			CommonFileInfo info = new CommonFileInfo();

			FileInfo fi = new FileInfo(srcPath);

			//Размер закодированного блока.
			info.FileLength = (ulong)(fi.Length + FileFormat.IvSize + 
			                          FileFormat.DataSizeInfo);

			//Для текущих настроек padding aes справедлив этот алгоритм.
			int rLen = (int)(fi.Length % aesBlockSize);
			if (rLen == 0)
			{
				info.FileLength += aesBlockSize;
			}
			else
			{
				info.FileLength += (ulong)(aesBlockSize - rLen);
			}

			info.FileHead = FileFormat.CreateCryptFileTitle(info.FileLength, _algoritm, false);
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

			//Делаем размер iv cовместимый с форматом файла.
			byte[] iv = CipherTools.GenerateRand(FileFormat.IvSize); //Формирую случайный начальный вектор.
			byte[] aesIv = new byte[aesBlockSize];
			Buffer.BlockCopy(iv,0,aesIv,0, aesBlockSize);

			info.BlockData = asWriter.GetData();

			int blockCount = (int)(fi.Length / bufferSize);
			setDataSize((ulong)fi.Length);
			setMaxBlockCount((ulong)blockCount);

			using (var aes = Aes.Create())
			{
				aes.Padding = PaddingMode.PKCS7;
				aes.Mode = CipherMode.CBC;
				aes.Key = info.SessionKey;
				aes.BlockSize = 128;
				aes.IV = aesIv;

				using (FileStream outFile = new FileStream(resultFileName, FileMode.Create, FileAccess.Write))
				using (FileStream inFile = new FileStream(srcPath, FileMode.Open, FileAccess.Read))
				{
					byte[] fileSizeInfo = new byte[FileSizeBlockLen];
					Buffer.BlockCopy(BitConverter.GetBytes(fi.Length), 0, fileSizeInfo, 0, FileSizeBlockLen);

					outFile.Write(info.FileHead);
					outFile.Write(fileSizeInfo); //Размер кодированного файла.

					using (var cryptoStream =
					       new CryptoStream(outFile, aes.CreateEncryptor(), CryptoStreamMode.Write))
					{
						//CopyStream(inFile,cryptoStream, (int)inFile.Length, endIteration);
						inFile.CopyTo(cryptoStream);
					}
				}
			}

			//CryptoStream закрывает файл и осовобождает ресурсы.
			using (FileStream outFile = new FileStream(resultFileName, FileMode.Append, FileAccess.Write))
			{
				outFile.Write(iv);
				outFile.Write(info.BlockData);
				outFile.Close();
			}

			sendMessage($"Шифрование файла {fileName} завершено.");


			endIteration((ulong)(blockCount * 80 / 100));

			sendMessage($"Подпись файла {fileName} ...");

			HmacSha256 sign = new HmacSha256();
			sign.SignFile(resultFileName, signPrivateKey);

			endIteration((ulong)blockCount);
			sendMessage($"Подпись файла {fileName} завершена.");

			return true;
		}
	}
}
