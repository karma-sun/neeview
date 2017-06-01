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
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace NeeView
{
    /// <summary>
    /// マウスルーペ
    /// </summary>
    public class MouseInputLoupe : MouseInputBase
    {
        /// <summary>
        /// 角度、スケール変更イベント
        /// </summary>
        public event EventHandler<TransformEventArgs> TransformChanged;


        /// <summary>
        /// IsVisibleLoupeInfo property.
        /// </summary>
        private bool _IsVisibleLoupeInfo = true;
        public bool IsVisibleLoupeInfo
        {
            get { return _IsVisibleLoupeInfo; }
            set { if (_IsVisibleLoupeInfo != value) { _IsVisibleLoupeInfo = value; RaisePropertyChanged(); } }
        }

        /// <summary>
        /// IsLoupeCenter property.
        /// </summary>
        private bool _IsLoupeCenter;
        public bool IsLoupeCenter
        {
            get { return _IsLoupeCenter; }
            set { if (_IsLoupeCenter != value) { _IsLoupeCenter = value; RaisePropertyChanged(); } }
        }



        /// <summary>
        /// MinimumScale property.
        /// </summary>
        public double MinimumScale
        {
            get { return _minimumScale; }
            set { if (_minimumScale != value) { _minimumScale = value; RaisePropertyChanged(); } }
        }

        private double _minimumScale = 2.0;

        /// <summary>
        /// MaximumScale property.
        /// </summary>
        public double MaximumScale
        {
            get { return _maximumScale; }
            set { if (_maximumScale != value) { _maximumScale = Math.Max(value, _minimumScale); RaisePropertyChanged(); } }
        }

        private double _maximumScale = 10.0;


        /// <summary>
        /// DefaultScale property.
        /// </summary>
        public double DefaultScale
        {
            get { return _defaultScale; }
            set { if (_defaultScale != value) { _defaultScale = NVUtility.Clamp(value, _minimumScale, MaximumScale); RaisePropertyChanged(); } }
        }

        private double _defaultScale = 2.0;


        /// <summary>
        /// ScaleStep property.
        /// </summary>
        public double ScaleStep
        {
            get { return _scaleStep; }
            set { if (_scaleStep != value) { _scaleStep = value; RaisePropertyChanged(); } }
        }

        private double _scaleStep = 1.0;

        /// <summary>
        /// IsResetByRestart property.
        /// </summary>
        public bool IsResetByRestart
        {
            get { return _isResetByRestart; }
            set { if (_isResetByRestart != value) { _isResetByRestart = value; RaisePropertyChanged(); } }
        }

        private bool _isResetByRestart = false;

        /// <summary>
        /// IsResetByPageChanged property.
        /// </summary>
        public bool IsResetByPageChanged
        {
            get { return _isResetByPageChanged; }
            set { if (_isResetByPageChanged != value) { _isResetByPageChanged = value; RaisePropertyChanged(); } }
        }

        private bool _isResetByPageChanged = true;




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
        //public bool IsCenterMode { get; set; }

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
        /// </summary>
        private double _loupeScale = double.NaN;
        public double LoupeScale
        {
            get
            {
                if (double.IsNaN(_loupeScale))
                {
                    _loupeScale = _defaultScale;
                }
                return _loupeScale;
            }
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
            _loupeBasePosition = (Point)(this.IsLoupeCenter ? -v : -v + v / LoupeScale);
            LoupePosition = _loupeBasePosition;

            this.IsEnabled = true;
            _isButtonDown = false;

            if (_isResetByRestart)
            {
                LoupeScale = _defaultScale;
            }
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
            LoupeScale = Math.Min(LoupeScale + _scaleStep, _maximumScale);
        }

        /// <summary>
        /// ズームアウト
        /// </summary>
        public void LoupeZoomOut()
        {
            LoupeScale = Math.Max(LoupeScale - _scaleStep, _minimumScale);
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


        #region Memento
        [DataContract]
        public class Memento
        {
            [DataMember]
            public bool IsLoupeCenter { get; set; }
            [DataMember]
            public bool IsVisibleLoupeInfo { get; set; }

            [DataMember, DefaultValue(2.0)]
            [PropertyMember("ルーペ標準倍率", Tips = "ルーペの初期倍率です")]
            public double DefaultScale { get; set; }

            [DataMember, DefaultValue(2.0)]
            [PropertyMember("ルーペ最小倍率", Tips = "ルーペの最小倍率です")]
            public double MinimumScale { get; set; }

            [DataMember, DefaultValue(10.0)]
            [PropertyMember("ルーペ最大倍率", Tips = "ルーペの最大倍率です")]
            public double MaximumScale { get; set; }

            [DataMember, DefaultValue(1.0)]
            [PropertyMember("ルーペ倍率変化単位", Tips = "ルーペ倍率をこの値で変化させます")]
            public double ScaleStep { get; set; }

            [DataMember, DefaultValue(false)]
            [PropertyMember("ルーペ倍率リセット", Tips = "ルーペを開始するたびに標準倍率に戻します")]
            public bool IsResetByRestart { get; set; }

            [DataMember, DefaultValue(true)]
            [PropertyMember("ルーペページ切り替え解除", Tips = "ページを切り替えるとルーペを解除します")]
            public bool IsResetByPageChanged { get; set; }
        }

        //
        public Memento CreateMemento()
        {
            var memento = new Memento();
            memento.IsLoupeCenter = this.IsLoupeCenter;
            memento.IsVisibleLoupeInfo = this.IsVisibleLoupeInfo;
            memento.DefaultScale = this.DefaultScale;
            memento.MinimumScale = this.MinimumScale;
            memento.MaximumScale = this.MaximumScale;
            memento.ScaleStep = this.ScaleStep;
            memento.IsResetByRestart = this.IsResetByRestart;
            memento.IsResetByPageChanged = this.IsResetByPageChanged;

            return memento;
        }

        //
        public void Restore(Memento memento)
        {
            if (memento == null) return;
            this.IsLoupeCenter = memento.IsLoupeCenter;
            this.IsVisibleLoupeInfo = memento.IsVisibleLoupeInfo;
            this.MinimumScale = memento.MinimumScale;
            this.MaximumScale = memento.MaximumScale;
            this.DefaultScale = memento.DefaultScale;
            this.ScaleStep = memento.ScaleStep;
            this.IsResetByRestart = memento.IsResetByRestart;
            this.IsResetByPageChanged = memento.IsResetByPageChanged;
        }
        #endregion

    }
}
