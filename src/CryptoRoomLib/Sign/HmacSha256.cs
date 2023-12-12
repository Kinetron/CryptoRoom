using CryptoRoomLib.AsymmetricInformation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Intrinsics.Arm;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CryptoRoomLib.Sign
{
	/// <summary>
	/// Подписывает и проверяет файл с помощью HMACSHA256.
	/// </summary>
	internal class HmacSha256
	{
		/// <summary>
		/// Подпись файла. Ключ подписи 64 байта, результат 32байта
		/// </summary>
		/// <param name="srcfile"></param>
		/// <param name="signPrivateKey"></param>
		public void SignFile(string srcfile, byte[] signPrivateKey)
		{
			using (HMACSHA256 hmac = new HMACSHA256(signPrivateKey))
			using (FileStream inStream = new FileStream(srcfile, FileMode.Open, FileAccess.ReadWrite))
			{
				byte[] hash = hmac.ComputeHash(inStream);

				//Добавление блока содержащего сведения о подписи.
				var asWriter = new AsDataWriter();
				inStream.Write(asWriter.CreateSignHmacSha256(hash, new byte[58]));
				inStream.Close();
			}
		}

		public bool VerifyFile(string srcfile, byte[] signPrivateKey, byte[] storedHash)
		{
			if (signPrivateKey.Length == 0 || storedHash.Length != 32) return false;

			using (HMACSHA256 hmac = new HMACSHA256(signPrivateKey))
			using (FileStream inStream = new FileStream(srcfile, FileMode.Open, FileAccess.Read))
			{
				byte[] computedHash = hmac.ComputeHash(inStream);

				for (int i = 0; i < storedHash.Length; i++)
				{
					if (computedHash[i] != storedHash[i])
					{
						return false;
					}
				}
			}

			return true;
		}
	}
}
