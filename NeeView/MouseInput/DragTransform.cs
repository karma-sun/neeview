// Copyright (c) 2016-2018 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using NeeView.ComponentModel;
using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Animation;

// TODO: 整備
// TODO: 関数が大きすぎる？細分化を検討

namespace NeeView
{
    /// <summary>
    /// ドラッグ操作による変換
    /// </summary>
    public class DragTransform : BindableBase
    {
        public static DragTransform Current { get; private set; }

        #region Fields

        // コンテンツの平行移動行列。アニメーション用。
        private TranslateTransform _translateTransform;

        #endregion

        #region Constructors

        //
        public DragTransform()
        {
            Current = this;

            this.TransformView = CreateTransformGroup();
            this.TransformCalc = CreateTransformGroup();

            _translateTransform = this.TransformView.Children.OfType<TranslateTransform>().First();
        }

        #endregion

        #region Events

        /// <summary>
        /// 表示コンテンツのトランスフォーム変更イベント
        /// </summary>
        public event EventHandler<TransformEventArgs> TransformChanged;

        #endregion

        #region Properties

        public TransformGroup TransformView { get; private set; }
        public TransformGroup TransformCalc { get; private set; }


        // ウィンドウ枠内の移動に制限するフラグ
        private bool _isLimitMove = true;
        public bool IsLimitMove
        {
            get { return _isLimitMove; }
            set { if (_isLimitMove != value) { _isLimitMove = value; RaisePropertyChanged(); } }
        }


        // 回転スナップ。0で無効
        public double AngleFrequency { get; set; } = 0;


        // 移動アニメーション有効フラグ(内部管理)
        private bool _isEnableTranslateAnimation;

        // 移動アニメーション中フラグ(内部管理)
        private bool _isTranslateAnimated;

        //
        public bool IsEnableTranslateAnimation
        {
            get { return _isEnableTranslateAnimation; }
            set { _isEnableTranslateAnimation = value; }
        }


        // コンテンツの座標 (アニメーション対応)
        private Point _position;
        public Point Position
        {
            get { return _position; }
            set
            {
                ////Debug.WriteLine($"Pos: {value}");

                if (_isEnableTranslateAnimation)
                {
                    Duration duration = TimeSpan.FromMilliseconds(100); // 100msアニメ

                    if (!_isTranslateAnimated)
                    {
                        // 開始
                        _isTranslateAnimated = true;
                        _translateTransform.BeginAnimation(TranslateTransform.XProperty,
                            new DoubleAnimation(_position.X, value.X, duration), HandoffBehavior.SnapshotAndReplace);
                        _translateTransform.BeginAnimation(TranslateTransform.YProperty,
                            new DoubleAnimation(_position.Y, value.Y, duration), HandoffBehavior.SnapshotAndReplace);
                    }
                    else
                    {
                        // 継続
                        _translateTransform.BeginAnimation(TranslateTransform.XProperty,
                            new DoubleAnimation(value.X, duration), HandoffBehavior.Compose);
                        _translateTransform.BeginAnimation(TranslateTransform.YProperty,
                            new DoubleAnimation(value.Y, duration), HandoffBehavior.Compose);
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

                _position = value;
                RaisePropertyChanged();
            }
        }

        // コンテンツの角度
        private double _angle;
        public double Angle
        {
            get { return _angle; }
            set
            {
                _angle = value;
                RaisePropertyChanged();
            }
        }


        // コンテンツの拡大率
        private double _scale = 1.0;
        public double Scale
        {
            get { return _scale; }
            set
            {
                _scale = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(ScaleX));
                RaisePropertyChanged(nameof(ScaleY));
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
        private bool _isFlipHorizontal;
        public bool IsFlipHorizontal
        {
            get { return _isFlipHorizontal; }
            set
            {
                if (_isFlipHorizontal != value)
                {
                    _isFlipHorizontal = value;
                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(ScaleX));
                }
            }
        }

        // 上下反転
        private bool _isFlipVertical;
        public bool IsFlipVertical
        {
            get { return _isFlipVertical; }
            set
            {
                if (_isFlipVertical != value)
                {
                    _isFlipVertical = value;
                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(ScaleY));
                }
            }
        }

        #endregion

        #region Methods

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

        //
        public void SetAngle(double angle, TransformActionType actionType)
        {
            this.Angle = angle;
            TransformChanged?.Invoke(this, new TransformEventArgs(actionType));
        }

        //
        public void SetScale(double scale, TransformActionType actionType)
        {
            this.Scale = scale;
            TransformChanged?.Invoke(this, new TransformEventArgs(actionType));
        }

        #endregion

        #region Memento

        [DataContract]
        public class Memento
        {
            [DataMember]
            public bool IsLimitMove { get; set; }
            [DataMember]
            public double AngleFrequency { get; set; }
        }


        //
        public Memento CreateMemento()
        {
            var memento = new Memento();

            memento.IsLimitMove = this.IsLimitMove;
            memento.AngleFrequency = this.AngleFrequency;

            return memento;
        }

        //
        public void Restore(Memento memento)
        {
            if (memento == null) return;

            this.IsLimitMove = memento.IsLimitMove;
            this.AngleFrequency = memento.AngleFrequency;
        }

        #endregion

    }
}
