using CryptoRoomLib.AsymmetricInformation;
using CryptoRoomLib.Sign;
using System.Text;
using System.Xml.Serialization;

namespace CryptoRoomLib.KeyGenerator
{
    /// <summary>
    /// Содержит все необходимые методы для чтения секретного ключа(файл который храниться у пользователя) системы.
    /// </summary>
    public class KeyService : IKeyService
    {
        /// <summary>
        /// Последнее исключение.
        /// </summary>
        private string _lastException;

        /// <summary>
        /// Последнее сообщение об ошибке.
        /// </summary>
        private string _lastError;

        /// <summary>
        /// Последнее сообщение об ошибке.
        /// </summary>
        public string LastError
        {
            get
            {
                if (string.IsNullOrEmpty(_lastException)) return _lastError;

                return _lastError + $" Исключение: {_lastException}";
            }

            set
            {
                _lastError = value;
            }
        }

        /// <summary>
        /// Контейнер секретного ключа.
        /// </summary>
        public SecretKeyContainer KeyContainer;

        /// <summary>
        /// Закрытый ключ ассиметричной системы.
        /// </summary>
        private byte[] _rsaPrivateKey;

        /// <summary>
        /// Открытый ключ ассиметричной системы.
        /// </summary>
        private byte[] _rsaPublicKey;

        /// <summary>
        /// Закрытый ключ подписи.
        /// </summary>
        private byte[] _signPrivateKey;

        /// <summary>
        /// Открытый ключ подписи.
        /// </summary>
        public EcPoint EcPublicKey { get; set; }

        /// <summary>
        /// Считывает контейнер ключа из файла. Расшифровывает его, помещает в объект ключа KeyContainer.
        /// </summary>
        /// <param name="pathToSecretKey"></param>
        /// <returns></returns>
        public bool LoadKey(string pathToSecretKey)
        {
            //Проверка существования файла
            if (!File.Exists(pathToSecretKey))
            {
                LastError = $"ОШИБКА Л0: Не удалось найти файл ключа по указанному пути: {pathToSecretKey}. Убедитесь что флеш накопитель вставлен в компьютер.";
                return false;
            }

            //Проверка секретного ключа и получение контейнера.
            if (!CheckSK(pathToSecretKey, out byte[] container)) return false;

            //Длина меньше блока iv(16) и блока описания длины данных(4)
            if(container.Length < SecretKeyMaker.СontainerTitleSize)
            {
                LastError = "ОШИБКА Л4: Неверный  файл секретного ключа.";
                return false;
            }

            KeyContainer = DeserializeKeyContainer(SecretKeyMaker.UnpackSKContainer(container));
            if(KeyContainer == null) return false;
            
            return true;
        }

