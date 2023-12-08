namespace CryptoRoomLib.AsymmetricInformation
{
    /// <summary>
    /// Набор встроенных тестов для проверки правильности алгоритма.
    /// </summary>
    public class SelfTests
    {
        /// <summary>
        /// Сообщение об ошибке.
        /// </summary>
        public string Error { get; private set; }

        public SelfTests()
        {
            Error = string.Empty;
        }

        /// <summary>
        /// Общий метод тестирования всего алгоритма 34.12.
        /// </summary>
        /// <returns></returns>
        public bool RunTests()
        {
            List<Func<bool>> tests = new List<Func<bool>>
            {
                TestDecryptSessionKey,
                CheckKeyPairTest,
                CryptDecryptSessionKeyTest
        };

            foreach (var test in tests)
            {
                if (!test()) return false;
            }

            return true;
        }

        /// <summary>
        /// Тест декодирования сессионного ключа.
        /// </summary>
        /// <returns></returns>
        private bool TestDecryptSessionKey()
        {
            KeyDecoder kd = new KeyDecoder();
            byte[] sessionKey;

            kd.DecryptSessionKey(TestConst.RsaPrivateKey, TestConst.CryptData, out sessionKey);

            if (!sessionKey.SequenceEqual(TestConst.SessionKey))
            {
                Error = "Не удалось расшифровать сеансовый ключ.";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Сравнивает соответствует ли закрытый ключ открытому.
        /// </summary>
        /// <returns></returns>
        private bool CheckKeyPairTest()
        {
            KeyDecoder kd = new KeyDecoder();

            Span<byte> publicKey = TestConst.RsaPublicKey;
            Span<byte> privateKey = TestConst.RsaPrivateKey;

            if (!kd.CheckKeyPair(privateKey, publicKey))
            {
                Error = kd.Error;
                return false;
            }

            return true;
        }

        /// <summary>
        /// Тест шифрования/расшифровывания сеансового ключа.
        /// </summary>
        /// <returns></returns>
        private bool CryptDecryptSessionKeyTest()
        {
            Span<byte> publicKey = TestConst.RsaPublicKey;
            Span<byte> privateKey = TestConst.RsaPrivateKey;

            KeyDecoder kd = new KeyDecoder();

            if (!kd.EncryptSessionKey(publicKey, TestConst.SessionKey, out byte[] cryptKey))
            {
                Error = kd.Error;
                return false;
            }

            if (!kd.DecryptSessionKey(privateKey, cryptKey, out byte[] decryptKey))
            {
                Error = kd.Error;
                return false;
            }

            if (!decryptKey.SequenceEqual(TestConst.SessionKey))
            {
                Error = "Ошибка CryptDecryptSessionKeyTest.";
                return false;
            }

            return true;
        }
    }
}
