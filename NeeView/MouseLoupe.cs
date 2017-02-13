// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace NeeView
{
    public class MouseLoupe : INotifyPropertyChanged
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
        public event EventHandler<TransformChangedParam> TransformChanged;

        /// <summary>
        /// 状態変化イベント
        /// </summary>
        public event EventHandler IsEnabledChanged;

        // マウス入力イベント受付コントロール。ビューエリア。
        private FrameworkElement _sender;

        //
        public TransformGroup TransformView { get; private set; }
        public TransformGroup TransformCalc { get; private set; }

        /// <summary>
        /// IsEnabled property.
        /// </summary>
        private bool _IsEnabled;
        public bool IsEnabled
        {
            get { return _IsEnabled; }
            set
            {
                if (_IsEnabled != value)
                {
                    _IsEnabled = value;

                    if (_IsEnabled)
                        Start();
                    else
                        Stop();

                    RaisePropertyChanged(null);
                    IsEnabledChanged?.Invoke(this, null);
                }
            }
        }


        //
        public MouseLoupe(FrameworkElement sender)
        {
            _sender = sender;

            _sender.PreviewMouseDown += OnMouseButtonDown;
            _sender.PreviewMouseUp += OnMouseButtonUp;
            _sender.PreviewMouseWheel += OnMouseWheel;
            _sender.PreviewMouseMove += OnMouseMove;
            _sender.PreviewKeyDown += OnKeyDown;

            this.TransformView = CreateTransformGroup();
            this.TransformCalc = CreateTransformGroup();
        }

        //
        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (IsEnabled && e.Key == Key.Escape)
            {
                IsEnabled = false;
                e.Handled = true;
            }
        }


        // パラメータとトランスフォームを対応させる
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

        #region Property: LoupePosition
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
        public double LoupePositionX => this.IsEnabled ? LoupePosition.X : 0.0;
        public double LoupePositionY => this.IsEnabled ? LoupePosition.Y : 0.0;

        private Point _loupeBasePosition;
        #endregion


        #region Property: LoupeScale
        private double _loupeScale = 2.0;
        public double LoupeScale
        {
            get { return _loupeScale; }
            set
            {
                _loupeScale = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(LoupeScaleX));
                RaisePropertyChanged(nameof(LoupeScaleY));
                TransformChanged?.Invoke(this, new TransformChangedParam(TransformChangeType.LoupeScale, TransformActionType.LoupeScale));
            }
        }

        public double FixedLoupeScale => this.IsEnabled ? LoupeScale : 1.0;
        public double LoupeScaleX => FixedLoupeScale;
        public double LoupeScaleY => FixedLoupeScale;
        #endregion


        private Point _startPoint;
        private Point _endPoint;

        //
        private void Start()
        {
            _sender.Focus();
            _sender.Cursor = Cursors.None;
            _sender.CaptureMouse();

            _startPoint = Mouse.GetPosition(_sender);
            var center = new Point(_sender.ActualWidth * 0.5, _sender.ActualHeight * 0.5);
            Vector v = _startPoint - center;
            _loupeBasePosition = (Point)(-v + v / LoupeScale);
            LoupePosition = _loupeBasePosition;
        }

        //
        private void Stop()
        {
            _sender.Cursor = null;

            _sender.ReleaseMouseCapture();
        }


        /// <summary>
        /// 左ボタンを押したときの処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMouseButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!IsEnabled) return;

            _sender.CaptureMouse();
            e.Handled = true;
        }

        /// <summary>
        /// 左ボタンを離したときの処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMouseButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!IsEnabled) return;

            _sender.ReleaseMouseCapture();
            e.Handled = true;
        }

        /// <summary>
        /// カーソル移動処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (!IsEnabled) return;

            _endPoint = e.GetPosition(_sender);
            LoupePosition = _loupeBasePosition - (_endPoint - _startPoint);

            e.Handled = true;
        }

        /// <summary>
        /// ホイール処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (!IsEnabled) return;

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

        //
        public void LoupeZoomIn()
        {
            var newScale = LoupeScale + 1.0;
            if (newScale > 10.0) newScale = 10.0; // 最大 x10.0
            LoupeScale = newScale;
        }

        //
        public void LoupeZoomOut()
        {
            var newScale = LoupeScale - 1.0;
            if (newScale < 2.0) newScale = 2.0;
            LoupeScale = newScale;
        }

    }
}
