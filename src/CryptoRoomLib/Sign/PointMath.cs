namespace CryptoRoomLib.Sign
{
    /// <summary>
    /// Класс предназначен для выполнения арифметических действий с эллиптической кривой.
    /// </summary>
    public class PointMath
    {
        /// <summary>
        /// Сложение двух точек.
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        public static EcPoint Plus(EcPoint p1, EcPoint p2)
        {
            EcPoint p3 = new EcPoint();
            p3.A = p1.A;
            p3.B = p1.B;
            p3.P = p1.P;

            BigInteger dy = p2.Y - p1.Y;
            BigInteger dx = p2.X - p1.X;

            if (dx < 0) dx += p1.P;
            if (dy < 0) dy += p1.P;

            BigInteger m = (dy * dx.modInverse(p1.P)) % p1.P;
            if (m < 0) m += p1.P;
            
            p3.X = (m * m - p1.X - p2.X) % p1.P;
            p3.Y = (m * (p1.X - p3.X) - p1.Y) % p1.P;

            if (p3.X < 0) p3.X += p1.P;
            if (p3.Y < 0) p3.Y += p1.P;

            return p3;
        }

        /// <summary>
        /// Умножаю точку p на число. Фактически представляет n сложений точки самой с собой.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        public static EcPoint Multiply(BigInteger x, EcPoint p)
        {
            EcPoint temp = p;
            x = x - 1;
            while (x != 0)
            {
                if ((x % 2) != 0)
                {
                    if ((temp.X == p.X) || (temp.Y == p.Y))
                    {
                        temp = AddItSelf(temp);
                    }
                    else
                    {
                        temp = Plus(temp, p);
                    }
                    x = x - 1;
                }
                x = x / 2;
                p = AddItSelf(p);
            }
            return temp;
        }
        
        /// <summary>
        /// Cложение точки p самой с собой.
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public static EcPoint AddItSelf(EcPoint p)
        {
            EcPoint p2 = new EcPoint();
            p2.A = p.A;
            p2.B = p.B;
            p2.P = p.P;

            BigInteger dy = 3 * p.X * p.X + p.A;
            BigInteger dx = 2 * p.Y;

            if (dx < 0) dx += p.P;
            if (dy < 0) dy += p.P;

            BigInteger m = (dy * dx.modInverse(p.P)) % p.P;
            p2.X = (m * m - p.X - p.X) % p.P;
            p2.Y = (m * (p.X - p2.X) - p.Y) % p.P;

            if (p2.X < 0) p2.X += p.P;
            if (p2.Y < 0) p2.Y += p.P;

            return p2;
        }

        /// <summary>
        /// Вычисление квадратного корня по модулю простого числа q.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="q"></param>
        /// <returns></returns>
        public static BigInteger ModSqrt(BigInteger a, BigInteger q)
        {
            BigInteger b = new BigInteger();
            do
            {
                b.genRandomBits(255, new Random());
            } while (Legendre(b, q) == 1);

            BigInteger s = 0;
            BigInteger t = q - 1;
            
            while ((t & 1) != 1)
            {
                s++;
                t = t >> 1;
            }

            BigInteger invA = a.modInverse(q);
            BigInteger c = b.modPow(t, q);
            BigInteger r = a.modPow(((t + 1) / 2), q);
            BigInteger d = new BigInteger();
            
            for (int i = 1; i < s; i++)
            {
                BigInteger temp = 2;
                temp = temp.modPow((s - i - 1), q);
                d = (r.modPow(2, q) * invA).modPow(temp, q);

                if (d == (q - 1)) r = (r * c) % q;
                c = c.modPow(2, q);
            }

            return r;
        }
        
        /// <summary>
        /// Вычисляет символ Лежандра.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="q"></param>
        /// <returns></returns>
        public static BigInteger Legendre(BigInteger a, BigInteger q)
        {
            return a.modPow((q - 1) / 2, q);
        }

        /// <summary>
        /// Формирует координаты x, y точки из координаты x и бита четности y.
        /// </summary>
        /// <param name="gCoord"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        public static EcPoint GDecompression(byte[] gCoord, EcPoint p)
        {
            byte y = gCoord[0];
            byte[] x = new byte[gCoord.Length - 1];

            Array.Copy(gCoord, 1, x, 0, gCoord.Length - 1);
            BigInteger xCord = new BigInteger(x);
            BigInteger temp = (xCord * xCord * xCord + p.A * xCord + p.B) % p.P;
            BigInteger beta = ModSqrt(temp, p.P);

            BigInteger yCord = new BigInteger();
            if ((beta % 2) == (y % 2))
            {
                yCord = beta;
            }
            else
            {
                yCord = p.P - beta;
            }

            EcPoint g = new EcPoint();
            g.A = p.A;
            g.B = p.B;
            g.P = p.P;
            g.X = xCord;
            g.Y = yCord;

            return g;
        }

        /// <summary>
        /// Генерирует псевдо случайное число(не криптографически стойкое) заданной длины.
        /// </summary>
        /// <param name="q"></param>
        /// <returns></returns>
        public static BigInteger GeneratedPseudoRandom(int size)
        {
            BigInteger k = new BigInteger();
            do
            {
                k.genRandomBits(size, new Random());
            } while ((k < 0) || (k.bitCount() > size));

            return k;
        }

        /// <summary>
        /// Создает открытый ключ - точка эллиптической кривой Q = d * G.
        /// </summary>
        /// <param name="d"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        public static EcPoint GenPublicKey(BigInteger d, EcPoint p)
        {
            return Multiply(d, p);
        }
    }
}
