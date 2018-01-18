// Copyright (c) 2016-2018 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System.Windows;

namespace NeeView
{
    /// <summary>
    /// タッチ操作用トランスフォーム
    /// </summary>
    public class TouchDragTransform
    {
        public Vector Trans { get; set; }
        public double Angle { get; set; }
        public double Scale { get; set; }

        // 回転、拡大縮小の中心
        public bool IsValidCenter { get; set; }
        public Vector Center { get; set; }

        //
        public TouchDragTransform Clone()
        {
            return (TouchDragTransform)this.MemberwiseClone();
        }

        internal void Add(TouchDragTransform m)
        {
            this.Trans += m.Trans;
            this.Angle += m.Angle;
            this.Scale += m.Scale;
        }

        internal void Sub(TouchDragTransform m)
        {
            this.Trans -= m.Trans;
            this.Angle -= m.Angle;
            this.Scale -= m.Scale;
        }

        internal void Multiply(double v)
        {
            this.Trans *= v;
            this.Angle *= v;
            this.Scale *= v;
        }


        public static TouchDragTransform Sub(TouchDragTransform m0, TouchDragTransform m1)
        {
            var m = m0.Clone();
            m.Sub(m1);
            return m;
        }

        public static TouchDragTransform Lerp(TouchDragTransform m0, TouchDragTransform m1, double t)
        {
            t = NVUtility.Clamp(t, 0.0, 1.0);

            return new TouchDragTransform()
            {
                Trans = m0.Trans + (m1.Trans - m0.Trans) * t,
                Angle = m0.Angle + (m1.Angle - m0.Angle) * t,
                Scale = m0.Scale + (m1.Scale - m0.Scale) * t,
            };
        }

    }
}
