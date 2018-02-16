// Copyright (c) 2016-2018 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php


using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows;

namespace NeeView
{
    /// <summary>
    /// ページの準備中に表示するもの
    /// </summary>
    public enum LoadingPageView
    {
        [AliasName("なし")]
        None,

        [AliasName("直前のページのサムネイル")]
        PreThumbnail,

        [AliasName("直前のページの画像")]
        PreImage,
    }


    /// <summary>
    /// 本：設定
    /// </summary>
    public class BookProfile
    {
        public static BookProfile Current { get; private set; }

        #region Constructors

        public BookProfile()
        {
            Current = this;
        }

        #endregion

        #region Properties

        /// <summary>
        /// ページ移動優先設定
        /// </summary>
        [PropertyMember("ページ移動優先", Tips = "ページの表示を待たずにページ移動を実行します。")]
        public bool IsPrioritizePageMove { get; set; } = true;

        /// <summary>
        /// ページ移動命令重複許可
        /// </summary>
        [PropertyMember("ページ移動コマンドの重複許可", Tips = "発行されたページ移動コマンドを全て実行します。OFFにすると重複したページ移動コマンドはキャンセルされます。")]
        public bool IsMultiplePageMove { get; set; } = true;

        /// <summary>
        /// 先読みモード
        /// </summary>
        [PropertyMember("先読み", Tips = "先読みは前後のページを保持するためメモリを消費します。「自動先読み」にすると画像サイズに応じで先読み有効無効を切り替えます。「先読みする(開放なし)」は読み込んだ画像を破棄しない最もメモリを消費するモードです。")]
        public PreLoadMode PreLoadMode { get; set; } = PreLoadMode.AutoPreLoad;

        /// <summary>
        /// 先読み自動判定許サイズ
        /// </summary>
        [PropertyMember("自動先読み判定用画像サイズ", Tips = "自動先読みモードで使用します。この面積より大きい画像で先読みが無効になります。2ページ表示の場合は2ページの合計面積で判定されます。")]
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
        [PropertyMember("横長画像を判定するための縦横比(横 / 縦)", Tips = "「横長ページを分割する」で使用されます。")]
        public double WideRatio { get; set; } = 1.0;

        /// <summary>
        /// 除外パス
        /// </summary>
        [PropertyMember("ページ除外パス", Tips = "「サポート外ファイルもページに含める」設定では無効です。")]
        public StringCollection Excludes { get; set; } = new StringCollection("__MACOSX;.DS_Store");

        // GIFアニメ有効
        [PropertyMember("アニメーションGIFを再生する", Tips = "アニメーションGIF再生を行います。長時間のGIFでメモリ消費の問題が発生する可能性があります。")]
        public bool IsEnableAnimatedGif { get; set; }

        // サポート外ファイル有効
        [PropertyMember("サポート外ファイルもページに含める", Tips = "画像として表示できないファイルやフォルダーもページとして表示します。")]
        public bool IsEnableNoSupportFile { get; set; }

        // ページ読み込み中表示
        [PropertyMember("読み込み中ページの表示方法", Tips = "ページの読み込みが完了するまでに表示するものを指定します。「直前のページの画像」が一番メモリを消費します。")]
        public LoadingPageView LoadingPageView { get; set; } = LoadingPageView.PreThumbnail;

        #endregion

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
        public bool CanMultiplePageMove()
        {
            return this.IsMultiplePageMove && !SlideShow.Current.IsPlayingSlideShow;
        }

        // 除外パス判定
        public bool IsExcludedPath(string path)
        {
            return path.Split('/', '\\').Any(e => this.Excludes.Contains(e));
        }


        #region Memento
        [DataContract]
        public class Memento
        {
            [DataMember, DefaultValue(true)]
            public bool IsPrioritizePageMove { get; set; }

            [DataMember, DefaultValue(true)]
            public bool IsMultiplePageMove { get; set; }

            [DataMember, DefaultValue("__MACOSX;.DS_Store")]
            public string ExcludePath { get; set; }

            [DataMember, DefaultValue(1.0)]
            public PreLoadMode PreLoadMode { get; set; }

            [DataMember, DefaultValue(typeof(Size), "4096,4096")]
            public Size PreloadLimitSize { get; set; }

            [DataMember, DefaultValue(1.0)]
            public double WideRatio { get; set; }

            [DataMember, DefaultValue(false)]
            public bool IsEnableAnimatedGif { get; set; }

            [DataMember, DefaultValue(false)]
            public bool IsEnableNoSupportFile { get; set; }

            [DataMember, DefaultValue(LoadingPageView.PreThumbnail)]
            public LoadingPageView LoadingPageView { get; set; }


            [OnDeserializing]
            private void OnDeserializing(StreamingContext c)
            {
                LoadingPageView = LoadingPageView.PreThumbnail;
            }
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
            memento.IsEnableNoSupportFile = this.IsEnableNoSupportFile;
            memento.LoadingPageView = this.LoadingPageView;
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
            this.IsEnableNoSupportFile = memento.IsEnableNoSupportFile;
            this.LoadingPageView = memento.LoadingPageView;
        }
        #endregion

    }

}
