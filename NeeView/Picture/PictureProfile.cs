using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows;

namespace NeeView
{
    public class PictureProfile : BindableBase
    {
        static PictureProfile() => Current = new PictureProfile();
        public static PictureProfile Current { get; }


        public static readonly Uri HEIFImageExtensions = new Uri(@"ms-windows-store://pdp/?ProductId=9pmmsr1cgpwg");


        // 有効ファイル拡張子
        private PictureFileExtension _fileExtension = new PictureFileExtension();

        private Size _MaximumSize = new Size(4096, 4096);


        private PictureProfile()
        {
        }


        [PropertyMember("@ParamPictureProfileExtensions")]
        public FileTypeCollection SupportFileTypes => _fileExtension.DefaultExtensions;


        // 対応拡張子判定 (ALL)
        public bool IsSupported(string fileName)
        {
            string ext = LoosePath.GetExtension(fileName);

            if (_fileExtension.DefaultExtensions.Contains(ext)) return true;

            if (Config.Current.Susie.IsEnabled)
            {
                if (_fileExtension.SusieExtensions.Contains(ext)) return true;
            }

            if (Config.Current.Image.Svg.IsEnabled)
            {
                if (Config.Current.Image.Svg.SupportFileTypes.Contains(ext)) return true;
            }

            return false;
        }

        // 対応拡張子判定 (標準)
        public bool IsDefaultSupported(string fileName)
        {
            string ext = LoosePath.GetExtension(fileName);
            return _fileExtension.DefaultExtensions.Contains(ext);
        }

        // 対応拡張子判定 (Susie)
        public bool IsSusieSupported(string fileName)
        {
            if (!Config.Current.Susie.IsEnabled) return false;

            string ext = LoosePath.GetExtension(fileName);
            return _fileExtension.SusieExtensions.Contains(ext);
        }

        // 対応拡張子判定 (Svg)
        public bool IsSvgSupported(string fileName)
        {
            if (!Config.Current.Image.Svg.IsEnabled) return false;

            string ext = LoosePath.GetExtension(fileName);
            return Config.Current.Image.Svg.SupportFileTypes.Contains(ext);
        }


        // 最大サイズ内におさまるサイズを返す
        public Size CreateFixedSize(Size size)
        {
            if (size.IsEmpty) return size;

            return size.Limit(Config.Current.Performance.MaximumSize);
        }


        #region Memento

        [DataContract]
        public class Memento : IMemento
        {
            [DataMember, DefaultValue(false)]
            public bool IsLimitSourceSize { get; set; }

            [DataMember, DefaultValue(typeof(Size), "4096,4096")]
            public Size Maximum { get; set; }

            [DataMember]
            public bool IsResizeFilterEnabled { get; set; }

            [DataMember]
            public PictureCustomSize.Memento CustomSize { get; set; }

            [DataMember]
            public bool IsAspectRatioEnabled { get; set; }

            [DataMember, DefaultValue(true)]
            public bool IsSvgEnabled { get; set; }


            [OnDeserializing]
            private void OnDeserializing(StreamingContext c)
            {
                this.InitializePropertyDefaultValues();
            }

            public void RestoreConfig(Config config)
            {
                CustomSize.RestoreConfig(config);

                config.Performance.IsLimitSourceSize = IsLimitSourceSize;
                config.Performance.MaximumSize = Maximum;
                config.ImageResizeFilter.IsEnabled = IsResizeFilterEnabled;
                config.Image.Standard.IsAspectRatioEnabled = IsAspectRatioEnabled;
                config.Image.Svg.IsEnabled = IsSvgEnabled;
            }
        }

        public Memento CreateMemento()
        {
            var memento = new Memento();
            memento.IsLimitSourceSize = Config.Current.Performance.IsLimitSourceSize;
            memento.Maximum = Config.Current.Performance.MaximumSize;
            memento.IsResizeFilterEnabled = Config.Current.ImageResizeFilter.IsEnabled;
            memento.CustomSize = new PictureCustomSize().CreateMemento();
            memento.IsAspectRatioEnabled = Config.Current.Image.Standard.IsAspectRatioEnabled;
            memento.IsSvgEnabled = Config.Current.Image.Svg.IsEnabled;
            return memento;
        }
        #endregion

    }
}
