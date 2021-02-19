using NeeLaboratory.ComponentModel;
using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace NeeView
{
    /// <summary>
    /// ドラッグ操作による変換
    /// </summary>
    public class DragTransform : BindableBase
    {
        private TranslateTransformAnime _translateTransformAnime;
        private Point _position;
        private double _angle;
        private double _scale = 1.0;
        private bool _isFlipHorizontal;
        private bool _isFlipVertical;


        public DragTransform()
        {
            this.TransformView = CreateTransformGroup(false);
            this.TransformCalc = CreateTransformGroup(true);

            var translateTransform = this.TransformView.Children.OfType<TranslateTransform>().First();
            _translateTransformAnime = new TranslateTransformAnime(translateTransform);
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
            private set
            {
                SetProperty(ref _angle, value);
            }
        }

        // コンテンツの拡大率
        public double Scale
        {
            get { return _scale; }
            private set
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
        private TransformGroup CreateTransformGroup(bool bindPosition)
        {
            var scaleTransform = new ScaleTransform();
            BindingOperations.SetBinding(scaleTransform, ScaleTransform.ScaleXProperty, new Binding(nameof(ScaleX)) { Source = this });
            BindingOperations.SetBinding(scaleTransform, ScaleTransform.ScaleYProperty, new Binding(nameof(ScaleY)) { Source = this });

            var rotateTransform = new RotateTransform();
            BindingOperations.SetBinding(rotateTransform, RotateTransform.AngleProperty, new Binding(nameof(Angle)) { Source = this });

            var translateTransform = new TranslateTransform();
            if (bindPosition)
            {
                BindingOperations.SetBinding(translateTransform, TranslateTransform.XProperty, new Binding("Position.X") { Source = this });
                BindingOperations.SetBinding(translateTransform, TranslateTransform.YProperty, new Binding("Position.Y") { Source = this });
            }

            var transformGroup = new TransformGroup();
            transformGroup.Children.Add(scaleTransform);
            transformGroup.Children.Add(rotateTransform);
            transformGroup.Children.Add(translateTransform);

            return transformGroup;
        }


        public void SetPosition(Point value)
        {
            SetPosition(value, TranslateTransformEasing.Direct, default);
        }

        public void SetPosition(Point value, TranslateTransformEasing easing)
        {
            SetPosition(value, easing, default);
        }

        public void SetPosition(Point value, TranslateTransformEasing easing, TimeSpan span)
        {
            if (Position == value) return;

            if (easing == TranslateTransformEasing.Animation && span == TimeSpan.Zero)
            {
                easing = TranslateTransformEasing.Direct;
            }

            Position = value;

            _translateTransformAnime.SetPosition(value, easing, span);
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
