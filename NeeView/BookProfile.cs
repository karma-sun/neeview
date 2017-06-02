// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php


using NeeView.ComponentModel;
using NeeView.Windows.Property;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Windows;

namespace NeeView
{

    /// <summary>
    /// 本：設定
    /// </summary>
    public class BookProfile
    {
        public static BookProfile Current { get; private set; }

        /// <summary>
        /// ページ移動優先設定
        /// </summary>
        public bool IsPrioritizePageMove { get; set; } = true;

        /// <summary>
        /// ページ移動優先設定
        /// </summary>
        public bool CanPrioritizePageMove()
        {
            return this.IsPrioritizePageMove && !SlideShow.Current.IsPlayingSlideShow;
        }

        /// <summary>
        /// ページ移動命令重複許可
        /// </summary>
        public bool IsMultiplePageMove { get; set; } = true;

        /// <summary>
        /// ページ移動命令重複許可
        /// </summary>
        public bool CanMultiplePageMove()
        {
            return this.IsMultiplePageMove && !SlideShow.Current.IsPlayingSlideShow;
        }

        /// <summary>
        /// 先読みモード
        /// </summary>
        public PreLoadMode PreLoadMode { get; set; } = PreLoadMode.AutoPreLoad;

        /// <summary>
        /// 先読み自動判定許サイズ
        /// </summary>
        public Size PreloadLimitSize { get; set; } = new Size(4096, 4096);

        /// <summary>
        /// 先読み自動判定許サイズ
        /// </summary>
        public int PreLoadLimitSize
        {
            get { return (int)(PreloadLimitSize.Width * PreloadLimitSize.Height); }
        }
        
        /// <summary>
        /// WideRatio property.
        /// </summary>
        public double WideRatio { get; set; } = 1.0;

        /// <summary>
        /// 除外パス
        /// </summary>
        public StringCollection Excludes { get; set; } = new StringCollection("__MACOSX;.DS_Store");

        // GIFアニメ有効
        public bool IsEnableAnimatedGif { get; set; }

        // EXIF回転有効
        public bool IsEnableExif { get; set; }

        // サポート外ファイル有効
        public bool IsEnableNoSupportFile { get; set; }


        /// <summary>
        /// constructor
        /// </summary>
        public BookProfile()
        {
            Current = this;
        }


        #region Memento
        [DataContract]
        public class Memento
        {
            [DataMember, DefaultValue(true)]
            [PropertyMember("ページ送り優先", Tips = "ページの表示を待たずにページ送りを実行します", IsVisible = false)]
            public bool IsPrioritizePageMove { get; set; }

            [DataMember, DefaultValue(true)]
            [PropertyMember("ページ送りコマンドの重複許可", Tips = "発行されたページ移動コマンドを全て実行します。\nFalseの場合は重複したページ送りコマンドはキャンセルされます", IsVisible = false)]
            public bool IsMultiplePageMove { get; set; }

            [DataMember, DefaultValue("__MACOSX;.DS_Store")]
            [PropertyMember("ページ除外パス", Tips = ";(セミコロン)区切りで除外するパス名を羅列します。「サポート外ファイルもページに含める」設定では無効です")]
            public string ExcludePath { get; set; }

            [DataMember]
            public PreLoadMode PreLoadMode { get; set; }

            [DataMember, DefaultValue(typeof(Size), "4096,4096")]
            [PropertyMember(Name = "自動先読み判定用画像サイズ", Tips = "自動先読みモードで使用します。この面積より大きい画像で先読みが無効になります\n2ページ表示の場合は2ページの合計面積で判定されます")]
            public Size PreloadLimitSize { get; set; }

            [DataMember, DefaultValue(1.0)]
            [PropertyMember("横長画像を判定するための縦横比(横 / 縦)", Tips = "「横長ページを分割する」で使用されます")]
            public double WideRatio { get; set; }

            [DataMember]
            public bool IsEnableAnimatedGif { get; set; }

            [DataMember]
            public bool IsEnableExif { get; set; }

            [DataMember]
            public bool IsEnableNoSupportFile { get; set; }
        }

        //
        public Memento CreateMemento()
        {
            var memento = new Memento();
            memento.IsPrioritizePageMove = this.IsPrioritizePageMove;
            memento.IsMultiplePageMove = this.IsMultiplePageMove;
            memento.PreLoadMode = this.PreLoadMode;
            memento.PreloadLimitSize = this.PreloadLimitSize;
            memento.WideRatio = this.WideRatio;
            memento.ExcludePath = this.Excludes.ToString();
            memento.IsEnableAnimatedGif = this.IsEnableAnimatedGif;
            memento.IsEnableExif = this.IsEnableExif;
            memento.IsEnableNoSupportFile = this.IsEnableNoSupportFile;
            return memento;
        }

        //
        public void Restore(Memento memento)
        {
            if (memento == null) return;
            this.IsPrioritizePageMove = memento.IsPrioritizePageMove;
            this.IsMultiplePageMove = memento.IsMultiplePageMove;
            this.PreLoadMode = memento.PreLoadMode;
            this.PreloadLimitSize = memento.PreloadLimitSize;
            this.WideRatio = memento.WideRatio;
            this.Excludes.FromString(memento.ExcludePath);
            this.IsEnableAnimatedGif = memento.IsEnableAnimatedGif;
            this.IsEnableExif = memento.IsEnableExif;
            this.IsEnableNoSupportFile = memento.IsEnableNoSupportFile;
        }
        #endregion

    }

}
