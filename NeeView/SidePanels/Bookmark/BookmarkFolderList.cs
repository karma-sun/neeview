using NeeLaboratory.ComponentModel;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace NeeView
{
    /// <summary>
    /// BookmarkFolderList
    /// </summary>
    public class BookmarkFolderList : FolderList
    {
        static BookmarkFolderList() => Current = new BookmarkFolderList();
        public static BookmarkFolderList Current { get; }

        private BookmarkFolderList() : base(false, false)
        {
            IsSyncBookshelfEnabled = true;

            ApplicationDisposer.Current.Add(this);
        }

        public override void IsVisibleChanged(bool isVisible)
        {
            if (FolderCollection == null)
            {
                RequestPlace(new QueryPath(QueryScheme.Bookmark, null), null, FolderSetPlaceOption.None);
            }
        }

        public override bool CanMoveToParent()
        {
            var parentQuery = FolderCollection?.GetParentQuery();
            if (parentQuery == null) return false;
            return parentQuery.Scheme == QueryScheme.Bookmark;
        }

        public override void Sync()
        {
        }

        public override QueryPath GetFixedHome()
        {
            return new QueryPath(QueryScheme.Bookmark, null, null);
        }

        #region Memento

        [DataContract]
        public new class Memento
        {
            [DataMember]
            public FolderList.Memento FolderList { get; set; }

            [DataMember, DefaultValue(true)]
            public bool IsSyncBookshelfEnabled { get; set; }

            [OnDeserializing]
            private void Deserializing(StreamingContext c)
            {
                this.InitializePropertyDefaultValues();
            }
        }

        public new Memento CreateMemento()
        {
            var memento = new Memento();

            memento.FolderList = base.CreateMemento();
            memento.IsSyncBookshelfEnabled = this.IsSyncBookshelfEnabled;

            return memento;
        }

        public void Restore(Memento memento)
        {
            if (memento == null) return;

            base.Restore(memento.FolderList);
            this.IsSyncBookshelfEnabled = memento.IsSyncBookshelfEnabled;
        }

        #endregion
    }

}
