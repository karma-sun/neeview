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
                case nameof(NavigateModel.IsFlipHorizontal):
                    RaisePropertyChanged(nameof(IsFlipHorizontal));
                    break;
                case nameof(NavigateModel.IsFlipVertical):
                    RaisePropertyChanged(nameof(IsFlipVertical));
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


        public bool IsFlipHorizontal
        {
            get => _model.IsFlipHorizontal;
            set => _model.IsFlipHorizontal = value;
        }

        public bool IsFlipVertical
        {
            get => _model.IsFlipVertical;
            set => _model.IsFlipVertical = value;
        }

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
    }
}
