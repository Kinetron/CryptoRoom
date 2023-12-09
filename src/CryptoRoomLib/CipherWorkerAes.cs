﻿using System;
using System.Collections.Generic;
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

		public CipherWorkerAes(ICipherAlgoritm algoritm)
		{
			_algoritm = algoritm;
		}

		public bool DecryptingFile(string srcPath, string resultFileName, byte[] privateAsymmetricKey, string ecOid,
			EcPoint ecPublicKey, Action<ulong> setDataSize, Action<ulong> setMaxBlockCount, Action<ulong> endIteration, Action<string> sendProcessText)
		{
			throw new NotImplementedException();
		}
		private void CopyStream(Stream input, Stream output, int bytes)
		{
			byte[] buffer = new byte[32768];
			int read;
			while (bytes > 0 &&
			       (read = input.Read(buffer, 0, Math.Min(buffer.Length, bytes))) > 0)
			{
				output.Write(buffer, 0, read);
				bytes -= read;
			}
		}

		public bool DecryptingFileParallel(string srcPath, string resultFileName, byte[] privateAsymmetricKey, string ecOid,
			EcPoint ecPublicKey, Action<ulong> setDataSize, Action<ulong> setMaxBlockCount, Action<ulong> endIteration, Action<string> sendProcessText)
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
						using (SymmetricAlgorithm cipher = Aes.Create())
						using (FileStream outFile = new FileStream(resultFileName, FileMode.Create, FileAccess.Write))
						using (FileStream inFile = new FileStream(srcPath, FileMode.Open, FileAccess.Read))
						{
							//Читаем с места где содержатся данные.
							inFile.Position = commonInfo.BeginDataPosition;

							byte[] readBuffer = new byte[FileFormat.DataSizeInfo];
							Span<byte> buffSpan = new Span<byte>(readBuffer);
							inFile.Read(buffSpan);

							//Размер рельного файла.
							int fileLength = BitConverter.ToInt32(readBuffer);

							cipher.Key = commonInfo.SessionKey;
							cipher.Padding = PaddingMode.PKCS7;
							cipher.Mode = CipherMode.CBC;

							//Согласно формату файла iv 32 байта.
							byte[] iv = new byte[16];
							Buffer.BlockCopy(commonInfo.Iv, 0, iv, 0, iv.Length);
							cipher.IV = iv;

							using (var cryptoStream =
							       new CryptoStream(outFile, cipher.CreateDecryptor(), CryptoStreamMode.Write))
							{
								int rest = fileLength % 16;
								if (rest != 0)
								{
									fileLength += rest;
								}
								CopyStream(inFile, cryptoStream, fileLength);
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
			info.FileLength = (ulong)(fi.Length + FileFormat.IvSize + 
			                          FileFormat.DataSizeInfo); //Размер закодированного блока.

			long rLen = fi.Length % 16;
			if(rLen != 0) info.FileLength += (ulong)rLen;


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
			byte[] aesIv = new byte[16];
			Buffer.BlockCopy(iv,0,aesIv,0,16);

			info.BlockData = asWriter.GetData();

			setDataSize(100);
			setMaxBlockCount(100);
			endIteration(1);

			using (var aes = AesManaged.Create())
			{
				aes.Padding = PaddingMode.PKCS7;
				aes.Mode = CipherMode.CBC;
				aes.Key = info.SessionKey;
				aes.BlockSize = 128; // AES-128
				aes.IV = aesIv;

				using (var encryptor = aes.CreateEncryptor())
				using (FileStream outFile = new FileStream(resultFileName, FileMode.Create, FileAccess.Write))
				using (FileStream inFile = new FileStream(srcPath, FileMode.Open, FileAccess.Read))
				{
					byte[] fileSizeInfo = new byte[FileSizeBlockLen];
					Buffer.BlockCopy(BitConverter.GetBytes(fi.Length), 0, fileSizeInfo, 0, FileSizeBlockLen);

					outFile.Write(info.FileHead);
					outFile.Write(fileSizeInfo); //Размер кодированного файла.

					using (var cryptoStream =
					       new CryptoStream(outFile, encryptor, CryptoStreamMode.Write))
					{
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

			endIteration(50);

			sendMessage($"Шифрование файла {fileName} завершено.");

			sendMessage($"Подпись файла {fileName} ...");
			var signTools = new SignTools();
			if (!signTools.SignFile(resultFileName, sendMessage, ecOid, signPrivateKey, ecPublicKey))
			{
				LastError = signTools.LastError;
				return false;
			}

			endIteration(100);
			sendMessage($"Подпись файла {fileName} завершена.");

			return true;
		}
	}
}
