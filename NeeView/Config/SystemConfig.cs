using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System.Globalization;
using System.Runtime.Serialization;

namespace NeeView
{
    [DataContract]
    public class SystemConfig : BindableBase
    {
        private ArchiveEntryCollectionMode _archiveRecursiveMode;
        private bool _isNetworkEnalbe;


        public SystemConfig()
        {
            Constructor();
        }

        /// <summary>
        /// 言語
        /// </summary>
        [IgnoreDataMember]
        [PropertyMember("@ParamLanguage", Tips = "@ParamLanguageTips")]
        public Language Language { get; set; }

        [DataMember(Name = nameof(Language))]
        public string LanguageString
        {
            get { return Language.ToString(); }
            set { Language = value.ToEnum<Language>(); }
        }

        [IgnoreDataMember]
        [PropertyMember("@ParamArchiveRecursiveMode", Tips = "@ParamArchiveRecursiveModeTips")]
        public ArchiveEntryCollectionMode ArchiveRecursiveMode
        {
            get { return _archiveRecursiveMode; }
            set { SetProperty(ref _archiveRecursiveMode, value); }
        }

        [DataMember(Name = nameof(ArchiveRecursiveMode))]
        public string ArchiveRecursiveModeString
        {
            get { return ArchiveRecursiveMode.ToString(); }
            set { ArchiveRecursiveMode = value.ToEnum<ArchiveEntryCollectionMode>(); }
        }

        // ページ収集モード
        [IgnoreDataMember]
        [PropertyMember("@ParamBookPageCollectMode", Tips = "@ParamBookPageCollectModeTips")]
        public BookPageCollectMode BookPageCollectMode { get; set; } 

        [DataMember(Name = nameof(BookPageCollectMode))]
        public string BookPageCollectModeString
        {
            get { return BookPageCollectMode.ToString(); }
            set { BookPageCollectMode = value.ToEnum<BookPageCollectMode>(); }
        }

        [DataMember]
        [PropertyMember("@ParamIsRemoveConfirmed")]
        public bool IsRemoveConfirmed { get; set; }

        [DataMember]
        [PropertyMember("@ParamIsRemoveExplorerDialogEnabled", Tips = "@ParamIsRemoveExplorerDialogEnabledTips")]
        public bool IsRemoveExplorerDialogEnabled { get; set; }

        // ネットワークアクセス許可
        [DataMember]
        [PropertyMember("@ParamIsNetworkEnabled", Tips = "@ParamIsNetworkEnabledTips")]
        public bool IsNetworkEnabled
        {
            get { return _isNetworkEnalbe || Environment.IsAppxPackage; } // Appxは強制ON
            set { SetProperty(ref _isNetworkEnalbe, value); }
        }


        private void Constructor()
        {
            Language = LanguageExtensions.GetLanguage(CultureInfo.CurrentCulture.Name);
            ArchiveRecursiveMode = ArchiveEntryCollectionMode.IncludeSubArchives;
            BookPageCollectMode = BookPageCollectMode.ImageAndBook;
            IsRemoveConfirmed = true;
            IsNetworkEnabled = true;
        }

        [OnDeserializing]
        private void OnDeserializing(StreamingContext c)
        {
            Constructor();
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext c)
        {
        }
    }
}