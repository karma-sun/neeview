// Copyright (c) 2016-2018 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows;

namespace NeeView
{
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
        [PropertyMember("読み込み画像サイズ制限", Tips = "「最大画像サイズ」を上限として読み込み画像を縮小します。速度、メモリ節約用の設定です。この制限を適用して読み込まれた画像である場合、ファイル情報のサイズ欄に\"*\"が表示されます。")]
        public bool IsLimitSourceSize { get; set; }

        // 画像処理の最大サイズ
        // リサイズフィルターで使用される。
        // IsLimitSourceSize フラグがONのときには、読み込みサイズにもこの制限が適用される
        private Size _MaximumSize = new Size(4096, 4096);
        [PropertyMember("最大画像サイズ", Tips = "リサイズフィルターで拡大される最大画像サイズです。")]
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
        private bool _isResizeFilterEnabled = false;
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

        /// <summary>
        /// IsMagicScaleSimdEnabled property.
        /// </summary>
        private bool _IsMagicScaleSimdEnabled = true;
        [PropertyMember("リサイズフィルター処理にSIMDを使用する")]
        public bool IsMagicScaleSimdEnabled
        {
            get { return _IsMagicScaleSimdEnabled; }
            set
            {
                if (_IsMagicScaleSimdEnabled != value)
                {
                    _IsMagicScaleSimdEnabled = value;
                    MagicScalerBitmapFactory.EnabmeSimd = _IsMagicScaleSimdEnabled;
                    RaisePropertyChanged();
                }
            }
        }


        #endregion

        #region Constructors

        /// <summary>
        /// コンストラクタ
        /// </summary>
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
            public bool IsLimitSourceSize { get; set; }

            [DataMember, DefaultValue(typeof(Size), "4096,4096")]
            public Size Maximum { get; set; }

            [DataMember]
            public bool IsResizeFilterEnabled { get; set; }

            [DataMember]
            public PictureCustomSize.Memento CustomSize { get; set; }

            [DataMember, DefaultValue(true)]
            public bool IsMagicScaleSimdEnabled { get; set; }

            #region Constructors

            public Memento()
            {
                Constructor();
            }

            [OnDeserializing]
            private void Deserializing(StreamingContext c)
            {
                Constructor();
            }

            private void Constructor()
            {
                IsMagicScaleSimdEnabled = true;
            }

            #endregion
        }

        //
        public Memento CreateMemento()
        {
            var memento = new Memento();
            memento.IsLimitSourceSize = this.IsLimitSourceSize;
            memento.Maximum = this.MaximumSize;
            memento.IsResizeFilterEnabled = this.IsResizeFilterEnabled;
            memento.CustomSize = this.CustomSize.CreateMemento();
            memento.IsMagicScaleSimdEnabled = this.IsMagicScaleSimdEnabled;
            return memento;
        }

        //
        public void Restore(Memento memento)
        {
            if (memento == null) return;
            this.IsLimitSourceSize = memento.IsLimitSourceSize;
            this.MaximumSize = memento.Maximum;
            this.IsResizeFilterEnabled = memento.IsResizeFilterEnabled;
            this.CustomSize.Restore(memento.CustomSize);
            this.IsMagicScaleSimdEnabled = memento.IsMagicScaleSimdEnabled;
        }
        #endregion

    }
}
