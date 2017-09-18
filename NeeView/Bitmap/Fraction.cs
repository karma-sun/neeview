// Copyright (c) 2016-2017 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php


namespace NeeView
{
    /// <summary>
    /// 分数表現
    /// </summary>
    public class Fraction
    {
        public Fraction(int numerator, int denominator)
        {
            this.Numerator = numerator;
            this.Denominator = denominator;
        }

        public int Numerator { get; private set; } // 分子
        public int Denominator { get; private set; } // 分母

        public double Value => (double)Numerator / Denominator;

        // 約分
        public void Reduction()
        {
            // 0除算例外発生を回避
            if (this.Numerator == 0 || this.Denominator == 0) return;

            int gcd = GreatestCommonDivisor(Numerator, Denominator);
            Numerator /= gcd;
            Denominator /= gcd;
        }

        // 最大公約数
        private int GreatestCommonDivisor(int x, int y)
        {
            while (true)
            {
                x = x % y;
                if (x == 0)
                    return y;
                y = y % x;
                if (y == 0)
                    return x;
            }
        }
    }
}
