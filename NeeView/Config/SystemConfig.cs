using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System.Globalization;
using System.Runtime.Serialization;

namespace NeeView
{
    public class SystemConfig : BindableBase
    {
        private ArchiveEntryCollectionMode _archiveRecursiveMode = ArchiveEntryCollectionMode.IncludeSubArchives;
        private bool _isNetworkEnalbe = true;


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
    }

}