        /// <summary>
        /// Проверяет секретный ключ, в случае ошибки возвращает false.
        /// Проверка поверхностная. Проверяется соответствие заголовка и контрольной суммы контейнера ключа
        /// Возвращает размер контейнера ключа container_length
        /// </summary>
        /// <param name="pathToSecretKey"></param>
        /// <returns></returns>
        private bool CheckSK(string pathToSecretKey, out byte[] container)
        {
            container = new byte[0];

            FileInfo fileInfo = new FileInfo(pathToSecretKey);
            long fileSize = fileInfo.Length;

            if (fileSize == 0)
            {
                LastError = "ОШИБКА Р0:Не удалось открыть файл секретного ключа.";
                return false;
            }

            if (fileSize < SecretKeyMaker.KeyHeadLen)
            {
                LastError = "ОШИБКА Р1:Не корректный размер файла секретного ключа.";
                return false;
            }
            
            try
            {
                byte[] title = new byte[SecretKeyMaker.KeyHeadLen];
                
                using (var fs = new FileStream(pathToSecretKey, FileMode.Open))
                {
                    fs.Read(title, 0, SecretKeyMaker.KeyHeadLen);

                    //Проверка правильности заголовка
                    int containerLen = CheckHeadFile(title, fileSize); //Длина контейнера ключа.
                                           
                    if (containerLen == 0)
                    {
                        fs.Close();
                        return false;
                    }

                    //Читаю содержимое контейнера.
                    container = new byte[containerLen];
                    fs.Read(container, 0, containerLen);

                    if(!CheckContainerHash( container, title))
                    {
                        fs.Close();
                        return false;
                    }

                    fs.Close();
                }
            }
            catch (Exception e)
            {
                _lastException = e.Message;
                LastError = $"ОШИБКА Р2:Не удалось считать файл секретного ключа.";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Проверяет заголовок файла. Возвращает длину контейнера.
        /// </summary>
        /// <returns></returns>
        private int CheckHeadFile(byte[] title, long fileSize)
        {
            //[7-байт служебной информации]
            byte[] headText = new byte[SecretKeyMaker.KeyHeadText.Length];
            Buffer.BlockCopy(title, 0, headText, 0, SecretKeyMaker.KeyHeadText.Length);

            if (!headText.SequenceEqual(SecretKeyMaker.KeyHeadText))
            {
                LastError = "ОШИБКА Р4: Файл секретного ключа не предназначен для данной системы.";
                return 0;
            }

            //[Заголовок 47байт]
            byte[] programNameText = new byte[SecretKeyMaker.ProgramText.Length];
            Buffer.BlockCopy(title, SecretKeyMaker.KeyHeadText.Length, programNameText, 0, SecretKeyMaker.ProgramText.Length);

            byte[] programNameTextEtalon = Encoding.ASCII.GetBytes(SecretKeyMaker.ProgramText);

            if (!programNameText.SequenceEqual(programNameTextEtalon))
            {
                LastError = "ОШИБКА Р5: Файл секретного ключа не предназначен для данной системы.";
                return 0;
            }

            //Проверка длины блока контейнера
            byte[] containerLenArr = new byte[SecretKeyMaker.KeyContainerLen];
            Buffer.BlockCopy(title, SecretKeyMaker.KeyHeadLen - SecretKeyMaker.KeyContainerLen, containerLenArr, 0, SecretKeyMaker.KeyContainerLen);

            int containerLen = BitConverter.ToInt32(containerLenArr);

            //Длина ключа не может быть менее чем заголовок + контейнер.
            if (fileSize < SecretKeyMaker.KeyHeadLen + containerLen)
            {
                LastError = "ОШИБКА Р6: Неверный размер файла секретного ключа.";
                return 0;
            }

            return containerLen;
        }

        /// <summary>
        /// Проверяет не был ли поврежден(изменен) контейнер.
        /// Вычисляет хеш от данных контейнера и сравнивает с той что записана в заголовке файла.
        /// </summary>
        /// <param name="container"></param>
        /// <param name="title"></param>
        /// <returns></returns>
        private bool CheckContainerHash(byte[] container, byte[] title)
        {
            //Вычисляет хэш256 контейнера.
            var hasher = new Hash3411.Hash3411();
            byte[] hashContainer = new byte[Hash3411.Hash3411.Hash256Size];

            hasher.Hash256(container, hashContainer);
            
            byte[] hashFromHead = new byte[Hash3411.Hash3411.Hash256Size];

            int hashPos = SecretKeyMaker.KeyHeadLen - SecretKeyMaker.KeyContainerLen - Hash3411.Hash3411.Hash256Size;
            Buffer.BlockCopy(title, hashPos, hashFromHead, 0,Hash3411.Hash3411.Hash256Size);

            if (!hashFromHead.SequenceEqual(hashContainer))
            {
                LastError = "ОШИБКА Р9: Ошибка контрольной суммы. Файл поврежден или был изменен третьим лицом.";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Преобразовываю контейнер в объект.
        /// </summary>
        /// <param name="container"></param>
        /// <returns></returns>
        private SecretKeyContainer DeserializeKeyContainer(byte[] container)
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            string containerText = Encoding.GetEncoding(1251).GetString(container);
            
            try
            {
                //Определяю имя корневого элемента.
                System.Reflection.MemberInfo info = typeof(SecretKeyContainer);
                var attributes = info.GetCustomAttributes(true);

                var rootName = ((XmlRootAttribute)attributes.Where(x => x is XmlRootAttribute).FirstOrDefault()).ElementName;
                
                if (!containerText.Contains($"<{rootName}>"))
                {
                    LastError = "ОШИБКА Л5: Неверный  файл секретного ключа.";
                    return null;
                }

                using (MemoryStream stream = new MemoryStream(container))
                {
                    var serializer = new XmlSerializer(typeof(SecretKeyContainer));
                    return (SecretKeyContainer)serializer.Deserialize(stream);
                }
            }
            catch (Exception e)
            {
                _lastException = e.Message;
                LastError = $"ОШИБКА Л6: Неверный  файл секретного ключа.";
                return null;
            }
        }

        /// <summary>
        /// Проверяет пароль для ключа.
        /// </summary>
        /// <returns></returns>
        public bool CheckPassword(string password)
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            byte[] passwordArray = Encoding.GetEncoding(1251).GetBytes(password);

            SecretKeyMaker maker = new SecretKeyMaker();

            //Вычисляю хэш соленого пароля
            byte[] hashPassword = new byte[Hash3411.Hash3411.Hash256Size];

            var salt = Convert.FromHexString(KeyContainer.RsaSalt);

            maker.HashedPassword(salt, passwordArray, hashPassword);
            
            byte[] privateKey = Convert.FromHexString(KeyContainer.CryptRsaPrivateKey);
            byte[] publicKey = Convert.FromHexString(KeyContainer.RsaPublicKey);
            byte[] iv = Convert.FromHexString(KeyContainer.RsaIv);

            maker.DecryptSecretKey(hashPassword, iv, privateKey);

            Span<byte> publicKeySpan = publicKey;
            Span<byte> privateKeySpan = privateKey;
            
            try
            {
                KeyDecoder kd = new KeyDecoder();

                //Если пользователь ввел неправильный пароль, то расшифрованный ключ выглядит не как ASN1,
                //и провайдер его не может загрузить вызывая исключение.
                if (!kd.CheckKeyPair(privateKey, publicKey))
                {
                    LastError = "Неправильная ключевая пара.";
                    return false;
                }

                _rsaPrivateKey = privateKey;
                _rsaPublicKey = publicKey;

                //Распаковка ключа подписи.
                var signPrivateKey = UnpackSignPrivateKey(passwordArray);
                
                var curve = EcGroups.GetCurve(KeyContainer.EcOid);
                if (curve == null)
                {
                    LastError = "Ошибка Sg5: Не удалось определить параметры кривой.";
                    return false;
                }

                //Создаю точку с указанными в кривой координатами точки P.
                EcPoint q = new EcPoint(curve, true);
                q.X = new BigInteger(FromHex(KeyContainer.OpenSignKeyPointX));
                q.Y = new BigInteger(FromHex(KeyContainer.OpenSignKeyPointY));

                if (!CheckEcPair(signPrivateKey, q))
                {
                    LastError = "Неправильная ключевая пара подписи.";
                    return false;
                }

                EcPublicKey = q; 
                _signPrivateKey = signPrivateKey;
            }
            catch (Exception e)
            {
                /*
                 * При чтении не расшифрованного RSA ключа – всегда возникает исключение с данным сообщением.
                 * На основании этого можем определить – ключ не расшифрован из-за не корректного пароля.
                 */
                if (e.Message == "ASN1 corrupted data.")
                {
                    _lastException = string.Empty;
                }
                else
                {
                  _lastException = e.Message;
                }
                
                LastError = "Неверный пароль.";
                return false;
            }
            
            //Добавить проверку ключа подписи.
            return true;
        }

        /// <summary>
        /// Извлекает ключ подписи из контейнера ключа.
        /// </summary>
        private byte[] UnpackSignPrivateKey(byte[] passwordArray)
        {
            SecretKeyMaker maker = new SecretKeyMaker();

            //Вычисляю хэш соленого пароля
            byte[] hashPassword = new byte[Hash3411.Hash3411.Hash256Size];

            var salt = Convert.FromHexString(KeyContainer.SaltCryptSignKey); //Соль в 16 ричном формате

            maker.HashedPassword(salt, passwordArray, hashPassword);

            byte[] privateKey = Convert.FromHexString(KeyContainer.CryptSignKey);
            byte[] iv = FromHex(KeyContainer.IvCryptSignKey);

            maker.DecryptSecretKey(hashPassword, iv, privateKey);

            return privateKey;
        }

        /// <summary>
        /// Проверка ключевой пары подписи.
        /// </summary>
        bool CheckEcPair(byte[] signPrivateKey, EcPoint publicKey)
        {
            var curve = EcGroups.GetCurve(KeyContainer.EcOid);
            if (curve == null)
            {
                LastError = "Ошибка Sk0: Не удалось определить параметры кривой.";
                return false;
            }

            //Создаю точку с указанными в кривой координатами точки P.
            EcPoint p = new EcPoint(curve, true);
            var q1 = PointMath.GenPublicKey(new BigInteger(signPrivateKey), p); //Генерирую открытый ключ.
            
            //Проверка координат
            if (q1.X != publicKey.X) return false;
            if (q1.Y != publicKey.Y) return false;

            return true;
        }

        /// <summary>
        /// Возвращает закрытый ключ ассиметричной системы шифрования.
        /// </summary>
        /// <returns></returns>
        public byte[] GetPrivateAsymmetricKey()
        {
            return _rsaPrivateKey;
        }

        /// <summary>
        /// Возвращает открытый ключ ассиметричной системы шифрования.
        /// </summary>
        /// <returns></returns>
        public byte[] GetPublicAsymmetricKey()
        {
            return _rsaPublicKey;
        }

        /// <summary>
        /// Возвращает закрытый ключ подписи.
        /// </summary>
        /// <returns></returns>
        public byte[] GetSignPrivateKey()
        {
            return _signPrivateKey;
        }

        /// <summary>
        /// Конвертирует 16 ричную строку в число, при необходимости дополняет строку нулем. 
        /// </summary>
        /// <returns></returns>
        private byte[] FromHex(string hex)
        {
            if (hex.Length % 2 != 0) hex = "0" + hex;
            return Convert.FromHexString(hex);
        }
    }
}
