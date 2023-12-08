using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace CryptoRoomLib.KeyGenerator
{
    /// <summary>
    /// Контейнер секретного ключа.
    /// Большинство полей требуются для поддержки стандарта.
    /// </summary>
    [XmlRoot("PkContainer")]
    public class SecretKeyContainer
    {
        /// <summary>
        /// Версия  ключа
        /// </summary>
        [XmlElement(ElementName = "P0")]
        public string KeyVersion { get; set; }

        /// <summary>
        /// Версия программы генератора ключа
        /// </summary>
        [XmlElement(ElementName = "P1")]
        public string KeyGenVersion { get; set; }

        /// <summary>
        /// Наименование организации.
        /// </summary>
        [XmlElement(ElementName = "P2")]
        public string OrgName { get; set; }

        /// <summary>
        /// Код организации.
        /// </summary>
        [XmlElement(ElementName = "P3")]
        public string OrgCode { get; set; }

        /// <summary>
        /// Подразделение.
        /// </summary>
        [XmlElement(ElementName = "P4")]
        public string Department { get; set; }

        /// <summary>
        /// Телефон.
        /// </summary>
        [XmlElement(ElementName = "P5")]
        public string PhoneNumber { get; set; }

        /// <summary>
        /// Фамилия лица создавшего ключ.
        /// </summary>
        [XmlElement(ElementName = "P6")]
        public string Familia { get; set; }

        /// <summary>
        /// Имя лица создавшего ключ.
        /// </summary>
        [XmlElement(ElementName = "P7")]
        public string Imia { get; set; }

        /// <summary>
        /// Отчество лица создавшего ключ.
        /// </summary>
        [XmlElement(ElementName = "P8")]
        public string Otchestvo { get; set; }

        /// <summary>
        /// Дата и время создания ключа.
        /// </summary>
        [XmlElement(ElementName = "P9")]
        public DateTime DateBegin { get; set; }

        /// <summary>
        /// Дата и время окончания действия ключа.
        /// </summary>

        [XmlElement(ElementName = "P10")]
        public DateTime DateEnd { get; set; }

        /// <summary>
        /// Шифрованный секретный ключ подписи файлов.
        /// </summary>

        [XmlElement(ElementName = "P11")]
        public string CryptSignKey { get; set; }

        /// <summary>
        /// Соль для декодирования закрытого ключа подписи.
        /// </summary>
        [XmlElement(ElementName = "P12")]
        public string SaltCryptSignKey { get; set; }

        /// <summary>
        /// Дополнительный параметры для использования в будущем.
        /// </summary>
        [XmlElement(ElementName = "P13")]
        public string ReservedParameter0 { get; set; }

        /// <summary>
        /// Начальный вектор для декодирования закрытого ключа подписи.
        /// </summary>
        [XmlElement(ElementName = "P14")]
        public string IvCryptSignKey { get; set; }
        
        /// <summary>
        /// Время и дата когда ключ был сгенерирован.
        /// </summary>
        [XmlElement(ElementName = "P15")]
        public DateTime KeyGenTimeStamp { get; set; }

        /// <summary>
        /// Открытый ключ подписи, координата X.
        /// </summary>

        [XmlElement(ElementName = "P16")]
        public string OpenSignKeyPointX { get; set; }

        /// <summary>
        /// Открытый ключ подписи, координата Y.
        /// </summary>

        [XmlElement(ElementName = "P17")]
        public string OpenSignKeyPointY { get; set; }

        /// <summary>
        /// Шифрованное значение закрытого ключа RSA.
        /// </summary>
        [XmlElement(ElementName = "P18")] 
        public string CryptRsaPrivateKey { get; set; }

        /// <summary>
        /// Значение вектора iv для закрытого ключа RSA. Для расшифровки.
        /// </summary>
        [XmlElement(ElementName = "P19")]
        public string RsaIv { get; set; }

        /// <summary>
        /// Открытый ключ RSA.
        /// </summary>
        [XmlElement(ElementName = "P20")]
        public string RsaPublicKey { get; set; }

        /// <summary>
        /// Соль для закрытого ключа RSA. Для расшифровки.
        /// </summary>
        [XmlElement(ElementName = "P21")]
        public string RsaSalt { get; set; }
        
        /// <summary>
        /// Идентификатор алгоритма открытого ключа подписи.
        /// </summary>
        [XmlElement(ElementName = "P22")]
        public string PublicKeyAlgoritmOid { get; set; }
        
        /// <summary>
        /// Дополнительный параметры для использования в будущем.
        /// </summary>
        [XmlElement(ElementName = "P23")]
        public string ReservedParameter1 { get; set; }

        /// <summary>
        /// Идентификатор эллиптической кривой на основании которой был создан ключ подписи.
        /// </summary>
        [XmlElement(ElementName = "P24")]
        public string EcOid { get; set; }
    }
}
