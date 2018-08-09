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
    public interface IPagemarkEntry : IHasPage, IHasName
    {
    }

    [DataContract]
    public class Pagemark : BindableBase, IPagemarkEntry, IVirtualItem
    {
        private string _place;

        public Pagemark(BookMementoUnit unit, string entryName, long length, DateTime? lastWriteTime)
        {
            Place = unit.Place;
            EntryName = entryName;
            Unit = unit;
            Length = length;
            LastWriteTime = lastWriteTime;
        }

        [DataMember]
        public string Place
        {
            get { return _place; }
            set
            {
                if (SetProperty(ref _place, value))
                {
                    _unit = null;
                    RaisePropertyChanged(null);
                }
            }
        }

        [DataMember]
        public string EntryName { get; set; }

        /// <summary>
        /// ファイルサイズ。
        /// </summary>
        [DataMember]
        public long Length { get; set; }

        /// <summary>
        /// ファイル更新日。nullは未設定
        /// </summary>
        [DataMember]
        public DateTime? LastWriteTime { get; set; }


        [OnDeserialized]
        private void Deserialized(StreamingContext c)
        {
            this.EntryName = LoosePath.NormalizeSeparator(this.EntryName);
        }

        public string FullName => LoosePath.Combine(Place, EntryName);
        public string Name => LoosePath.GetFileName(EntryName);
        public string Note => LoosePath.GetFileName(Place);
        public string Detail => Place + "\n" + EntryName;

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

        private BookMementoUnit _unit;
        public BookMementoUnit Unit
        {
            get { return _unit = _unit ?? BookMementoCollection.Current.Set(Place); }
            private set { _unit = value; }
        }


        private volatile ArchivePage _archivePage;
        public ArchivePage ArchivePage
        {
            get
            {
                if (_archivePage == null)
                {
                    _archivePage = new ArchivePage(LoosePath.Combine(Place, EntryName));
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

        // TODO: これはPageで保持するべきか？
        private JobRequest _jobRequest;

        public int DetachCount { get; set; }

        public void Attached()
        {
            /// Debug.WriteLine($"Attach: {Name}");
            _jobRequest?.Cancel();
            _jobRequest = ArchivePage.LoadThumbnail(QueueElementPriority.BookmarkThumbnail);
        }

        public void Detached()
        {
            ////Debug.WriteLine($"Detach: {Name}");
            _jobRequest?.Cancel();
            _jobRequest = null;
        }

        #endregion

        public override string ToString()
        {
            return base.ToString() + " Name:" + Name;
        }

        /// <summary>
        /// 足りない情報の補完
        /// </summary>
        public async Task ValidateAsync()
        {
            if (LastWriteTime != null)
            {
                return;
            }

            try
            {
                using (var entry = await ArchiveFileSystem.CreateArchiveEntry(FullName, CancellationToken.None))
                {
                    this.Length = entry.Length;
                    this.LastWriteTime = entry.LastWriteTime;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Update pagemark parameter faied: {FullName}: {ex.Message}");
            }
        }
    }

}
