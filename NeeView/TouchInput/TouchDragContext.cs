// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace NeeView
{
    /// <summary>
    /// 複数のタッチからなる情報
    /// </summary>
    public class TouchDragContext
    {
        /// <summary>
        /// 座標系
        /// </summary>
        private FrameworkElement _sender;

        /// <summary>
        /// タッチ点情報
        /// </summary>
        private List<Point> _touches;

        /// <summary>
        /// 中心座標
        /// </summary>
        public Point Center { get; private set; }

        /// <summary>
        /// 半径
        /// </summary>
        public double Radius { get; private set; }


        /// <summary>
        /// コンストラクター
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="touchDevices"></param>
        public TouchDragContext(FrameworkElement sender, IEnumerable<StylusDevice> touchDevices)
        {
            _sender = sender;
            _touches = touchDevices.Select(e => e.GetPosition(sender)).ToList();
            this.Center = new Point(_touches.Average(e => e.X), _touches.Average(e => e.Y));
            this.Radius = _touches.Select(e => (e - this.Center).Length).Max();
        }

        /// <summary>
        /// 移動量取得
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public Vector GetMove(TouchDragContext source)
        {
            return this.Center - source.Center;
        }

        /// <summary>
        /// 拡大率取得
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public double GetScale(TouchDragContext source)
        {
            if (_touches.Count < 2) return 1.0;
            return this.Radius / source.Radius;
        }

        /// <summary>
        /// 角度取得
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public double GetAngle(TouchDragContext source)
        {
            if (_touches.Count < 2) return 0.0;

            var v1 = source.GetVector();
            var v2 = this.GetVector();
            return Vector.AngleBetween(v1, v2);
        }

        /// <summary>
        /// 代表２点のベクトルを取得
        /// </summary>
        /// <returns></returns>
        public Vector GetVector()
        {
            return _touches.Last() - _touches.First();
        }
    }
}
