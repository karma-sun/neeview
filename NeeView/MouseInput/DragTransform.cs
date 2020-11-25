using NeeLaboratory.ComponentModel;
using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace NeeView
{
    /// <summary>
    /// ドラッグ操作による変換
    /// </summary>
    public class DragTransform : BindableBase
    {
        // コンテンツの平行移動行列。アニメーション用。
        private TranslateTransform _translateTransform;

        // 移動アニメーション中フラグ(内部管理)
        private bool _isTranslateAnimated;

        private Point _position;
        private double _angle;
        private double _scale = 1.0;
        private bool _isFlipHorizontal;
        private bool _isFlipVertical;


        public DragTransform()
        {
            this.TransformView = CreateTransformGroup();
            this.TransformCalc = CreateTransformGroup();

            _translateTransform = this.TransformView.Children.OfType<TranslateTransform>().First();
        }


        /// <summary>
        /// 表示コンテンツのトランスフォーム変更イベント
        /// </summary>
        public event EventHandler<TransformEventArgs> TransformChanged;


        public TransformGroup TransformView { get; private set; }
        public TransformGroup TransformCalc { get; private set; }

        // コンテンツの座標
        public Point Position
        {
            get { return _position; }
            private set
            {
                SetProperty(ref _position, value);
            }
        }

        // コンテンツの角度
        public double Angle
        {
            get { return _angle; }
            set
            {
                SetProperty(ref _angle, value);
            }
        }

        // コンテンツの拡大率
        public double Scale
        {
            get { return _scale; }
            set
            {
                if (SetProperty(ref _scale, value))
                {
                    RaisePropertyChanged(nameof(ScaleX));
                    RaisePropertyChanged(nameof(ScaleY));
                }
            }
        }

        // コンテンツのScaleX
        public double ScaleX
        {
            get { return _isFlipHorizontal ? -_scale : _scale; }
        }

        // コンテンツのScaleY
        public double ScaleY
        {
            get { return _isFlipVertical ? -_scale : _scale; }
        }

        // 左右反転
        public bool IsFlipHorizontal
        {
            get { return _isFlipHorizontal; }
            set
            {
                if (SetProperty(ref _isFlipHorizontal, value))
                {
                    RaisePropertyChanged(nameof(ScaleX));
                }
            }
        }

        // 上下反転
        public bool IsFlipVertical
        {
            get { return _isFlipVertical; }
            set
            {
                if (SetProperty(ref _isFlipVertical, value))
                {
                    RaisePropertyChanged(nameof(ScaleY));
                }
            }
        }


        // パラメータとトランスフォームを対応させる
        private TransformGroup CreateTransformGroup()
        {
            var scaleTransform = new ScaleTransform();
            BindingOperations.SetBinding(scaleTransform, ScaleTransform.ScaleXProperty, new Binding(nameof(ScaleX)) { Source = this });
            BindingOperations.SetBinding(scaleTransform, ScaleTransform.ScaleYProperty, new Binding(nameof(ScaleY)) { Source = this });

            var rotateTransform = new RotateTransform();
            BindingOperations.SetBinding(rotateTransform, RotateTransform.AngleProperty, new Binding(nameof(Angle)) { Source = this });

            var translateTransform = new TranslateTransform();
            BindingOperations.SetBinding(translateTransform, TranslateTransform.XProperty, new Binding("Position.X") { Source = this });
            BindingOperations.SetBinding(translateTransform, TranslateTransform.YProperty, new Binding("Position.Y") { Source = this });

            var transformGroup = new TransformGroup();
            transformGroup.Children.Add(scaleTransform);
            transformGroup.Children.Add(rotateTransform);
            transformGroup.Children.Add(translateTransform);

            return transformGroup;
        }

        public void SetPosition(Point value)
        {
            SetPosition(value, default);
        }

        public void SetPosition(Point value, TimeSpan span)
        {
            if (span.TotalMilliseconds > 0)
            {
                Duration duration = span;

                if (!_isTranslateAnimated)
                {
                    // 開始
                    _isTranslateAnimated = true;
                    _translateTransform.BeginAnimation(TranslateTransform.XProperty,
                        DecorateDoubleAnimation(new DoubleAnimation(_position.X, value.X, duration)), HandoffBehavior.SnapshotAndReplace);
                    _translateTransform.BeginAnimation(TranslateTransform.YProperty,
                        DecorateDoubleAnimation(new DoubleAnimation(_position.Y, value.Y, duration)), HandoffBehavior.SnapshotAndReplace);
                }
                else
                {
                    // 継続
                    _translateTransform.BeginAnimation(TranslateTransform.XProperty,
                        DecorateDoubleAnimation(new DoubleAnimation(value.X, duration)), HandoffBehavior.Compose);
                    _translateTransform.BeginAnimation(TranslateTransform.YProperty,
                        DecorateDoubleAnimation(new DoubleAnimation(value.Y, duration)), HandoffBehavior.Compose);
                }
            }
            else
            {
                if (_isTranslateAnimated)
                {
                    // 解除
                    _translateTransform.ApplyAnimationClock(TranslateTransform.XProperty, null);
                    _translateTransform.ApplyAnimationClock(TranslateTransform.YProperty, null);
                    _isTranslateAnimated = false;
                }
            }

            Position = value;

            DoubleAnimation DecorateDoubleAnimation(DoubleAnimation source)
            {
                source.AccelerationRatio = 0.4;
                source.DecelerationRatio = 0.4;
                return source;
            }
        }

        public void SetAngle(double angle, TransformActionType actionType)
        {
            this.Angle = angle;
            TransformChanged?.Invoke(this, new TransformEventArgs(actionType));
        }

        public void SetScale(double scale, TransformActionType actionType)
        {
            this.Scale = scale;
            TransformChanged?.Invoke(this, new TransformEventArgs(actionType));
        }

        #region Memento

        [DataContract]
        public class Memento : IMemento
        {
            [DataMember]
            public bool IsLimitMove { get; set; }
            [DataMember]
            public double AngleFrequency { get; set; }

            public void RestoreConfig(Config config)
            {
                config.View.IsLimitMove = IsLimitMove;
                config.View.AngleFrequency = AngleFrequency;
            }
        }

        #endregion

    }
}
