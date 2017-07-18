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
        #region Fields

        /// <summary>
        /// ジェスチャー入力
        /// </summary>
        private MouseGestureSequenceTracker _gesture;

        /// <summary>
        /// 監視するデバイス
        /// </summary>
        private TouchContext _touch;

        #endregion

        #region Constructors

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="context"></param>
        public TouchInputGesture(TouchInputContext context) : base(context)
        {
            _gesture = new MouseGestureSequenceTracker();
            _gesture.GestureMinimumDistanceX = 16.0;
            _gesture.GestureMinimumDistanceY = 16.0;
            _gesture.GestureProgressed += (s, e) => GestureProgressed.Invoke(this, new MouseGestureEventArgs(_gesture.Sequence));
        }

        #endregion

        #region Events

        /// <summary>
        /// ジェスチャー進捗通知
        /// </summary>
        public event EventHandler<MouseGestureEventArgs> GestureProgressed;

        /// <summary>
        /// ジェスチャー確定通知
        /// </summary>
        public event EventHandler<MouseGestureEventArgs> GestureChanged;

        #endregion
        
        #region Properties

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


        #endregion

        #region Methods

        //
        public void Reset()
        {
            if (_touch == null) return;
            _gesture.Reset(_touch.StartPoint);
        }
        

        /// <summary>
        /// 状態開始
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="parameter"></param>
        public override void OnOpened(FrameworkElement sender, object parameter)
        {
            ////Debug.WriteLine("TouchState: Gesture");

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
        public override void OnStylusDown(object sender, StylusDownEventArgs e)
        {
            if (e.Handled) return;

            // マルチタッチはドラッグへ
            SetState(TouchInputState.Drag, _touch);
        }

        /// <summary>
        /// ボタン離されたときの処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public override void OnStylusUp(object sender, StylusEventArgs e)
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
        public override void OnStylusMove(object sender, StylusEventArgs e)
        {
            if (e.Handled) return;
            if (e.StylusDevice != _touch?.StylusDevice) return;

            var point = e.GetPosition(_context.Sender);

            _gesture.Move(point);
        }

        #endregion

        #region Memento
        [DataContract]
        public class Memento
        {

            [DataMember, DefaultValue(16.0)]
            [PropertyMember("タッチドラッグ判定の最小移動距離(X)", Tips = "この距離を移動して初めてドラッグ開始と判定されます")]
            public double GestureMinimumDistanceX { get; set; }

            [DataMember, DefaultValue(16.0)]
            [PropertyMember("タッチドラッグ判定の最小移動距離(Y)", Tips = "この距離を移動して初めてドラッグ開始と判定されます")]
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
