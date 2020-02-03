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
        string DispName { get; }
    }

    [DataContract]
    public class Pagemark : BindableBase, IPagemarkEntry, IVirtualItem, IHasPage
    {
        private string _place;
        private string _entryName;

        public Pagemark(string place, string entryName)
        {
            Place = place;
            EntryName = entryName;
        }

        [DataMember]
        public string Place
        {
            get { return _place; }
            set
            {
                if (SetProperty(ref _place, value))
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
            set { SetProperty(ref _dispName, value); }
        }


        [OnDeserialized]
        private void Deserialized(StreamingContext c)
        {
            this.EntryName = LoosePath.NormalizeSeparator(this.EntryName);
        }

        public string FullName => LoosePath.Combine(Place, EntryName);
        public string Name => EntryName;
        public string Note => LoosePath.GetFileName(Place);
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
                    _archivePage = new Page("", new ArchiveContent(LoosePath.Combine(Place, EntryName)));
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
            return entry is Pagemark pagemark && this.Name == pagemark.Name && this.Place == pagemark.Place;
        }

        public override string ToString()
        {
            return base.ToString() + " Name:" + Name;
        }
    }

}
