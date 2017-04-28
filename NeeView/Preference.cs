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
    /// 保存される静的設定情報。
    /// アプリの動的内部情報は<see cref="AppContext"/>で。 
    /// </summary>
    public class Preference
    {
        /// <summary>
        /// 現在のシステムオブジェクト
        /// </summary>
        public static Preference _current;
        public static Preference Current
        {
            get
            {
                _current = _current ?? new Preference();
                return _current;
            }
        }

        /// <summary>
        /// Properties
        /// </summary>

        [DataMember, DefaultValue(true)]
        [PropertyMember()]
        public bool _configure_enabled { get; set; }


        [DataMember, DefaultValue(false)]
        [PropertyMember("「開く」を現在開いているフォルダーの場所から始める"
            , Tips = "[ファイル] >[開く]で開くフォルダーです\nドラッグ＆ドロップや履歴から開いた場所も基準になります")]
        public bool openbook_begin_current { get; set; }

        [DataMember, DefaultValue(2)]
        [PropertyMember("画像読み込みに使用するスレッド数", Tips = "有効値は1～4です")]
        public int loader_thread_size { get; set; }

        [DataMember, DefaultValue(true)]
        [PropertyMember("画像のDPI非対応", Tips = "画像をオリジナルサイズで表示する場合にディスプレイのピクセルと一致させます")]
        public bool dpi_image_ignore { get; set; }

        [DataMember, DefaultValue(false)]
        [PropertyMember("ウィンドウサイズのDPI非対応", Tips = "DPI変更にウィンドウサイズを追従させません")]
        public bool dpi_window_ignore { get; set; }

        [DataMember, DefaultValue(1.0)]
        [PropertyMember("横長画像を判定するための縦横比(横 / 縦)", Tips = "「横長ページを分割する」で使用されます")]
        public double view_image_wideratio { get; set; }

        [DataMember, DefaultValue(false)]
        [PropertyMember("履歴、ブックマーク、ページマークを保存しない", Tips = "履歴、ブックマーク、ページマークの情報がファイルに一切保存されなくなります")]
        public bool userdata_save_disable { get; set; }

        [DataMember, DefaultValue(30.0)]
        [PropertyMember("マウスジェスチャー判定の最小移動距離(X)", Tips = "この距離(pixel)移動して初めてジェスチャー開始と判定されます")]
        public double input_gesture_minimumdistance_x { get; set; }

        [DataMember, DefaultValue(30.0)]
        [PropertyMember("マウスジェスチャー判定の最小移動距離(Y)", Tips = "この距離(pixel)移動して初めてジェスチャー開始と判定されます")]
        public double input_gesture_minimumdistance_y { get; set; }

        [DataMember, DefaultValue(1.0)]
        [PropertyMember("パネルが自動的に消えるまでの時間(秒)")]
        public double panel_autohide_delaytime { get; set; }

        [DataMember, DefaultValue(true)]
        [PropertyMember("ページ送り優先", Tips = "ページの表示を待たずにページ送りを実行します"
            , Flags = PropertyMemberFlag.None)]
        public bool book_is_prioritize_pagemove { get; set; }

        [DataMember, DefaultValue(true)]
        [PropertyMember("ページ送りコマンドの重複許可", Tips = "発行されたページ移動コマンドを全て実行します。\nFalseの場合は重複したページ送りコマンドはキャンセルされます"
            , Flags = PropertyMemberFlag.None)]
        public bool book_allow_multiple_pagemove { get; set; }

        [DataMember, DefaultValue("4096x4096")]
        [PropertyMember(Name = "自動先読み判定用画像サイズ", Tips = "自動先読みモードで使用します。この面積より大きい画像で先読みが無効になります\n\"数値x数値\"で指定します")]
        public string book_preload_limitsize { get; set; }

        [DataMember, DefaultValue("")]
        [PropertyPath(Name = "7z.dll(32bit)の場所", Tips = "別の7z.dllを使用したい場合に設定します。反映にはアプリを開き直す必要があります")]
        public string loader_archiver_7z_dllpath { get; set; }

        [DataMember, DefaultValue("")]
        [PropertyPath(Name = "7z.dll(64bit)の場所", Tips = "別の7z.dllを使用したい場合に設定します。反映にはアプリを開き直す必要があります")]
        public string loader_archiver_7z_dllpath_x64 { get; set; }

        [DataMember, DefaultValue(".7z;.rar;.lzh")]
        [PropertyMember("7z.dllで展開する圧縮ファイルの拡張子", Tips = ";(セミコロン)区切りでサポートする拡張子を羅列します。\n拡張子は .zip のように指定します")]
        public string loader_archiver_7z_supprtfiletypes { get; set; }

        [DataMember, DefaultValue("__MACOSX;.DS_Store")]
        [PropertyMember("ページ除外パス", Tips = ";(セミコロン)区切りで除外するパス名を羅列します。「サポート外ファイルもページに含める」設定では無効です")]
        public string loader_archiver_exclude { get; set; }

        [DataMember, DefaultValue(-1.0)]
        [PropertyMember("7z.dllがファイルをロックする時間(秒)", Tips = "この時間アクセスがなければロック解除さます。\n-1でロック保持したままになります")]
        public double loader_archiver_7z_locktime { get; set; }

        [DataMember, DefaultValue(80)]
        [PropertyMember("サムネイル品質", Tips = "サムネイルのJpeg品質です。1-100で指定します")]
        public int thumbnail_quality { get; set; }

        [DataMember, DefaultValue(true)]
        [PropertyMember("サムネイルキャッシュを使用する", Tips = "フォルダーサムネイルをキャッシュします。キャッシュファイルはCache.dbです")]
        public bool thumbnail_cache { get; set; }

        [DataMember, DefaultValue(1000)]
        [PropertyMember("ページサムネイル容量", Tips = "ページサムネイル保持枚数です。フォルダーを閉じると全てクリアされます")]
        public int thumbnail_book_capacity { get; set; }

        [DataMember, DefaultValue(200)]
        [PropertyMember("フォルダーサムネイル容量", Tips = "フォルダーリスト等でのサムネイル保持枚数です")]
        public int thumbnail_folder_capacity { get; set; }

        [DataMember, DefaultValue(true)]
        [PropertyMember("フォルダーリスト追加ファイルは挿入", Tips = "フォルダーリストで追加されたファイルを現在のソート順で挿入します。\nFalseのときはリストの終端に追加します。")]
        public bool folderlist_addfile_insert { get; set; }
        
        [DataMember, DefaultValue(2.0)]
        [PropertyMember("ルーペ標準倍率", Tips = "ルーペの初期倍率です")]
        public double loupe_scale_default { get; set; }

        [DataMember, DefaultValue(2.0)]
        [PropertyMember("ルーペ最小倍率", Tips = "ルーペの最小倍率です")]
        public double loupe_scale_min { get; set; }

        [DataMember, DefaultValue(10.0)]
        [PropertyMember("ルーペ最大倍率", Tips = "ルーペの最大倍率です")]
        public double loupe_scale_max { get; set; }

        [DataMember, DefaultValue(false)]
        [PropertyMember("ルーペ倍率リセット", Tips = "ルーペを開始するたびに標準倍率に戻します")]
        public bool loupe_scale_reset { get; set; }

        [DataMember, DefaultValue(true)]
        [PropertyMember("ファイル削除確認", Tips = "ファイル削除時に確認ダイアログを表示します")]
        public bool file_remove_confirm { get; set; }

        [DataMember, DefaultValue(true)]
        [PropertyMember("ファイル操作許可", Tips = "削除や名前変更等のファイル操作コマンドを使用可能にします"
            , Flags = PropertyMemberFlag.None)]
        public bool file_permit_command { get; set; }

        [DataMember, DefaultValue(true)]
        [PropertyMember("ネットワークアスセス許可", Tips = "ネットワークアクセスを許可します。\n(バージョンウィンドウからのバージョン更新確認、各種WEBリンク)")]
        public bool network_enabled { get; set; }

        [DataMember, DefaultValue(true)]
        [PropertyMember("タイトルバー非表示でウィンドウ枠非表示", Tips = "タイトルバー非表示時にウィンドウ枠も完全に非表示にします")]
        public bool window_chrome { get; set; }

        [DataMember, DefaultValue(true)]
        [PropertyMember("フルスクリーン時のタイトルバー操作", Tips = "フルスクリーン時のメニュー上でのタイトルバー操作(ダブルクリックやドラッグ)を有効にします")]
        public bool window_captionemunate_fullscreen { get; set; }


        /// <summary>
        /// 
        /// </summary>
        public PropertyDocument Document { get; private set; }


        /// <summary>
        /// 
        /// </summary>
        public Preference()
        {
            Document = PropertyDocument.Create(this);
        }

        /// <summary>
        /// 全ての設定値を初期化
        /// </summary>
        public void Reset(bool isAll)
        {
            foreach (var item in Document.Elements.OfType<PropertyMemberElement>())
            {
                if (isAll || item.Flags.HasFlag(PropertyMemberFlag.Details))
                {
                    item.ResetValue();
                }
            }
        }


        /// <summary>
        /// 正規化
        /// </summary>
        public void Validate()
        {
            // loader_preload_limitsize
            var sizeString = new SizeString(book_preload_limitsize);
            if (!sizeString.IsValid())
            {
                var element = Document.GetPropertyMember(nameof(book_preload_limitsize));
                element.ResetValue();
            }

            // thumbnail_quality
            thumbnail_quality = NVUtility.Clamp(thumbnail_quality, 1, 100);

            // loupe_scale_min
            //loupe_scale_min = Math.Max(loupe_scale_min, 1.0);

            // loupe_scale_max
            loupe_scale_max = Math.Max(loupe_scale_max, loupe_scale_min);

            // loupe_scale
            loupe_scale_default = NVUtility.Clamp(loupe_scale_default, loupe_scale_min, loupe_scale_max);
        }



        /// <summary>
        /// 旧設定(Configurationファイル)のパラメータとの対応表
        /// </summary>
        private Dictionary<string, string> _configurationAppSettingTable = new Dictionary<string, string>()
        {
            ["GestureMinimumDistanceX"] = "input_gesture_minimumdistance_x",
            ["GestureMinimumDistanceY"] = "input_gesture_minimumdistance_y",
            ["PanelHideDelayTime"] = "panel_autohide_delaytime",
            ["SevenZipSupportFileType"] = "loader_archiver_7z_supprtfiletypes",
            ["ThreadSize"] = "loader_thread_size",
            ["WideRatio"] = "view_image_wideratio",
        };

        /// <summary>
        /// 旧設定(Configureファイル)からの読込。互換用
        /// </summary>
        public void LoadFromConfiguration()
        {
            // Configureファイルからの読込は最初の１度だけ
            if (!_configure_enabled) return;
            _configure_enabled = false;

            // configureファイルから読込
            foreach (var pair in _configurationAppSettingTable)
            {
                string value = System.Configuration.ConfigurationManager.AppSettings.Get(pair.Key);
                if (value != null)
                {
                    Document.GetPropertyMember(pair.Value)?.SetValue(value);
                }
            }
        }


        #region Memento

        [DataContract]
        public class Memento
        {
            [DataMember]
            public Dictionary<string, string> Items { get; set; }

            private void Constructor()
            {
                Items = new Dictionary<string, string>();
            }

            public Memento()
            {
                Constructor();
            }

            [OnDeserializing]
            private void Deserializing(StreamingContext c)
            {
                Constructor();
            }

            public void Add(string key, string value)
            {
                Items[key] = value;
            }
        }

        /// <summary>
        /// Memento作成
        /// </summary>
        /// <returns></returns>
        public Memento CreateMemento()
        {
            var memento = new Memento();

            foreach (var item in Document.Elements.OfType<PropertyMemberElement>())
            {
                if (item.HasCustomValue)
                {
                    var key = item.Path.Replace('_', '.');
                    var value = item.GetValue().ToString();
                    memento.Items.Add(key, value);
                }
            }

            return memento;
        }

        /// <summary>
        /// Memento適用
        /// </summary>
        /// <param name="memento"></param>
        public void Restore(Memento memento)
        {
            this.Reset(true);

            foreach (var item in memento.Items)
            {
                try
                {
                    var path = item.Key.Replace('.', '_');
                    var element = Document.GetPropertyMember(path);
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

            LoadFromConfiguration();
        }

        #endregion
    }
}
