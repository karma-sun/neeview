using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NeeView.Windows
{
    public static class VectorExtensions
    {
        // from http://stackoverflow.com/questions/22818531/how-to-rotate-2d-vector

        private const double DegToRad = Math.PI / 180;

        public static Vector Rotate(this Vector v, double degrees)
        {
            return v.RotateRadians(degrees * DegToRad);
        }

        public static Vector RotateRadians(this Vector v, double radians)
        {
            var ca = Math.Cos(radians);
            var sa = Math.Sin(radians);
            return new Vector(ca * v.X - sa * v.Y, sa * v.X + ca * v.Y);
        }
    }
}
