using NeeLaboratory.ComponentModel;
using NeeView.Windows.Controls;
using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
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
        ////private bool _isSettingBackup;
        ////private bool _isSaveWindowPlacement = true;
        ////private string _temporaryDirectory;
        ////private string _cacheDirectory;
        ////private string _cacheDirectoryOld;
        ////private bool _isSaveHistory = true;
        ////private string _historyFilePath;
        ////private bool _isSaveBookmark = true;
        ////private string _bookmarkFilePath;
        ////private bool _isSavePagemark = true;
        ////private string _pagemarkFilePath;
        ////private double _autoHideDelayTime = 1.0;
        ////private double _autoHideDelayVisibleTime = 0.0;
        ////private AutoHideFocusLockMode _autoHideFocusLockMode = AutoHideFocusLockMode.LogicalTextBoxFocusLock;
        ////private bool _isAutoHideKeyDownDelay = true;
        ////private double _autoHideHitTestMargin = 32.0;

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

            public void RestoreConfig(Config config)
            {
                // ver 37.0
                config.StartUp.IsMultiBootEnabled = IsMultiBootEnabled;
                config.StartUp.IsRestoreFullScreen = IsSaveFullScreen;
                config.StartUp.IsRestoreWindowPlacement = IsSaveWindowPlacement;
                config.StartUp.IsRestoreSecondWindowPlacement = IsRestoreSecondWindow;
                config.System.IsNetworkEnabled = IsNetworkEnabled;
                config.StartUp.IsOpenLastBook = IsOpenLastBook;
                config.System.Language = Language;
                config.StartUp.IsSplashScreenEnabled = IsSplashScreenEnabled;
                config.History.IsSaveHistory = IsSaveHistory;
                config.History.HistoryFilePath = HistoryFilePath;
                config.Bookmark.IsSaveBookmark = IsSaveBookmark;
                config.Bookmark.BookmarkFilePath = BookmarkFilePath;
                config.Pagemark.IsSavePagemark = IsSavePagemark;
                config.Pagemark.PagemarkFilePath = PagemarkFilePath;
                config.System.IsSettingBackup = IsSettingBackup;
                config.System.IsSyncUserSetting = IsSyncUserSetting;
                config.System.TemporaryDirectory = TemporaryDirectory;
                config.Thumbnail.ThumbnailCacheFilePath = CacheDirectory != null ? Path.Combine(CacheDirectory, ThumbnailCache.ThumbnailCacheFileName) : null;
                ////config.System.CacheDirectoryOld = CacheDirectoryOld;
                config.Window.WindowChromeFrame = WindowChromeFrame;
                config.System.IsIgnoreImageDpi = IsIgnoreImageDpi;
                config.AutoHide.AutoHideDelayTime = AutoHideDelayTime;
                config.AutoHide.AutoHideDelayVisibleTime = AutoHideDelayVisibleTime;
                config.AutoHide.AutoHideFocusLockMode = AutoHideFocusLockMode;
                config.AutoHide.IsAutoHideKeyDownDelay = IsAutoHideKeyDownDelay;
                config.AutoHide.AutoHideHitTestMargin = AutoHideHitTestMargin;
                config.System.DownloadPath = DownloadPath;
            }
        }

        public Memento CreateMemento()
        {
            var memento = new Memento();
            memento.IsMultiBootEnabled = Config.Current.StartUp.IsMultiBootEnabled;
            memento.IsSaveFullScreen = Config.Current.StartUp.IsRestoreFullScreen;
            memento.IsSaveWindowPlacement = Config.Current.StartUp.IsRestoreWindowPlacement;
            memento.IsNetworkEnabled = Config.Current.System.IsNetworkEnabled;
            memento.IsIgnoreImageDpi = Config.Current.System.IsIgnoreImageDpi;
            memento.IsSaveHistory = Config.Current.History.IsSaveHistory;
            memento.HistoryFilePath = Config.Current.History.HistoryFilePath;
            memento.IsSaveBookmark = Config.Current.Bookmark.IsSaveBookmark;
            memento.BookmarkFilePath = Config.Current.Bookmark.BookmarkFilePath;
            memento.IsSavePagemark = Config.Current.Pagemark.IsSavePagemark;
            memento.PagemarkFilePath = Config.Current.Pagemark.PagemarkFilePath;
            memento.AutoHideDelayTime = Config.Current.AutoHide.AutoHideDelayTime;
            memento.AutoHideDelayVisibleTime = Config.Current.AutoHide.AutoHideDelayVisibleTime;
            memento.WindowChromeFrame = Config.Current.Window.WindowChromeFrame;
            memento.IsOpenLastBook = Config.Current.StartUp.IsOpenLastBook;
            memento.DownloadPath = Config.Current.System.DownloadPath;
            memento.IsRestoreSecondWindow = Config.Current.StartUp.IsRestoreSecondWindowPlacement;
            memento.IsSettingBackup = Config.Current.System.IsSettingBackup;
            memento.Language = Config.Current.System.Language;
            memento.IsSplashScreenEnabled = Config.Current.StartUp.IsSplashScreenEnabled;
            memento.IsSyncUserSetting = Config.Current.System.IsSyncUserSetting;
            memento.TemporaryDirectory = Config.Current.System.TemporaryDirectory;
            memento.CacheDirectory = Config.Current.Thumbnail.ThumbnailCacheFilePath != null ? Path.GetDirectoryName(Config.Current.Thumbnail.ThumbnailCacheFilePath) : null;
            memento.CacheDirectoryOld = memento.CacheDirectory; //// CacheDirectoryOld廃止(ver.37)
            memento.AutoHideFocusLockMode = Config.Current.AutoHide.AutoHideFocusLockMode;
            memento.IsAutoHideKeyDownDelay = Config.Current.AutoHide.IsAutoHideKeyDownDelay;
            memento.AutoHideHitTestMargin = Config.Current.AutoHide.AutoHideHitTestMargin;
            return memento;
        }

        #endregion

    }
}
