// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace NeeView
{
    /// <summary>
    /// タッチ通常ドラッグ状態
    /// </summary>
    public class TouchInputDrag : TouchInputBase
    {
        private Dictionary<TouchDevice, TouchContext> _touchMap;

        //
        private TouchDragContext _origin;

        /// <summary>
        /// コンストラクター
        /// </summary>
        /// <param name="context"></param>
        public TouchInputDrag(TouchInputContext context) : base(context)
        {
        }

        /// <summary>
        /// 状態開始
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="parameter"></param>
        public override void OnOpened(FrameworkElement sender, object parameter)
        {
            Debug.WriteLine("TouchState: Drag");

            InitializeTouchMap();
        }


        /// <summary>
        /// 状態終了
        /// </summary>
        /// <param name="sender"></param>
        public override void OnClosed(FrameworkElement sender)
        {
        }


        /// <summary>
        /// マウスボタンが押されたときの処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public override void OnTouchDown(object sender, TouchEventArgs e)
        {
            InitializeTouchMap();
        }

        /// <summary>
        /// マウスボタンが離されたときの処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public override void OnTouchUp(object sender, TouchEventArgs e)
        {
            // マルチタッチでなくなったら解除
            if (_context.TouchMap.Count < 2)
            {
                // TODO: スナップ挙動

                ResetState();
            }
            else
            {
                InitializeTouchMap();
            }
        }


        /// <summary>
        /// マウス移動処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public override void OnTouchMove(object sender, TouchEventArgs e)
        {
            var current = new TouchDragContext(_context.TouchMap, _context.Sender);

            // transform
            var move = current.GetMove(_origin);
            Debug.WriteLine($"Drag: move: {move}");

            // TODO: rotate

            // TODO: scale
            var scale = current.GetScale(_origin);
            Debug.WriteLine($"Drag: scale: {scale}");

        }

        //
        private void InitializeTouchMap()
        {
            Debug.WriteLine($"Drag: reset");

            // clone touch map
            _touchMap = new Dictionary<TouchDevice, TouchContext>(_context.TouchMap);

            // get origin
            _origin = new TouchDragContext(_context.TouchMap, _context.Sender);
        }
    }


    //
    public class TouchDragUnit
    {
        //public TouchDevice TouchDevice { get; private set; }
        public Point Position { get; private set; }
        public double Length { get; private set; }

        public TouchDragUnit(Point position, Point center)
        {
            this.Position = position;
            this.Length = (this.Position - center).Length;
        }
    }

    //
    public class TouchDragContext
    {
        public Dictionary<TouchDevice, TouchDragUnit> Touchs { get; private set; }

        public Point Center { get; private set; }

        //
        public TouchDragContext(Dictionary<TouchDevice, TouchContext> touchMap, FrameworkElement sender)
        {
            this.Center = GetCenter(touchMap.Values, sender);
            this.Touchs = touchMap.ToDictionary(e=>e.Key, e => new TouchDragUnit(e.Value.TouchDevice.GetTouchPoint(sender).Position, this.Center));
        }

        //
        private Point GetCenter(IEnumerable<TouchContext> touchContext, FrameworkElement sender)
        {
            var positions = touchContext.Select(e => e.TouchDevice.GetTouchPoint(sender).Position);
            return new Point(positions.Average(e => e.X), positions.Average(e => e.Y));
        }

        //
        public Vector GetMove(TouchDragContext source)
        {
            return this.Center - source.Center;
        }

        //
        public double GetScale(TouchDragContext source)
        {
            return this.Touchs.Select(e => e.Value.Length / source.Touchs[e.Key].Length).Average();
        }

    }
}
