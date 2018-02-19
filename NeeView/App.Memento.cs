// Copyright (c) 2016-2018 Mitsuhiro Ito (nee)
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
using System.Windows;

namespace NeeView
{
    public partial class App : Application, INotifyPropertyChanged
    {
        // ここでのパラメータは値の保持のみを行う。機能は提供しない。

        #region PropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public void AddPropertyChanged(string propertyName, PropertyChangedEventHandler handler)
        {
            PropertyChanged += (s, e) => { if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == propertyName) handler?.Invoke(s, e); };
        }

        #endregion

        #region Fields

        private bool _isNetworkEnalbe = true;
        private bool _isSaveWindowPlacement;

        #endregion

        #region Properties

        // マルチブートを許可する
        [PropertyMember("多重起動を許可する")]
        public bool IsMultiBootEnabled { get; set; }

        // フルスクリーン状態を復元する
        [PropertyMember("フルスクリーン状態を復元する")]
        public bool IsSaveFullScreen { get; set; }

        // ウィンドウ座標を復元する
        [PropertyMember("ウィンドウ座標を復元する")]
        public bool IsSaveWindowPlacement
        {
            get { return _isSaveWindowPlacement; }
            set { if (_isSaveWindowPlacement != value) { _isSaveWindowPlacement = value; RaisePropertyChanged(); } }
        }

        // ネットワークアクセス許可
        [PropertyMember("ネットワークアスセス許可", Tips = "このアプリでは、「このアプリについて」ダイアログからのバージョン更新確認とオンラインヘルプ等のWEBリンクのみにネットワークを使用します。")]
        public bool IsNetworkEnabled
        {
            get { return _isNetworkEnalbe; }
            set { _isNetworkEnalbe = Config.Current.IsAppxPackage ? true : value; } // Appxは強制ON
        }

        // 画像のDPI非対応
        [PropertyMember("画像のドットバイドット表示", Tips = "オリジナルサイズで表示する場合、DPIに依存せずにディスプレイのピクセルと一致させます。")]
        public bool IsIgnoreImageDpi { get; set; } = true;

        // ウィンドウサイズのDPI非対応
        [PropertyMember("ウィンドウサイズのDPI非対応", Tips = "異なるDPIのディスプレイ間の移動でウィンドウサイズをDPIに追従させません")]
        public bool IsIgnoreWindowDpi { get; set; }

        // 複数ウィンドウの座標復元
        [PropertyMember("2つめのウィンドウ座標の復元", Tips = "重複起動した場合もウィンドウ座標を復元します。OFFにすると初期座標で表示されます。")]
        public bool IsRestoreSecondWindow { get; set; } = true;

        // 履歴、ブックマーク、ページマークを保存しない
        [PropertyMember("履歴、ブックマーク、ページマークをファイル保存しない")]
        public bool IsDisableSave { get; set; }

        // パネルやメニューが自動的に消えるまでの時間(秒)
        [PropertyMember("パネルやメニューが自動的に消えるまでの時間(秒)")]
        public double AutoHideDelayTime { get; set; } = 1.0;

        // ウィンドウクローム枠
        [PropertyMember("タイトルバー非表示でのウィンドウ枠")]
        public WindowChromeFrame WindowChromeFrame { get; set; } = WindowChromeFrame.Line;

        // 前回開いていたブックを開く
        [PropertyMember("開いていたブックを復元する")]
        public bool IsOpenLastBook { get; set; }

        // ダウンロードファイル置き場
        [DefaultValue("")]
        [PropertyPath("ダウンロードフォルダ", Tips = "ブラウザ等がらドロップした画像の保存場所です。指定がない場合は一時フォルダーが使用されます。", IsDirectory = true)]
        public string DownloadPath { get; set; }

        [PropertyMember("ユーザー設定ファイルのバックアップを作る", Tips ="保存データのバックアップを作成します。ファイル名は UserSetting.xaml.old です。更新タイミングは、設定ウィンドウを閉じた時と、アプリを終了した時です。")]
        public bool IsSettingBackup { get; set; } = true;

        #endregion

        #region Memento
        [DataContract]
        public class Memento
        {
            [DataMember, DefaultValue(false)]
            public bool IsMultiBootEnabled { get; set; }

            [DataMember, DefaultValue(false)]
            public bool IsSaveFullScreen { get; set; }

            [DataMember, DefaultValue(false)]
            public bool IsSaveWindowPlacement { get; set; }

            [DataMember, DefaultValue(true)]
            public bool IsNetworkEnabled { get; set; }

            [DataMember, DefaultValue(false)]
            public bool IsDisableSave { get; set; }

            [DataMember, DefaultValue(true)]
            public bool IsIgnoreImageDpi { get; set; }

            [DataMember, DefaultValue(false)]
            public bool IsIgnoreWindowDpi { get; set; }

            [DataMember, DefaultValue(true)]
            public bool IsRestoreSecondWindow { get; set; }

            [DataMember, DefaultValue(WindowChromeFrame.Line)]
            public WindowChromeFrame WindowChromeFrame { get; set; }

            [DataMember, DefaultValue(1.0)]
            public double AutoHideDelayTime { get; set; }

            [DataMember, DefaultValue(false)]
            public bool IsOpenLastBook { get; set; }

            [DataMember, DefaultValue("")]
            public string DownloadPath { get; set; }

            [DataMember, DefaultValue(true)]
            public bool IsSettingBackup { get; set; }

            [OnDeserializing]
            private void Deserializing(StreamingContext c)
            {
                this.IsRestoreSecondWindow = true;
                this.WindowChromeFrame = WindowChromeFrame.Line;
                this.IsSettingBackup = true;
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
            memento.IsSettingBackup = this.IsSettingBackup;
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
            this.IsSettingBackup = memento.IsSettingBackup;
        }

#pragma warning disable CS0612

        public void RestoreCompatible(UserSetting setting)
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
