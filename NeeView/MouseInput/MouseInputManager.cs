// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
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
    /// 状態コンテキスト
    /// </summary>
    public class MouseInputContext
    {
        /// <summary>
        /// 所属ウィンドウ
        /// </summary>
        public Window Window { get; set; }

        /// <summary>
        /// イベント受取エレメント
        /// </summary>
        public FrameworkElement Sender { get; set; }

        /// <summary>
        /// 操作対象エレメント
        /// アニメーション対応
        /// </summary>
        public FrameworkElement TargetView { get; set; }

        /// <summary>
        /// 操作対象エレメント計算用
        /// アニメーション非対応。非表示の矩形のみ。
        /// 表示領域計算にはこちらを利用する
        /// </summary>
        public FrameworkElement TargetShadow { get; set; }

        /// <summary>
        /// ドラッグ開始座標
        /// </summary>
        public Point StartPoint { get; set; }
    }

    /// <summary>
    /// MouseInputManager
    /// </summary>
    public class MouseInputManager : INotifyPropertyChanged
    {
        /// <summary>
        /// システムオブジェクト
        /// </summary>
        public static MouseInputManager Current { get; set; }

        /// <summary>
        /// PropertyChanged event. 
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }


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
        /// ジェスチャー入力イベント
        /// </summary>
        public event EventHandler<MouseGestureEventArgs> MouseGestureChanged;

        /// <summary>
        /// 表示コンテンツのトランスフォーム変更イベント
        /// </summary>
        public event EventHandler<TransformEventArgs> TransformChanged;

        /// <summary>
        /// イベントクリア
        /// </summary>
        public void ClearMouseEventHandler()
        {
            MouseButtonChanged = null;
            MouseWheelChanged = null;
            MouseGestureChanged = null;
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
        public MouseInputManager(Window window, FrameworkElement sender, FrameworkElement targetView, FrameworkElement targetShadow)
        {
            _context = new MouseInputContext() { Sender = sender };
            _context.Window = window;
            _context.TargetView = targetView;
            _context.TargetShadow = targetShadow;

            _sender = sender;

            this.Normal = new MouseInputNormal(_context);
            this.Normal.StateChanged += StateChanged;
            this.Normal.MouseButtonChanged += (s, e) => MouseButtonChanged?.Invoke(sender, e);
            this.Normal.MouseWheelChanged += (s, e) => MouseWheelChanged?.Invoke(sender, e);

            this.Loupe = new MouseInputLoupe(_context);
            this.Loupe.StateChanged += StateChanged;
            this.Loupe.MouseButtonChanged += (s, e) => MouseButtonChanged?.Invoke(sender, e);
            this.Loupe.MouseWheelChanged += (s, e) => MouseWheelChanged?.Invoke(sender, e);
            this.Loupe.TransformChanged += OnTransformChanged;

            this.Drag = new MouseInputDrag(_context);
            this.Drag.StateChanged += StateChanged;
            this.Drag.MouseButtonChanged += (s, e) => MouseButtonChanged?.Invoke(sender, e);
            this.Drag.MouseWheelChanged += (s, e) => MouseWheelChanged?.Invoke(sender, e);
            this.Drag.TransformChanged += OnTransformChanged;

            this.Gesture = new MouseInputGesture(_context);
            this.Gesture.StateChanged += StateChanged;
            this.Gesture.MouseButtonChanged += (s, e) => MouseButtonChanged?.Invoke(sender, e);
            this.Gesture.MouseWheelChanged += (s, e) => MouseWheelChanged?.Invoke(sender, e);
            this.Gesture.MouseGestureChanged += (s, e) => MouseGestureChanged?.Invoke(sender, e);

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
            _current.OnMouseWheel(_sender, e);
        }

        /// <summary>
        /// OnMouseMove
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (sender != _sender) return;
            _current.OnMouseMove(_sender, e);
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
    }

}
