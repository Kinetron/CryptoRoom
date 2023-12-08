using System.Security.Cryptography;
using System.Text;
using CryptoRoomLib.Cipher3412;
using CryptoRoomLib.CipherMode3413;
using System.Xml;
using System.Xml.Serialization;
using CryptoRoomLib.Sign;

namespace CryptoRoomLib.KeyGenerator
{
    /// <summary>
    /// Генерирует закрытый ключ шифрования.
    /// </summary>
    public class SecretKeyMaker
    {
        /// <summary>
        /// Минимальная длина пароля.
        /// </summary>
        public static int PasswordMinLength = 8;

        /// <summary>
        /// Максимальная длина пароля.
        /// </summary>
        public static int PasswordMaxLength = 64;

        /// <summary>
        /// Расширение файла ключа.
        /// </summary>
        public static string KeyExtension = "grk";

        /// <summary>
        /// Размер соли для кодирование ключа подписи.
        /// </summary>
        private const int SignKeySaltLength = 17;

        /// <summary>
        /// Длина начального вектора для шифрования закрытого ключа подписи.
        /// </summary>
        private const int SignKeyIvLength = 16;

        /// <summary>
        /// Алгоритм открытого ключа подписи.
        /// </summary>
        private const string PublicKeySignAlgoritmOid = "1.2.643.7.1.1.1.2";

        /// <summary>
        /// Идентификатор эллиптической кривой на основании которой создается ключ.
        /// </summary>
        private const string EcOid = "1.2.643.7.1.2.1.2.2";

        /// <summary>
        /// Константа защиты ключевого контейнера.
        /// </summary>
        private const string KeyChipperConstant = "17dfbfc9acfa787e242d75c7f0764bfd83e79eef08d4581a881527d92dbad1d4";

        /// <summary>
        /// Размер заголовка контейнера. 16(iv) + 4(длина шифрованного блока)
        /// </summary>
        public static int СontainerTitleSize = 20;

        //Размер Заголовока файла-служебная информация [7-байт служебной информации][Заголовок 47байт][Хэш контента 256бит]
        //[Длина контейнера ключа 4байта][[iv 16байт][4байта длина контейнера]Контейнер]
        public static int KeyHeadLen = 90;

        /// <summary>
        /// Первые 7 байт содержащих что то вроде версии ключа.
        /// </summary>
        public static byte[] KeyHeadText = FileFormat.FirstText;

        /// <summary>
        /// Название программы для которой используется ключ.
        /// </summary>
        public static string ProgramText = "Little Rose radials security protection system ";

        /// <summary>
        /// Количество байт содержащих длину контейнера секретного ключа.
        /// </summary>
        public static int KeyContainerLen = 4;

        /// <summary>
        /// Сообщение об ошибке.
        /// </summary>
        public string LastError { get; set; }

        /// <summary>
        /// Последнее возникшее исключение.
        /// </summary>
        private string _lastException = string.Empty;

        /// <summary>
        /// Размер блока симметричного шифра используемого для кодирования.
        /// </summary>
        private int _blockCipherSize;
        
        public SecretKeyMaker()
        {
            var alg = new ModifyAlgoritm3412();
            _blockCipherSize = alg.BlockSize;
        }

        /// <summary>
        /// Формирует секретный ключ, без генерации запроса на получение сертификата.
        /// </summary>
        public bool CreateKeyFileNoReq(string password, string pathToSave)
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            byte[] passwordArray = Encoding.GetEncoding(1251).GetBytes(password);

            SecretKeyContainer container = new SecretKeyContainer();

            var settings = SettingsLib.Settings;

            container.KeyVersion = settings.KeyVersion;
            container.KeyGenVersion = settings.KeyGenVersion;

            container.OrgName = settings.OrgName;
            container.Department = settings.Department;
            container.OrgCode = settings.Department;
            container.Familia = settings.Familia;
            container.Imia = settings.Imia;
            container.Otchestvo = settings.Otchestvo;
            container.PhoneNumber = settings.PhoneNumber;

