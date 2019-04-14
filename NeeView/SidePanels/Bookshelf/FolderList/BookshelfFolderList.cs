using NeeLaboratory.ComponentModel;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace NeeView
{
    /// <summary>
    /// BookshelfFolderList
    /// </summary>
    public class BookshelfFolderList : FolderList
    {
        static BookshelfFolderList() => Current = new BookshelfFolderList();
        public static BookshelfFolderList Current { get; }

        private BookshelfFolderList() : base(true, true)
        {
            ApplicationDisposer.Current.Add(this);
        }

        #region Memento

        [DataContract]
        public new class Memento
        {
            [DataMember]
            public FolderList.Memento FolderList { get; set; }

            [DataMember]
            public bool IsVisibleHistoryMark { get; set; }

            [DataMember]
            public bool IsVisibleBookmarkMark { get; set; }

            [DataMember]
            public string Home { get; set; }

            [DataMember, DefaultValue(true)]
            public bool IsInsertItem { get; set; }

            [DataMember]
            public bool IsMultipleRarFilterEnabled { get; set; }

            [DataMember]
            public string ExcludePattern { get; set; }

            [DataMember]
            public bool IsCruise { get; set; }

            [DataMember]
            public bool IsCloseBookWhenMove { get; set; }

            [DataMember, DefaultValue(true)]
            public bool IsIncrementalSearchEnabled { get; set; }

            [DataMember, DefaultValue(true)]
            public bool IsSearchIncludeSubdirectories { get; set; }


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
            memento.IsVisibleHistoryMark = this.IsVisibleHistoryMark;
            memento.IsVisibleBookmarkMark = this.IsVisibleBookmarkMark;
            memento.Home = this.Home;
            memento.IsInsertItem = this.IsInsertItem;
            memento.IsMultipleRarFilterEnabled = this.IsMultipleRarFilterEnabled;
            memento.ExcludePattern = this.ExcludePattern;
            memento.IsCruise = this.IsCruise;
            memento.IsCloseBookWhenMove = this.IsCloseBookWhenMove;
            memento.IsIncrementalSearchEnabled = this.IsIncrementalSearchEnabled;
            memento.IsSearchIncludeSubdirectories = this.IsSearchIncludeSubdirectories;

            return memento;
        }

        public void Restore(Memento memento)
        {
            if (memento == null) return;

            base.Restore(memento.FolderList);
            this.IsVisibleHistoryMark = memento.IsVisibleHistoryMark;
            this.IsVisibleBookmarkMark = memento.IsVisibleBookmarkMark;
            this.Home = memento.Home;
            this.IsInsertItem = memento.IsInsertItem;
            this.IsMultipleRarFilterEnabled = memento.IsMultipleRarFilterEnabled;
            this.ExcludePattern = memento.ExcludePattern;
            this.IsCruise = memento.IsCruise;
            this.IsCloseBookWhenMove = memento.IsCloseBookWhenMove;
            this.IsIncrementalSearchEnabled = memento.IsIncrementalSearchEnabled;
            this.IsSearchIncludeSubdirectories = memento.IsSearchIncludeSubdirectories;
        }

        #endregion
    }

}
