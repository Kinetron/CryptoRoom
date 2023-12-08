using CryptoRoomLib.Cipher3412;
using CryptoRoomLib.CipherMode3413;
using CryptoRoomLib.KeyGenerator;

namespace CryptoRoomLib.Tests
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test, Order(11)]
        public void CryptoLibTest()
        {
            //Запускает внутренние тесты алгоритма.
            var self = new Cipher3412.SelfTests();
            var res = self.RunTests();

            Assert.IsTrue(res, self.Error);
        }

        [Test, Order(2)]
        public void CipherMode3413()
        {
            var self = new CipherMode3413.SelfTests();
            var res = self.RunTests();

            Assert.IsTrue(res, self.Error);
        }

        [Test, Order(3)]
        public void AsymmetricCipherTest()
        {
            var self = new CryptoRoomLib.AsymmetricInformation.SelfTests();
            var res = self.RunTests();

            Assert.IsTrue(res, self.Error);
        }

        [Test, Order(4)]
        [TestCase("ТестовыйПароль99EngLater", "key.grk")]
        public void SecretKeyMakerTest(string password, string pathToKey)
        {
            SecretKeyMaker maker = new SecretKeyMaker();
            var res = maker.CreateKeyFileNoReq(password, pathToKey);

            Assert.IsTrue(res);
        }

        /// <summary>
        /// Загружает секретный ключ и проверяет его.
        /// </summary>
        [Test, Order(5)]
        [TestCase("key.grk")]
        public void KeyServiceLoadKeyTest(string pathToKey)
        {
            KeyService service = new KeyService();
            var res = service.LoadKey(pathToKey);
            
            Assert.IsTrue(res, service.LastError);
        }

        /// <summary>
        /// Читает содержимое одного ключа и копирует в другой.
        /// </summary>
        [Test, Order(6)]
        [TestCase("key.grk", "copyKey.grk")]
        public void CopySecretKeyTest(string srcPath, string dstPath)
        {
            KeyService service = new KeyService();
            var res = service.LoadKey(srcPath);
            Assert.IsTrue(res, service.LastError);

            SecretKeyMaker maker = new SecretKeyMaker();
            res = maker.SaveToFile(service.KeyContainer, dstPath);
            Assert.IsTrue(res, maker.LastError);
        }

        /// <summary>
        /// Проверяет пароль для секретного ключа.
        /// </summary>
        [Test, Order(7)]
        [TestCase("ТестовыйПароль99EngLater", "key.grk")]
        public void CheckPasswordTest(string password, string pathToKey)
        {
            KeyService service = new KeyService();
            var res = service.LoadKey(pathToKey);
            Assert.IsTrue(res, service.LastError);

            res = service.CheckPassword(password);
            Assert.IsTrue(res, service.LastError);
        }

        /// <summary>
        /// Тест хеш функции ГОСТ 34.11-2012.
        /// </summary>
        [Test, Order(8)]
        public void Hash3411Test()
        {
            var self = new Hash3411.SelfTests();
            var res = self.RunTests();

            Assert.IsTrue(res, self.Error);
        }

        /// <summary>
        /// Тест алгоритмов генерации секретного ключа.
        /// </summary>
        [Test, Order(9)]
        public void KeyGeneratorTest()
        {
            var self = new KeyGenerator.SelfTests();
            var res = self.RunTests();

            Assert.IsTrue(res, self.LastError);
        }

        /// <summary>
        /// Тест подписи данных.
        /// </summary>
        [Test, Order(10)]
        public void SingTest()
        {
            var self = new Sign.SelfTests();
            var res = self.RunTests();

            Assert.IsTrue(res, self.LastError);
        }
        
        [Test, Order(11)]
        [TestCase("ТестовыйПароль99EngLater", "key.grk", "Test1.jpg", "Test1.jpg.crypt")]

        public void CryptingFile(string password, string pathToKey, string srcFilePath, string dstFilePath)
        {
            KeyService keyService = new KeyService();
            var res = keyService.LoadKey(pathToKey);
            Assert.IsTrue(res, keyService.LastError);

            res = keyService.CheckPassword(password);
            Assert.IsTrue(res, keyService.LastError);

            ICipherAlgoritm algoritm = new CipherAlgoritm3412();
            IBlockCipherMode cipherMode = new ModeCBC(algoritm);

            CipherWorker worker = new CipherWorker(cipherMode);

            ulong blockCount = 0;
            ulong blockNum = 0;
            ulong decryptDataSize = 0;

            res = worker.CryptingFile(srcFilePath, dstFilePath, keyService.GetPublicAsymmetricKey(),
                keyService.KeyContainer.EcOid, keyService.GetSignPrivateKey(), keyService.EcPublicKey,
                (size) => { decryptDataSize = size; },
                (max) => { blockCount = max; },
                (number) => { blockNum = number; },
                (text)=>{});

            Assert.IsTrue(res, worker.LastError);
        }

        /// <summary>
        /// Расшифровывает файл.
        /// </summary>
        [Test, Order(12)]
        [TestCase("ТестовыйПароль99EngLater", "key.grk", "Test1.jpg.crypt", "Test2.jpg")]
        public void DecryptingFile(string password, string pathToKey, string srcFilePath, string dstFilePath)
        {
            KeyService keyService = new KeyService();
            var res = keyService.LoadKey(pathToKey);
            Assert.IsTrue(res, keyService.LastError);

            res = keyService.CheckPassword(password);
            Assert.IsTrue(res, keyService.LastError);

            ICipherAlgoritm algoritm = new CipherAlgoritm3412();
            IBlockCipherMode cipherMode = new ModeCBC(algoritm);
            
            CipherWorker worker = new CipherWorker(cipherMode);

            ulong blockCount = 0;
            ulong blockNum = 0;
            ulong decryptDataSize = 0;

            res = worker.DecryptingFile(srcFilePath, dstFilePath,
                keyService.GetPrivateAsymmetricKey(),
                keyService.KeyContainer.EcOid,
                keyService.EcPublicKey,
                (size) => { decryptDataSize = size; },
                (max) => { blockCount = max; },
                (number) => { blockNum = number; },
                (text)=>{});
            
            Assert.IsTrue(res, worker.LastError);
        }

    }
}