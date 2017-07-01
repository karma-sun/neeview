// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace NeeView
{
    /// <summary>
    /// データ互換用
    /// </summary>
    [Obsolete]
    public class Preference
    {
        /// <summary>
        /// Properties
        /// </summary>

        public int _Version { get; set; } = Config.Current.ProductVersionNumber;

        [DataMember, DefaultValue(true)]
        [PropertyMember()]
        public bool _configure_enabled { get; set; }

        [Obsolete]
        [DataMember, DefaultValue(false)]
        [PropertyMember("「開く」を現在開いているブックの場所から始める", Tips = "[ファイル] >[開く]で開くフォルダーです\nドラッグ＆ドロップや履歴から開いた場所も基準になります")]
        public bool openbook_begin_current { get; set; }

        [Obsolete]
        [DataMember, DefaultValue(2)]
        [PropertyMember("画像読み込みに使用するスレッド数", Tips = "有効値は1～4です")]
        public int loader_thread_size { get; set; }

        [Obsolete]
        [DataMember, DefaultValue(true)]
        [PropertyMember("画像のDPI非対応", Tips = "画像をオリジナルサイズで表示する場合にディスプレイのピクセルと一致させます")]
        public bool dpi_image_ignore { get; set; }

        [Obsolete]
        [DataMember, DefaultValue(false)]
        [PropertyMember("ウィンドウサイズのDPI非対応", Tips = "DPI変更にウィンドウサイズを追従させません")]
        public bool dpi_window_ignore { get; set; }

        [Obsolete]
        [DataMember, DefaultValue(1.0)]
        [PropertyMember("横長画像を判定するための縦横比(横 / 縦)", Tips = "「横長ページを分割する」で使用されます")]
        public double view_image_wideratio { get; set; }

        [Obsolete]
        [DataMember, DefaultValue(false)]
        [PropertyMember("履歴、ブックマーク、ページマークを保存しない", Tips = "履歴、ブックマーク、ページマークの情報がファイルに一切保存されなくなります")]
        public bool userdata_save_disable { get; set; }

        [Obsolete]
        [DataMember, DefaultValue(30.0)]
        [PropertyMember("マウスジェスチャー判定の最小移動距離(X)", Tips = "この距離(pixel)移動して初めてジェスチャー開始と判定されます")]
        public double input_gesture_minimumdistance_x { get; set; }

        [Obsolete]
        [DataMember, DefaultValue(30.0)]
        [PropertyMember("マウスジェスチャー判定の最小移動距離(Y)", Tips = "この距離(pixel)移動して初めてジェスチャー開始と判定されます")]
        public double input_gesture_minimumdistance_y { get; set; }

        [Obsolete]
        [DataMember, DefaultValue(1.0)]
        [PropertyMember("パネルが自動的に消えるまでの時間(秒)")]
        public double panel_autohide_delaytime { get; set; }

        [Obsolete]
        [DataMember, DefaultValue(true)]
        [PropertyMember("ページ送り優先", Tips = "ページの表示を待たずにページ送りを実行します")]
        public bool book_is_prioritize_pagemove { get; set; }

        [Obsolete]
        [DataMember, DefaultValue(true)]
        [PropertyMember("ページ送りコマンドの重複許可", Tips = "発行されたページ移動コマンドを全て実行します。\nFalseの場合は重複したページ送りコマンドはキャンセルされます")]
        public bool book_allow_multiple_pagemove { get; set; }

        [Obsolete]
        [DataMember, DefaultValue("4096x4096")]
        [PropertyMember(Name = "自動先読み判定用画像サイズ", Tips = "自動先読みモードで使用します。この面積より大きい画像で先読みが無効になります\n\"数値x数値\"で指定します")]
        public string book_preload_limitsize { get; set; }

        [Obsolete]
        [DataMember, DefaultValue("")]
        [PropertyPath(Name = "7z.dll(32bit)の場所", Tips = "別の7z.dllを使用したい場合に設定します。反映にはアプリを開き直す必要があります")]
        public string loader_archiver_7z_dllpath { get; set; }

        [Obsolete]
        [DataMember, DefaultValue("")]
        [PropertyPath(Name = "7z.dll(64bit)の場所", Tips = "別の7z.dllを使用したい場合に設定します。反映にはアプリを開き直す必要があります")]
        public string loader_archiver_7z_dllpath_x64 { get; set; }

        [Obsolete]
        [DataMember, DefaultValue(".7z;.rar;.lzh")]
        [PropertyMember("7z.dllで展開する圧縮ファイルの拡張子", Tips = ";(セミコロン)区切りでサポートする拡張子を羅列します。\n拡張子は .zip のように指定します")]
        public string loader_archiver_7z_supprtfiletypes { get; set; }

        [Obsolete]
        [DataMember, DefaultValue("__MACOSX;.DS_Store")]
        [PropertyMember("ページ除外パス", Tips = ";(セミコロン)区切りで除外するパス名を羅列します。「サポート外ファイルもページに含める」設定では無効です")]
        public string loader_archiver_exclude { get; set; }

        [Obsolete]
        [DataMember, DefaultValue(-1.0)]
        [PropertyMember("7z.dllがファイルをロックする時間(秒)", Tips = "この時間アクセスがなければロック解除さます。\n-1でロック保持したままになります")]
        public double loader_archiver_7z_locktime { get; set; }

        [Obsolete]
        [DataMember, DefaultValue(80)]
        [PropertyMember("サムネイル品質", Tips = "サムネイルのJpeg品質です。1-100で指定します")]
        public int thumbnail_quality { get; set; }

        [Obsolete]
        [DataMember, DefaultValue(true)]
        [PropertyMember("サムネイルキャッシュを使用する", Tips = "ブックサムネイルをキャッシュします。キャッシュファイルはCache.dbです")]
        public bool thumbnail_cache { get; set; }

        [Obsolete]
        [DataMember, DefaultValue(1000)]
        [PropertyMember("ページサムネイル容量", Tips = "ページサムネイル保持枚数です。ブックを閉じると全てクリアされます")]
        public int thumbnail_book_capacity { get; set; }

        [Obsolete]
        [DataMember, DefaultValue(200)]
        [PropertyMember("ブックサムネイル容量", Tips = "フォルダーリスト等でのサムネイル保持枚数です")]
        public int thumbnail_folder_capacity { get; set; }

        [Obsolete]
        [DataMember, DefaultValue(true)]
        [PropertyMember("フォルダーリスト追加ファイルは挿入", Tips = "フォルダーリストで追加されたファイルを現在のソート順で挿入します。\nFalseのときはリストの終端に追加します。")]
        public bool folderlist_addfile_insert { get; set; }

        [Obsolete]
        [DataMember, DefaultValue(2.0)]
        [PropertyMember("ルーペ標準倍率", Tips = "ルーペの初期倍率です")]
        public double loupe_scale_default { get; set; }

        [Obsolete]
        [DataMember, DefaultValue(2.0)]
        [PropertyMember("ルーペ最小倍率", Tips = "ルーペの最小倍率です")]
        public double loupe_scale_min { get; set; }

        [Obsolete]
        [DataMember, DefaultValue(10.0)]
        [PropertyMember("ルーペ最大倍率", Tips = "ルーペの最大倍率です")]
        public double loupe_scale_max { get; set; }

        [Obsolete]
        [DataMember, DefaultValue(1.0)]
        [PropertyMember("ルーペ倍率変化単位", Tips = "ルーペ倍率をこの値で変化させます")]
        public double loupe_scale_step { get; set; }

        [Obsolete]
        [DataMember, DefaultValue(false)]
        [PropertyMember("ルーペ倍率リセット", Tips = "ルーペを開始するたびに標準倍率に戻します")]
        public bool loupe_scale_reset { get; set; }

        [Obsolete]
        [DataMember, DefaultValue(true)]
        [PropertyMember("ルーペページ切り替え解除", Tips = "ページを切り替えるとルーペを解除します")]
        public bool loupe_pagechange_reset { get; set; }


        [Obsolete]
        [DataMember, DefaultValue(true)]
        [PropertyMember("ファイル削除確認", Tips = "ファイル削除時に確認ダイアログを表示します")]
        public bool file_remove_confirm { get; set; }

        [Obsolete]
        [DataMember, DefaultValue(true)]
        [PropertyMember("ファイル操作許可", Tips = "削除や名前変更等のファイル操作コマンドを使用可能にします")]
        public bool file_permit_command { get; set; }

        [Obsolete]
        [DataMember, DefaultValue(true)]
        [PropertyMember("ネットワークアスセス許可(OLD)", Tips = "ネットワークアクセスを許可します。\n(バージョンウィンドウからのバージョン更新確認、各種WEBリンク)")]
        public bool network_enabled { get; set; }

        [Obsolete]
        [DataMember, DefaultValue(WindowChromeFrame.Line)]
        [PropertyEnum("タイトルバー非表示でのウィンドウ枠", Tips = "タイトルバー非表示時のウィンドウ枠表示方法です")]
        public WindowChromeFrame window_chrome_frame { get; set; }

        [Obsolete]
        [DataMember, DefaultValue(false)]
        [PropertyMember("フルスクリーン時のタイトルバー操作", Tips = "フルスクリーン時のメニュー上でのタイトルバー操作(ダブルクリックやドラッグ)を有効にします")]
        public bool window_captionemunate_fullscreen { get; set; }

        [Obsolete]
        [DataMember, DefaultValue(1.0)]
        [PropertyMember("長押し判定時間(秒)", Tips = "長押しの判定時間です")]
        public double input_longbuttondown_time { get; set; }

        [Obsolete]
        [DataMember, DefaultValue(200)]
        [PropertyMember("バナーサイズ", Tips = "バナーの横幅です。縦幅は横幅の1/4になります。\nサムネイル画像を流用しているため、大きいサイズほど画像が荒くなります")]
        public int banner_width { get; set; }

        [Obsolete]
        [DataMember, DefaultValue(false)]
        [PropertyMember("前回開いていたブックを開く", Tips = "起動時に前回開いていたブックを開きます")]
        public bool bootup_lastfolder { get; set; }



        /// <summary>
        /// 
        /// </summary>
        public Preference()
        {
            _document = new PropertyDocument(this);
        }

        private PropertyDocument _document;



        #region Memento

        [DataContract]
        public class Memento
        {
            [DataMember]
            public int _Version { get; set; } = Config.Current.ProductVersionNumber;

            [DataMember]
            public Dictionary<string, string> Items { get; set; } = new Dictionary<string, string>();

            public void Add(string key, string value)
            {
                Items[key] = value;
            }
        }


        /// <summary>
        /// Memento適用
        /// </summary>
        /// <param name="memento"></param>
        public void Restore(Memento memento)
        {
            if (memento == null) return;

            this._document.Reset();

            this._Version = memento._Version;

            if (memento.Items != null)
            {
                foreach (var item in memento.Items)
                {
                    try
                    {
                        var path = item.Key.Replace('.', '_');
                        var element = _document.GetPropertyMember(path);
                        if (element != null)
                        {
                            element.SetValueFromString(item.Value);
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e.Message);
                    }
                }
            }
        }

        // Appへの適用のみ
        public void RestoreCompatibleApp()
        {
            // compatible before ver.23
            if (_Version < Config.GenerateProductVersionNumber(1, 23, 0))
            {
                App.Current.IsNetworkEnabled = this.network_enabled;
                App.Current.IsIgnoreImageDpi = this.dpi_image_ignore;
                App.Current.IsIgnoreWindowDpi = this.dpi_window_ignore;
                App.Current.IsDisableSave = this.userdata_save_disable;
                App.Current.AutoHideDelayTime = this.panel_autohide_delaytime;
                App.Current.WindowChromeFrame = this.window_chrome_frame;
                App.Current.IsOpenLastBook = this.bootup_lastfolder;
            }
        }

        // App以外への適用のみ
        public void RestoreCompatible()
        {
            // compatible before ver.23
            if (_Version < Config.GenerateProductVersionNumber(1, 23, 0))
            {
                FileIOProfile.Current.IsRemoveConfirmed = this.file_remove_confirm;
                FileIOProfile.Current.IsEnabled = this.file_permit_command;

                JobEngine.Current.WorkerSize = this.loader_thread_size;

                SevenZipArchiverProfile.Current.X86DllPath = this.loader_archiver_7z_dllpath;
                SevenZipArchiverProfile.Current.X64DllPath = this.loader_archiver_7z_dllpath_x64;
                SevenZipArchiverProfile.Current.SupportFileTypes.FromString(this.loader_archiver_7z_supprtfiletypes);
                SevenZipArchiverProfile.Current.SupportFileTypes.AddString(".cbr");
                SevenZipArchiverProfile.Current.SupportFileTypes.AddString(".cbz");
                SevenZipArchiverProfile.Current.LockTime = this.loader_archiver_7z_locktime;

                ThumbnailProfile.Current.Quality = this.thumbnail_quality;
                ThumbnailProfile.Current.IsCacheEnabled = this.thumbnail_cache;
                ThumbnailProfile.Current.PageCapacity = this.thumbnail_book_capacity;
                ThumbnailProfile.Current.BookCapacity = this.thumbnail_folder_capacity;
                ThumbnailProfile.Current.BannerWidth = this.banner_width;

                MainWindowModel.Current.IsOpenbookAtCurrentPlace = this.openbook_begin_current;

                BookProfile.Current.IsPrioritizePageMove = this.book_is_prioritize_pagemove;
                BookProfile.Current.IsMultiplePageMove = this.book_allow_multiple_pagemove;
                BookProfile.Current.PreloadLimitSize = new SizeString(this.book_preload_limitsize ?? "4096x4096").ToSize();
                BookProfile.Current.WideRatio = this.view_image_wideratio;
                BookProfile.Current.Excludes.FromString(this.loader_archiver_exclude);

                MouseInput.Current.Normal.LongLeftButtonDownTime = this.input_longbuttondown_time;

                MouseInput.Current.Gesture.GestureMinimumDistanceX = this.input_gesture_minimumdistance_x;
                MouseInput.Current.Gesture.GestureMinimumDistanceY = this.input_gesture_minimumdistance_y;

                MouseInput.Current.Loupe.MinimumScale = this.loupe_scale_min;
                MouseInput.Current.Loupe.MaximumScale = this.loupe_scale_max;
                MouseInput.Current.Loupe.DefaultScale = this.loupe_scale_default;
                MouseInput.Current.Loupe.ScaleStep = this.loupe_scale_step;
                MouseInput.Current.Loupe.IsResetByRestart = this.loupe_scale_reset;
                MouseInput.Current.Loupe.IsResetByPageChanged = this.loupe_pagechange_reset;

                MenuBar.Current.IsCaptionEmulateInFullScreen = this.window_captionemunate_fullscreen;

                FolderList.Current.IsInsertItem = this.folderlist_addfile_insert;
            }
        }

        #endregion
    }
}
