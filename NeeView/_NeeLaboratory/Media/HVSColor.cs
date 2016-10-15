// http://sourcechord.hatenablog.com/entry/20130710/1373476676

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;


namespace NeeLaboratory.Media
{
    /// <summary>
    /// 
    /// </summary>
    public struct HSVColor
    {
        public double H { get; set; }
        public double S { get; set; }
        public double V { get; set; }

        public static HSVColor FromHSV(double h, double s, double v)
        {
            return new HSVColor() { H = h, S = s, V = v };
        }

        public override string ToString()
        {
            return string.Format("H:{0}, S:{1}, V:{2}", H, S, V);
        }

        public Color ToRGB()
        {
            int Hi = ((int)(H / 60.0)) % 6;
            double f = H / 60.0f - (int)(H / 60.0);
            double p = V * (1 - S);
            double q = V * (1 - f * S);
            double t = V * (1 - (1 - f) * S);

            switch (Hi)
            {
                case 0:
                    return FromRGB(V, t, p);
                case 1:
                    return FromRGB(q, V, p);
                case 2:
                    return FromRGB(p, V, t);
                case 3:
                    return FromRGB(p, q, V);
                case 4:
                    return FromRGB(t, p, V);
                case 5:
                    return FromRGB(V, p, q);
            }

            // ここには来ない
            throw new InvalidOperationException();
        }

        private Color FromRGB(double fr, double fg, double fb)
        {
            fr *= 255;
            fg *= 255;
            fb *= 255;
            byte r = (byte)((fr < 0) ? 0 : (fr > 255) ? 255 : fr);
            byte g = (byte)((fg < 0) ? 0 : (fg > 255) ? 255 : fg);
            byte b = (byte)((fb < 0) ? 0 : (fb > 255) ? 255 : fb);
            return Color.FromRgb(r, g, b);
        }
    }


    /// <summary>
    /// 
    /// </summary>
    public static class ColorExtension
    {
        public static HSVColor ToHSV(this Color c)
        {
            double r = c.R / 255.0;
            double g = c.G / 255.0;
            double b = c.B / 255.0;

            var list = new double[] { r, g, b };
            var max = list.Max();
            var min = list.Min();

            double h, s, v;
            if (max == min)
                h = 0;
            else if (max == r)
                h = (60 * (g - b) / (max - min) + 360) % 360;
            else if (max == g)
                h = 60 * (b - r) / (max - min) + 120;
            else
                h = 60 * (r - g) / (max - min) + 240;

            if (max == 0)
                s = 0;
            else
                s = (max - min) / max;

            v = max;

            return new HSVColor() { H = h, S = s, V = v };
        }
    }
}