            container.DateBegin = DateTime.Now;
            container.DateEnd = DateTime.Now.AddYears(1);

            if (!СreateSignKey(container, passwordArray)) return false;
            if (!CreateRsaKey(container, 4096, passwordArray)) return false;
            
            container.KeyGenTimeStamp = DateTime.Now; //Время и дата когда ключ был сгенерирован.

            //Дополнительные параметры для использования в будущем.
            container.ReservedParameter0 = "0";
            container.ReservedParameter1 = "1";

            KeyСontainerToFile(container, pathToSave);
            
            return true;
        }

        /// <summary>
        /// Сохраняет контейнер в файл.
        /// </summary>
        /// <param name="container"></param>
        /// <param name="pathToSave"></param>
        /// <returns></returns>
        public bool SaveToFile(SecretKeyContainer container, string pathToSave)
        {
            try
            {
                KeyСontainerToFile(container, pathToSave);
            }
            catch (Exception e)
            {
                _lastException = e.Message;
                LastError = "Ошибка сохранения.";
                return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// Генерация ключевой пары для накладывания и проверки подписи.
        /// </summary>
        /// <param name="container"></param>
        private bool СreateSignKey(SecretKeyContainer container, byte[] password)
        {
            var salt = CipherTools.GenerateRand(SignKeySaltLength);
            var hashPassword = new byte[Hash3411.Hash3411.Hash256Size];

            HashedPassword(salt, password, hashPassword);

            SignTools tools = new SignTools();
            if (!tools.CreateEcKeyPair(EcOid, out byte[] privateKey, out EcPoint ecPublicKey))
            {
                LastError = tools.LastError;
                return false;
            }

            var iv = CipherTools.GenerateRand(SignKeyIvLength); //Генерирую вектор инициализации.
            CryptSecretKey(hashPassword, iv, privateKey); //Шифрую на хеши от соли и пароля закрытый ключ.

            container.CryptSignKey = Convert.ToHexString(privateKey).ToLower();
            container.IvCryptSignKey = Convert.ToHexString(iv).ToLower();
            container.SaltCryptSignKey = Convert.ToHexString(salt).ToLower();

            container.OpenSignKeyPointX = ecPublicKey.X.ToHexString().ToLower();
            container.OpenSignKeyPointY = ecPublicKey.Y.ToHexString().ToLower();

            container.PublicKeyAlgoritmOid = PublicKeySignAlgoritmOid;
            container.EcOid = EcOid;

            Array.Clear(privateKey); //Затираю ключ.
            Array.Clear(hashPassword);

            return true;
        }
        
        /// <summary>
        /// Хэширует пароль с солью.
        /// </summary>
        /// <param name="salt"></param>
        /// <param name="password"></param>
        /// <param name="result"></param>
        public void HashedPassword(byte[] salt, byte[] password, byte[] result)
        {
            byte[] data = new byte[password.Length + salt.Length];

            Buffer.BlockCopy(password,0,data,0,password.Length);
            Buffer.BlockCopy(salt, 0, data, password.Length, salt.Length);

            var hasher = new Hash3411.Hash3411();
            hasher.Hash256(data, result);
        }

        /// <summary>
        /// Сохраняет контейнер ключа в файл
        /// </summary>
        /// <param name="container"></param>
        /// <param name="PathToSave"></param>
        /// <returns></returns>
        private bool KeyСontainerToFile(SecretKeyContainer container, string pathToSave)
        {
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.PreserveWhitespace = true;
            XmlSerializer serializer = new XmlSerializer(container.GetType());
            var emptyNamespaces = new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty });

            var strXml = string.Empty;

            using (MemoryStream stream = new MemoryStream())
            {
                serializer.Serialize(stream, container, emptyNamespaces);
                stream.Position = 0;
                xmlDocument.Load(stream);
                StringWriter sw = new StringWriter();
                XmlTextWriter xw = new XmlTextWriter(sw);
                xmlDocument.WriteTo(xw);

                strXml = sw.ToString();
                stream.Close();
            }

            strXml = strXml.Replace("\r", string.Empty);

            if (!PackContainer(strXml, out byte[] package))
            {
                LastError = "Ошибка W0: Ошибка упаковки.";
                return false;
            }
            
            //Вычисляет хэш256 контейнера.
            var hasher = new Hash3411.Hash3411();
            byte[] hashContainer = new byte[Hash3411.Hash3411.Hash256Size];

            hasher.Hash256(package, hashContainer);

            //Дополнение файла служебной информацией
            byte[] fileHead = new byte[KeyHeadLen];//Заголовок файла-служебная информация
            SetServiceInformation(fileHead, hashContainer, package.Length);

            string result = System.Text.Encoding.ASCII.GetString(fileHead);

            try
            {
                using (FileStream outFile = new FileStream(pathToSave, FileMode.Create, FileAccess.Write))
                {
                    outFile.Write(fileHead, 0, fileHead.Length);
                    outFile.Write(package, 0, package.Length);
                    outFile.Close();
                }
            }
            catch (Exception e)
            {
                _lastException = e.Message;
                return false;
            }
            
            return true;
        }

