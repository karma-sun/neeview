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
        private bool _isResizeFilterEnabled = false;
        private PictureCustomSize _customSize;
        private bool _isAspectRatioEnabled;


        private PictureProfile()
        {
            _customSize = new PictureCustomSize()
            {
                IsEnabled = false,
                IsUniformed = false,
                Size = new Size(256, 256)
            };
        }


        [PropertyMember("@ParamPictureProfileExtensions")]
        public FileTypeCollection SupportFileTypes => _fileExtension.DefaultExtensions;

        [PropertyMember("@ParamPictureProfileSvgExtensions")]
        public FileTypeCollection SvgFileTypes => _fileExtension.SvgExtensions;

#if false
        // 読み込みデータのサイズ制限適用フラグ
        [PropertyMember("@ParamPictureProfileIsLimitSourceSize", Tips = "@ParamPictureProfileIsLimitSourceSizeTips")]
        public bool IsLimitSourceSize { get; set; }

        // 画像処理の最大サイズ
        // リサイズフィルターで使用される。
        // IsLimitSourceSize フラグがONのときには、読み込みサイズにもこの制限が適用される
        [PropertyMember("@ParamPictureProfileMaximumSize", Tips = "@ParamPictureProfileMaximumSizeTips")]
        public Size MaximumSize
        {
            get { return _MaximumSize; }
            set
            {
                var size = new Size(Math.Max(value.Width, 1024), Math.Max(value.Height, 1024));
                if (_MaximumSize != size) { _MaximumSize = size; RaisePropertyChanged(); }
            }
        }
#endif

        public bool IsResizeFilterEnabled
        {
            get { return _isResizeFilterEnabled; }
            set { if (_isResizeFilterEnabled != value) { _isResizeFilterEnabled = value; RaisePropertyChanged(); } }
        }

        public PictureCustomSize CustomSize
        {
            get { return _customSize; }
            set { if (_customSize != value) { _customSize = value; RaisePropertyChanged(); } }
        }

        // 画像の解像度情報を表示に反映する
        [PropertyMember("@ParamPictureProfileIsAspectRatioEnabled", Tips = "@ParamPictureProfileIsAspectRatioEnabledTips")]
        public bool IsAspectRatioEnabled
        {
            get { return _isAspectRatioEnabled; }
            set { SetProperty(ref _isAspectRatioEnabled, value); }
        }

        // support SVG
        [PropertyMember("@ParamPictureProfileIsSvgEnabled", Tips = "@ParamPictureProfileIsSvgEnabledTips")]
        public bool IsSvgEnabled { get; set; } = true;



        // 対応拡張子判定 (ALL)
        public bool IsSupported(string fileName)
        {
            string ext = LoosePath.GetExtension(fileName);

            if (_fileExtension.DefaultExtensions.Contains(ext)) return true;

            if (SusiePluginManager.Current.IsEnabled)
            {
                if (_fileExtension.SusieExtensions.Contains(ext)) return true;
            }

            if (IsSvgEnabled)
            {
                if (_fileExtension.SvgExtensions.Contains(ext)) return true;
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
            if (!SusiePluginManager.Current.IsEnabled) return false;

            string ext = LoosePath.GetExtension(fileName);
            return _fileExtension.SusieExtensions.Contains(ext);
        }

        // 対応拡張子判定 (Svg)
        public bool IsSvgSupported(string fileName)
        {
            if (!IsSvgEnabled) return false;

            string ext = LoosePath.GetExtension(fileName);
            return _fileExtension.SvgExtensions.Contains(ext);
        }


        // 最大サイズ内におさまるサイズを返す
        public Size CreateFixedSize(Size size)
        {
            if (size.IsEmpty) return size;

            return size.Limit(Config.Current.Performance.MaximumSize);
        }


        #region Memento

        [DataContract]
        public class Memento
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
            private void Deserializing(StreamingContext c)
            {
                this.InitializePropertyDefaultValues();
            }
        }

        public Memento CreateMemento()
        {
            var memento = new Memento();
            ////memento.IsLimitSourceSize = Config.Current.Performance.IsLimitSourceSize;
            ////memento.Maximum = Config.Current.Performance.MaximumSize;
            memento.IsResizeFilterEnabled = this.IsResizeFilterEnabled;
            memento.CustomSize = this.CustomSize.CreateMemento();
            memento.IsAspectRatioEnabled = this.IsAspectRatioEnabled;
            memento.IsSvgEnabled = this.IsSvgEnabled;
            return memento;
        }

        public void Restore(Memento memento)
        {
            if (memento == null) return;
            Config.Current.Performance.IsLimitSourceSize = memento.IsLimitSourceSize;
            Config.Current.Performance.MaximumSize = memento.Maximum;
            this.IsResizeFilterEnabled = memento.IsResizeFilterEnabled;
            this.CustomSize.Restore(memento.CustomSize);
            this.IsAspectRatioEnabled = memento.IsAspectRatioEnabled;
            this.IsSvgEnabled = memento.IsSvgEnabled;
        }
        #endregion

    }
}
