using CryptoRoomLib.AsymmetricInformation;
using CryptoRoomLib.Models;

namespace CryptoRoomLib.Sign
{
    /// <summary>
    /// Класс для создания ЭЦП ГОСТ 34.11-2012.
    /// </summary>
    internal class SignTools
    {
        /// <summary>
        /// Размер файла в байтах, который будет подписан.
        /// Свыше указанного размера – файл подписан не будет (операция будет выполняться долго).
        /// </summary>
        private const int SignAllowSize = 1900000000;

        /// <summary>
        /// Последнее сообщение об ошибке.
        /// </summary>
        public string LastError { get; set; }

        /// <summary>
        /// Результат хеширования сообщения.
        /// </summary>
        private byte[] HashResult { get; set; }
        
        /// <summary>
        /// Подписывает сообщение message, используя закрытый ключ d и точку P.
        /// </summary>
        /// <returns></returns>
        public Signature Sign(BigInteger d, byte[] message, EcPoint p)
        {
            if (d == 0) throw new ArgumentException("Private key for sign can't be 0.");

            //Узнаю количество бит порядка эллиптической кривой.
            int qSize = p.Q.bitCount();

            var hash = CalculatedHash(message, qSize);
            HashResult = hash;
            var alpha = new BigInteger(hash);
            var t1 = alpha.ToHexString();

            var q = p.Q;
            var e = alpha % q;
            if (e == 0) e = 1;

            EcPoint c = new EcPoint();
            BigInteger r = new BigInteger();
            BigInteger s = new BigInteger();

            //Шаг 3 Генерирую псеводослучайное число k; 0<k<q
            var k = new BigInteger(CipherTools.GenerateRand(qSize / 8));

            do
            {
                c = PointMath.Multiply(k, p); //Шаг 4. Вычисляю точку эллиптической кривой C=k*G;
                r = c.X % q; //Определить r = x mod n, где x — x-координата точки C.
                s = ((r * d) + (k * e)) % q;
            } while ((r == 0) || (s == 0));

            //Проверка векторов на выполнение условий 0 < r < q , 0 < s < q.
            if (!((0 < r) && (r < q))) new ArgumentException("SignTools::Sign bad vector 'r'");
            if (!((0 < s) && (s < q))) new ArgumentException("SignTools::Sign bad vector 's'");

            return new Signature()
            {
                R = r,
                S = s,
                Qlength = qSize
            };
        }

        /// <summary>
        /// Проверяет подпись sign, сообщения message, используя откр. ключ Q. В случае успеха возвращает true.
        /// </summary>
        public bool Verify(byte[] message, Signature sign, EcPoint pointQ, EcPoint p)
        {
            BigInteger alpha = new BigInteger(CalculatedHash(message, p.Q.bitCount()));
            return Verify(sign, pointQ, p, alpha);
        }

        /// <summary>
        /// Проверяет подпись sign, сообщения message, используя откр. ключ Q. В случае успеха возвращает true.
        /// </summary>
        public bool VerifyUseHashResult(Signature sign, EcPoint pointQ, EcPoint p)
        {
            BigInteger alpha = new BigInteger(HashResult);
            return Verify(sign, pointQ, p, alpha);
        }

        /// <summary>
        /// Проверяет подпись sign, сообщения message, используя откр. ключ Q. В случае успеха возвращает true.
        /// </summary>
        public bool Verify(Signature sign, EcPoint pointQ, EcPoint p, BigInteger alpha)
        {
            BigInteger r = sign.R;
            BigInteger s = sign.S;

            var q = p.Q;

            if ((r < 1) || (r > (q - 1)) || (s < 1) || (s > (q - 1))) return false;

            //Иначе умножение перестанет работать, если векторы будут равны 0. Возникнет исключение. 
            if ((r == 0) || (s == 0)) return false;
            
            BigInteger e = alpha % q;
            if (e == 0) e = 1;

            BigInteger v = e.modInverse(q);
            BigInteger z1 = (s * v) % q;
            BigInteger z2 = p.Q + ((-(r * v)) % q);

            //Так как нет реализации умножения точки на нуль, делаем проверку.
            if (z1 <= 0) return false;
            if (z2 <= 0) return false;

            EcPoint a = PointMath.Multiply(z1, p);
            EcPoint b = PointMath.Multiply(z2, pointQ);
            EcPoint c = PointMath.Plus(a, b);

            BigInteger vectorR = c.X % q;

            if (vectorR == r) return true;
            
            return false;
        }

        /// <summary>
        /// Вычисляет хеш сообщения.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="bitCount"></param>
        /// <returns></returns>
        private byte[] CalculatedHash(byte[] message, int bitCount)
        {
            Hash3411.Hash3411 hasher = new Hash3411.Hash3411();
            byte[] hashResult;

            if (bitCount < 500)
            {
                hashResult = new byte[32];
                hasher.Hash256(message, hashResult);
            }
            else
            {
                hashResult = new byte[64];
                hasher.Hash512(message, hashResult);
            }

            return hashResult;
        }

