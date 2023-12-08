using System.Text;

namespace CryptoRoomLib.Sign
{
    /// <summary>
    /// Набор встроенных тестов для проверки правильности алгоритма.
    /// </summary>
    public class SelfTests : SelfTestsBase
    {
        public SelfTests()
        {
            AppendFunc(PaddingLeftTest);
            AppendFunc(CoordUnpackTest);
            AppendFunc(SignTest);
        }

        /// <summary>
        /// Тест распаковки координат точки.
        /// </summary>
        /// <returns></returns>
        private bool CoordUnpackTest()
        {
            var curve = EcGroups.GetCurve("ECC-192");
            if (curve == null)
            {
                LastError = "Ошибка Sg3.1: Не удалось определить параметры кривой.";
                return false;
            }

            //Создаю точку с указанными в кривой координатами точки P.
            EcPoint p = new EcPoint(curve, true);

            byte[] packCoord = Convert.FromHexString("03188da80eb03090f67cbf20eb43a18800f4ff0afd82ff1012");

            var q = PointMath.GDecompression(packCoord, p);

            if (q.X != p.X || q.Y != p.Y)
            {
                LastError = "CoordUnpackTest: Координаты не совпадают.";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Тест подписи строки.
        /// </summary>
        /// <returns></returns>
        private bool SignTest()
        {
            var curve = EcGroups.GetCurve("ECC-192");
            if (curve == null)
            {
                LastError = "Ошибка Sg3.1: Не удалось определить параметры кривой.";
                return false;
            }

            //Создаю точку с указанными в кривой координатами точки P.
            EcPoint p = new EcPoint(curve, true);

            SignTools signToolsGen = new SignTools();

            string text = "Alice and Bob are fictional characters commonly used as placeholders " +
                          "in discussions about cryptographic systems and protocols, and in " +
                          "other science and engineering literature where there are several " +
                          "participants in a thought experiment. The Alice and Bob characters were " +
                          "invented by Ron Rivest, Adi Shamir, and Leonard Adleman in their 1978 paper" +
                          " \"A Method for Obtaining Digital Signatures and Public-key Cryptosystems\"." +
                          "Subsequently, they have become common archetypes in many scientific and " +
                          "engineering fields, such as quantum cryptography, game theory and physics." +
                          " As the use of Alice and Bob became more widespread, additional characters were " +
                          "added, sometimes each with a particular meaning. " +
                          "These characters do not have to refer to people; they refer to generic agents which " +
                          "might be different computers or even different programs running on a single computer.";

            byte[] message = Encoding.Default.GetBytes(text);
            
            var d = PointMath.GeneratedPseudoRandom(p.Q.bitCount()); //Закрытый ключ.
            EcPoint Q = new EcPoint();
            Q = PointMath.GenPublicKey(d, p); //Открытый ключ.

            var sign = signToolsGen.Sign(d, message, p);
           
            bool result = signToolsGen.Verify(message, sign, Q, p);
            if (!result)
            {
                LastError = "SignTest: Ошибка проверки подписи.";
                return false;
            }

            message[0] = 0;
            result = signToolsGen.Verify(message, sign, Q, p);
            if (result)
            {
                LastError = "SignTest: Неверный алгоритм проверки подписи";
                return false;
            }
            
            return true;
        }

        /// <summary>
        /// Дополнение массива до кратной длины.
        /// </summary>
        /// <returns></returns>
        private bool PaddingLeftTest()
        {
            byte[] message = new byte[63];
            var result = CipherTools.PaddingLeft(message);

            if (result.Length != message.Length + 1)
            {
                LastError = "Ошибка теста дополнения нулями слева.";
                return false;
            }

            return true;
        }
    }
}