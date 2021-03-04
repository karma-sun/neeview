using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System;
using System.Text.Json.Serialization;
using System.Windows;

namespace NeeView
{
    public class InformationConfig : BindableBase
    {
        private bool _isVisibleBitsPerPixel;
        private bool _isVisibleLoader;
        private bool _isVisibleFilePath;
        private bool _isVisibleFileSection = true;
        private bool _isVisibleImageSection = true;
        private bool _isVisibleDescriptionSection = true;
        private bool _isVisibleOriginSection = true;
        private bool _isVisibleCameraSection = true;
        private bool _isVisibleAdvancedPhotoSection = true;
        private bool _isVisibleGpsSection = true;
        private GridLength _propertyHeaderWidth = new GridLength(128.0);


        [Obsolete]
        [PropertyMember]
        public bool IsVisibleBitsPerPixel
        {
            get { return _isVisibleBitsPerPixel; }
            set { SetProperty(ref _isVisibleBitsPerPixel, value); }
        }

        [Obsolete]
        [PropertyMember]
        public bool IsVisibleLoader
        {
            get { return _isVisibleLoader; }
            set { SetProperty(ref _isVisibleLoader, value); }
        }

        [Obsolete]
        [PropertyMember]
        public bool IsVisibleFilePath
        {
            get { return _isVisibleFilePath; }
            set { SetProperty(ref _isVisibleFilePath, value); }
        }



        [PropertyMember]
        public bool IsVisibleFileSection
        {
            get { return _isVisibleFileSection; }
            set { SetProperty(ref _isVisibleFileSection, value); }
        }

        [PropertyMember]
        public bool IsVisibleImageSection
        {
            get { return _isVisibleImageSection; }
            set { SetProperty(ref _isVisibleImageSection, value); }
        }

        [PropertyMember]
        public bool IsVisibleDescriptionSection
        {
            get { return _isVisibleDescriptionSection; }
            set { SetProperty(ref _isVisibleDescriptionSection, value); }
        }

        [PropertyMember]
        public bool IsVisibleOriginSection
        {
            get { return _isVisibleOriginSection; }
            set { SetProperty(ref _isVisibleOriginSection, value); }
        }

        [PropertyMember]
        public bool IsVisibleCameraSection
        {
            get { return _isVisibleCameraSection; }
            set { SetProperty(ref _isVisibleCameraSection, value); }
        }

        [PropertyMember]
        public bool IsVisibleAdvancedPhotoSection
        {
            get { return _isVisibleAdvancedPhotoSection; }
            set { SetProperty(ref _isVisibleAdvancedPhotoSection, value); }
        }

        [PropertyMember]
        public bool IsVisibleGpsSection
        {
            get { return _isVisibleGpsSection; }
            set { SetProperty(ref _isVisibleGpsSection, value); }
        }

        #region HiddenParameters

        [PropertyMapIgnore]
        public GridLength PropertyHeaderWidth
        {
            get { return _propertyHeaderWidth; }
            set { SetProperty(ref _propertyHeaderWidth, value); }
        }

        #endregion HiddenParameters
    }
}


