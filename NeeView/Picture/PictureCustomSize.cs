using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Windows;

namespace NeeView
{
    /// <summary>
    /// 画像指定サイズ
    /// </summary>
    public class PictureCustomSize : BindableBase
    {
        #region Fields

        private bool _IsEnabled;
        private bool _IsUniformed;
        private Size _Size;

        #endregion

        #region Properties

        /// <summary>
        /// 指定サイズ有効
        /// </summary>
        public bool IsEnabled
        {
            get { return _IsEnabled; }
            set { if (_IsEnabled != value) { _IsEnabled = value; RaisePropertyChanged(); } }
        }

        /// <summary>
        /// カスタムサイズ
        /// </summary>
        public Size Size
        {
            get { return _Size; }
            set { if (_Size != value) { _Size = value; RaisePropertyChanged(); RaisePropertyChanged(nameof(Width)); RaisePropertyChanged(nameof(Height)); } }
        }

        /// <summary>
        /// カスタムサイズ：横幅
        /// </summary>
        [PropertyRange("@ParamPictureCustomWidth", 16, 4096)]
        [DefaultValue(256)]
        public int Width
        {
            get { return (int)_Size.Width; }
            set { if (value != _Size.Width) { Size = new Size(value, _Size.Height); } }
        }

        /// <summary>
        /// カスタムサイズ：縦幅
        /// </summary>
        [PropertyRange("@ParamPictureCustomHeight", 16, 4096)]
        [DefaultValue(256)]
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
            set { if (_IsUniformed != value) { _IsUniformed = value; RaisePropertyChanged(); } }
        }

        #endregion

        #region Methods

        /// <summary>
        /// ハッシュ値取得
        /// </summary>
        /// <returns></returns>
        public int GetHashCodde()
        {
            var hash = (_IsEnabled.GetHashCode() << 30) ^ (_IsUniformed.GetHashCode() << 29) ^ _Size.GetHashCode();
            ////System.Diagnostics.Debug.WriteLine($"hash={hash}");
            return hash;
        }

        #endregion

        #region Memento

        [DataContract]
        public class Memento : IMemento
        {
            [DataMember]
            public bool IsEnabled { get; set; }
            [DataMember]
            public bool IsUniformed { get; set; }
            [DataMember]
            public Size Size { get; set; }
        }

        public Memento CreateMemento()
        {
            var memento = new Memento();

            memento.IsEnabled = this.IsEnabled;
            memento.IsUniformed = this.IsUniformed;
            memento.Size = this.Size;

            return memento;
        }

        public void Restore(Memento memento)
        {
            if (memento == null) return;

            this.IsEnabled = memento.IsEnabled;
            this.IsUniformed = memento.IsUniformed;
            this.Size = memento.Size;
        }

        #endregion
    }
}
