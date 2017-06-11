// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using NeeView.Windows.Property;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Input;

namespace NeeView
{
    /// <summary>
    /// タッチジェスチャー
    /// </summary>
    public class TouchInputGesture : TouchInputBase
    {
        /// <summary>
        /// ジェスチャー入力
        /// </summary>
        private MouseGestureSequenceTracker _gesture;

        /// <summary>
        /// 監視するデバイス
        /// </summary>
        private TouchContext _touch;


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="context"></param>
        public TouchInputGesture(TouchInputContext context) : base(context)
        {
            _gesture = new MouseGestureSequenceTracker();
            _gesture.GestureProgressed += (s, e) => GestureProgressed.Invoke(this, new MouseGestureEventArgs(_gesture.Sequence));
        }


        /// <summary>
        /// ジェスチャー進捗通知
        /// </summary>
        public event EventHandler<MouseGestureEventArgs> GestureProgressed;

        /// <summary>
        /// ジェスチャー確定通知
        /// </summary>
        public event EventHandler<MouseGestureEventArgs> GestureChanged;


        //
        public double GestureMinimumDistanceX
        {
            get { return _gesture.GestureMinimumDistanceX; }
            set { _gesture.GestureMinimumDistanceX = value; }
        }

        //
        public double GestureMinimumDistanceY
        {
            get { return _gesture.GestureMinimumDistanceY; }
            set { _gesture.GestureMinimumDistanceY = value; }
        }

        //
        public void Reset()
        {
            if (_touch == null) return;
            _gesture.Reset(_touch.StartPoint.Position);
        }



        /// <summary>
        /// 状態開始
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="parameter"></param>
        public override void OnOpened(FrameworkElement sender, object parameter)
        {
            Debug.WriteLine("TouchState: Gesture");

            _touch = (TouchContext)parameter;

            sender.CaptureMouse();
            sender.Cursor = null;
            Reset();
        }

        /// <summary>
        /// 状態終了
        /// </summary>
        /// <param name="sender"></param>
        public override void OnClosed(FrameworkElement sender)
        {
            sender.ReleaseMouseCapture();
        }

        /// <summary>
        /// ボタン押されたときの処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public override void OnTouchDown(object sender, TouchEventArgs e)
        {
            if (e.Handled) return;

            // ジェスチャー解除
            // TODO: Drag操作へ？
            ResetState();
        }

        /// <summary>
        /// ボタン離されたときの処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public override void OnTouchUp(object sender, TouchEventArgs e)
        {
            // ジェスチャーコマンド実行
            if (_gesture.Sequence.Count > 0)
            {
                var args = new MouseGestureEventArgs(_gesture.Sequence);
                GestureChanged?.Invoke(sender, args);
                e.Handled = args.Handled;
            }

            // ジェスチャー解除
            ResetState();
        }


        /// <summary>
        /// タッチ移動処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public override void OnTouchMove(object sender, TouchEventArgs e)
        {
            if (e.Handled) return;
            if (e.TouchDevice != _touch?.TouchDevice) return;

            var point = e.GetTouchPoint(_context.Sender).Position;

            _gesture.Move(point);
        }

        #region Memento
        [DataContract]
        public class Memento
        {

            [DataMember, DefaultValue(30.0)]
            [PropertyMember("タッチジェスチャー判定の最小移動距離(X)", Tips = "この距離(pixel)移動して初めてジェスチャー開始と判定されます")]
            public double GestureMinimumDistanceX { get; set; }

            [DataMember, DefaultValue(30.0)]
            [PropertyMember("タッチジェスチャー判定の最小移動距離(Y)", Tips = "この距離(pixel)移動して初めてジェスチャー開始と判定されます")]
            public double GestureMinimumDistanceY { get; set; }
        }

        //
        public Memento CreateMemento()
        {
            var memento = new Memento();
            memento.GestureMinimumDistanceX = this.GestureMinimumDistanceX;
            memento.GestureMinimumDistanceY = this.GestureMinimumDistanceY;
            return memento;
        }

        //
        public void Restore(Memento memento)
        {
            if (memento == null) return;
            this.GestureMinimumDistanceX = memento.GestureMinimumDistanceX;
            this.GestureMinimumDistanceY = memento.GestureMinimumDistanceY;
        }
        #endregion

    }
}
