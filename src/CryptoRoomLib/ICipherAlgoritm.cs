namespace CryptoRoomLib
{
    /// <summary>
    /// Алгоритм шифрования.
    /// </summary>
    public interface ICipherAlgoritm
    {
        /// <summary>
        /// Размер ключа криптоалгоритма(байт).
        /// </summary>
        int KeySize { get; }

        /// <summary>
        /// Размер блока кодируемых данных(байт).
        /// </summary>
        int BlockSize { get; }

        /// <summary>
        /// Развертывание раундовых ключей.
        /// </summary>
        void DeployDecryptRoundKeys(byte[] key);

        /// <summary>
        /// Расшифровывает блок данных.
        /// </summary>
        /// <param name="block"></param>
        void DecryptBlock(ref Block128t block);

        // <summary>
        /// Шифрует блок.
        /// </summary>
        /// <param name="block"></param>
        void EncryptBlock(ref Block128t block);

        /// <summary>
        /// Развертывание раундовых ключей шифрования.
        /// </summary>
        void DeployСryptRoundKeys(byte[] key);
    }
}
