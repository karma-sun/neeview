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
            DragTransform.Current.PropertyChanged += DragTransform_PropertyChanged;
            Config.Current.View.AddPropertyChanged(nameof(ViewConfig.IsKeepAngle), (s, e) => RaisePropertyChanged(nameof(IsKeepAngle)));
            Config.Current.View.AddPropertyChanged(nameof(ViewConfig.IsKeepScale), (s, e) => RaisePropertyChanged(nameof(IsKeepScale)));
            Config.Current.View.AddPropertyChanged(nameof(ViewConfig.IsKeepFlip), (s, e) => RaisePropertyChanged(nameof(IsKeepFlip)));
        }


        [PropertyRange("@ParamNavigateAngle", -180.0, 180.0, TickFrequency = 1.0)]
        public double Angle
        {
            get { return Math.Truncate(DragTransform.Current.Angle); }
            set { DragTransform.Current.Angle = value; }
        }

        /*
        [PropertyMember("@ParamNavigateAutoRotate")]
        public AutoRotateType AutoRotate
        {
            get => Config.Current.View.AutoRotate;
            set => Config.Current.View.AutoRotate = value;
        }


        [PropertyRange("@ParamNavigateScale", -5.0, 5.0, TickFrequency = 0.01, RangeProperty = nameof(ScaleSlider))]
        public double Scale
        {
            get { return Math.Truncate(DragTransform.Current.Scale * 100.0); }
            set { DragTransform.Current.Scale = value / 100.0; }
        }

        public double ScaleSlider
        {
            get { return DragTransform.Current.Scale > 0.0 ? Math.Log(DragTransform.Current.Scale, 2.0) : -5.0; }
            set { DragTransform.Current.Scale = Math.Pow(2, value); }
        }

        [PropertyMember("@ParamNavigateIsFlipHorizontal")]
        public bool IsFlipHorizontal
        {
            get => DragTransform.Current.IsFlipHorizontal;
            set => DragTransform.Current.IsFlipHorizontal = value;
        }

        [PropertyMember("@ParamNavigateIsFlipVertical")]
        public bool IsFlipVertical
        {
            get => DragTransform.Current.IsFlipVertical;
            set => DragTransform.Current.IsFlipVertical = value;
        }
        */

        private bool _IsStretchEnabled;
        public bool IsStretchEnabled
        {
            get { return _IsStretchEnabled; }
            set
            {
                if (SetProperty(ref _IsStretchEnabled, value))
                {
                    if (_IsStretchEnabled)
                    {
                        Stretch();
                    }
                }
            }
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


        private void DragTransform_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case null:
                case "":
                    RaisePropertyChanged("");
                    break;

                case nameof(DragTransform.Angle):
                    RaisePropertyChanged(nameof(Angle));
                    break;

                    /*
                case nameof(DragTransform.Scale):
                    RaisePropertyChanged(nameof(Scale));
                    RaisePropertyChanged(nameof(ScaleSlider));
                    break;

                case nameof(DragTransform.IsFlipHorizontal):
                    RaisePropertyChanged(nameof(IsFlipHorizontal));
                    break;

                case nameof(DragTransform.IsFlipVertical):
                    RaisePropertyChanged(nameof(IsFlipVertical));
                    break;
                    */
            }
        }


        public void RotateLeft()
        {
            var angle = DragTransformControl.NormalizeLoopRange(DragTransform.Current.Angle - 90.0, -180.0, 180.0);
            angle = Math.Truncate((angle + 180.0) / 90.0) * 90.0 - 180.0;

            DragTransform.Current.Angle = angle;

            if (IsStretchEnabled)
            {
                Stretch();
            }
        }

        public void RotateRight()
        {
            var angle = DragTransformControl.NormalizeLoopRange(DragTransform.Current.Angle + 90.0, -180.0, 180.0);
            angle = Math.Truncate((angle + 180.0) / 90.0) * 90.0 - 180.0;

            DragTransform.Current.Angle = angle;

            if (IsStretchEnabled)
            {
                Stretch();
            }
        }

        public void RotateReset()
        {
            DragTransform.Current.Angle = 0.0;

            if (IsStretchEnabled)
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
