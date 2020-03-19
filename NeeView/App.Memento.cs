using NeeLaboratory.ComponentModel;
using NeeView.Windows.Controls;
using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
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

        #region INotifyPropertyChanged Support

        public event PropertyChangedEventHandler PropertyChanged;

        protected bool SetProperty<T>(ref T storage, T value, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            if (object.Equals(storage, value)) return false;
            storage = value;
            this.RaisePropertyChanged(propertyName);
            return true;
        }

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

        ////private bool _isNetworkEnalbe = true;
        private bool _isSettingBackup;
        ////private bool _isSaveWindowPlacement = true;
        private double _autoHideDelayTime = 1.0;
        private double _autoHideDelayVisibleTime = 0.0;
        private string _temporaryDirectory;
        private string _cacheDirectory;
        private string _cacheDirectoryOld;
        ////private bool _isSaveHistory = true;
        ////private string _historyFilePath;
        ////private bool _isSaveBookmark = true;
        ////private string _bookmarkFilePath;
        ////private bool _isSavePagemark = true;
        ////private string _pagemarkFilePath;
        private AutoHideFocusLockMode _autoHideFocusLockMode = AutoHideFocusLockMode.LogicalTextBoxFocusLock;
        private bool _isAutoHideKeyDownDelay = true;
        private double _autoHideHitTestMargin = 32.0;

        #endregion

        #region Properties

        // 適用した設定データのバージョン
        public int SettingVersion { get; set; }

#if false
        // 多重起動を許可する
        [PropertyMember("@ParamIsMultiBootEnabled")]
        public bool IsMultiBootEnabled { get; set; }

        // フルスクリーン状態を復元する
        [PropertyMember("@ParamIsSaveFullScreen")]
        public bool IsSaveFullScreen { get; set; }

        // ウィンドウ座標を復元する
        [PropertyMember("@ParamIsSaveWindowPlacement")]
        public bool IsSaveWindowPlacement
        {
            get { return _isSaveWindowPlacement; }
            set { if (_isSaveWindowPlacement != value) { _isSaveWindowPlacement = value; RaisePropertyChanged(); } }
        }

        // ネットワークアクセス許可
        [PropertyMember("@ParamIsNetworkEnabled", Tips = "@ParamIsNetworkEnabledTips")]
        public bool IsNetworkEnabled
        {
            get { return _isNetworkEnalbe || Environment.IsAppxPackage; } // Appxは強制ON
            set { if (_isNetworkEnalbe != value) { _isNetworkEnalbe = value; RaisePropertyChanged(); } }
        }

        // 複数ウィンドウの座標復元
        [PropertyMember("@ParamIsRestoreSecondWindow", Tips = "@ParamIsRestoreSecondWindowTips")]
        public bool IsRestoreSecondWindow { get; set; } = true;

        // 履歴データの保存
        [PropertyMember("@ParamIsSaveHistory")]
        public bool IsSaveHistory
        {
            get { return _isSaveHistory; }
            set { SetProperty(ref _isSaveHistory, value); }
        }

        // 履歴データの保存場所
        [PropertyPath("@ParamHistoryFilePath", FileDialogType = FileDialogType.SaveFile, Filter = "XML|*.xml")]
        public string HistoryFilePath
        {
            get => _historyFilePath ?? SaveData.DefaultHistoryFilePath;
            set => _historyFilePath = string.IsNullOrWhiteSpace(value) || value == SaveData.DefaultHistoryFilePath ? null : value;
        }
        // ブックマークの保存
        [PropertyMember("@ParamIsSaveBookmark")]
        public bool IsSaveBookmark
        {
            get { return _isSaveBookmark; }
            set { SetProperty(ref _isSaveBookmark, value); }
        }

        // ブックマークの保存場所
        [PropertyPath("@ParamBookmarkFilePath", FileDialogType = FileDialogType.SaveFile, Filter = "XML|*.xml")]
        public string BookmarkFilePath
        {
            get => _bookmarkFilePath ?? SaveData.DefaultBookmarkFilePath;
            set => _bookmarkFilePath = string.IsNullOrWhiteSpace(value) || value == SaveData.DefaultBookmarkFilePath ? null : value;
        }

        // ページマークの保存
        [PropertyMember("@ParamIsSavePagemark")]
        public bool IsSavePagemark
        {
            get { return _isSavePagemark; }
            set { SetProperty(ref _isSavePagemark, value); }
        }

        // ページマークの保存場所
        [PropertyPath("@ParamPagemarkFilePath", FileDialogType = FileDialogType.SaveFile, Filter = "XML|*.xml")]
        public string PagemarkFilePath
        {
            get => _pagemarkFilePath ?? SaveData.DefaultPagemarkFilePath;
            set => _pagemarkFilePath = string.IsNullOrWhiteSpace(value) || value == SaveData.DefaultPagemarkFilePath ? null : value;
        }

