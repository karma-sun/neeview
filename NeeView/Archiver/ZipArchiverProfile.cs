using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace NeeView
{
    public class ZipArchiverProfile : BindableBase
    {
        static ZipArchiverProfile() => Current = new ZipArchiverProfile();
        public static ZipArchiverProfile Current { get; }


        #region Memento

        [DataContract]
        public class Memento : IMemento
        {
            [DataMember, DefaultValue(true)]
            public bool IsEnabled { get; set; }

            [DataMember, DefaultValue(".zip")]
            public string SupportFileTypes { get; set; }

            public void RestoreConfig(Config config) 
            {
                config.Archive.Zip.IsEnabled = IsEnabled;
                config.Archive.Zip.SupportFileTypes.OneLine = SupportFileTypes;
            }
        }

        public Memento CreateMemento()
        {
            var memento = new Memento();

            memento.IsEnabled = Config.Current.Archive.Zip.IsEnabled;
            memento.SupportFileTypes = Config.Current.Archive.Zip.SupportFileTypes.OneLine;

            return memento;
        }

        #endregion

    }
}
