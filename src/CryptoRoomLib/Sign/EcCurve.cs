namespace CryptoRoomLib.Sign
{
    /// <summary>
    /// Параметры эллиптической кривой.
    /// </summary>
    public class EcCurve
    {
        /// <summary>
        /// Имя кривой.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Простое число p, определяющее размерность конечного поля;
        /// </summary>
        public string P { get; set; }

        /// <summary>
        /// Коэффициент a уравнения эллиптической кривой;
        /// </summary>
        public string A { get; set; }

        /// <summary>
        /// Коэффициент b уравнения эллиптической кривой;
        /// </summary>
        public string B { get; set; }

        /// <summary>
        /// Координата x базовой точки G, генерирующая подгруппу.
        /// </summary>
        public string Gx { get; set; }

        /// <summary>
        /// Координата y базовой точки G, генерирующая подгруппу.
        /// </summary>
        public string Gy { get; set; }

        /// <summary>
        /// Порядок n подгруппы.
        /// </summary>
        public string N { get; set; }

        /// <summary>
        /// Кофактор h подгруппы.
        /// </summary>
        public string H { get; set; }

        /// <summary>
        /// Идентификатор кривой.
        /// </summary>
        public string Oid { get; set; }
    }
}
