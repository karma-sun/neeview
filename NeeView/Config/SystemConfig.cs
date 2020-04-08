using NeeLaboratory.ComponentModel;
using NeeView.Windows.Controls;
using NeeView.Windows.Property;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.Serialization;

namespace NeeView
{
    public class SystemConfig : BindableBase
    {
        private ArchiveEntryCollectionMode _archiveRecursiveMode = ArchiveEntryCollectionMode.IncludeSubArchives;
        private bool _isNetworkEnalbe = true;
        private bool _isSettingBackup;
        private string _temporaryDirectory;
        private string _cacheDirectory;
        private bool _isHiddenFileVisibled;
        private bool _isFileWriteAccessEnabled = true;
        private Language _language = LanguageExtensions.GetLanguage(CultureInfo.CurrentCulture.Name);
        private BookPageCollectMode _bookPageCollectMode = BookPageCollectMode.ImageAndBook;
        private bool _isRemoveConfirmed = true;
        private bool _isRemoveExplorerDialogEnabled;
        private bool _isSyncUserSetting = true;
        private bool _isIgnoreImageDpi = true;
        private string _downloadPath = "";
        private bool _isOpenbookAtCurrentPlace;
        private bool _isNaturalSortEnabled;


        /// <summary>
        /// 言語
        /// </summary>
        [PropertyMember("@ParamLanguage", Tips = "@ParamLanguageTips")]
        public Language Language
        {
            get { return _language; }
            set { SetProperty(ref _language, value); }
        }

        [PropertyMember("@ParamArchiveRecursiveMode", Tips = "@ParamArchiveRecursiveModeTips")]
        public ArchiveEntryCollectionMode ArchiveRecursiveMode
        {
            get { return _archiveRecursiveMode; }
            set { SetProperty(ref _archiveRecursiveMode, value); }
        }

        // ページ収集モード
        [PropertyMember("@ParamBookPageCollectMode", Tips = "@ParamBookPageCollectModeTips")]
        public BookPageCollectMode BookPageCollectMode
        {
            get { return _bookPageCollectMode; }
            set { SetProperty(ref _bookPageCollectMode, value); }
        }

        [PropertyMember("@ParamIsRemoveConfirmed")]
        public bool IsRemoveConfirmed
        {
            get { return _isRemoveConfirmed; }
            set { SetProperty(ref _isRemoveConfirmed, value); }
        }

        [PropertyMember("@ParamIsRemoveExplorerDialogEnabled", Tips = "@ParamIsRemoveExplorerDialogEnabledTips")]
        public bool IsRemoveExplorerDialogEnabled
        {
            get { return _isRemoveExplorerDialogEnabled; }
            set { SetProperty(ref _isRemoveExplorerDialogEnabled, value); }
        }

        // ネットワークアクセス許可
        [PropertyMember("@ParamIsNetworkEnabled", Tips = "@ParamIsNetworkEnabledTips")]
        public bool IsNetworkEnabled
        {
            get { return _isNetworkEnalbe || Environment.IsAppxPackage; } // Appxは強制ON
            set { SetProperty(ref _isNetworkEnalbe, value); }
        }

        // 設定データの同期
        [PropertyMember("@ParamIsSyncUserSetting", Tips = "@ParamIsSyncUserSettingTips")]
        public bool IsSyncUserSetting
        {
            get { return _isSyncUserSetting; }
            set { SetProperty(ref _isSyncUserSetting, value); }
        }

        // 設定データのバックアップ作成
        [PropertyMember("@ParamIsSettingBackup", Tips = "@ParamIsSettingBackupTips")]
        public bool IsSettingBackup
        {
            get { return _isSettingBackup || Environment.IsAppxPackage; }  // Appxは強制ON
            set { SetProperty(ref _isSettingBackup, value); }
        }

        // 画像のDPI非対応
        [PropertyMember("@ParamIsIgnoreImageDpi", Tips = "@ParamIsIgnoreImageDpiTips")]
        public bool IsIgnoreImageDpi
        {
            get { return _isIgnoreImageDpi; }
            set { SetProperty(ref _isIgnoreImageDpi, value); }
        }

        // テンポラリフォルダーの場所
        [PropertyPath("@ParamTemporaryDirectory", Tips = "@ParamTemporaryDirectoryTips", FileDialogType = FileDialogType.Directory)]
        public string TemporaryDirectory
        {
            get { return _temporaryDirectory; }
            set { SetProperty(ref _temporaryDirectory, (string.IsNullOrWhiteSpace(value) || value?.Trim() == Temporary.TempRootPathDefault) ? null : value); }
        }

        // サムネイルキャッシュの場所
        [PropertyPath("@ParamCacheDirectory", Tips = "@ParamCacheDirectoryTips", FileDialogType = FileDialogType.Directory)]
        public string CacheDirectory
        {
            get { return _cacheDirectory; }
            set { SetProperty(ref _cacheDirectory, (string.IsNullOrWhiteSpace(value) || value?.Trim() == ThumbnailCache.CacheFolderPathDefault) ? null : value); }
        }

        // ダウンロードファイル置き場
        [DefaultValue("")]
        [PropertyPath("@ParamDownloadPath", Tips = "@ParamDownloadPathTips", FileDialogType = FileDialogType.Directory)]
        public string DownloadPath
        {
            get { return _downloadPath; }
            set { SetProperty(ref _downloadPath, value ?? ""); }
        }

        // 隠しファイルを表示する？
        [PropertyMember("@ParamIsHiddenFileVisibled")]
        public bool IsHiddenFileVisibled
        {
            get { return _isHiddenFileVisibled; }
            set { SetProperty(ref _isHiddenFileVisibled, value); }
        }

        [PropertyMember("@ParamIsFileOperationEnabled")]
        public bool IsFileWriteAccessEnabled
        {
            get { return _isFileWriteAccessEnabled; }
            set { SetProperty(ref _isFileWriteAccessEnabled, value); }
        }


        // 「ブックを開く」ダイアログを現在の場所を基準にして開く
        // TODO: LoadAs のコマンドパラメータにする
        [PropertyMember("@ParamIsOpenbookAtCurrentPlace")]
        public bool IsOpenbookAtCurrentPlace
        {
            get { return _isOpenbookAtCurrentPlace; }
            set { SetProperty(ref _isOpenbookAtCurrentPlace, value); }
        }

        // カスタム自然順ソート
        [PropertyMember("@ParamIsNaturalSortEnabled", Tips="@ParamIsNaturalSortEnabledTips")]
        public bool IsNaturalSortEnabled
        {
            get { return _isNaturalSortEnabled; }
            set { SetProperty(ref _isNaturalSortEnabled, value); }
        }
    }

}