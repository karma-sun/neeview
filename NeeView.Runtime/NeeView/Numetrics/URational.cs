namespace NeeView.Numetrics
{
    public class URational : IRational
    {
        public URational(uint numerator, uint denominator)
        {
            this.Numerator = numerator;
            this.Denominator = denominator;
        }

        public uint Numerator { get; private set; }
        public uint Denominator { get; private set; }
        public double Value => ToValue();

        IRational IRational.Reduction()
        {
            return Reduction();
        }

        // 約分
        public URational Reduction()
        {
            if (this.Numerator == 0 || this.Denominator == 0) return this;

            uint gcd = GreatestCommonDivisor(Numerator, Denominator);
            return new URational(Numerator / gcd, Denominator / gcd);
        }

        // 最大公約数
        private uint GreatestCommonDivisor(uint x, uint y)
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

        public static bool TryParse(string value, out URational rational)
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

            if (!uint.TryParse(tokens[0], out uint numerator))
            {
                return false;
            }
            if (!uint.TryParse(tokens[1], out uint denominator))
            {
                return false;
            }

            rational = new URational(numerator, denominator);
            return true;
        }
    }
}
