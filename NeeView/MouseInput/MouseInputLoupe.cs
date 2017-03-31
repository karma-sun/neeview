// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace NeeView
{
    /// <summary>
    /// マウスルーペ
    /// </summary>
    public class MouseInputLoupe : MouseInputBase, INotifyPropertyChanged
    {
        /// <summary>
        /// PropertyChanged event. 
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }


        /// <summary>
        /// 角度、スケール変更イベント
        /// </summary>
        public event EventHandler<TransformEventArgs> TransformChanged;


        /// <summary>
        /// 表示コンテンツ用トランスフォーム
        /// </summary>
        public TransformGroup TransformView { get; private set; }

        /// <summary>
        /// 表示コンテンツ用トランスフォーム（計算用）
        /// </summary>
        public TransformGroup TransformCalc { get; private set; }

        /// <summary>
        /// カーソル位置を画面中心にしてルーペ開始するフラグ
        /// TODO: 設定方法
        /// </summary>
        public bool IsCenterMode { get; set; }

        /// <summary>
        /// IsEnabled property.
        /// 表示通知用にプロパティ化
        /// TODO: 直接そうさしていないので、しっくりこない
        /// </summary>
        private bool _isEnabled;
        public bool IsEnabled
        {
            get { return _isEnabled; }
            private set
            {
                if (_isEnabled != value)
                {
                    _isEnabled = value;
                    FlushFixedLoupeScale();
                    RaisePropertyChanged(null);
                }
            }
        }

        #region Property: LoupePosition
        /// <summary>
        /// ルーペ座標
        /// </summary>
        private Point _loupePosition;
        public Point LoupePosition
        {
            get { return _loupePosition; }
            set
            {
                _loupePosition = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(LoupePositionX));
                RaisePropertyChanged(nameof(LoupePositionY));
            }
        }
        public double LoupePositionX => _isEnabled ? LoupePosition.X : 0.0;
        public double LoupePositionY => _isEnabled ? LoupePosition.Y : 0.0;

        /// <summary>
        /// ルーペ開始座標
        /// </summary>
        private Point _loupeBasePosition;
        #endregion


        #region Property: LoupeScale
        /// <summary>
        /// ルーペ倍率
        /// TODO: 初期倍率設定
        /// </summary>
        private double _loupeScale = 2.0;
        public double LoupeScale
        {
            get { return _loupeScale; }
            set
            {
                _loupeScale = value;
                RaisePropertyChanged();
                FlushFixedLoupeScale();
            }
        }

        /// <summary>
        /// update FixedLoupeScale
        /// </summary>
        private void FlushFixedLoupeScale()
        {
            FixedLoupeScale = _isEnabled ? LoupeScale : 1.0;
        }

        /// <summary>
        /// FixedLoupeScale property.
        /// </summary>
        private double _FixedLoupeScale;
        public double FixedLoupeScale
        {
            get { return _FixedLoupeScale; }
            set
            {
                if (_FixedLoupeScale != value)
                {
                    _FixedLoupeScale = value;
                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(LoupeScaleX));
                    RaisePropertyChanged(nameof(LoupeScaleY));

                    var args = new TransformEventArgs(TransformChangeType.LoupeScale, TransformActionType.LoupeScale)
                    {
                        LoupeScale = FixedLoupeScale
                    };
                    TransformChanged?.Invoke(this, args);
                }
            }
        }


        public double LoupeScaleX => FixedLoupeScale;
        public double LoupeScaleY => FixedLoupeScale;
        #endregion


        /// <summary>
        /// コンストラクター
        /// </summary>
        /// <param name="context"></param>
        public MouseInputLoupe(MouseInputContext context) : base(context)
        {
            this.TransformView = CreateTransformGroup();
            this.TransformCalc = CreateTransformGroup();

            FlushFixedLoupeScale();
        }

        /// <summary>
        /// パラメータとトランスフォームを関連付ける
        /// </summary>
        /// <returns></returns>
        private TransformGroup CreateTransformGroup()
        {
            var loupeTransraleTransform = new TranslateTransform();
            BindingOperations.SetBinding(loupeTransraleTransform, TranslateTransform.XProperty, new Binding(nameof(LoupePositionX)) { Source = this });
            BindingOperations.SetBinding(loupeTransraleTransform, TranslateTransform.YProperty, new Binding(nameof(LoupePositionY)) { Source = this });

            var loupeScaleTransform = new ScaleTransform();
            BindingOperations.SetBinding(loupeScaleTransform, ScaleTransform.ScaleXProperty, new Binding(nameof(LoupeScaleX)) { Source = this });
            BindingOperations.SetBinding(loupeScaleTransform, ScaleTransform.ScaleYProperty, new Binding(nameof(LoupeScaleY)) { Source = this });

            var transformGroup = new TransformGroup();
            transformGroup.Children.Add(loupeTransraleTransform);
            transformGroup.Children.Add(loupeScaleTransform);

            return transformGroup;
        }

        /// <summary>
        /// 長押しモード？
        /// 長押しモードの場合、全てのマウス操作がルーペ専属になる
        /// </summary>
        private bool _isLongDownMode;

        /// <summary>
        /// ボタン押されている？
        /// </summary>
        private bool _isButtonDown;

        /// <summary>
        /// 状態開始処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="parameter">trueならば長押しモード</param>
        public override void OnOpened(FrameworkElement sender, object parameter)
        {
            if (parameter is bool isLongDownMode)
            {
                _isLongDownMode = isLongDownMode;
            }
            else
            {
                _isLongDownMode = false;
            }

            sender.Focus();
            sender.CaptureMouse();
            sender.Cursor = Cursors.None;

            _context.StartPoint = Mouse.GetPosition(sender);
            var center = new Point(sender.ActualWidth * 0.5, sender.ActualHeight * 0.5);
            Vector v = _context.StartPoint - center;
            _loupeBasePosition = (Point)(IsCenterMode ? -v : -v + v / LoupeScale);
            LoupePosition = _loupeBasePosition;

            this.IsEnabled = true;
            _isButtonDown = false;
        }

        /// <summary>
        /// 状態終了処理
        /// </summary>
        /// <param name="sender"></param>
        public override void OnClosed(FrameworkElement sender)
        {
            sender.Cursor = null;
            sender.ReleaseMouseCapture();

            this.IsEnabled = false;
        }

        /// <summary>
        /// マウスボタンが押されたときの処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public override void OnMouseButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isButtonDown = true;

            if (_isLongDownMode)
            {
            }
            else
            {
                // ダブルクリック？
                if (e.ClickCount >= 2)
                {
                    // コマンド決定
                    MouseButtonChanged?.Invoke(sender, e);
                    if (e.Handled)
                    {
                        // その後の操作は全て無効
                        _isButtonDown = false;
                    }
                }
            }
        }

        /// <summary>
        /// マウスボタンが離されたときの処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public override void OnMouseButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_isLongDownMode)
            {
                if (e.ChangedButton == MouseButton.Left)
                {
                    // ルーペ解除
                    ResetState();
                }
            }
            else
            {
                if (!_isButtonDown) return;

                // コマンド決定
                // 離されたボタンがメインキー、それ以外は装飾キー
                MouseButtonChanged?.Invoke(sender, e);

                // その後の入力は全て無効
                _isButtonDown = false;
            }
        }

        /// <summary>
        /// マウス移動処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public override void OnMouseMove(object sender, MouseEventArgs e)
        {
            var point = e.GetPosition(_context.Sender);
            LoupePosition = _loupeBasePosition - (point - _context.StartPoint);

            e.Handled = true;
        }

        /// <summary>
        /// マウスホイール処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public override void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            // TODO: 長押しでない時の他のホイール操作のあつかい
            if (e.Delta > 0)
            {
                LoupeZoomIn();
            }
            else
            {
                LoupeZoomOut();
            }

            e.Handled = true;
        }

        /// <summary>
        /// ズームイン
        /// </summary>
        public void LoupeZoomIn()
        {
            var newScale = LoupeScale + 1.0;
            if (newScale > 10.0) newScale = 10.0; // 最大 x10.0
            LoupeScale = newScale;
        }

        /// <summary>
        /// ズームアウト
        /// </summary>
        public void LoupeZoomOut()
        {
            var newScale = LoupeScale - 1.0;
            if (newScale < 2.0) newScale = 2.0;
            LoupeScale = newScale;
        }

        /// <summary>
        /// キー入力処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public override void OnKeyDown(object sender, KeyEventArgs e)
        {
            // ESC で 状態解除
            if (e.Key == Key.Escape)
            {
                // ルーペ解除
                ResetState();

                e.Handled = true;
            }
        }
    }
}
