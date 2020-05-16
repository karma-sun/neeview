using NeeLaboratory.ComponentModel;
using NeeLaboratory.Windows.Input;
using NeeView.Effects;
using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace NeeView
{
    /// <summary>
    /// Navigate : ViewModel
    /// </summary>
    public class NavigateViewModel : BindableBase
    {
        private NavigateModel _model;


        public NavigateViewModel(NavigateModel model)
        {
            _model = model;
            _model.PropertyChanged += Model_PropertyChanged;

            DragTransform.Current.PropertyChanged += DragTransform_PropertyChanged;

            Config.Current.View.AddPropertyChanged(nameof(ViewConfig.AutoRotate), (s, e) => RaisePropertyChanged(nameof(AutoRotate)));
            Config.Current.View.AddPropertyChanged(nameof(ViewConfig.StretchMode), (s, e) => RaisePropertyChanged(nameof(StretchMode)));

            this.NavigateProfile = new PropertyDocument(_model);
            this.NavigateProfile.SetVisualType<PropertyValue_Boolean>(PropertyVisualType.ToggleSwitch);

            RotateLeftCommand = new RelayCommand(_model.RotateLeft);
            RotateRightCommand = new RelayCommand(_model.RotateRight);
            RotateResetCommand = new RelayCommand(_model.RotateReset);

            ScaleDownCommand = new RelayCommand(_model.ScaleDown);
            ScaleUpCommand = new RelayCommand(_model.ScaleUp);
            ScaleResetCommand = new RelayCommand(_model.ScaleReset);

            StretchCommand = new RelayCommand(_model.Stretch);
        }

        private void Model_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case null:
                case "":
                    RaisePropertyChanged("");
                    break;

                case nameof(NavigateModel.Angle):
                    RaisePropertyChanged(nameof(Angle));
                    break;

                case nameof(NavigateModel.IsStretchEnabled):
                    RaisePropertyChanged(nameof(IsStretchEnabled));
                    break;
                case nameof(NavigateModel.IsKeepAngle):
                    RaisePropertyChanged(nameof(IsKeepAngle));
                    break;
                case nameof(NavigateModel.IsKeepScale):
                    RaisePropertyChanged(nameof(IsKeepScale));
                    break;
                case nameof(NavigateModel.IsKeepFlip):
                    RaisePropertyChanged(nameof(IsKeepScale));
                    break;
            }
        }

        public PropertyDocument NavigateProfile { get; set; }

        public double Angle
        {
            get => _model.Angle;
            set => _model.Angle = value;
        }

        public AutoRotateType AutoRotate
        {
            get => Config.Current.View.AutoRotate;
            set => Config.Current.View.AutoRotate = value;
        }

        public Dictionary<AutoRotateType, string> AutoRotateTypeList { get; } = AliasNameExtensions.GetAliasNameDictionary<AutoRotateType>();


        public double Scale
        {
            get { return Math.Truncate(DragTransform.Current.Scale * 100.0); }
            set { DragTransform.Current.Scale = value / 100.0; }
        }

        public double ScaleLog
        {
            get { return DragTransform.Current.Scale > 0.0 ? Math.Log(DragTransform.Current.Scale, 2.0) : -5.0; }
            set { DragTransform.Current.Scale = Math.Pow(2, value); }
        }


        public bool IsFlipHorizontal
        {
            get => DragTransform.Current.IsFlipHorizontal;
            set => DragTransform.Current.IsFlipHorizontal = value;
        }

        public bool IsFlipVertical
        {
            get => DragTransform.Current.IsFlipVertical;
            set => DragTransform.Current.IsFlipVertical = value;
        }


        public PageStretchMode StretchMode
        {
            get => Config.Current.View.StretchMode;
            set => Config.Current.View.StretchMode = value;
        }

        public Dictionary<PageStretchMode, string> StretchModeList { get; } = AliasNameExtensions.GetAliasNameDictionary<PageStretchMode>();


        public bool IsStretchEnabled
        {
            get { return _model.IsStretchEnabled; }
            set { _model.IsStretchEnabled = value; }
        }

        public bool IsKeepAngle
        {
            get => _model.IsKeepAngle;
            set => _model.IsKeepAngle = value;
        }

        public bool IsKeepScale
        {
            get => _model.IsKeepScale;
            set => _model.IsKeepScale = value;
        }

        public bool IsKeepFlip
        {
            get => _model.IsKeepFlip;
            set => _model.IsKeepFlip = value;
        }


        public RelayCommand RotateLeftCommand { get; private set; }
        public RelayCommand RotateRightCommand { get; private set; }
        public RelayCommand RotateResetCommand { get; private set; }

        public RelayCommand ScaleDownCommand { get; private set; }
        public RelayCommand ScaleUpCommand { get; private set; }
        public RelayCommand ScaleResetCommand { get; private set; }

        public RelayCommand StretchCommand { get; private set; }


        private void DragTransform_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case null:
                case "":
                    RaisePropertyChanged("");
                    break;

#if false
                case nameof(DragTransform.Angle):
                    RaisePropertyChanged(nameof(Angle));
                    break;

#endif
                case nameof(DragTransform.Scale):
                    RaisePropertyChanged(nameof(Scale));
                    RaisePropertyChanged(nameof(ScaleLog));
                    break;

                case nameof(DragTransform.IsFlipHorizontal):
                    RaisePropertyChanged(nameof(IsFlipHorizontal));
                    break;

                case nameof(DragTransform.IsFlipVertical):
                    RaisePropertyChanged(nameof(IsFlipVertical));
                    break;
            }
        }
    }
}