        /// <summary>
        /// Формирует заголовок файла содержащий служебную информацию.
        /// </summary>
        /// <param name="head"></param>
        /// <param name="hash"></param>
        /// <param name="conteinerLen"></param>
        private void SetServiceInformation(byte[] head, byte[] hash, int conteinerLen)
        {
            //Формирую заголовок файла [7-байт служебной информации]
             Buffer.BlockCopy(KeyHeadText, 0, head, 0,KeyHeadText.Length);

            //Название программы ProgramText [Заголовок 47байт]
            byte[] programText = Encoding.ASCII.GetBytes(ProgramText);
            Buffer.BlockCopy(programText, 0, head, KeyHeadText.Length, programText.Length);

            //Хэш 32 байта
            Buffer.BlockCopy(hash, 0, head, KeyHeadText.Length + programText.Length, hash.Length);

            //Размер блока данных 4байта
            byte[] len = BitConverter.GetBytes(conteinerLen);
            Buffer.BlockCopy(len, 0, head, KeyHeadText.Length + programText.Length + hash.Length, len.Length);
        }
        
        /// <summary>
        /// Запаковывает(шифрует) контейнер секретного ключа.
        /// </summary>
        /// <param name="container"></param>
        /// <returns></returns>
        private bool PackContainer(string container, out byte[] package)
        {
            try
            {
                //Генерация вектора инициализации для шифрования
                byte[] iv = CipherTools.GenerateRand(_blockCipherSize);

                System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

                byte[] containerArray = Encoding.GetEncoding(1251).GetBytes(container);

                var cipherKey = Convert.FromHexString(KeyChipperConstant);

                //Выполняю шифрование в режиме обратной связи по шифротексту
                CryptSecretKey(cipherKey, iv, containerArray);

                //Размер заголовка. 16(iv) + 4(длина не шифрованного блока)
                package = new byte[СontainerTitleSize + containerArray.Length];

                Buffer.BlockCopy(iv, 0, package, 0, iv.Length);

                byte[] len = BitConverter.GetBytes(containerArray.Length);
                Buffer.BlockCopy(len, 0, package, iv.Length, СontainerTitleSize - iv.Length);

                Buffer.BlockCopy(containerArray, 0, package, СontainerTitleSize, containerArray.Length);
            }
            catch (Exception e)
            {
                package = new byte[0];
                _lastException = e.Message;
                return false;
            }

            return true;
        }

