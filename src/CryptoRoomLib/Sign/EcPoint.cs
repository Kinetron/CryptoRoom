namespace CryptoRoomLib.Sign
{
    /// <summary>
    /// Содержит набор методов для работы с точкой эллиптической кривой. 
    /// </summary>
    public class EcPoint
    {
        /// <summary>
        /// Координата x точки.
        /// </summary>
        public BigInteger X;

        /// <summary>
        /// Координата y точки.
        /// </summary>
        public BigInteger Y;

        /// <summary>
        /// Коэффициент a уравнения эллиптической кривой.
        /// </summary>
        public BigInteger A;

        /// <summary>
        /// Коэффициент b уравнения эллиптической кривой.
        /// </summary>
        public BigInteger B;

        /// <summary>
        /// Модуль эллиптической кривой.
        /// </summary>
        public BigInteger P;

        /// <summary>
        /// Порядок точки. В зарубежной литераторе обозначается как n.
        /// </summary>
        public BigInteger Q;
        
        public EcPoint()
        {
            
        }

        /// <summary>
        /// setXY=1 устанавливать координаты точки P
        /// </summary>
        /// <param name="Ec"></param>
        /// <param name="setXY"></param>
        public EcPoint(EcCurve Ec, bool setXY)
        {
            //Cчитываю необходимые параметры кривой.
            if (Ec.A.Contains("-"))
            {
                A = new BigInteger(Ec.A, 10);
            }
            else
            {
                A = new BigInteger(Ec.A, 16);
            }
            
            B = new BigInteger(Ec.B, 16);
            P = new BigInteger(Ec.P, 16);
            Q = new BigInteger(Ec.N, 16);

            //Задавать координаты точке
            if (setXY)
            {
                X = new BigInteger(Ec.Gx, 16);
                Y = new BigInteger(Ec.Gy, 16);
            }
            else
            {
                X = 0;
                Y = 0;
            }
        }
    }
}