#endif

        // 画像のDPI非対応
        [PropertyMember("@ParamIsIgnoreImageDpi", Tips = "@ParamIsIgnoreImageDpiTips")]
        public bool IsIgnoreImageDpi { get; set; } = true;



        // パネルやメニューが自動的に消えるまでの時間(秒)
        [PropertyMember("@ParamAutoHideDelayTime")]
        public double AutoHideDelayTime
        {
            get { return _autoHideDelayTime; }
            set { if (SetProperty(ref _autoHideDelayTime, value)) { RaisePropertyChanged(nameof(AutoHideDelayTimeMillisecond)); } }
        }

        // パネルやメニューが自動的に消えるまでの時間(ミリ秒)
        public double AutoHideDelayTimeMillisecond
        {
            get { return _autoHideDelayTime * 1000.0; }
        }


        // パネルやメニューが自動的に消えるまでの時間(秒)
        [PropertyMember("@ParamAutoHideDelayVisibleTime")]
        public double AutoHideDelayVisibleTime
        {
            get { return _autoHideDelayVisibleTime; }
            set { if (SetProperty(ref _autoHideDelayVisibleTime, value)) { RaisePropertyChanged(nameof(AutoHideDelayVisibleTimeMillisecond)); } }
        }

        // パネルやメニューが自動的に消えるまでの時間(ミリ秒)
        public double AutoHideDelayVisibleTimeMillisecond
        {
            get { return _autoHideDelayVisibleTime * 1000.0; }
        }


        // パネル自動非表示のフォーカス挙動モード
        [PropertyMember("@AutoHideFocusLockMode", Tips = "@AutoHideFocusLockModeTips")]
        public AutoHideFocusLockMode AutoHideFocusLockMode
        {
            get { return _autoHideFocusLockMode; }
            set { SetProperty(ref _autoHideFocusLockMode, value); }
        }

        // パネル自動非表示のキー入力遅延
        [PropertyMember("@IsAutoHideKeyDownDelay", Tips = "@IsAutoHideKeyDownDelayTips")]
        public bool IsAutoHideKeyDownDelay
        {
            get { return _isAutoHideKeyDownDelay; }
            set { SetProperty(ref _isAutoHideKeyDownDelay, value); }
        }

        // パネル自動非表示の表示判定マージン
        [PropertyMember("@ParamSidePanelHitTestMargin")]
        public double AutoHideHitTestMargin
        {
            get { return _autoHideHitTestMargin; }
            set { SetProperty(ref _autoHideHitTestMargin, value); }
        }

        // ウィンドウクローム枠
        [PropertyMember("@ParamWindowChromeFrame")]
        public WindowChromeFrame WindowChromeFrame { get; set; } = WindowChromeFrame.WindowFrame;

#if false
        // 前回開いていたブックを開く
        [PropertyMember("@ParamIsOpenLastBook")]
        public bool IsOpenLastBook { get; set; }
#endif

        // ダウンロードファイル置き場
        [DefaultValue("")]
        [PropertyPath("@ParamDownloadPath", Tips = "@ParamDownloadPathTips", FileDialogType = FileDialogType.Directory)]
        public string DownloadPath { get; set; } = "";

        [PropertyMember("@ParamIsSettingBackup", Tips = "@ParamIsSettingBackupTips")]
        public bool IsSettingBackup
        {
            get { return _isSettingBackup || Environment.IsAppxPackage; }  // Appxは強制ON
            set { _isSettingBackup = value; }
        }

