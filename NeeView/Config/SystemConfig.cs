using NeeLaboratory.ComponentModel;
using NeeView.Windows.Controls;
using NeeView.Windows.Property;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace NeeView
{
    public class SystemConfig : BindableBase
    {
        private ArchiveEntryCollectionMode _archiveRecursiveMode = ArchiveEntryCollectionMode.IncludeSubArchives;
        private bool _isNetworkEnalbe = true;
        private bool _isSettingBackup;
        private bool _isHiddenFileVisibled;
        private bool _isFileWriteAccessEnabled = true;
        private Language _language = LanguageExtensions.GetLanguage(CultureInfo.CurrentCulture.Name);
        private BookPageCollectMode _bookPageCollectMode = BookPageCollectMode.ImageAndBook;
        private bool _isRemoveConfirmed = true;
        private bool _isRemoveWantNukeWarning;
        private bool _isSyncUserSetting = true;
        private bool _isIgnoreImageDpi = true;
        private string _downloadPath = "";
        private bool _isOpenbookAtCurrentPlace;
        private bool _isNaturalSortEnabled;
        private bool _isInputMehotdEnabled;
        private DestinationFolderCollection _destinationFolderCollection = new DestinationFolderCollection();
        private ExternalAppCollection _externalAppCollection = new ExternalAppCollection() { new ExternalApp() };

        [JsonInclude, JsonPropertyName(nameof(TemporaryDirectory))]
        public string _temporaryDirectory;


        /// <summary>
        /// 言語
        /// </summary>
        [PropertyMember]
        public Language Language
        {
            get { return _language; }
            set { SetProperty(ref _language, value); }
        }

        [PropertyMember]
        public ArchiveEntryCollectionMode ArchiveRecursiveMode
        {
            get { return _archiveRecursiveMode; }
            set { SetProperty(ref _archiveRecursiveMode, value); }
        }

        // ページ収集モード
        [PropertyMember]
        public BookPageCollectMode BookPageCollectMode
        {
            get { return _bookPageCollectMode; }
            set { SetProperty(ref _bookPageCollectMode, value); }
        }

        [PropertyMember]
        public bool IsRemoveConfirmed
        {
            get { return _isRemoveConfirmed; }
            set { SetProperty(ref _isRemoveConfirmed, value); }
        }

        [PropertyMember]
        public bool IsRemoveWantNukeWarning
        {
            get { return _isRemoveWantNukeWarning; }
            set { SetProperty(ref _isRemoveWantNukeWarning, value); }
        }

        // ネットワークアクセス許可
        [PropertyMember]
        public bool IsNetworkEnabled
        {
            get { return _isNetworkEnalbe || Environment.IsAppxPackage; } // Appxは強制ON
            set { SetProperty(ref _isNetworkEnalbe, value); }
        }

        // 設定データの同期
        [PropertyMember]
        public bool IsSyncUserSetting
        {
            get { return _isSyncUserSetting; }
            set { SetProperty(ref _isSyncUserSetting, value); }
        }

        // 設定データのバックアップ作成
        [PropertyMember]
        public bool IsSettingBackup
        {
            get { return _isSettingBackup || Environment.IsAppxPackage; }  // Appxは強制ON
            set { SetProperty(ref _isSettingBackup, value); }
        }

        // 画像のDPI非対応
        [PropertyMember]
        public bool IsIgnoreImageDpi
        {
            get { return _isIgnoreImageDpi; }
            set { SetProperty(ref _isIgnoreImageDpi, value); }
        }

        // テンポラリフォルダーの場所
        [JsonIgnore]
        [PropertyPath(FileDialogType = FileDialogType.Directory)]
        public string TemporaryDirectory
        {
            get { return _temporaryDirectory ?? Temporary.TempRootPathDefault; }
            set { SetProperty(ref _temporaryDirectory, (string.IsNullOrWhiteSpace(value) || value.Trim() == Temporary.TempRootPathDefault) ? null : value.Trim()); }
        }

        // ダウンロードファイル置き場
        [DefaultValue("")]
        [PropertyPath(FileDialogType = FileDialogType.Directory)]
        public string DownloadPath
        {
            get { return _downloadPath; }
            set { SetProperty(ref _downloadPath, value ?? ""); }
        }

        // 隠しファイルを表示する？
        [PropertyMember]
        public bool IsHiddenFileVisibled
        {
            get { return _isHiddenFileVisibled; }
            set { SetProperty(ref _isHiddenFileVisibled, value); }
        }

        [PropertyMember]
        public bool IsFileWriteAccessEnabled
        {
            get { return _isFileWriteAccessEnabled; }
            set { SetProperty(ref _isFileWriteAccessEnabled, value); }
        }


        // 「ブックを開く」ダイアログを現在の場所を基準にして開く
        // TODO: LoadAs のコマンドパラメータにする
        [PropertyMember]
        public bool IsOpenbookAtCurrentPlace
        {
            get { return _isOpenbookAtCurrentPlace; }
            set { SetProperty(ref _isOpenbookAtCurrentPlace, value); }
        }

        // カスタム自然順ソート
        [PropertyMember]
        public bool IsNaturalSortEnabled
        {
            get { return _isNaturalSortEnabled; }
            set { SetProperty(ref _isNaturalSortEnabled, value); }
        }

        // テキストボックス以外でのIME有効 (現状では非公開)
        [PropertyMember]
        public bool IsInputMethodEnabled
        {
            get { return _isInputMehotdEnabled; }
            set { SetProperty(ref _isInputMehotdEnabled, value); }
        }


        // コピーまたは移動先フォルダーのリスト
        [PropertyMember]
        [PropertyMapIgnore]
        [ObjectMergeReferenceCopy]
        public DestinationFolderCollection DestinationFodlerCollection
        {
            get { return _destinationFolderCollection; }
            set { SetProperty(ref _destinationFolderCollection, value); }
        }

        // 外部実行アプリ設定のリスト
        [PropertyMember]
        [PropertyMapIgnore]
        [ObjectMergeReferenceCopy]
        public ExternalAppCollection ExternalAppCollection
        {
            get { return _externalAppCollection; }
            set { SetProperty(ref _externalAppCollection, value); }
        }
    }
}