        /// <summary>
        /// Создает ключевые пары для алгоритма RSA.
        /// </summary>
        private bool CreateRsaKey(SecretKeyContainer container, int keySize, byte[] password)
        {
            try
            {
                byte[] privateKey;
                byte[] publicKey;
                using (RSA rsa = RSA.Create())
                {
                    rsa.KeySize = keySize;

                    privateKey = rsa.ExportPkcs8PrivateKey();
                    publicKey = rsa.ExportSubjectPublicKeyInfo();
                }

                //Генерирую 17байтную соль. Соль случайное число.
                var salt = CipherTools.GenerateRand(_blockCipherSize + 1);

                //Вычисляю хэш соленого пароля
                byte[] hashPassword = new byte[Hash3411.Hash3411.Hash256Size];

                HashedPassword(salt, password, hashPassword);

                //Генерирую вектор инициализации
                var iv = CipherTools.GenerateRand(_blockCipherSize);

                //Шифрую закрытый ключ алгоритма RSA. В качестве ключа шифра используется хэш.
                CryptSecretKey(hashPassword, iv, privateKey);

                container.CryptRsaPrivateKey = Convert.ToHexString(privateKey).ToLower();
                container.RsaIv = Convert.ToHexString(iv).ToLower();
                container.RsaSalt = Convert.ToHexString(salt).ToLower();
                container.RsaPublicKey = Convert.ToHexString(publicKey).ToLower();
            }
            catch (Exception ex)
            {
                LastError = $"Ошибка в методе CreateRsaKey: {ex.Message}";
                return false;
            }

            return true;
        }
        
        /// <summary>
        /// Шифрую секретный ключ.
        /// </summary>
        /// <returns></returns>
        public bool CryptSecretKey(byte[] cipherKey, byte[] iv, byte[] privateKey)
        {
            try
            {
                ICipherAlgoritm algoritm = new ModifyAlgoritm3412();
                algoritm.DeployDecryptRoundKeys(cipherKey);
                ModeCFB cfb = new ModeCFB(algoritm);

                cfb.CfbEncrypt(privateKey, iv);
            }
            catch (Exception e)
            {
                LastError = $"В методе CryptSecretKey возникло исключение: {e.Message}";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Расшифровывает секретный ключ.
        /// </summary>
        /// <param name="cipherKey"></param>
        /// <param name="iv"></param>
        /// <param name="privateKey"></param>
        /// <returns></returns>
        public bool DecryptSecretKey(byte[] cipherKey, byte[] iv, byte[] privateKey)
        {
            try
            {
                ICipherAlgoritm algoritm = new ModifyAlgoritm3412();
                algoritm.DeployDecryptRoundKeys(cipherKey);
                ModeCFB cfb = new ModeCFB(algoritm);

                cfb.CfbDecrypt(privateKey, iv);
            }
            catch (Exception e)
            {
                LastError = $"В методе DecryptSecretKey возникло исключение: {e.Message}";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Расшифровывает контейнер секретного ключа и проверяю его заголовок, в случает ошибки = null.
        /// </summary>
        /// <param name="container"></param>
        /// <returns></returns>
        public static byte[] UnpackSKContainer(byte[] container)
        {
            var alg = new ModifyAlgoritm3412();

            byte[] initVector = new byte[alg.BlockSize];
            Buffer.BlockCopy(container, 0, initVector, 0, alg.BlockSize);

            byte[] cryptedMessage = new byte[container.Length - SecretKeyMaker.СontainerTitleSize];
            Buffer.BlockCopy(container, SecretKeyMaker.СontainerTitleSize, cryptedMessage, 0, cryptedMessage.Length);

            var cipherKey = Convert.FromHexString(KeyChipperConstant);

            ICipherAlgoritm algoritm = new ModifyAlgoritm3412();
            algoritm.DeployDecryptRoundKeys(cipherKey);
            ModeCFB cfb = new ModeCFB(algoritm);

            cfb.CfbDecrypt(cryptedMessage, initVector);
            
            return cryptedMessage;
        }
    }
}
