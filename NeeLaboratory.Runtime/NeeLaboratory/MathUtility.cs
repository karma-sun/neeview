using System;
using System.Windows;

namespace NeeLaboratory
{
    public static class MathUtility
    {
        // from http://stackoverflow.com/questions/2683442/where-can-i-find-the-clamp-function-in-net
        public static T Clamp<T>(this T val, T min, T max) where T : IComparable<T>
        {
            if (val.CompareTo(min) < 0) return min;
            else if (val.CompareTo(max) > 0) return max;
            else return val;
        }

        public static bool WithinRange<T>(this T val, T min, T max) where T : IComparable<T>
        {
            if (val.CompareTo(min) < 0) return false;
            else if (val.CompareTo(max) > 0) return false;
            else return true;
        }

        public static T Max<T>(this T v0, T v1) where T : IComparable<T>
        {
            return v0.CompareTo(v1) > 0 ? v0 : v1;
        }

        public static T Min<T>(this T v0, T v1) where T : IComparable<T>
        {
            return v0.CompareTo(v1) < 0 ? v0 : v1;
        }

        public static double Lerp(double v0, double v1, double rate)
        {
            return v0 + (v1 - v0) * rate;
        }

#if false
        public static Vector Lerp(Vector v0, Vector v1, double rate)
        {
            return v0 + (v1 - v0) * rate;
        }
#endif

        public static double Snap(double val, double tick)
        {
            return Math.Floor((val + tick * 0.5) / tick) * tick;
        }
    }
}

