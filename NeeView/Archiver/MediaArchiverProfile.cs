using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System;
using System.Runtime.Serialization;

namespace NeeView
{
    public class MediaArchiverProfile : BindableBase
    {
        static MediaArchiverProfile() => Current = new MediaArchiverProfile();
        public static MediaArchiverProfile Current { get; }


        #region Memento

        [DataContract]
        public class Memento : IMemento
        {
            [DataMember]
            public bool IsEnabled { get; set; }

            [DataMember]
            public string SupportFileTypes { get; set; }

            public void RestoreConfig(Config config)
            {
                config.Archive.Media.IsEnabled = IsEnabled;
                config.Archive.Media.SupportFileTypes.OneLine = SupportFileTypes;
            }
        }

        public Memento CreateMemento()
        {
            var memento = new Memento();

            memento.IsEnabled = Config.Current.Archive.Media.IsEnabled;
            memento.SupportFileTypes = Config.Current.Archive.Media.SupportFileTypes.OneLine;

            return memento;
        }

        #endregion

    }
}
