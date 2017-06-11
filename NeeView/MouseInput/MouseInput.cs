// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using NeeView.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace NeeView
{
    /// <summary>
    /// マウス入力状態
    /// </summary>
    public enum MouseInputState
    {
        None,
        Normal,
        Loupe,
        Drag,
        Gesture,
    }

    /// <summary>
    /// MouseInputManager
    /// </summary>
    public class MouseInput : BindableBase
    {
        /// <summary>
        /// システムオブジェクト
        /// </summary>
        public static MouseInput Current { get; private set; }

        //
        private FrameworkElement _sender;

        /// <summary>
        /// 状態：既定
        /// </summary>
        public MouseInputNormal Normal { get; private set; }

        /// <summary>
        /// 状態：ルーペ
        /// </summary>
        public MouseInputLoupe Loupe { get; private set; }

        /// <summary>
        /// 状態：ドラッグ
        /// </summary>
        public MouseInputDrag Drag { get; private set; }

        /// <summary>
        /// 状態：ジェスチャー
        /// </summary>
        public MouseInputGesture Gesture { get; private set; }

        /// <summary>
        /// 遷移テーブル
        /// </summary>
        private Dictionary<MouseInputState, MouseInputBase> _mouseInputCollection;

        /// <summary>
        /// 現在状態
        /// </summary>
        private MouseInputState _state;
        public MouseInputState State => _state;

        /// <summary>
        /// 現在状態（実体）
        /// </summary>
        private MouseInputBase _current;

        /// <summary>
        /// ボタン入力イベント
        /// </summary>
        public event EventHandler<MouseButtonEventArgs> MouseButtonChanged;

        /// <summary>
        /// ホイール入力イベント
        /// </summary>
        public event EventHandler<MouseWheelEventArgs> MouseWheelChanged;

        /// <summary>
        /// 表示コンテンツのトランスフォーム変更イベント
        /// </summary>
        public event EventHandler<TransformEventArgs> TransformChanged;

        /// <summary>
        /// 一定距離カーソルが移動したイベント
        /// </summary>
        public event EventHandler MouseMoved;


        /// <summary>
        /// コマンド系イベントクリア
        /// </summary>
        public void ClearMouseEventHandler()
        {
            MouseButtonChanged = null;
            MouseWheelChanged = null;
        }

        /// <summary>
        /// 状態コンテキスト
        /// </summary>
        private MouseInputContext _context;


        /// <summary>
        /// コンストラクター
        /// </summary>
        /// <param name="window"></param>
        /// <param name="sender"></param>
        /// <param name="targetView"></param>
        /// <param name="targetShadow"></param>
        public MouseInput(MouseInputContext context)
        {
            Current = this;

            _context = context;
            _sender = context.Sender;

            this.Normal = new MouseInputNormal(_context);
            this.Normal.StateChanged += StateChanged;
            this.Normal.MouseButtonChanged += (s, e) => MouseButtonChanged?.Invoke(_sender, e);
            this.Normal.MouseWheelChanged += (s, e) => MouseWheelChanged?.Invoke(_sender, e);

            this.Loupe = new MouseInputLoupe(_context);
            this.Loupe.StateChanged += StateChanged;
            this.Loupe.MouseButtonChanged += (s, e) => MouseButtonChanged?.Invoke(_sender, e);
            this.Loupe.MouseWheelChanged += (s, e) => MouseWheelChanged?.Invoke(_sender, e);
            this.Loupe.TransformChanged += OnTransformChanged;

            this.Drag = new MouseInputDrag(_context);
            this.Drag.StateChanged += StateChanged;
            this.Drag.MouseButtonChanged += (s, e) => MouseButtonChanged?.Invoke(_sender, e);
            this.Drag.MouseWheelChanged += (s, e) => MouseWheelChanged?.Invoke(_sender, e);
            this.Drag.TransformChanged += OnTransformChanged;

            this.Gesture = new MouseInputGesture(_context);
            this.Gesture.StateChanged += StateChanged;
            this.Gesture.MouseButtonChanged += (s, e) => MouseButtonChanged?.Invoke(_sender, e);
            this.Gesture.MouseWheelChanged += (s, e) => MouseWheelChanged?.Invoke(_sender, e);
            this.Gesture.GestureChanged += (s, e) => _context.GestureCommandCollection.Execute(e.Sequence);
            this.Gesture.GestureProgressed += (s, e) => _context.GestureCommandCollection.ShowProgressed(e.Sequence);

            // initialize state
            _mouseInputCollection = new Dictionary<MouseInputState, MouseInputBase>();
            _mouseInputCollection.Add(MouseInputState.Normal, this.Normal);
            _mouseInputCollection.Add(MouseInputState.Loupe, this.Loupe);
            _mouseInputCollection.Add(MouseInputState.Drag, this.Drag);
            _mouseInputCollection.Add(MouseInputState.Gesture, this.Gesture);
            SetState(MouseInputState.Normal, null);

            // initialize event
            _sender.PreviewMouseDown += OnMouseButtonDown;
            _sender.PreviewMouseUp += OnMouseButtonUp;
            _sender.PreviewMouseWheel += OnMouseWheel;
            _sender.PreviewMouseMove += OnMouseMove;
            _sender.PreviewKeyDown += OnKeyDown;
        }


        /// <summary>
        /// コンテンツのトランフォーム変更通知
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTransformChanged(object sender, TransformEventArgs e)
        {
            var args = new TransformEventArgs(e.ChangeType, e.ActionType);
            args.Scale = Drag.Scale;
            args.Angle = Drag.Angle;
            args.IsFlipHorizontal = Drag.IsFlipHorizontal;
            args.IsFlipVertical = Drag.IsFlipVertical;
            args.LoupeScale = Loupe.FixedLoupeScale;

            TransformChanged?.Invoke(sender, args);
        }

        /// <summary>
        /// 状態変更イベント処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StateChanged(object sender, MouseInputStateEventArgs e)
        {
            SetState(e.State, e.Parameter);
        }

        /// <summary>
        /// 状態変更
        /// </summary>
        /// <param name="state"></param>
        /// <param name="parameter"></param>
        public void SetState(MouseInputState state, object parameter)
        {
            if (state == _state) return;

            _current?.OnClosed(_sender);
            _state = state;
            _current = _mouseInputCollection[_state];
            _current.OnOpened(_sender, parameter);
        }

        /// <summary>
        /// IsLoupeMode property.
        /// </summary>
        public bool IsLoupeMode
        {
            get { return _state == MouseInputState.Loupe; }
            set { SetState(value ? MouseInputState.Loupe : MouseInputState.Normal, false); }
        }

        /// <summary>
        /// OnMouseButtonDown
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMouseButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender != _sender) return;
            _context.StylusDevice = e.StylusDevice;
            _current.OnMouseButtonDown(_sender, e);
        }

        /// <summary>
        /// OnMouseButtonUp
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMouseButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender != _sender) return;
            _context.StylusDevice = e.StylusDevice;
            _current.OnMouseButtonUp(_sender, e);

            // 右クリックでのコンテキストメニュー無効
            // TODO: コンテキストメニュー自体をViewTreeに登録しておく必要はない？
            e.Handled = true;
        }

        /// <summary>
        /// OnMouseWheel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (sender != _sender) return;
            _context.StylusDevice = e.StylusDevice;
            _current.OnMouseWheel(_sender, e);
        }

        // マウス移動検知用
        private Point _lastActionPoint;

        /// <summary>
        /// OnMouseMove
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (sender != _sender) return;
            _context.StylusDevice = e.StylusDevice;
            _current.OnMouseMove(_sender, e);

            // マウス移動を通知
            var nowPoint = e.GetPosition(_sender);
            if (Math.Abs(nowPoint.X - _lastActionPoint.X) > SystemParameters.MinimumHorizontalDragDistance || Math.Abs(nowPoint.Y - _lastActionPoint.Y) > SystemParameters.MinimumVerticalDragDistance)
            {
                MouseMoved?.Invoke(this, null);
                _lastActionPoint = nowPoint;
            }
        }

        /// <summary>
        /// OnKeyDown
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (sender != _sender) return;
            _current.OnKeyDown(_sender, e);
        }


        // メッセージとして状態表示
        public void ShowMessage(TransformActionType ActionType, ViewContent mainContent)
        {
            var infoMessage = InfoMessage.Current;
            if (infoMessage.ViewTransformShowMessageStyle == ShowMessageStyle.None) return;

            switch (ActionType)
            {
                case TransformActionType.Scale:
                    string scaleText = this.Drag.IsOriginalScaleShowMessage && mainContent.IsValid
                        ? $"{(int)(this.Drag.Scale * mainContent.Scale * Config.Current .Dpi.DpiScaleX * 100 + 0.1)}%"
                        : $"{(int)(this.Drag.Scale * 100.0 + 0.1)}%";
                    infoMessage.SetMessage(InfoMessageType.ViewTransform, scaleText);
                    break;
                case TransformActionType.Angle:
                    infoMessage.SetMessage(InfoMessageType.ViewTransform, $"{(int)(this.Drag.Angle)}°");
                    break;
                case TransformActionType.FlipHorizontal:
                    infoMessage.SetMessage(InfoMessageType.ViewTransform, "左右反転 " + (this.Drag.IsFlipHorizontal ? "ON" : "OFF"));
                    break;
                case TransformActionType.FlipVertical:
                    infoMessage.SetMessage(InfoMessageType.ViewTransform, "上下反転 " + (this.Drag.IsFlipVertical ? "ON" : "OFF"));
                    break;
                case TransformActionType.LoupeScale:
                    if (this.Loupe.LoupeScale != 1.0)
                    {
                        infoMessage.SetMessage(InfoMessageType.ViewTransform, $"×{this.Loupe.LoupeScale:0.0}");
                    }
                    break;
            }
        }

#region Memento
        [DataContract]
        public class Memento
        {
            [DataMember]
            public MouseInputNormal.Memento Normal { get; set; }
            [DataMember]
            public MouseInputLoupe.Memento Loupe { get; set; }
            [DataMember]
            public MouseInputDrag.Memento Drag { get; set; }
            [DataMember]
            public MouseInputGesture.Memento Gesture { get; set; }
        }

        //
        public Memento CreateMemento()
        {
            var memento = new Memento();
            memento.Normal = this.Normal.CreateMemento();
            memento.Loupe = this.Loupe.CreateMemento();
            memento.Drag = this.Drag.CreateMemento();
            memento.Gesture = this.Gesture.CreateMemento();
            return memento;
        }

        //
        public void Restore(Memento memento)
        {
            if (memento == null) return;
            this.Normal.Restore(memento.Normal);
            this.Loupe.Restore(memento.Loupe);
            this.Drag.Restore(memento.Drag);
            this.Gesture.Restore(memento.Gesture);
        }
#endregion

    }

}
