using NeeLaboratory.ComponentModel;
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

        private BookmarkFolderList() : base(true)
        {
        }

        #region Memento

        [DataContract]
        public new class Memento
        {
            [DataMember]
            public FolderList.Memento FolderList { get; set; }

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

            return memento;
        }

        public void Restore(Memento memento)
        {
            if (memento == null) return;

            base.Restore(memento.FolderList);
        }

        #endregion
    }

}
