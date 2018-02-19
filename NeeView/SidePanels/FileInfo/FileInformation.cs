// Copyright (c) 2016-2018 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace NeeView
{
    /// <summary>
    /// FileInformation : Model
    /// </summary>
    public class FileInformation : BindableBase
    {
        public static FileInformation Current { get; private set; }

        #region Fields

        private bool _IsUseExifDateTime;
        private bool _IsVisibleBitsPerPixel;
        private bool _IsVisibleLoader;
        private bool _IsVisibleFilePath;
        private ViewContent _viewContent;

        #endregion

        #region Properties

        [PropertyMember("EXIFの日時を使用する", Tips = "ファイル情報パネルでの表示に限ります。フォルダーリストでの日付順ソートには影響しません")]
        public bool IsUseExifDateTime
        {
            get { return _IsUseExifDateTime; }
            set { if (_IsUseExifDateTime != value) { _IsUseExifDateTime = value; RaisePropertyChanged(); } }
        }

        [PropertyMember("画像サイズにピクセルあたりのビット数を表示する")]
        public bool IsVisibleBitsPerPixel
        {
            get { return _IsVisibleBitsPerPixel; }
            set { if (_IsVisibleBitsPerPixel != value) { _IsVisibleBitsPerPixel = value; RaisePropertyChanged(); } }
        }

        [PropertyMember("使用されたアーカイバー、画像デコーダー名を表示する")]
        public bool IsVisibleLoader
        {
            get { return _IsVisibleLoader; }
            set { if (_IsVisibleLoader != value) { _IsVisibleLoader = value; RaisePropertyChanged(); } }
        }

        [PropertyMember("圧縮ファイル内のファイルパスを表示する")]
        public bool IsVisibleFilePath
        {
            get { return _IsVisibleFilePath; }
            set { if (_IsVisibleFilePath != value) { _IsVisibleFilePath = value; RaisePropertyChanged(); } }
        }

        //
        public ViewContent ViewContent
        {
            get { return _viewContent; }
            set { if (_viewContent != value) { _viewContent = value; RaisePropertyChanged(); } }
        }

        #endregion

        #region Constructors

        public FileInformation(ContentCanvas contentCanvas)
        {
            Current = this;

            contentCanvas.AddPropertyChanged(nameof(contentCanvas.MainContent),
                (s, e) => ViewContent = contentCanvas.MainContent);
        }

        #endregion

        #region Methods

        /// <summary>
        /// 表示更新
        /// </summary>
        public void Flush()
        {
            RaisePropertyChanged(nameof(ViewContent));
        }

        #endregion

        #region Memento

        [DataContract]
        public class Memento
        {
            [DataMember]
            public bool IsUseExifDateTime { get; set; }
            [DataMember]
            public bool IsVisibleBitsPerPixel { get; set; }
            [DataMember]
            public bool IsVisibleLoader { get; set; }
            [DataMember]
            public bool IsVisibleFilePath { get; set; }
        }

        //
        public Memento CreateMemento()
        {
            var memento = new Memento();
            memento.IsUseExifDateTime = this.IsUseExifDateTime;
            memento.IsVisibleBitsPerPixel = this.IsVisibleBitsPerPixel;
            memento.IsVisibleLoader = this.IsVisibleLoader;
            memento.IsVisibleFilePath = this.IsVisibleFilePath;
            return memento;
        }

        //
        public void Restore(Memento memento)
        {
            if (memento == null) return;
            IsUseExifDateTime = memento.IsUseExifDateTime;
            IsVisibleBitsPerPixel = memento.IsVisibleBitsPerPixel;
            IsVisibleLoader = memento.IsVisibleLoader;
            IsVisibleFilePath = memento.IsVisibleFilePath;
        }

        #endregion
    }


    /// <summary>
    /// 旧：ファイル情報パネル設定
    /// 互換性のために残してあります
    /// </summary>
    [DataContract]
    public class FileInfoSetting
    {
        [DataMember]
        public bool IsUseExifDateTime { get; set; }

        [DataMember]
        public bool IsVisibleBitsPerPixel { get; set; }

        [DataMember]
        public bool IsVisibleLoader { get; set; }
    }
}
