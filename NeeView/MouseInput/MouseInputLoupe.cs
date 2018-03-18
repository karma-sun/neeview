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
        private bool _IsLoupeCenter;
        private double _minimumScale = 2.0;
        private double _maximumScale = 10.0;
        private double _scaleStep = 1.0;
        private bool _isResetByRestart = false;
        private bool _isResetByPageChanged = true;
        private bool _isLongDownMode;
        private bool _isButtonDown;

        #endregion

        #region Constructors

        public MouseInputLoupe(MouseInputContext context) : base(context)
        {
            _loupe = LoupeTransform.Current;
        }

        #endregion

        #region Properties

        [PropertyMember("@ParamLoupeIsLoupeCenter")]
        public bool IsLoupeCenter
        {
            get { return _IsLoupeCenter; }
            set { if (_IsLoupeCenter != value) { _IsLoupeCenter = value; RaisePropertyChanged(); } }
        }

        [PropertyRange("@ParamLoupeMinimumScale", 1, 20, TickFrequency = 1.0, IsEditable = true)]
        public double MinimumScale
        {
            get { return _minimumScale; }
            set
            {
                if (_minimumScale != value)
                {
                    _minimumScale = value;
                    RaisePropertyChanged();
                }
            }
        }

        [PropertyRange("@ParamLoupeMaximumScale", 1, 20, TickFrequency = 1.0, IsEditable = true)]
        public double MaximumScale
        {
            get { return _maximumScale; }
            set
            {
                if (_maximumScale != value)
                {
                    _maximumScale = value;
                    RaisePropertyChanged();
                }
            }
        }

        [PropertyRange("@ParamLoupeDefaultScale", 1, 20, TickFrequency = 1.0, IsEditable = true)]
        public double DefaultScale
        {
            get { return _loupe.DefaultScale; }
            set
            {
                if (_loupe.DefaultScale != value)
                {
                    _loupe.DefaultScale = value;
                    RaisePropertyChanged();
                }
            }
        }

        [PropertyRange("@ParamLoupeScaleStep", 0.1, 5.0, TickFrequency = 0.1, IsEditable = true)]
        public double ScaleStep
        {
            get { return _scaleStep; }
            set { if (_scaleStep != value) { _scaleStep = Math.Max(value, 0.0); RaisePropertyChanged(); } }
        }

        [PropertyMember("@ParamLoupeIsResetByRestart", Tips = "@ParamLoupeIsResetByRestartTips")]
        public bool IsResetByRestart
        {
            get { return _isResetByRestart; }
            set { if (_isResetByRestart != value) { _isResetByRestart = value; RaisePropertyChanged(); } }
        }

        [PropertyMember("@ParamLoupeIsResetByPageChanged")]
        public bool IsResetByPageChanged
        {
            get { return _isResetByPageChanged; }
            set { if (_isResetByPageChanged != value) { _isResetByPageChanged = value; RaisePropertyChanged(); } }
        }

        [PropertyMember("@ParamLoupeIsWheelScalingEnabled", Tips = "@ParamLoupeIsWheelScalingEnabledTips")]
        public bool IsWheelScalingEnabled { get; set; } = true;

        [PropertyRange("@ParamLoupeSpeed", 0.0, 10.0, TickFrequency = 0.1, Format ="×{0:0.0}")]
        public double Speed { get; set; } = 1.0;

        [PropertyMember("@ParamLoupeIsEscapeKeyEnabled")]
        public bool IsEscapeKeyEnabled { get; set; } = true;

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
            sender.CaptureMouse();
            sender.Cursor = Cursors.None;

            _context.StartPoint = Mouse.GetPosition(sender);
            var center = new Point(sender.ActualWidth * 0.5, sender.ActualHeight * 0.5);
            Vector v = _context.StartPoint - center;
            _loupeBasePosition = (Point)(this.IsLoupeCenter ? -v : -v + v / _loupe.Scale);
            _loupe.Position = _loupeBasePosition;

            _loupe.IsEnabled = true;
            _isButtonDown = false;

            if (_isResetByRestart)
            {
                _loupe.Scale = _loupe.DefaultScale;
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

            _loupe.IsEnabled = false;
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
            _loupe.Position = _loupeBasePosition - (point - _context.StartPoint) * Speed;

            e.Handled = true;
        }

        /// <summary>
        /// マウスホイール処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public override void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (IsWheelScalingEnabled)
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
            _loupe.Scale = Math.Min(_loupe.Scale + _scaleStep, _maximumScale);
        }

        /// <summary>
        /// ズームアウト
        /// </summary>
        public void LoupeZoomOut()
        {
            _loupe.Scale = Math.Max(_loupe.Scale - _scaleStep, _minimumScale);
        }

        /// <summary>
        /// キー入力処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public override void OnKeyDown(object sender, KeyEventArgs e)
        {
            // ESC で 状態解除
            if (IsEscapeKeyEnabled && e.Key == Key.Escape)
            {
                // ルーペ解除
                ResetState();

                e.Handled = true;
            }
        }

        #endregion

        #region Memento
        [DataContract]
        public class Memento
        {
            [DataMember]
            public int _Version { get; set; } = Config.Current.ProductVersionNumber;

            [DataMember]
            public bool IsLoupeCenter { get; set; }

            [Obsolete, DataMember(EmitDefaultValue = false)]
            public bool IsVisibleLoupeInfo { get; set; }

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
            private void Deserializing(StreamingContext c)
            {
                this.InitializePropertyDefaultValues();
            }
        }

        //
        public Memento CreateMemento()
        {
            var memento = new Memento();
            memento.IsLoupeCenter = this.IsLoupeCenter;
            memento.DefaultScale = this.DefaultScale;
            memento.MinimumScale = this.MinimumScale;
            memento.MaximumScale = this.MaximumScale;
            memento.ScaleStep = this.ScaleStep;
            memento.IsResetByRestart = this.IsResetByRestart;
            memento.IsResetByPageChanged = this.IsResetByPageChanged;
            memento.IsWheelScalingEnabled = this.IsWheelScalingEnabled;
            memento.Speed = this.Speed;
            memento.IsEscapeKeyEnabled = this.IsEscapeKeyEnabled;

            return memento;
        }

        //
        public void Restore(Memento memento)
        {
            if (memento == null) return;
            this.IsLoupeCenter = memento.IsLoupeCenter;
            this.MinimumScale = memento.MinimumScale;
            this.MaximumScale = memento.MaximumScale;
            this.DefaultScale = memento.DefaultScale;
            this.ScaleStep = memento.ScaleStep;
            this.IsResetByRestart = memento.IsResetByRestart;
            this.IsResetByPageChanged = memento.IsResetByPageChanged;
            this.IsWheelScalingEnabled = memento.IsWheelScalingEnabled;
            this.Speed = memento.Speed;
            this.IsEscapeKeyEnabled = memento.IsEscapeKeyEnabled;

#pragma warning disable CS0612

            // compatible before ver.26
            if (memento._Version < Config.GenerateProductVersionNumber(1, 26, 0))
            {
                _loupe.IsVisibleLoupeInfo = memento.IsVisibleLoupeInfo;
            }

#pragma warning restore CS0612

        }
        #endregion
    }
}
