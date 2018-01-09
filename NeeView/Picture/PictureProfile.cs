// Copyright (c) 2016-2017 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using NeeView.ComponentModel;
using NeeView.Windows.Property;
using System;
using System.ComponentModel;
using System.Linq;
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
        public bool IsEnabled
        {
            get { return _IsEnabled; }
            set { if (_IsEnabled != value) { _IsEnabled = value; RaisePropertyChanged(); } }
        }

        /// <summary>
        /// 縦横比を固定する
        /// </summary>
        public bool IsUniformed
        {
            get { return _IsUniformed; }
            set { if (_IsUniformed != value) { _IsUniformed = value; RaisePropertyChanged(); } }
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
        [PropertyRange(16, 4096, Name = "横幅")]
        [DefaultValue(256)]
        public int Width
        {
            get { return (int)_Size.Width; }
            set { if (value != _Size.Width) { Size = new Size(value, _Size.Height); } }
        }

        /// <summary>
        /// カスタムサイズ：縦幅
        /// </summary>
        [PropertyRange(16, 4096, Name = "縦幅")]
        [DefaultValue(256)]
        public int Height
        {
            get { return (int)_Size.Height; }
            set { if (value != _Size.Height) { Size = new Size(_Size.Width, value); } }
        }

        #endregion
    }

    //
    public class PictureProfile : BindableBase
    {
        // 
        public static PictureProfile Current { get; private set; }

        #region Fields

        // 有効ファイル拡張子
        private PictureFileExtension _fileExtension = new PictureFileExtension();

        #endregion

        #region Properties

        // 読み込みデータのサイズ制限適用フラグ
        public bool IsLimitSourceSize { get; set; }

        // 画像処理の最大サイズ
        // リサイズフィルターで使用される。
        // IsLimitSourceSize フラグがONのときには、読み込みサイズにもこの制限が適用される
        private Size _MaximumSize = new Size(4096, 4096);
        public Size MaximumSize
        {
            get { return _MaximumSize; }
            set
            {
                var size = new Size(Math.Max(value.Width, 1024), Math.Max(value.Height, 1024));
                if (_MaximumSize != size) { _MaximumSize = size; RaisePropertyChanged(); }
            }
        }

        /// <summary>
        /// IsResizeEnabled property.
        /// </summary>
        private bool _isResizeFilterEnabled;
        public bool IsResizeFilterEnabled
        {
            get { return _isResizeFilterEnabled; }
            set { if (_isResizeFilterEnabled != value) { _isResizeFilterEnabled = value; RaisePropertyChanged(); } }
        }


        /// <summary>
        /// CustomSize property.
        /// </summary>
        private PictureCustomSize _CustomSize;
        public PictureCustomSize CustomSize
        {
            get { return _CustomSize; }
            set { if (_CustomSize != value) { _CustomSize = value; RaisePropertyChanged(); } }
        }

        #endregion

        #region Constructors

        //
        public PictureProfile()
        {
            Current = this;

            _CustomSize = new PictureCustomSize()
            {
                IsEnabled = false,
                IsUniformed = false,
                Size = new Size(256, 256)
            };
        }

        #endregion

        #region Methods

        // 対応拡張子判定
        public bool IsSupported(string fileName)
        {
            return _fileExtension.IsSupported(fileName);
        }

        // 最大サイズ内におさまるサイズを返す
        public Size CreateFixedSize(Size size)
        {
            if (size.IsEmpty) return size;

            return size.Limit(this.MaximumSize);
        }

        #endregion

        #region Memento

        [DataContract]
        public class Memento
        {
            [DataMember, DefaultValue(false)]
            [PropertyMember("読み込み画像サイズ制限", Tips = "「最大画像サイズ」を上限として読み込み画像を縮小します。速度、メモリ節約用の設定です")]
            public bool IsLimitSourceSize { get; set; }

            [DataMember, DefaultValue(typeof(Size), "4096,4096")]
            [PropertyMember("最大画像サイズ", Tips = "フィルターで拡大される最大画像サイズです。「読み込み画像サイズ制限」フラグがONの場合にはこのサイズに読み込み画像自体を縮小します")]
            public Size Maximum { get; set; }
            [DataMember]
            public bool IsResizeFilterEnabled { get; set; }
        }

        //
        public Memento CreateMemento()
        {
            var memento = new Memento();
            memento.IsLimitSourceSize = this.IsLimitSourceSize;
            memento.Maximum = this.MaximumSize;
            memento.IsResizeFilterEnabled = this.IsResizeFilterEnabled;
            return memento;
        }

        //
        public void Restore(Memento memento)
        {
            if (memento == null) return;
            this.IsLimitSourceSize = memento.IsLimitSourceSize;
            this.MaximumSize = memento.Maximum;
            this.IsResizeFilterEnabled = memento.IsResizeFilterEnabled;
        }
        #endregion

    }
}
