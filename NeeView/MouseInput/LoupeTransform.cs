// Copyright (c) 2016-2018 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

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
        public static LoupeTransform Current { get; private set; }

        #region Constructors

        /// <summary>
        /// コンストラクター
        /// </summary>
        public LoupeTransform()
        {
            Current = this;

            this.TransformView = CreateTransformGroup();
            this.TransformCalc = CreateTransformGroup();

            FlushFixedLoupeScale();
        }

        #endregion

        #region Events

        /// <summary>
        /// 角度、スケール変更イベント
        /// </summary>
        public event EventHandler<TransformEventArgs> TransformChanged;

        #endregion

        #region Properties

        /// <summary>
        /// 表示コンテンツ用トランスフォーム
        /// </summary>
        public TransformGroup TransformView { get; private set; }

        /// <summary>
        /// 表示コンテンツ用トランスフォーム（計算用）
        /// </summary>
        public TransformGroup TransformCalc { get; private set; }

        /// <summary>
        /// 標準スケール
        /// </summary>
        public double DefaultScale { get; set; } = 2.0;
        
        /// <summary>
        /// IsVisibleLoupeInfo property.
        /// </summary>
        private bool _IsVisibleLoupeInfo = true;
        [PropertyMember("ルーペ倍率を表示する", Tips = "ルーペ時に右上に倍率が表示されます。倍率はマウスホイール操作で変更できます")]
        public bool IsVisibleLoupeInfo
        {
            get { return _IsVisibleLoupeInfo; }
            set { if (_IsVisibleLoupeInfo != value) { _IsVisibleLoupeInfo = value; RaisePropertyChanged(); } }
        }
        
        /// <summary>
        /// IsEnabled property.
        /// </summary>
        private bool _isEnabled;
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
        private Point _position;
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
        private double _scale = double.NaN;
        public double Scale
        {
            get
            {
                if (double.IsNaN(_scale))
                {
                    _scale = this.DefaultScale;
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


        /// <summary>
        /// FixedLoupeScale property.
        /// </summary>
        private double _fixedScale;
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

        #endregion

        #region Methods

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

        #endregion

        #region Memento
        [DataContract]
        public class Memento
        {
            [DataMember]
            public bool IsVisibleLoupeInfo { get; set; }
        }

        //
        public Memento CreateMemento()
        {
            var memento = new Memento();
            memento.IsVisibleLoupeInfo = this.IsVisibleLoupeInfo;
            return memento;
        }

        //
        public void Restore(Memento memento)
        {
            if (memento == null) return;
            this.IsVisibleLoupeInfo = memento.IsVisibleLoupeInfo;
        }
        #endregion
    }
}
