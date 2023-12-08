namespace CryptoRoomLib.AsymmetricInformation
{
    /// <summary>
    /// Хранит блоки данных, в которых передается иформация для ассиметричного шифрования.
    /// </summary>
    internal class AsBlockData
    {
        /// <summary>
        /// Тип блока.
        /// </summary>
        public AsBlockDataTypes Type { get; set; }

        /// <summary>
        /// Данные блока.
        /// </summary>
        public byte[] Data { get; set; }
    }
}
