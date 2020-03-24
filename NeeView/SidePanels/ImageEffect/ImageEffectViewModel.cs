using NeeLaboratory.ComponentModel;
using NeeView.Effects;
using NeeView.Windows.Property;
using System.Collections.Generic;
using System.ComponentModel;

namespace NeeView
{
    /// <summary>
    /// ImageEffect : ViewModel
    /// </summary>
    public class ImageEffectViewModel : BindableBase
    {
        //
        public ImageEffectViewModel(ImageEffect model, ImageFilter imageFilter)
        {
            _model = model;
            _imageFilter = imageFilter;

            this.UnsharpMaskProfile = new PropertyDocument(_imageFilter.UnsharpMaskProfile);

            this.CustomSizeProfile = new PropertyDocument(Config.Current.ImageCustomSize);
            this.CustomSizeProfile.SetVisualType<PropertyValue_Boolean>(PropertyVisualType.ToggleSwitch);

            this.GridLineProfile = new PropertyDocument(ContentCanvas.Current.GridLine);
            this.GridLineProfile.SetVisualType<PropertyValue_Boolean>(PropertyVisualType.ToggleSwitch);
            this.GridLineProfile.SetVisualType<PropertyValue_Color>(PropertyVisualType.ComboColorPicker);
        }


        /// <summary>
        /// Model property.
        /// </summary>
        private ImageEffect _model;
        public ImageEffect Model
        {
            get { return _model; }
            set { if (_model != value) { _model = value; RaisePropertyChanged(); } }
        }

        /// <summary>
        /// ImageFilter property.
        /// </summary>
        private ImageFilter _imageFilter;
        public ImageFilter ImageFilter
        {
            get { return _imageFilter; }
            set { if (_imageFilter != value) { _imageFilter = value; RaisePropertyChanged(); } }
        }

        // PictureProfile
        public PictureProfile PictureProfile => PictureProfile.Current;

        // ContentCanvs
        public ContentCanvas ContentCanvas => ContentCanvas.Current;

        //
        public PropertyDocument UnsharpMaskProfile { get; set; }

        //
        public PropertyDocument CustomSizeProfile { get; set; }

        //
        public PropertyDocument GridLineProfile { get; set; }

        //
        public Dictionary<EffectType, string> EffectTypeList { get; } = AliasNameExtensions.GetAliasNameDictionary<EffectType>();

        // TODO: これモデルじゃね？
        public void ResetValue()
        {
            using (var lockerKey = ContentRebuild.Current.Locker.Lock())
            {
                _imageFilter.ResizeInterpolation = ResizeInterpolation.Lanczos;
                _imageFilter.Sharpen = true;
                this.UnsharpMaskProfile.Reset();
            }
        }

    }

}
