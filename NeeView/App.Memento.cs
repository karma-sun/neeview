// Copyright (c) 2016-2018 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NeeView
{
    public partial class App : Application
    {
        // ここでのパラメータは値の保持のみを行う。機能は提供しない。

        #region Fields

        private bool _isNetworkEnalbe = true;

        #endregion

        #region Properties

        // マルチブートを許可する
        [PropertyMember("多重起動を許可する", IsVisible = false)]
        public bool IsMultiBootEnabled { get; set; }

        // フルスクリーン状態を復元する
        [PropertyMember("フルスクリーン状態を復元する", IsVisible = false)]
        public bool IsSaveFullScreen { get; set; }

        // ウィンドウ座標を復元する
        [PropertyMember("ウィンドウ座標を復元する", IsVisible = false)]
        public bool IsSaveWindowPlacement { get; set; }

        // ネットワークアクセス許可
        [PropertyMember("ネットワークアスセス許可", Tips = "ネットワークアクセスを許可します。\n(バージョンウィンドウからのバージョン更新確認、各種WEBリンク)", IsAppxVisible = false)]
        public bool IsNetworkEnabled
        {
            get { return _isNetworkEnalbe; }
            set { _isNetworkEnalbe = Config.Current.IsAppxPackage ? true : value; } // Appxは強制ON
        }

        // 画像のDPI非対応
        [PropertyMember("画像のドットバイドット表示", Tips = "画像をオリジナルサイズで表示する場合にDPIに依存せずにディスプレイのピクセルと一致させます")]
        public bool IsIgnoreImageDpi { get; set; } = true;

        // ウィンドウサイズのDPI非対応
        [PropertyMember("ウィンドウサイズのDPI非対応", Tips = "DPI変更にウィンドウサイズを追従させません")]
        public bool IsIgnoreWindowDpi { get; set; }

        // 複数ウィンドウの座標復元
        [PropertyMember("２つめのウィンドウ座標の復元", Tips = "重複起動される場合にウィンドウ座標の復元を適用する。falseにすると2つめのウィンドウは初期座標で表示されます")]
        public bool IsRestoreSecondWindow { get; set; } = true;

        // 履歴、ブックマーク、ページマークを保存しない
        [PropertyMember("履歴、ブックマーク、ページマークを保存しない", Tips = "履歴、ブックマーク、ページマークの情報がファイルに一切保存されなくなります")]
        public bool IsDisableSave { get; set; }

        // パネルやメニューが自動的に消えるまでの時間(秒)
        [PropertyMember("パネルやメニューが自動的に消えるまでの時間(秒)")]
        public double AutoHideDelayTime { get; set; } = 1.0;

        // ウィンドウクローム枠
        [PropertyEnum("タイトルバー非表示でのウィンドウ枠", Tips = "タイトルバー非表示時のウィンドウ枠表示方法です")]
        public WindowChromeFrame WindowChromeFrame { get; set; } = WindowChromeFrame.Line;

        // 前回開いていたブックを開く
        [PropertyMember("前回開いていたブックを開く", Tips = "起動時に前回開いていたブックを開きます", IsVisible = false)]
        public bool IsOpenLastBook { get; set; }

        // ダウンロードファイル置き場
        [DefaultValue("")]
        [PropertyPath("ダウンロードフォルダ", Tips = "ブラウザ等がらドロップした画像の保存場所です。\n指定がない場合は一時フォルダーが使用されます", IsVisible = false, IsDirectory = true)]
        public string DownloadPath { get; set; }

        #endregion

        #region Memento
        [DataContract]
        public class Memento
        {
            [DataMember, DefaultValue(false)]
            [PropertyMember("多重起動を許可する", IsVisible = false)]
            public bool IsMultiBootEnabled { get; set; }

            [DataMember, DefaultValue(false)]
            [PropertyMember("フルスクリーン状態を復元する", IsVisible = false)]
            public bool IsSaveFullScreen { get; set; }

            [DataMember, DefaultValue(false)]
            [PropertyMember("ウィンドウ座標を復元する", IsVisible = false)]
            public bool IsSaveWindowPlacement { get; set; }

            [DataMember, DefaultValue(true)]
            [PropertyMember("ネットワークアスセス許可", Tips = "ネットワークアクセスを許可します。\n(バージョンウィンドウからのバージョン更新確認、各種WEBリンク)", IsAppxVisible = false)]
            public bool IsNetworkEnabled { get; set; }

            [DataMember, DefaultValue(false)]
            [PropertyMember("履歴、ブックマーク、ページマークを保存しない", Tips = "履歴、ブックマーク、ページマークの情報がファイルに一切保存されなくなります")]
            public bool IsDisableSave { get; set; }

            [DataMember, DefaultValue(true)]
            [PropertyMember("画像のドットバイドット表示", Tips = "画像をオリジナルサイズで表示する場合にDPIに依存せずにディスプレイのピクセルと一致させます")]
            public bool IsIgnoreImageDpi { get; set; }

            [DataMember, DefaultValue(false)]
            [PropertyMember("ウィンドウサイズのDPI非対応", Tips = "DPI変更にウィンドウサイズを追従させません")]
            public bool IsIgnoreWindowDpi { get; set; }

            [DataMember, DefaultValue(true)]
            [PropertyMember("２つめのウィンドウ座標の復元", Tips = "重複起動される場合にウィンドウ座標の復元を適用する。falseにすると2つめのウィンドウは初期座標で表示されます")]
            public bool IsRestoreSecondWindow { get; set; }

            [DataMember, DefaultValue(WindowChromeFrame.Line)]
            [PropertyEnum("タイトルバー非表示でのウィンドウ枠", Tips = "タイトルバー非表示時のウィンドウ枠表示方法です")]
            public WindowChromeFrame WindowChromeFrame { get; set; }

            [DataMember, DefaultValue(1.0)]
            [PropertyMember("パネルやメニューが自動的に消えるまでの時間(秒)")]
            public double AutoHideDelayTime { get; set; }

            [DataMember, DefaultValue(false)]
            [PropertyMember("前回開いていたブックを開く", Tips = "起動時に前回開いていたブックを開きます", IsVisible = false)]
            public bool IsOpenLastBook { get; set; }

            [DataMember, DefaultValue("")]
            [PropertyPath("ダウンロードフォルダ", Tips = "ブラウザ等がらドロップした画像の保存場所です。\n既定では一時フォルダを使用します。", IsVisible = false, IsDirectory = true)]
            public string DownloadPath { get; set; }

            [OnDeserializing]
            private void Deserializing(StreamingContext c)
            {
                this.IsRestoreSecondWindow = true;
            }
        }

        //
        public Memento CreateMemento()
        {
            var memento = new Memento();
            memento.IsMultiBootEnabled = this.IsMultiBootEnabled;
            memento.IsSaveFullScreen = this.IsSaveFullScreen;
            memento.IsSaveWindowPlacement = this.IsSaveWindowPlacement;
            memento.IsNetworkEnabled = this.IsNetworkEnabled;
            memento.IsIgnoreImageDpi = this.IsIgnoreImageDpi;
            memento.IsIgnoreWindowDpi = this.IsIgnoreWindowDpi;
            memento.IsDisableSave = this.IsDisableSave;
            memento.AutoHideDelayTime = this.AutoHideDelayTime;
            memento.WindowChromeFrame = this.WindowChromeFrame;
            memento.IsOpenLastBook = this.IsOpenLastBook;
            memento.DownloadPath = this.DownloadPath;
            memento.IsRestoreSecondWindow = this.IsRestoreSecondWindow;
            return memento;
        }

        //
        public void Restore(Memento memento)
        {
            if (memento == null) return;
            this.IsMultiBootEnabled = memento.IsMultiBootEnabled;
            this.IsSaveFullScreen = memento.IsSaveFullScreen;
            this.IsSaveWindowPlacement = memento.IsSaveWindowPlacement;
            this.IsNetworkEnabled = memento.IsNetworkEnabled;
            this.IsIgnoreImageDpi = memento.IsIgnoreImageDpi;
            this.IsIgnoreWindowDpi = memento.IsIgnoreWindowDpi;
            this.IsDisableSave = memento.IsDisableSave;
            this.AutoHideDelayTime = memento.AutoHideDelayTime;
            this.WindowChromeFrame = memento.WindowChromeFrame;
            this.IsOpenLastBook = memento.IsOpenLastBook;
            this.DownloadPath = memento.DownloadPath;
            this.IsRestoreSecondWindow = memento.IsRestoreSecondWindow;
        }

#pragma warning disable CS0612

        public void RestoreCompatible(Setting setting)
        {
            // compatible before ver.23
            if (setting._Version < Config.GenerateProductVersionNumber(1, 23, 0))
            {
                this.IsMultiBootEnabled = !setting.ViewMemento.IsDisableMultiBoot;
                this.IsSaveFullScreen = setting.ViewMemento.IsSaveFullScreen;
                this.IsSaveWindowPlacement = setting.ViewMemento.IsSaveWindowPlacement;
            }

            // Preferenceの復元 (APP)
            if (setting.PreferenceMemento != null)
            {
                var preference = new Preference();
                preference.Restore(setting.PreferenceMemento);
                preference.RestoreCompatibleApp();
            }
        }

#pragma warning restore CS0612

        #endregion

    }
}
