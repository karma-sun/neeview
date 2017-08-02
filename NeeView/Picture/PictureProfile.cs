// Copyright (c) 2016 Mitsuhiro Ito (nee)
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

        #endregion

        #region Constructors

        //
        public PictureProfile()
        {
            Current = this;
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
