using NeeLaboratory;
using NeeLaboratory.ComponentModel;
using NeeLaboratory.Windows.Input;
using NeeView.Effects;
using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Data;

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

            _model.DragTransform.PropertyChanged += DragTransform_PropertyChanged;

            Config.Current.View.PropertyChanged += ViewConfig_PropertyChanged;

            RotateLeftCommand = new RelayCommand(_model.RotateLeft);
            RotateRightCommand = new RelayCommand(_model.RotateRight);
            RotateResetCommand = new RelayCommand(_model.RotateReset);
            ScaleDownCommand = new RelayCommand(_model.ScaleDown);
            ScaleUpCommand = new RelayCommand(_model.ScaleUp);
            ScaleResetCommand = new RelayCommand(_model.ScaleReset);
            StretchCommand = new RelayCommand(_model.Stretch);

            MoreMenuDescription = new NavigateMoreMenuDescription();
        }


        public double Angle
        {
            get => _model.DragTransform.Angle;
            set => _model.DragTransform.SetAngle(value, TransformActionType.Navigate);
        }

        public AutoRotateType AutoRotate
        {
            get => Config.Current.View.AutoRotate;
            set => Config.Current.View.AutoRotate = value;
        }

        public Dictionary<AutoRotateType, string> AutoRotateTypeList { get; } = AliasNameExtensions.GetAliasNameDictionary<AutoRotateType>();


        public double Scale
        {
            get { return _model.DragTransform.Scale * 100.0; }
            set { _model.DragTransform.SetScale(value / 100.0, TransformActionType.Navigate); }
        }

        public double ScaleLog
        {
            get { return _model.DragTransform.Scale > 0.0 ? Math.Log(_model.DragTransform.Scale, 2.0) : -5.0; }
            set { _model.DragTransform.SetScale(Math.Pow(2, value), TransformActionType.Navigate); }
        }


        public bool IsFlipHorizontal
        {
            get => _model.DragTransform.IsFlipHorizontal;
            set => _model.DragTransform.IsFlipHorizontal = value;
        }

        public bool IsFlipVertical
        {
            get => _model.DragTransform.IsFlipVertical;
            set => _model.DragTransform.IsFlipVertical = value;
        }


        public PageStretchMode StretchMode
        {
            get => Config.Current.View.StretchMode;
            set => Config.Current.View.StretchMode = value;
        }

        public Dictionary<PageStretchMode, string> StretchModeList { get; } = AliasNameExtensions.GetAliasNameDictionary<PageStretchMode>();


        public bool IsRotateStretchEnabled
        {
            get { return _model.IsRotateStretchEnabled; }
            set { _model.IsRotateStretchEnabled = value; }
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

        public bool AllowStretchScaleUp
        {
            get => Config.Current.View.AllowStretchScaleUp;
            set => Config.Current.View.AllowStretchScaleUp = value;
        }

        public bool AllowStretchScaleDown
        {
            get => Config.Current.View.AllowStretchScaleDown;
            set => Config.Current.View.AllowStretchScaleDown = value;
        }

        public bool IsBaseScaleEnabled
        {
            get => Config.Current.View.IsBaseScaleEnabled;
            set => Config.Current.View.IsBaseScaleEnabled = value;
        }

        public double BaseScale
        {
            get => Config.Current.View.BaseScale * 100.0;
            set => Config.Current.View.BaseScale = value / 100.0;
        }



        public RelayCommand RotateLeftCommand { get; private set; }
        public RelayCommand RotateRightCommand { get; private set; }
        public RelayCommand RotateResetCommand { get; private set; }
        public RelayCommand ScaleDownCommand { get; private set; }
        public RelayCommand ScaleUpCommand { get; private set; }
        public RelayCommand ScaleResetCommand { get; private set; }
        public RelayCommand StretchCommand { get; private set; }



        private void Model_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case null:
                case "":
                    RaisePropertyChanged("");
                    break;

                case nameof(NavigateModel.IsRotateStretchEnabled):
                    RaisePropertyChanged(nameof(IsRotateStretchEnabled));
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

        private void ViewConfig_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case null:
                case "":
                    RaisePropertyChanged("");
                    break;

                case nameof(ViewConfig.AutoRotate):
                    RaisePropertyChanged(nameof(AutoRotate));
                    break;

                case nameof(ViewConfig.StretchMode):
                    RaisePropertyChanged(nameof(StretchMode));
                    break;

                case nameof(ViewConfig.AllowStretchScaleUp):
                    RaisePropertyChanged(nameof(AllowStretchScaleUp));
                    break;

                case nameof(ViewConfig.AllowStretchScaleDown):
                    RaisePropertyChanged(nameof(AllowStretchScaleDown));
                    break;

                case nameof(ViewConfig.IsBaseScaleEnabled):
                    RaisePropertyChanged(nameof(IsBaseScaleEnabled));
                    break;

                case nameof(ViewConfig.BaseScale):
                    RaisePropertyChanged(nameof(BaseScale));
                    break;
            }
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

        public void AddBaseScaleTick(int delta)
        {
            var tick = 1.0;
            BaseScale = MathUtility.Snap(BaseScale + delta * tick, tick);
        }

        public void AddScaleTick(int delta)
        {
            var tick = 1.0;
            Scale = MathUtility.Snap(Scale + delta * tick, tick);
        }

        public void AddAngleTick(int delta)
        {
            var tick = 1.0;
            Angle = MathUtility.Snap(Angle + delta * tick, tick);
        }



        #region MoreMenu

        public NavigateMoreMenuDescription MoreMenuDescription { get; }

        public class NavigateMoreMenuDescription : MoreMenuDescription
        {
            public override ContextMenu Create()
            {
                var menu = new ContextMenu();
                menu.Items.Add(CreateCheckMenuItem(Properties.Resources.Navigator_MoreMenu_IsVisibleThumbnail, new Binding(nameof(NavigatorConfig.IsVisibleThumbnail)) { Source = Config.Current.Navigator }));
                return menu;
            }
        }

        #endregion
    }
}
