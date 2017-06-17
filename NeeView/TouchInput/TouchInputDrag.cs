// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
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
        private TouchDragManipulation _manipulation;

        /*
        private Dictionary<TouchDevice, TouchContext> _touchMap;

        //
        private DragTransform _transform;

        //
        private TouchDragContext _origin;
        */

        /// <summary>
        /// コンストラクター
        /// </summary>
        /// <param name="context"></param>
        public TouchInputDrag(TouchInputContext context) : base(context)
        {
            _manipulation = new TouchDragManipulation(context);
            //_transform = context.DragTransform;
        }

        /// <summary>
        /// 状態開始
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="parameter"></param>
        public override void OnOpened(FrameworkElement sender, object parameter)
        {
            Debug.WriteLine("TouchState: Drag");

            //InitializeTouchMap();
            _manipulation.Start();
        }


        /// <summary>
        /// 状態終了
        /// </summary>
        /// <param name="sender"></param>
        public override void OnClosed(FrameworkElement sender)
        {
            _manipulation.Stop();
        }


        /// <summary>
        /// マウスボタンが押されたときの処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public override void OnTouchDown(object sender, TouchEventArgs e)
        {
            _manipulation.Start();
            //InitializeTouchMap();
            //_now.Add(e.TouchDevice, e.GetTouchPoint(_context.Sender).Position);
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
                ResetState();
            }
            else
            {
                _manipulation.Start();
                //InitializeTouchMap();
                // _now.Remove(e.TouchDevice);
            }
        }


        /// <summary>
        /// マウス移動処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public override void OnTouchMove(object sender, TouchEventArgs e)
        {
            _manipulation.Darty();
#if false
            var current = new TouchDragContext(_context.Sender, _context.TouchMap.Keys);
            var area = _context.GetArea();


            // transform
            var move = current.GetMove(_origin);
            //Debug.WriteLine($"Drag: move: {move} : {_context.TouchMap.Count}");

            var pos = e.GetTouchPoint(_context.Sender).Position;
            //Debug.WriteLine($"({e.TouchDevice.Id}): {(int)pos.X}, {(int)pos.Y}:  {move}"); 

            var position = _basePosition + move;
            //_transform.Position = _basePosition + move;


            // ターゲット座標系での操作系中心
            var center = current.Center - new Point(area.View.Width * 0.5, area.View.Height * 0.5); // - (Vector)position;
            Debug.WriteLine($"center: {(int)center.X,3}, {(int)center.Y,3}: {move}");


            // TODO: rotate
            var angle = current.GetAngle(_origin);
            _transform.Angle = _baseAngle + angle;


            // TODO: scale
            //var scale = current.Radius > 50 ?  current.GetScale(_origin) : 1.0;
            var scale = current.GetScale(_origin);
            //Debug.WriteLine($"Drag: scale: {scale}");

            _transform.Scale = _baseScale * scale;


            var p = _basePosition;

            // move
            p = p + move;

            // rotate
            var m = new RotateTransform(angle);
            var v = (Point)(p - (Point)center);
            p = center + m.Transform(v);
            
            // scale
            var rate = _transform.Scale / _baseScale;
            //p = p - (center - (Vector)(_basePosition + move)) * (rate - 1.0);
            p = p - (center - (Vector)p) * (rate - 1.0);


            _transform.Position = p;
#endif
        }

#if false
        //
        private Point _basePosition;
        private double _baseScale;
        private double _baseAngle;

        //
        //private TouchDragContext _now;

        //
        private void InitializeTouchMap()
        {
            Debug.WriteLine($"Drag: reset");

            // clone touch map
            _touchMap = new Dictionary<TouchDevice, TouchContext>(_context.TouchMap);

            // get origin
            //_origin = new TouchDragContext(_context.TouchMap, _context.Sender);

            _origin = new TouchDragContext(_context.Sender, _context.TouchMap.Keys);

            //
            _basePosition = _transform.Position;
            _baseScale = _transform.Scale;
            _baseAngle = _transform.Angle;
        }
