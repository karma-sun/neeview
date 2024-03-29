﻿using NeeLaboratory.ComponentModel;
using NeeView.Collections.Generic;
using NeeView.Windows.Property;
using PhotoSauce.MagicScaler;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace NeeView
{

    /// <summary>
    /// 画像コントロール
    /// </summary>
    public class NavigateModel : BindableBase
    {
        static NavigateModel() => Current = new NavigateModel();
        public static NavigateModel Current { get; }


        private readonly double[] _scaleSnaps = new double[]
        {
            0.01, 0.1, 0.25, 0.5, 0.75, 1.0, 1.25, 1.5, 2.0, 4.0, 8.0, 16.0, 32.0
        };


        private DragTransform _dragTransform;
        private ContentCanvas _contentCanvas;

        public NavigateModel()
        {
            _dragTransform = MainViewComponent.Current.DragTransform;
            _contentCanvas = MainViewComponent.Current.ContentCanvas;

            Config.Current.View.PropertyChanged += ViewConfig_PropertyChanged;
        }


        public DragTransform DragTransform => _dragTransform;

        public bool IsRotateStretchEnabled
        {
            get => Config.Current.View.IsRotateStretchEnabled;
            set => Config.Current.View.IsRotateStretchEnabled = value;
        }

        public bool IsKeepAngle
        {
            get => Config.Current.View.IsKeepAngle;
            set => Config.Current.View.IsKeepAngle = value;
        }

        public bool IsKeepAngleBooks
        {
            get => Config.Current.View.IsKeepAngleBooks;
            set => Config.Current.View.IsKeepAngleBooks = value;
        }

        public bool IsKeepScale
        {
            get => Config.Current.View.IsKeepScale;
            set => Config.Current.View.IsKeepScale = value;
        }

        public bool IsKeepScaleBooks
        {
            get => Config.Current.View.IsKeepScaleBooks;
            set => Config.Current.View.IsKeepScaleBooks = value;
        }

        public bool IsKeepFlip
        {
            get => Config.Current.View.IsKeepFlip;
            set => Config.Current.View.IsKeepFlip = value;
        }

        public bool IsKeepFlipBooks
        {
            get => Config.Current.View.IsKeepFlipBooks;
            set => Config.Current.View.IsKeepFlipBooks = value;
        }


        private void ViewConfig_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case null:
                case "":
                    RaisePropertyChanged("");
                    break;

                case nameof(ViewConfig.IsKeepAngle):
                    RaisePropertyChanged(nameof(IsKeepAngle));
                    break;
                case nameof(ViewConfig.IsKeepAngleBooks):
                    RaisePropertyChanged(nameof(IsKeepAngleBooks));
                    break;
                case nameof(ViewConfig.IsKeepScale):
                    RaisePropertyChanged(nameof(IsKeepScale));
                    break;
                case nameof(ViewConfig.IsKeepScaleBooks):
                    RaisePropertyChanged(nameof(IsKeepScaleBooks));
                    break;
                case nameof(ViewConfig.IsKeepFlip):
                    RaisePropertyChanged(nameof(IsKeepFlip));
                    break;
                case nameof(ViewConfig.IsKeepFlipBooks):
                    RaisePropertyChanged(nameof(IsKeepFlipBooks));
                    break;
                case nameof(ViewConfig.IsRotateStretchEnabled):
                    RaisePropertyChanged(nameof(IsRotateStretchEnabled));
                    break;
            }
        }

        public void RotateLeft()
        {
            var angle = DragTransformControl.NormalizeLoopRange(_dragTransform.Angle - 90.0, -180.0, 180.0);
            angle = Math.Truncate((angle + 180.0) / 90.0) * 90.0 - 180.0;

            _dragTransform.SetAngle(angle, TransformActionType.Navigate);

            if (IsRotateStretchEnabled)
            {
                Stretch();
            }
        }

        public void RotateRight()
        {
            var angle = DragTransformControl.NormalizeLoopRange(_dragTransform.Angle + 90.0, -180.0, 180.0);
            angle = Math.Truncate((angle + 180.0) / 90.0) * 90.0 - 180.0;

            _dragTransform.SetAngle(angle, TransformActionType.Navigate);

            if (IsRotateStretchEnabled)
            {
                Stretch();
            }
        }

        public void RotateReset()
        {
            _dragTransform.SetAngle(0.0, TransformActionType.Navigate);

            if (IsRotateStretchEnabled)
            {
                Stretch();
            }
        }

        public void ScaleDown()
        {
            var scale = _dragTransform.Scale - 0.01;
            var index = _scaleSnaps.FindIndex(e => scale < e);
            if (0 < index)
            {
                scale = _scaleSnaps[index - 1];
            }
            else
            {
                scale = _scaleSnaps.First();
            }

            _dragTransform.SetScale(scale, TransformActionType.Navigate);
        }

        public void ScaleUp()
        {
            var scale = _dragTransform.Scale + 0.01;
            var index = _scaleSnaps.FindIndex(e => scale < e);
            if (0 <= index)
            {
                scale = _scaleSnaps[index];
            }
            else
            {
                scale = _scaleSnaps.Last();
            }

            _dragTransform.SetScale(scale, TransformActionType.Navigate);
        }

        public void ScaleReset()
        {
            _dragTransform.SetScale(1.0, TransformActionType.Navigate);
        }

        public void Stretch()
        {
            _contentCanvas.Stretch();
        }
    }


}
