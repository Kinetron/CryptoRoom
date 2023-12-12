namespace CryptoRoomLib.Enums
{
	/// <summary>
	/// Алгоритмы поддерживаемые библиотекой.
	/// </summary>
	public enum СipherAlgoritmsEnum
	{
		/// <summary>
		/// Ассиметричное шифрование rsa 4096, блочный шифр аes 256, подпись HmacSha256
		/// </summary>
		RsaAesSha256 = 0,

		RsaGost = 1,
	}
}
