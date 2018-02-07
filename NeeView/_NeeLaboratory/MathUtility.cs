// Copyright (c) 2016-2018 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

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

        //
        public static double Lerp(double v0, double v1, double rate)
        {
            return v0 + (v1 - v0) * rate;
        }

        //
        public static Vector Lerp(Vector v0, Vector v1, double rate)
        {
            return v0 + (v1 - v0) * rate;
        }
    }
}

