using NeeLaboratory.ComponentModel;
using NeeView.Data;
using NeeView.Windows.Property;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

namespace NeeView
{
    public class ConfigAccessor
    {
        private Config _config;

        public ConfigAccessor(Config source)
        {
            _config = source;
        }



        public byte[] Serialize()
        {
            return Json.SerializeRaw(_config, null, true);
        }

        public void RestoreSerialized(byte[] memento)
        {
            var source = Json.Deserialize<Config>(memento);

            // TODO: Version互換性

            OverwriteProperties(source, _config);
        }

        /// <summary>
        /// 他のインスタンスへプロパティを上書き
        /// </summary>
        /// <param name="src"></param>
        /// <param name="dst"></param>
        private static void OverwriteProperties(object src, object dst)
        {
            var type = src.GetType();
            if (type != dst.GetType()) throw new InvalidOperationException();

            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var property in properties)
            {
                // NOTE: DataMember
                var attribute = property.GetCustomAttribute(typeof(DataMemberAttribute));
                if (attribute == null) continue;

                if (property.PropertyType.IsClass && property.PropertyType != typeof(string))
                {
                    OverwriteProperties(property.GetValue(src), property.GetValue(dst));
                }
                else
                {
                    property.GetSetMethod()?.Invoke(dst, new object[] { property.GetValue(src) });
                }
            }
        }

        // 従来のMemento形式
        #region Memento

        [DataContract]
        public class Memento
        {
            [DataMember]
            public Config Config { get; set; }

            [OnDeserializing]
            private void Deserializing(StreamingContext c)
            {
            }
        }

        public Memento CreateMemento()
        {
            var memento = new Memento();
            memento.Config = _config;

            Debug.WriteLine(Encoding.UTF8.GetString(Serialize()));

            return memento;
        }

        public void Restore(Memento memento)
        {
            if (memento == null) return;

            OverwriteProperties(memento.Config, _config);
        }

        #endregion

    }


    [DataContract]
    public class Config : BindableBase
    {
        public static Config Current { get; } = new Config();


        public Config()
        {
            Constructor();
        }


        [DataMember]
        public int _Version { get; set; }

        [DataMember]
        public SystemConfig System { get; set; }

        [DataMember]
        public PerformanceConfig Performance { get; set; }


        private void Constructor()
        {
            _Version = Environment.ProductVersionNumber;
            System = new SystemConfig();
            Performance = new PerformanceConfig();
        }

        [OnDeserializing]
        private void OnDeserializing(StreamingContext c)
        {
            Constructor();
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext c)
        {
            // データの互換性保持はここで行う？
        }
    }


    [DataContract]
    public class SystemConfig : BindableBase
    {
        private ArchiveEntryCollectionMode _archiveRecursiveMode;


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


        private void Constructor()
        {
            Language = LanguageExtensions.GetLanguage(CultureInfo.CurrentCulture.Name);
            ArchiveRecursiveMode = ArchiveEntryCollectionMode.IncludeSubArchives;
            BookPageCollectMode = BookPageCollectMode.ImageAndBook;
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