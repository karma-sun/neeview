using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace NeeView
{
    /// <summary>
    /// マウスルーペ
    /// </summary>
    public class LoupeTransform : BindableBase
    {
        private bool _isEnabled;
        private Point _position;
        private double _scale = double.NaN;
        private double _fixedScale;


        public LoupeTransform()
        {
            this.TransformView = CreateTransformGroup();
            this.TransformCalc = CreateTransformGroup();

            FlushFixedLoupeScale();
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

        public bool IsEnabled
        {
            get { return _isEnabled; }
            set
            {
                if (_isEnabled != value)
                {
                    _isEnabled = value;
                    FlushFixedLoupeScale();
                    RaisePropertyChanged(null);
                }
            }
        }

        /// <summary>
        /// ルーペ座標
        /// </summary>
        public Point Position
        {
            get { return _position; }
            set
            {
                _position = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(PositionX));
                RaisePropertyChanged(nameof(PositionY));
            }
        }

        public double PositionX => _isEnabled ? Position.X : 0.0;
        public double PositionY => _isEnabled ? Position.Y : 0.0;


        /// <summary>
        /// ルーペ倍率
        /// </summary>
        public double Scale
        {
            get
            {
                if (double.IsNaN(_scale))
                {
                    _scale = Config.Current.Loupe.DefaultScale;
                }
                return _scale;
            }
            set
            {
                _scale = value;
                RaisePropertyChanged();
                FlushFixedLoupeScale();
            }
        }

        public double FixedScale
        {
            get { return _fixedScale; }
            set
            {
                if (_fixedScale != value)
                {
                    _fixedScale = value;
                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(ScaleX));
                    RaisePropertyChanged(nameof(ScaleY));

                    TransformChanged?.Invoke(this, new TransformEventArgs(TransformActionType.LoupeScale));
                }
            }
        }

        public double ScaleX => FixedScale;
        public double ScaleY => FixedScale;



        /// <summary>
        /// パラメータとトランスフォームを関連付ける
        /// </summary>
        /// <returns></returns>
        private TransformGroup CreateTransformGroup()
        {
            var loupeTransraleTransform = new TranslateTransform();
            BindingOperations.SetBinding(loupeTransraleTransform, TranslateTransform.XProperty, new Binding(nameof(PositionX)) { Source = this });
            BindingOperations.SetBinding(loupeTransraleTransform, TranslateTransform.YProperty, new Binding(nameof(PositionY)) { Source = this });

            var loupeScaleTransform = new ScaleTransform();
            BindingOperations.SetBinding(loupeScaleTransform, ScaleTransform.ScaleXProperty, new Binding(nameof(ScaleX)) { Source = this });
            BindingOperations.SetBinding(loupeScaleTransform, ScaleTransform.ScaleYProperty, new Binding(nameof(ScaleY)) { Source = this });

            var transformGroup = new TransformGroup();
            transformGroup.Children.Add(loupeTransraleTransform);
            transformGroup.Children.Add(loupeScaleTransform);

            return transformGroup;
        }


        /// <summary>
        /// update FixedLoupeScale
        /// </summary>
        private void FlushFixedLoupeScale()
        {
            FixedScale = _isEnabled ? Scale : 1.0;
        }

        public void ZoomIn()
        {
            Scale = Math.Min(Scale + Config.Current.Loupe.ScaleStep, Config.Current.Loupe.MaximumScale);
        }

        public void ZoomOut()
        {
            Scale = Math.Max(Scale - Config.Current.Loupe.ScaleStep, Config.Current.Loupe.MinimumScale);
        }


        #region Memento
        [DataContract]
        public class Memento : IMemento
        {
            [DataMember]
            public bool IsVisibleLoupeInfo { get; set; }

            public void RestoreConfig(Config config)
            {
                config.Loupe.IsVisibleLoupeInfo = IsVisibleLoupeInfo;

            }
        }

        #endregion
    }
}
