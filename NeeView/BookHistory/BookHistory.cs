using NeeLaboratory.ComponentModel;
using NeeView.Collections;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Text;
using System.Windows.Data;

namespace NeeView
{
    [DataContract]
    public class BookHistory : BindableBase, IHasPage, IHasName
    {
        private string _path;

        public BookHistory()
        {
        }

        public BookHistory(BookMementoUnit unit, DateTime lastAccessTime)
        {
            Path = unit.Path;
            LastAccessTime = lastAccessTime;
            Unit = unit;
        }

        [DataMember(Name = "Place")]
        public string Path
        {
            get { return _path; }
            set
            {
                if (SetProperty(ref _path, value))
                {
                    _unit = null;
                    RaisePropertyChanged(null);
                }
            }
        }


        [DataMember]
        public DateTime LastAccessTime { get; set; }


        public Page ArchivePage => Unit.ArchivePage;

        public string Name => Unit.Memento.Name;
        public string Note => Unit.ArchivePage.Entry?.RootArchiverName;
        public string Detail => Path + "\n" + LastAccessTime;

        public IThumbnail Thumbnail => Unit.ArchivePage.Thumbnail;

        private BookMementoUnit _unit;
        public BookMementoUnit Unit
        {
            get { return _unit = _unit ?? BookMementoCollection.Current.Set(Path); }
            private set { _unit = value; }
        }

        public override string ToString()
        {
            return Path ?? base.ToString();
        }

        public Page GetPage()
        {
            return Unit.ArchivePage;
        }
    }
}
