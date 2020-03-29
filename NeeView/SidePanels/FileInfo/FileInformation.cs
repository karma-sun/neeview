﻿using NeeLaboratory.ComponentModel;
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
        static FileInformation() => Current = new FileInformation();
        public static FileInformation Current { get; }

        #region Fields

        private ViewContent _viewContent;

        #endregion

        #region Constructors

        private FileInformation()
        {
            ContentCanvas.Current.AddPropertyChanged(nameof(ContentCanvas.Current.MainContent),
                (s, e) => ViewContent = ContentCanvas.Current.MainContent);
        }

        #endregion

        #region Properties

        public ViewContent ViewContent
        {
            get { return _viewContent; }
            set { if (_viewContent != value) { _viewContent = value; RaisePropertyChanged(); } }
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
        public class Memento : IMemento
        {
            [DataMember]
            public bool IsVisibleBitsPerPixel { get; set; }
            [DataMember]
            public bool IsVisibleLoader { get; set; }
            [DataMember]
            public bool IsVisibleFilePath { get; set; }

            public void RestoreConfig(Config config)
            {
                config.Information.IsVisibleBitsPerPixel = IsVisibleBitsPerPixel;
                config.Information.IsVisibleLoader = IsVisibleLoader;
                config.Information.IsVisibleFilePath = IsVisibleFilePath;
            }
        }

        public Memento CreateMemento()
        {
            var memento = new Memento();
            memento.IsVisibleBitsPerPixel = Config.Current.Information.IsVisibleBitsPerPixel;
            memento.IsVisibleLoader = Config.Current.Information.IsVisibleLoader;
            memento.IsVisibleFilePath = Config.Current.Information.IsVisibleFilePath;
            return memento;
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