#endif

    }

    //
    public class TouchDragTransform
    {
        public Point Position { get; set; }
        public double Angle { get; set; }
        public double Scale { get; set; }

        public TouchDragTransform Clone()
        {
            return (TouchDragTransform)this.MemberwiseClone();
        }

        public static TouchDragTransform Lerp(TouchDragTransform m0, TouchDragTransform m1, double t)
        {
            t = NVUtility.Clamp(t, 0.0, 1.0);

            return new TouchDragTransform()
            {
                Position = m0.Position + (m1.Position - m0.Position) * t,
                Angle = m0.Angle + (m1.Angle - m0.Angle) * t,
                Scale = m0.Scale + (m1.Scale - m0.Scale) * t,
            };
        }
    }

    //
    public class TouchDragManipulation
    {
        private Dictionary<TouchDevice, TouchContext> _touchMap;
        private DragTransform _transform;
        private TouchDragContext _origin;


        TouchDragTransform _start;
        TouchDragTransform _goal;
        TouchDragTransform _now;


        private bool _ticking;
        private bool _darty;

        private bool _allowAngle;
        private bool _allowScale;

        private TouchInputContext _context;

        //
        public TouchDragManipulation(TouchInputContext context)
        {
            _context = context;
            _transform = context.DragTransform;
        }


        //
        public void Start()
        {
            Debug.WriteLine($"Drag: reset");

            // clone touch map
            _touchMap = new Dictionary<TouchDevice, TouchContext>(_context.TouchMap);

            // get origin
            _origin = new TouchDragContext(_context.Sender, _context.TouchMap.Keys);

            // 
            _start = new TouchDragTransform()
            {
                Position = _transform.Position,
                Angle = _transform.Angle,
                Scale = _transform.Scale,
            };

            _now = _start.Clone();

            _darty = true;

            _allowAngle = false;
            _allowScale = false;

            //
            StartTicking();
        }

        //
        public void Stop()
        {
            StopTicking();
        }

        //
        public void Darty()
        {
            _darty = true;
        }

        private void StartTicking()
        {
            if (!_ticking)
            {
                _ticking = true;
                CompositionTarget.Rendering += new EventHandler(OnRendering);
            }
        }

        private void StopTicking()
        {
            if (_ticking)
            {
                CompositionTarget.Rendering -= new EventHandler(OnRendering);
                _ticking = false;
            }
        }

        private void OnRendering(object sender, EventArgs e)
        {
            ReportFrame();
        }

        private void ReportFrame()
        {
            // TODO: TIME SHARE

            if (_darty)
            {
                _darty = false;
                _goal = GetTransform();
            }

            _now = TouchDragTransform.Lerp(_now, _goal, 0.5);

            _transform.Position = _now.Position;
            _transform.Angle = _now.Angle;
            _transform.Scale = _now.Scale;

        }

        private TouchDragTransform GetTransform()
        {
            var current = new TouchDragContext(_context.Sender, _context.TouchMap.Keys);
            var area = _context.GetArea();


            // transform
            var move = current.GetMove(_origin);
            //Debug.WriteLine($"Drag: move: {move} : {_context.TouchMap.Count}");

            //var pos = e.GetTouchPoint(_context.Sender).Position;
            //Debug.WriteLine($"({e.TouchDevice.Id}): {(int)pos.X}, {(int)pos.Y}:  {move}"); 

            // var position = _basePosition + move;
            //_transform.Position = _basePosition + move;


            // ターゲット座標系での操作系中心
            var center = current.Center - new Point(area.View.Width * 0.5, area.View.Height * 0.5); // - (Vector)position;
            //Debug.WriteLine($"center: {(int)center.X,3}, {(int)center.Y,3}: {move}");


            // TODO: rotate
            var angle = current.GetAngle(_origin);
            //_transform.Angle = _baseAngle + angle;

            _allowAngle = _allowAngle || (current.Radius > 50.0 &&  Math.Abs(current.Radius * Math.Sin(angle * 0.5 * Math.PI / 180)) > 15.0);
            angle = _allowAngle ? angle : 0.0;



            // TODO: scale
            _allowScale = _allowScale || current.Radius > 50.0;

            //var scale = current.Radius > 50 ?  current.GetScale(_origin) : 1.0;
            var scale = current.GetScale(_origin);
            scale = _allowScale ? scale : 1.0;
            //Debug.WriteLine($"Drag: scale: {scale}");

            //_transform.Scale = _baseScale * scale;


            var p = _start.Position;

            // move
            p = p + move;

            // rotate
            var m = new RotateTransform(angle);
            var v = (Point)(p - (Point)center);
            p = center + m.Transform(v);

            // scale
            var rate = scale; // _transform.Scale / _baseScale;
            p = p - (center - (Vector)p) * (rate - 1.0);


            // _transform.Position = p;

            return new TouchDragTransform
            {
                Position = p,
                Angle = _start.Angle + angle,
                Scale = _start.Scale * scale
            };
        }

    }

    //
    public class TouchDragUnit
    {
        //public TouchDevice TouchDevice { get; private set; }
        public Point Position { get; set; }
        public double Length { get; set; }
    }

    //
    public class TouchDragContext
    {
        private FrameworkElement _sender;

        private Dictionary<TouchDevice, TouchDragUnit> _touches;

        public Point Center { get; private set; }

        public double Radius { get; private set; }


        //
        public TouchDragContext(FrameworkElement sender, IEnumerable<TouchDevice> touchDevices)
        {
            _sender = sender;

            _touches = touchDevices.ToDictionary(e => e, e => new TouchDragUnit() { Position = e.GetTouchPoint(sender).Position });

            var positions = _touches.Values.Select(e => e.Position);
            this.Center = new Point(positions.Average(e => e.X), positions.Average(e => e.Y));

            foreach (var touch in this._touches.Values)
            {
                touch.Length = (touch.Position - this.Center).Length;
            }

            this.Radius = _touches.Values.Select(e => e.Length).Max();
        }

        //
        public Vector GetMove(TouchDragContext source)
        {
            return this.Center - source.Center;
        }

        //
        public double GetScale(TouchDragContext source)
        {
            return _touches.Select(e => e.Value.Length / source._touches[e.Key].Length).Average();
        }

        //
        public double GetAngle(TouchDragContext source)
        {
            var v1 = source.GetVector();
            var v2 = this.GetVector();
            return Vector.AngleBetween(v1, v2);
        }

        //
        public Vector GetVector()
        {
            return _touches.Last().Value.Position - _touches.First().Value.Position;
        }

    }
}
