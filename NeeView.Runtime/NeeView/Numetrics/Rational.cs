namespace NeeView.Numetrics
{

    /// <summary>
    /// 分数表現
    /// </summary>
    public class Rational : IRational
    {
        public static Rational Zero { get; } = new Rational(0, 1);

        public Rational(int numerator, int denominator)
        {
            this.Numerator = numerator;
            this.Denominator = denominator;
        }

        public int Numerator { get; private set; }
        public int Denominator { get; private set; }
        public double Value => ToValue();

        IRational IRational.Reduction()
        {
            return Reduction();
        }

        // 約分
        public Rational Reduction()
        {
            if (this.Numerator == 0 || this.Denominator == 0) return this;

            int gcd = GreatestCommonDivisor(Numerator, Denominator);
            return new Rational(Numerator / gcd, Denominator / gcd);
        }

        // 最大公約数
        private int GreatestCommonDivisor(int x, int y)
        {
            while (true)
            {
                x = x % y;
                if (x == 0)
                {
                    return y;
                }
                y = y % x;
                if (y == 0)
                {
                    return x;
                }
            }
        }

        public double ToValue()
        {
            if (Denominator == 0)
            {
                return Numerator == 0 ? 0.0 : (Numerator > 0 ? double.PositiveInfinity : double.NegativeInfinity);
            }
            return (double)Numerator / Denominator;
        }

        public string ToRationalString()
        {
            var reduction = Reduction();
            return reduction.Denominator == 1 ? reduction.Numerator.ToString() : $"{reduction.Numerator}/{reduction.Denominator}";
        }

        public override string ToString()
        {
            return ToRationalString();
        }

        public static bool TryParse(string value, out Rational rational)
        {
            rational = null;

            if (value is null)
            {
                return false;
            }

            var tokens = value.Split('/');
            if (tokens.Length != 2)
            {
                return false;
            }

            if (!int.TryParse(tokens[0], out int numerator))
            {
                return false;
            }
            if (!int.TryParse(tokens[1], out int denominator))
            {
                return false;
            }

            rational = new Rational(numerator, denominator);
            return true;
        }
    }
}
