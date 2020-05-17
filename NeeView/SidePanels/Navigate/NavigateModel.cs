using NeeLaboratory.ComponentModel;
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


        public NavigateModel()
        {
            Config.Current.View.PropertyChanged += ViewConfig_PropertyChanged;
        }


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

        public bool IsKeepScale
        {
            get => Config.Current.View.IsKeepScale;
            set => Config.Current.View.IsKeepScale = value;
        }

        public bool IsKeepFlip
        {
            get => Config.Current.View.IsKeepFlip;
            set => Config.Current.View.IsKeepFlip = value;
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
                case nameof(ViewConfig.IsKeepScale):
                    RaisePropertyChanged(nameof(IsKeepScale));
                    break;
                case nameof(ViewConfig.IsKeepFlip):
                    RaisePropertyChanged(nameof(IsKeepFlip));
                    break;
                case nameof(ViewConfig.IsRotateStretchEnabled):
                    RaisePropertyChanged(nameof(IsRotateStretchEnabled));
                    break;
            }
        }

        public void RotateLeft()
        {
            var angle = DragTransformControl.NormalizeLoopRange(DragTransform.Current.Angle - 90.0, -180.0, 180.0);
            angle = Math.Truncate((angle + 180.0) / 90.0) * 90.0 - 180.0;

            DragTransform.Current.Angle = angle;

            if (IsRotateStretchEnabled)
            {
                Stretch();
            }
        }

        public void RotateRight()
        {
            var angle = DragTransformControl.NormalizeLoopRange(DragTransform.Current.Angle + 90.0, -180.0, 180.0);
            angle = Math.Truncate((angle + 180.0) / 90.0) * 90.0 - 180.0;

            DragTransform.Current.Angle = angle;

            if (IsRotateStretchEnabled)
            {
                Stretch();
            }
        }

        public void RotateReset()
        {
            DragTransform.Current.Angle = 0.0;

            if (IsRotateStretchEnabled)
            {
                Stretch();
            }
        }

        public void ScaleDown()
        {
            var scale = DragTransform.Current.Scale - 0.01;
            var index = _scaleSnaps.FindIndex(e => scale < e);
            if (0 < index)
            {
                scale = _scaleSnaps[index - 1];
            }
            else
            {
                scale = _scaleSnaps.First();
            }

            DragTransform.Current.Scale = scale;
        }

        public void ScaleUp()
        {
            var scale = DragTransform.Current.Scale + 0.01;
            var index = _scaleSnaps.FindIndex(e => scale < e);
            if (0 <= index)
            {
                scale = _scaleSnaps[index];
            }
            else
            {
                scale = _scaleSnaps.Last();
            }

            DragTransform.Current.Scale = scale;
        }

        public void ScaleReset()
        {
            DragTransform.Current.Scale = 1.0;
        }

        public void Stretch()
        {
            ContentCanvas.Current.Stretch();
        }
    }


}