        /// <summary>
        /// Подпись файла.
        /// </summary>
        /// <param name="file"></param>
        /// <param name="sendMessage"></param>
        /// <returns></returns>
        public bool SignFile(string srcfile, Action<string> sendMessage, string ecOid, byte[] signPrivateKey, EcPoint ecPublicKey)
        {
            FileInfo fi = new FileInfo(srcfile);

            if (fi.Length > SignAllowSize)
            {
                sendMessage($"Файл  более {String.Format("{0:0.00}", (float)SignAllowSize / 1024 / 1024)}Мб, подписан не будет.");
                return true;
            }

            using (FileStream inFile = new FileStream(srcfile, FileMode.Open, FileAccess.ReadWrite))
            {
                inFile.Seek(FileFormat.BeginDataBlock, SeekOrigin.Begin);
                byte[] fileData = new byte[fi.Length - FileFormat.BeginDataBlock];
                inFile.Read(fileData);
                
                var p = GetUserEcPoint(ecOid);
                if (p == null) return false;

                SignTools signToolsGen = new SignTools();
                var sign = signToolsGen.Sign(new BigInteger(signPrivateKey), fileData, p);
                var check = signToolsGen.VerifyUseHashResult(sign, ecPublicKey, p);
                if (!check)
                {
                    LastError = "Ошибка Sg4: Не удалось подписать файл. Ошибка подписи.";
                    return false;
                }

                //Добавление блока содержащего сведения о подписи.
                var asWriter = new AsDataWriter();
                inFile.Write(asWriter.CreateSignBlock(sign, new byte[58]));
                inFile.Close();
            }

            return true;
        }

        /// <summary>
        /// Проверка подписи файла.
        /// </summary>
        /// <param name="srcfile"></param>
        /// <param name="info"></param>
        /// <param name="ecOid"></param>
        /// <param name="ecPublicKey"></param>
        /// <returns></returns>
        public bool CheckSign(string srcfile, CommonFileInfo info, string ecOid, EcPoint ecPublicKey)
        {
            using (FileStream inFile = new FileStream(srcfile, FileMode.Open, FileAccess.Read))
            {
                inFile.Seek(FileFormat.BeginDataBlock, SeekOrigin.Begin);
                FileInfo fi = new FileInfo(srcfile);
                byte[] fileData = new byte[info.BeginSignBlockPosition - FileFormat.BeginDataBlock];
                inFile.Read(fileData);

                var p = GetUserEcPoint(ecOid);
                if (p == null) return false;

                var sign = new Signature();
                sign.R = new BigInteger(info.VectorR);
                sign.S = new BigInteger(info.VectorS);

                SignTools signToolsGen = new SignTools();
                if (!signToolsGen.Verify(fileData, sign, ecPublicKey, p))
                {
                    LastError = "Не верная подпись файла. Файл был поврежден или изменен третьим лицом.";
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Использую идентификатор эллиптической кривой, возвращает точку на ней.
        /// </summary>
        /// <param name="ecOid"></param>
        /// <returns></returns>
        private EcPoint GetUserEcPoint(string ecOid)
        {
            var curve = EcGroups.GetCurve(ecOid);
            if (curve == null)
            {
                LastError = "Ошибка Sg3.1: Не удалось определить параметры кривой.";
                return null;
            }

            //Создаю точку с указанными в кривой координатами точки P
            return new EcPoint(curve, true);
        }

        /// <summary>
        /// Создает ключевую пару – закрытый и открытый ключ.
        /// Генерируется ключ только длиной 512 бит. Генерация 256 битных ключей не предусмотрена.
        /// </summary>
        public bool CreateEcKeyPair(string ecOid, out byte[] privateKey, out EcPoint ecPublicKey)
        {
            privateKey = new byte[0];
            ecPublicKey = new EcPoint();

            EcPoint p = GetUserEcPoint(ecOid);
            if (p == null) return false;


            byte[] rawKey;
            BigInteger d; //Закрытый ключ подписи согласно ГОСТ.

            //Ключ целое число d, удовлетворяющим неравенству 0 < d < q.
            do
            {
                rawKey = CipherTools.GenerateRand(64);
                d = new BigInteger(rawKey);
            } while (d <= 0 || d >= p.Q);

            //Ключ проверки подписи — точка эллиптической кривой Q с координатами (xq, yq), удовлетворяющей равенству d * P = Q.
            ecPublicKey = PointMath.Multiply(d, p);
            privateKey = rawKey;

            return true;
        }
    }
}
