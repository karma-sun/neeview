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

        private bool _isNetworkEnalbe = true;
        private bool _isSettingBackup;
        private bool _isSaveWindowPlacement = true;
        private double _autoHideDelayTime = 1.0;
        private string _temporaryDirectory;
        private string _cacheDirectory;
        private string _cacheDirectoryOld;
        private bool _isSaveHistory = true;
        private string _historyFilePath;
        private bool _isSaveBookmark = true;
        private string _bookmarkFilePath;
        private bool _isSavePagemark = true;
        private string _pagemarkFilePath;


        #endregion

        #region Properties

        // 適用した設定データのバージョン
        public int SettingVersion { get; set; }

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
            get { return _isNetworkEnalbe || Config.Current.IsAppxPackage; } // Appxは強制ON
            set { if (_isNetworkEnalbe != value) { _isNetworkEnalbe = value; RaisePropertyChanged(); } }
        }

        // 画像のDPI非対応
        [PropertyMember("@ParamIsIgnoreImageDpi", Tips = "@ParamIsIgnoreImageDpiTips")]
        public bool IsIgnoreImageDpi { get; set; } = true;

        // 複数ウィンドウの座標復元
        [PropertyMember("@ParamIsRestoreSecondWindow", Tips = "@ParamIsRestoreSecondWindowTips")]
        public bool IsRestoreSecondWindow { get; set; } = true;

        [Obsolete]
        public bool IsDisableSave { get; set; }

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

        // パネルやメニューが自動的に消えるまでの時間(秒)
        [PropertyMember("@ParamAutoHideDelayTime")]
        public double AutoHideDelayTime
        {
            get { return _autoHideDelayTime; }
            set { if (_autoHideDelayTime != value) { _autoHideDelayTime = value; RaisePropertyChanged(); } }
        }

        // ウィンドウクローム枠
        [PropertyMember("@ParamWindowChromeFrame")]
        public WindowChromeFrame WindowChromeFrame { get; set; } = WindowChromeFrame.WindowFrame;

        // 前回開いていたブックを開く
        [PropertyMember("@ParamIsOpenLastBook")]
        public bool IsOpenLastBook { get; set; }

        // ダウンロードファイル置き場
        [DefaultValue("")]
        [PropertyPath("@ParamDownloadPath", Tips = "@ParamDownloadPathTips", FileDialogType = FileDialogType.Directory)]
        public string DownloadPath { get; set; } = "";

        [PropertyMember("@ParamIsSettingBackup", Tips = "@ParamIsSettingBackupTips")]
        public bool IsSettingBackup
        {
            get { return _isSettingBackup || Config.Current.IsAppxPackage; }  // Appxは強制ON
            set { _isSettingBackup = value; }
        }

        // 言語
        [PropertyMember("@ParamLanguage", Tips = "@ParamLanguageTips")]
        public Language Language { get; set; } = LanguageExtensions.GetLanguage(CultureInfo.CurrentCulture.Name);

        // スプラッシュスクリーン
        [PropertyMember("@ParamIsSplashScreenEnabled")]
        public bool IsSplashScreenEnabled { get; set; } = true;

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
            get => _cacheDirectory ?? Config.Current.LocalApplicationDataPath;
            set => _cacheDirectory = string.IsNullOrWhiteSpace(value) || value == Config.Current.LocalApplicationDataPath ? null : value;
        }

        // サムネイルキャッシュの場所 (変更前)
        public string CacheDirectoryOld
        {
            get => _cacheDirectoryOld ?? Config.Current.LocalApplicationDataPath;
            set => _cacheDirectoryOld = string.IsNullOrWhiteSpace(value) || value == Config.Current.LocalApplicationDataPath ? null : value;
        }

        #endregion

        #region Memento
        [DataContract]
        public class Memento
        {
            [DataMember]
            public int _Version { get; set; } = Config.Current.ProductVersionNumber;

            [DataMember, DefaultValue(false)]
            public bool IsMultiBootEnabled { get; set; }

            [DataMember, DefaultValue(false)]
            public bool IsSaveFullScreen { get; set; }

            [DataMember, DefaultValue(true)]
            public bool IsSaveWindowPlacement { get; set; }

            [DataMember, DefaultValue(true)]
            public bool IsNetworkEnabled { get; set; }

            [Obsolete]
            [DataMember(EmitDefaultValue = false)]
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

            [Obsolete]
            [DataMember(EmitDefaultValue = false), DefaultValue(false)]
            public bool IsIgnoreWindowDpi { get; set; }

            [DataMember, DefaultValue(true)]
            public bool IsRestoreSecondWindow { get; set; }

            [Obsolete]
            [DataMember(Name = "WindowChromeFrame", EmitDefaultValue = false)]
            public WindowChromeFrameV1 WindowChromeFrameV1 { get; set; }

            [DataMember(Name = "WindowChromeFrameV2"), DefaultValue(WindowChromeFrame.WindowFrame)]
            public WindowChromeFrame WindowChromeFrame { get; set; }

            [DataMember, DefaultValue(1.0)]
            public double AutoHideDelayTime { get; set; }

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


            [OnDeserializing]
            private void Deserializing(StreamingContext c)
            {
                this.InitializePropertyDefaultValues();

                this.Language = LanguageExtensions.GetLanguage(CultureInfo.CurrentCulture.Name);
            }

#pragma warning disable CS0612

            [OnDeserialized]
            private void Deserialized(StreamingContext c)
            {
                // before ver.30
                if (_Version < Config.GenerateProductVersionNumber(30, 0, 0))
                {
                    if (IsDisableSave)
                    {
                        IsSaveHistory = false;
                        IsSaveBookmark = false;
                        IsSavePagemark = false;
                    }
                }

                // before ver.34
                if (_Version < Config.GenerateProductVersionNumber(34, 0, 0))
                {
                    WindowChromeFrame = WindowChromeFrameV1 == WindowChromeFrameV1.None ? WindowChromeFrame.None : WindowChromeFrame.WindowFrame;
                }
            }

#pragma warning restore CS0612
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
            memento.IsSaveHistory = this.IsSaveHistory;
            memento.HistoryFilePath = _historyFilePath;
            memento.IsSaveBookmark = this.IsSaveBookmark;
            memento.BookmarkFilePath = _bookmarkFilePath;
            memento.IsSavePagemark = this.IsSavePagemark;
            memento.PagemarkFilePath = _pagemarkFilePath;
            memento.AutoHideDelayTime = this.AutoHideDelayTime;
            memento.WindowChromeFrame = this.WindowChromeFrame;
            memento.IsOpenLastBook = this.IsOpenLastBook;
            memento.DownloadPath = this.DownloadPath;
            memento.IsRestoreSecondWindow = this.IsRestoreSecondWindow;
            memento.IsSettingBackup = this.IsSettingBackup;
            memento.Language = this.Language;
            memento.IsSplashScreenEnabled = this.IsSplashScreenEnabled;
            memento.IsSyncUserSetting = this.IsSyncUserSetting;
            memento.TemporaryDirectory = _temporaryDirectory;
            memento.CacheDirectory = _cacheDirectory;
            memento.CacheDirectoryOld = _cacheDirectoryOld;
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

            this.IsMultiBootEnabled = memento.IsMultiBootEnabled;
            this.IsSaveFullScreen = memento.IsSaveFullScreen;
            this.IsSaveWindowPlacement = memento.IsSaveWindowPlacement;
            this.IsNetworkEnabled = memento.IsNetworkEnabled;
            this.IsIgnoreImageDpi = memento.IsIgnoreImageDpi;
            this.IsSaveHistory = memento.IsSaveHistory;
            this.HistoryFilePath = memento.HistoryFilePath;
            this.IsSaveBookmark = memento.IsSaveBookmark;
            this.BookmarkFilePath = memento.BookmarkFilePath;
            this.IsSavePagemark = memento.IsSavePagemark;
            this.PagemarkFilePath = memento.PagemarkFilePath;
            this.AutoHideDelayTime = memento.AutoHideDelayTime;
            this.WindowChromeFrame = memento.WindowChromeFrame;
            this.IsOpenLastBook = memento.IsOpenLastBook;
            this.DownloadPath = memento.DownloadPath;
            this.IsRestoreSecondWindow = memento.IsRestoreSecondWindow;
            this.IsSettingBackup = memento.IsSettingBackup;
            this.Language = memento.Language;
            this.IsSplashScreenEnabled = memento.IsSplashScreenEnabled;
            this.IsSyncUserSetting = memento.IsSyncUserSetting;
            this.TemporaryDirectory = memento.TemporaryDirectory;
            this.CacheDirectory = memento.CacheDirectory;
            ////this.CacheDirectoryOld = memento.CacheDirectoryOld; // RestoreOnce()で反映
        }

#pragma warning disable CS0612

        public void RestoreCompatible(UserSetting setting)
        {
            // compatible before ver.23
            if (setting._Version < Config.GenerateProductVersionNumber(1, 23, 0))
            {
                if (setting.ViewMemento != null)
                {
                    this.IsMultiBootEnabled = !setting.ViewMemento.IsDisableMultiBoot;
                    this.IsSaveFullScreen = setting.ViewMemento.IsSaveFullScreen;
                    this.IsSaveWindowPlacement = setting.ViewMemento.IsSaveWindowPlacement;
                }
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
