using NeeLaboratory;
using NeeLaboratory.ComponentModel;
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
    /// マウスルーペ
    /// </summary>
    public class MouseInputLoupe : MouseInputBase
    {
        #region Fields

        private LoupeTransform _loupe;
        private Point _loupeBasePosition;
        private bool _isLongDownMode;
        private bool _isButtonDown;

        #endregion

        #region Constructors

        public MouseInputLoupe(MouseInputContext context) : base(context)
        {
            _loupe = context.LoupeTransform;
        }

        #endregion

        #region Methods

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
            sender.Cursor = Cursors.None;

            _context.StartPoint = Mouse.GetPosition(sender);
            var center = new Point(sender.ActualWidth * 0.5, sender.ActualHeight * 0.5);
            Vector v = _context.StartPoint - center;
            _loupeBasePosition = (Point)(Config.Current.Loupe.IsLoupeCenter ? -v : -v + v / _loupe.Scale);
            _loupe.Position = _loupeBasePosition;

            _loupe.IsEnabled = true;
            _isButtonDown = false;

            if (Config.Current.Loupe.IsResetByRestart)
            {
                _loupe.Scale = Config.Current.Loupe.DefaultScale;
            }
        }

        /// <summary>
        /// 状態終了処理
        /// </summary>
        /// <param name="sender"></param>
        public override void OnClosed(FrameworkElement sender)
        {
            sender.Cursor = null;

            _loupe.IsEnabled = false;
        }


        public override void OnCaptureOpened(FrameworkElement sender)
        {
            MouseInputHelper.CaptureMouse(this, sender);
        }

        public override void OnCaptureClosed(FrameworkElement sender)
        {
            MouseInputHelper.ReleaseMouseCapture(this, sender);
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
                if (MouseButtonBitsExtensions.Create(e) == MouseButtonBits.None)
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
            _loupe.Position = _loupeBasePosition - (point - _context.StartPoint) * Config.Current.Loupe.Speed;

            e.Handled = true;
        }

        /// <summary>
        /// マウスホイール処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public override void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Config.Current.Loupe.IsWheelScalingEnabled)
            {
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
            else
            {
                // コマンド決定
                // ホイールがメインキー、それ以外は装飾キー
                MouseWheelChanged?.Invoke(sender, e);

                // その後の操作は全て無効
                _isButtonDown = false;
            }
        }

        /// <summary>
        /// ズームイン
        /// </summary>
        public void LoupeZoomIn()
        {
            _loupe.Scale = Math.Min(_loupe.Scale + Config.Current.Loupe.ScaleStep, Config.Current.Loupe.MaximumScale);
        }

        /// <summary>
        /// ズームアウト
        /// </summary>
        public void LoupeZoomOut()
        {
            _loupe.Scale = Math.Max(_loupe.Scale - Config.Current.Loupe.ScaleStep, Config.Current.Loupe.MinimumScale);
        }

        /// <summary>
        /// キー入力処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public override void OnKeyDown(object sender, KeyEventArgs e)
        {
            // ESC で 状態解除
            if (Config.Current.Loupe.IsEscapeKeyEnabled && e.Key == Key.Escape)
            {
                // ルーペ解除
                ResetState();

                e.Handled = true;
            }
        }

        #endregion

        #region Memento
        [DataContract]
        public class Memento : IMemento
        {
            [DataMember]
            public int _Version { get; set; } = Environment.ProductVersionNumber;

            [DataMember]
            public bool IsLoupeCenter { get; set; }

            [DataMember, DefaultValue(2.0)]
            public double DefaultScale { get; set; }

            [DataMember, DefaultValue(2.0)]
            public double MinimumScale { get; set; }

            [DataMember, DefaultValue(10.0)]
            public double MaximumScale { get; set; }

            [DataMember, DefaultValue(1.0)]
            public double ScaleStep { get; set; }

            [DataMember, DefaultValue(false)]
            public bool IsResetByRestart { get; set; }

            [DataMember, DefaultValue(true)]
            public bool IsResetByPageChanged { get; set; }

            [DataMember, DefaultValue(true)]
            public bool IsWheelScalingEnabled { get; set; }

            [DataMember, DefaultValue(1.0)]
            public double Speed { get; set; }

            [DataMember, DefaultValue(true)]
            public bool IsEscapeKeyEnabled { get; set; }


            [OnDeserializing]
            private void OnDeserializing(StreamingContext c)
            {
                this.InitializePropertyDefaultValues();
            }

            public void RestoreConfig(Config config)
            {
                config.Loupe.IsLoupeCenter = IsLoupeCenter;
                config.Loupe.MinimumScale = MinimumScale;
                config.Loupe.MaximumScale = MaximumScale;
                config.Loupe.DefaultScale = DefaultScale;
                config.Loupe.ScaleStep = ScaleStep;
                config.Loupe.IsResetByRestart = IsResetByRestart;
                config.Loupe.IsResetByPageChanged = IsResetByPageChanged;
                config.Loupe.IsWheelScalingEnabled = IsWheelScalingEnabled;
                config.Loupe.Speed = Speed;
                config.Loupe.IsEscapeKeyEnabled = IsEscapeKeyEnabled;
            }
        }

        #endregion
    }
}