#if false
        // 言語
        [PropertyMember("@ParamLanguage", Tips = "@ParamLanguageTips")]
        public Language Language { get; set; } = LanguageExtensions.GetLanguage(CultureInfo.CurrentCulture.Name);

        // スプラッシュスクリーン
        [PropertyMember("@ParamIsSplashScreenEnabled")]
        public bool IsSplashScreenEnabled { get; set; } = true;
#endif

        // 設定データの同期
        [PropertyMember("@ParamIsSyncUserSetting", Tips = "@ParamIsSyncUserSettingTips")]
        public bool IsSyncUserSetting { get; set; } = true;

        // テンポラリーフォルダーの場所
        [PropertyPath("@ParamTemporaryDirectory", Tips = "@ParamTemporaryDirectoryTips", FileDialogType = FileDialogType.Directory)]
        public string TemporaryDirectory
        {
            get => _temporaryDirectory ?? System.IO.Path.GetTempPath();
            set => _temporaryDirectory = string.IsNullOrWhiteSpace(value) || value == System.IO.Path.GetTempPath() ? null : value;
        }

        // サムネイルキャッシュの場所
        [PropertyPath("@ParamCacheDirectory", Tips = "@ParamCacheDirectoryTips", FileDialogType = FileDialogType.Directory)]
        public string CacheDirectory
        {
            get => _cacheDirectory ?? Environment.LocalApplicationDataPath;
            set => _cacheDirectory = string.IsNullOrWhiteSpace(value) || value == Environment.LocalApplicationDataPath ? null : value;
        }

        // サムネイルキャッシュの場所 (変更前)
        public string CacheDirectoryOld
        {
            get => _cacheDirectoryOld ?? Environment.LocalApplicationDataPath;
            set => _cacheDirectoryOld = string.IsNullOrWhiteSpace(value) || value == Environment.LocalApplicationDataPath ? null : value;
        }

        #endregion

        #region Memento
        [DataContract]
        public class Memento : IMemento
        {
            [DataMember]
            public int _Version { get; set; } = Environment.ProductVersionNumber;

            [DataMember, DefaultValue(false)]
            public bool IsMultiBootEnabled { get; set; }

            [DataMember, DefaultValue(false)]
            public bool IsSaveFullScreen { get; set; }

            [DataMember, DefaultValue(true)]
            public bool IsSaveWindowPlacement { get; set; }

            [DataMember, DefaultValue(true)]
            public bool IsNetworkEnabled { get; set; }

            [Obsolete, DataMember(EmitDefaultValue = false)]
            public bool IsDisableSave { get; set; }

            [DataMember, DefaultValue(true)]
            public bool IsSaveHistory { get; set; }

            [DataMember(EmitDefaultValue = false)]
            public string HistoryFilePath { get; set; }

            [DataMember, DefaultValue(true)]
            public bool IsSaveBookmark { get; set; }

            [DataMember(EmitDefaultValue = false)]
            public string BookmarkFilePath { get; set; }

            [DataMember, DefaultValue(true)]
            public bool IsSavePagemark { get; set; }

            [DataMember(EmitDefaultValue = false)]
            public string PagemarkFilePath { get; set; }

            [DataMember, DefaultValue(true)]
            public bool IsIgnoreImageDpi { get; set; }

            [Obsolete, DataMember(EmitDefaultValue = false), DefaultValue(false)]
            public bool IsIgnoreWindowDpi { get; set; }

            [DataMember, DefaultValue(true)]
            public bool IsRestoreSecondWindow { get; set; }

            [Obsolete, DataMember(Name = "WindowChromeFrame", EmitDefaultValue = false)]
            public WindowChromeFrameV1 WindowChromeFrameV1 { get; set; }

            [DataMember(Name = "WindowChromeFrameV2"), DefaultValue(WindowChromeFrame.WindowFrame)]
            public WindowChromeFrame WindowChromeFrame { get; set; }

            [DataMember, DefaultValue(1.0)]
            public double AutoHideDelayTime { get; set; }

            [DataMember, DefaultValue(0.0)]
            public double AutoHideDelayVisibleTime { get; set; }

            [DataMember, DefaultValue(false)]
            public bool IsOpenLastBook { get; set; }

            [DataMember, DefaultValue("")]
            public string DownloadPath { get; set; }

            [DataMember, DefaultValue(false)]
            public bool IsSettingBackup { get; set; }

            [DataMember]
            public Language Language { get; set; }

            [DataMember, DefaultValue(true)]
            public bool IsSplashScreenEnabled { get; set; }

            [DataMember, DefaultValue(true)]
            public bool IsSyncUserSetting { get; set; }

            [DataMember(EmitDefaultValue = false)]
            public string TemporaryDirectory { get; set; }

            [DataMember(EmitDefaultValue = false)]
            public string CacheDirectory { get; set; }

            [DataMember(EmitDefaultValue = false)]
            public string CacheDirectoryOld { get; set; }

            [DataMember, DefaultValue(AutoHideFocusLockMode.LogicalTextBoxFocusLock)]
            public AutoHideFocusLockMode AutoHideFocusLockMode { get; set; }

            [DataMember, DefaultValue(true)]
            public bool IsAutoHideKeyDownDelay { get; set; }

            [DataMember, DefaultValue(32.0)]
            public double AutoHideHitTestMargin { get; set; }


            [OnDeserializing]
            private void OnDeserializing(StreamingContext c)
            {
                this.InitializePropertyDefaultValues();

                this.Language = LanguageExtensions.GetLanguage(CultureInfo.CurrentCulture.Name);
            }

#pragma warning disable CS0612

            [OnDeserialized]
            public void OnDeserialized(StreamingContext c)
            {
                // before ver.34
                if (_Version < Environment.GenerateProductVersionNumber(34, 0, 0))
                {
                    WindowChromeFrame = WindowChromeFrameV1 == WindowChromeFrameV1.None ? WindowChromeFrame.None : WindowChromeFrame.WindowFrame;
                }
            }

#pragma warning restore CS0612

            public void RestoreConfig()
            {
                // ver 37.0
                Config.Current.StartUp.IsMultiBootEnabled = IsMultiBootEnabled;
                Config.Current.StartUp.IsRestoreFullScreen = IsSaveFullScreen;
                Config.Current.StartUp.IsRestoreWindowPlacement = IsSaveWindowPlacement;
                Config.Current.StartUp.IsRestoreSecondWindowPlacement = IsRestoreSecondWindow;
                Config.Current.System.IsNetworkEnabled = IsNetworkEnabled;
                Config.Current.StartUp.IsOpenLastBook = IsOpenLastBook;
                Config.Current.System.Language = Language;
                Config.Current.StartUp.IsSplashScreenEnabled = IsSplashScreenEnabled;
                Config.Current.History.IsSaveHistory = IsSaveHistory;
                Config.Current.History.HistoryFilePath = HistoryFilePath;
                Config.Current.Bookmark.IsSaveBookmark = IsSaveBookmark;
                Config.Current.Bookmark.BookmarkFilePath = BookmarkFilePath;
                Config.Current.Pagemark.IsSavePagemark = IsSavePagemark;
                Config.Current.Pagemark.PagemarkFilePath = PagemarkFilePath;
            }
        }

        public Memento CreateMemento()
        {
            var memento = new Memento();
            memento.IsMultiBootEnabled = Config.Current.StartUp.IsMultiBootEnabled;
            memento.IsSaveFullScreen = Config.Current.StartUp.IsRestoreFullScreen;
            memento.IsSaveWindowPlacement = Config.Current.StartUp.IsRestoreWindowPlacement;
            memento.IsNetworkEnabled = Config.Current.System.IsNetworkEnabled;
            memento.IsIgnoreImageDpi = this.IsIgnoreImageDpi;
            memento.IsSaveHistory = Config.Current.History.IsSaveHistory;
            memento.HistoryFilePath = Config.Current.History.HistoryFilePath;
            memento.IsSaveBookmark = Config.Current.Bookmark.IsSaveBookmark;
            memento.BookmarkFilePath = Config.Current.Bookmark.BookmarkFilePath;
            memento.IsSavePagemark = Config.Current.Pagemark.IsSavePagemark;
            memento.PagemarkFilePath = Config.Current.Pagemark.PagemarkFilePath;
            memento.AutoHideDelayTime = this.AutoHideDelayTime;
            memento.AutoHideDelayVisibleTime = this.AutoHideDelayVisibleTime;
            memento.WindowChromeFrame = this.WindowChromeFrame;
            memento.IsOpenLastBook = Config.Current.StartUp.IsOpenLastBook;
            memento.DownloadPath = this.DownloadPath;
            memento.IsRestoreSecondWindow = Config.Current.StartUp.IsRestoreSecondWindowPlacement;
            memento.IsSettingBackup = this.IsSettingBackup;
            memento.Language = Config.Current.System.Language;
            memento.IsSplashScreenEnabled = Config.Current.StartUp.IsSplashScreenEnabled;
            memento.IsSyncUserSetting = this.IsSyncUserSetting;
            memento.TemporaryDirectory = _temporaryDirectory;
            memento.CacheDirectory = _cacheDirectory;
            memento.CacheDirectoryOld = _cacheDirectoryOld;
            memento.AutoHideFocusLockMode = this.AutoHideFocusLockMode;
            memento.IsAutoHideKeyDownDelay = this.IsAutoHideKeyDownDelay;
            memento.AutoHideHitTestMargin = this.AutoHideHitTestMargin;
            return memento;
        }

        // 起動直後の１回だけの設定反映
        public void RestoreOnce(Memento memento)
        {
            if (memento == null) return;

            this.CacheDirectoryOld = memento.CacheDirectoryOld;
        }

        // 通常の設定反映
        public void Restore(Memento memento)
        {
            if (memento == null) return;

            this.SettingVersion = memento._Version;
            ////this.IsMultiBootEnabled = memento.IsMultiBootEnabled;
            ////this.IsSaveFullScreen = memento.IsSaveFullScreen;
            ////this.IsSaveWindowPlacement = memento.IsSaveWindowPlacement;
            ////this.IsNetworkEnabled = memento.IsNetworkEnabled;
            this.IsIgnoreImageDpi = memento.IsIgnoreImageDpi;
            ////this.IsSaveHistory = memento.IsSaveHistory;
            ////this.HistoryFilePath = memento.HistoryFilePath;
            ////this.IsSaveBookmark = memento.IsSaveBookmark;
            ////this.BookmarkFilePath = memento.BookmarkFilePath;
            ////this.IsSavePagemark = memento.IsSavePagemark;
            ////this.PagemarkFilePath = memento.PagemarkFilePath;
            this.AutoHideDelayTime = memento.AutoHideDelayTime;
            this.AutoHideDelayVisibleTime = memento.AutoHideDelayVisibleTime;
            this.WindowChromeFrame = memento.WindowChromeFrame;
            ////this.IsOpenLastBook = memento.IsOpenLastBook;
            this.DownloadPath = memento.DownloadPath;
            ////this.IsRestoreSecondWindow = memento.IsRestoreSecondWindow;
            this.IsSettingBackup = memento.IsSettingBackup;
            ////this.Language = memento.Language;
            ////this.IsSplashScreenEnabled = memento.IsSplashScreenEnabled;
            this.IsSyncUserSetting = memento.IsSyncUserSetting;
            this.TemporaryDirectory = memento.TemporaryDirectory;
            this.CacheDirectory = memento.CacheDirectory;
            ////this.CacheDirectoryOld = memento.CacheDirectoryOld; // RestoreOnce()で反映
            this.AutoHideFocusLockMode = memento.AutoHideFocusLockMode;
            this.IsAutoHideKeyDownDelay = memento.IsAutoHideKeyDownDelay;
            this.AutoHideHitTestMargin = memento.AutoHideHitTestMargin;
        }

        #endregion
    }
}
