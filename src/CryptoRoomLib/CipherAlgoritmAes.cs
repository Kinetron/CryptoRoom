using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoRoomLib
{
	public class CipherAlgoritmAes : ICipherAlgoritm
	{
		public int KeySize { get; }

		/// <summary>
		/// Используется для формирования заголовка файла.
		/// </summary>
		public int BlockSize
		{
			get => 16; 

		}
		public void DeployDecryptRoundKeys(byte[] key)
		{
		}

		public void DecryptBlock(ref Block128t block)
		{
		}

		public void EncryptBlock(ref Block128t block)
		{
		}

		public void DeployСryptRoundKeys(byte[] key)
		{
		}
	}
}
