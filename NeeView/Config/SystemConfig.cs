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


        /// <summary>
        /// 言語
        /// </summary>
        [PropertyMember("@ParamLanguage", Tips = "@ParamLanguageTips")]
        public Language Language { get; set; } = LanguageExtensions.GetLanguage(CultureInfo.CurrentCulture.Name);

        [PropertyMember("@ParamArchiveRecursiveMode", Tips = "@ParamArchiveRecursiveModeTips")]
        public ArchiveEntryCollectionMode ArchiveRecursiveMode
        {
            get { return _archiveRecursiveMode; }
            set { SetProperty(ref _archiveRecursiveMode, value); }
        }

        // ページ収集モード
        [PropertyMember("@ParamBookPageCollectMode", Tips = "@ParamBookPageCollectModeTips")]
        public BookPageCollectMode BookPageCollectMode { get; set; } = BookPageCollectMode.ImageAndBook;

        [PropertyMember("@ParamIsRemoveConfirmed")]
        public bool IsRemoveConfirmed { get; set; } = true;

        [PropertyMember("@ParamIsRemoveExplorerDialogEnabled", Tips = "@ParamIsRemoveExplorerDialogEnabledTips")]
        public bool IsRemoveExplorerDialogEnabled { get; set; }

        // ネットワークアクセス許可
        [PropertyMember("@ParamIsNetworkEnabled", Tips = "@ParamIsNetworkEnabledTips")]
        public bool IsNetworkEnabled
        {
            get { return _isNetworkEnalbe || Environment.IsAppxPackage; } // Appxは強制ON
            set { SetProperty(ref _isNetworkEnalbe, value); }
        }

        // 設定データの同期
        [PropertyMember("@ParamIsSyncUserSetting", Tips = "@ParamIsSyncUserSettingTips")]
        public bool IsSyncUserSetting { get; set; } = true;

        // 設定データのバックアップ作成
        [PropertyMember("@ParamIsSettingBackup", Tips = "@ParamIsSettingBackupTips")]
        public bool IsSettingBackup
        {
            get { return _isSettingBackup || Environment.IsAppxPackage; }  // Appxは強制ON
            set { _isSettingBackup = value; }
        }

        // 画像のDPI非対応
        [PropertyMember("@ParamIsIgnoreImageDpi", Tips = "@ParamIsIgnoreImageDpiTips")]
        public bool IsIgnoreImageDpi { get; set; } = true;

        // テンポラリフォルダーの場所
        [PropertyPath("@ParamTemporaryDirectory", Tips = "@ParamTemporaryDirectoryTips", FileDialogType = FileDialogType.Directory)]
        public string TemporaryDirectory
        {
            get => _temporaryDirectory;
            set => _temporaryDirectory = string.IsNullOrWhiteSpace(value) || value == Temporary.TempRootPathDefault ? null : value;
        }

        // サムネイルキャッシュの場所
        [PropertyPath("@ParamCacheDirectory", Tips = "@ParamCacheDirectoryTips", FileDialogType = FileDialogType.Directory)]
        public string CacheDirectory
        {
            get => _cacheDirectory;
            set => _cacheDirectory = string.IsNullOrWhiteSpace(value) || value == ThumbnailCache.CacheFolderPathDefault ? null : value;
        }

        // ダウンロードファイル置き場
        [DefaultValue("")]
        [PropertyPath("@ParamDownloadPath", Tips = "@ParamDownloadPathTips", FileDialogType = FileDialogType.Directory)]
        public string DownloadPath { get; set; } = "";

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
    }

}