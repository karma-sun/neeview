// Copyright (c) 2016-2018 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using NeeView.ComponentModel;
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
        /// <summary>
        /// IsUseExifDateTime property.
        /// </summary>
        public bool IsUseExifDateTime
        {
            get { return _IsUseExifDateTime; }
            set { if (_IsUseExifDateTime != value) { _IsUseExifDateTime = value; RaisePropertyChanged(); } }
        }

        private bool _IsUseExifDateTime;


        /// <summary>
        /// IsVisibleBitsPerPixel property.
        /// </summary>
        public bool IsVisibleBitsPerPixel
        {
            get { return _IsVisibleBitsPerPixel; }
            set { if (_IsVisibleBitsPerPixel != value) { _IsVisibleBitsPerPixel = value; RaisePropertyChanged(); } }
        }

        private bool _IsVisibleBitsPerPixel;


        /// <summary>
        /// IsVisibleLoader property.
        /// </summary>
        public bool IsVisibleLoader
        {
            get { return _IsVisibleLoader; }
            set { if (_IsVisibleLoader != value) { _IsVisibleLoader = value; RaisePropertyChanged(); } }
        }

        private bool _IsVisibleLoader;

        /// <summary>
        /// IsVisibleFilePath property.
        /// </summary>
        public bool IsVisibleFilePath
        {
            get { return _IsVisibleFilePath; }
            set { if (_IsVisibleFilePath != value) { _IsVisibleFilePath = value; RaisePropertyChanged(); } }
        }

        private bool _IsVisibleFilePath;




        /// <summary>
        /// ViewContent property.
        /// </summary>
        public ViewContent ViewContent
        {
            get { return _viewContent; }
            set { if (_viewContent != value) { _viewContent = value; RaisePropertyChanged(); } }
        }

        private ViewContent _viewContent;


        /// <summary>
        /// constructor
        /// </summary>
        public FileInformation(ContentCanvas contentCanvas)
        {
            contentCanvas.AddPropertyChanged(nameof(contentCanvas.MainContent),
                (s, e) => ViewContent = contentCanvas.MainContent);
        }



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
