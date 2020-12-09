using NeeLaboratory.ComponentModel;
using NeeView.Collections;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    public interface IPagemarkEntry : IHasName
    {
        string Path { get; }
        string DispName { get; }
    }

    [DataContract]
    public class Pagemark : BindableBase, IPagemarkEntry, IVirtualItem, IHasPage
    {
        private string _path;
        private string _entryName;

        public Pagemark(string path, string entryName)
        {
            Path = path;
            EntryName = entryName;
        }


        [DataMember(Name = "Place")]
        public string Path
        {
            get { return _path; }
            set
            {
                if (SetProperty(ref _path, value))
                {
                    RaisePropertyChanged(null);
                }
            }
        }

        [DataMember]
        public string EntryName
        {
            get { return _entryName; }
            set
            {
                if (SetProperty(ref _entryName, value))
                {
                    RaisePropertyChanged(nameof(DispName));
                }
            }
        }


        [DataMember(Name = "DispName", EmitDefaultValue = false)]
        private string _dispName;
        public string DispName
        {
            get { return _dispName ?? LoosePath.GetFileName(EntryName); }
            set { SetProperty(ref _dispName, (string.IsNullOrWhiteSpace(value) || value == LoosePath.GetFileName(EntryName)) ? null : value); }
        }

        public string DispNameRaw => _dispName;


        [OnDeserialized]
        private void Deserialized(StreamingContext c)
        {
            this.EntryName = LoosePath.NormalizeSeparator(this.EntryName);
        }

        public string FullName => LoosePath.Combine(Path, EntryName);
        public string Name => EntryName;
        public string Note => LoosePath.GetFileName(Path);
        public string Detail => EntryName;

        public IThumbnail Thumbnail
        {
            get
            {
                if (PagemarkList.Current.IsThumbnailVisibled)
                {
                    PagemarkListVertualCollection.Current.Attach(this);
                }
                return ArchivePage.Thumbnail;
            }
        }

        private Page _archivePage;
        public Page ArchivePage
        {
            get
            {
                if (_archivePage == null)
                {
                    _archivePage = new Page("", new ArchiveContent(LoosePath.Combine(Path, EntryName)));
                    _archivePage.Thumbnail.IsCacheEnabled = true;
                    _archivePage.Thumbnail.Touched += Thumbnail_Touched;
                }
                return _archivePage;
            }
        }

        private void Thumbnail_Touched(object sender, EventArgs e)
        {
            var thumbnail = (Thumbnail)sender;
            BookThumbnailPool.Current.Add(thumbnail);
        }

        public Page GetPage()
        {
            return ArchivePage;
        }


        #region IVirtualItem

        public int DetachCount { get; set; }

        public void Attached()
        {
        }

        public void Detached()
        {
        }

        #endregion


        public bool IsEqual(IPagemarkEntry entry)
        {
            return entry is Pagemark pagemark && this.Name == pagemark.Name && this.Path == pagemark.Path;
        }

        public override string ToString()
        {
            return base.ToString() + " Name:" + Name;
        }
    }

}
