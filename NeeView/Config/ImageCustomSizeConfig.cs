using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System.Text.Json.Serialization;
using System.Windows;

namespace NeeView
{
    public class ImageCustomSizeConfig : BindableBase
    {
        private bool _IsEnabled;
        private bool _IsUniformed;
        private Size _Size = new Size(256, 256);


        /// <summary>
        /// 指定サイズ有効
        /// </summary>
        [PropertyMember("@ParamImageCustomSizeIsEnabled", IsVisible = false)]
        public bool IsEnabled
        {
            get { return _IsEnabled; }
            set { SetProperty(ref _IsEnabled, value); }
        }

        /// <summary>
        /// カスタムサイズ
        /// </summary>
        [PropertyMember("@ParamImageCustomSize", IsVisible = false)]
        public Size Size
        {
            get { return _Size; }
            set
            {
                if (SetProperty(ref _Size, value))
                {
                    RaisePropertyChanged(nameof(Width));
                    RaisePropertyChanged(nameof(Height));
                }
            }
        }

        /// <summary>
        /// カスタムサイズ：横幅
        /// </summary>
        [PropertyRange("@ParamPictureCustomWidth", 16, 4096)]
        [JsonIgnore, PropertyMapIgnoreAttribute]
        public int Width
        {
            get { return (int)_Size.Width; }
            set { if (value != _Size.Width) { Size = new Size(value, _Size.Height); } }
        }

        /// <summary>
        /// カスタムサイズ：縦幅
        /// </summary>
        [PropertyRange("@ParamPictureCustomHeight", 16, 4096)]
        [JsonIgnore, PropertyMapIgnoreAttribute]
        public int Height
        {
            get { return (int)_Size.Height; }
            set { if (value != _Size.Height) { Size = new Size(_Size.Width, value); } }
        }

        /// <summary>
        /// 縦横比を固定する
        /// </summary>
        [PropertyMember("@ParamPictureCustomLockAspect")]
        public bool IsUniformed
        {
            get { return _IsUniformed; }
            set { SetProperty(ref _IsUniformed, value); }
        }

        /// <summary>
        /// ハッシュ値の計算
        /// </summary>
        /// <returns></returns>
        public int GetHashCodde()
        {
            var hash = (_IsEnabled.GetHashCode() << 30) ^ (_IsUniformed.GetHashCode() << 29) ^ _Size.GetHashCode();
            ////System.Diagnostics.Debug.WriteLine($"hash={hash}");
            return hash;
        }
    }
}