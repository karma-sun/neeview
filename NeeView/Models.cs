using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace NeeView
{
    public class Models
    {
        private static Models _current;
        public static Models Current { get { return _current = _current ?? new Models(); } }

        //
        public BookHub BookHub { get; private set; }

        //
        public HistoryList HistoryList { get; private set; }
        public BookmarkList BookmarkList { get; private set; }
        public PagemarkList PagemarkList { get; private set; }


        //
        public Models()
        {
            InitializeModels();
        }

        //
        private void InitializeModels()
        {
            this.BookHub = new BookHub();

            this.HistoryList = new HistoryList(this.BookHub);
            this.BookmarkList = new BookmarkList(this.BookHub);
            this.PagemarkList = new PagemarkList(this.BookHub);
        }



        #region Memento
        [DataContract]
        public class Memento
        {
            [DataMember]
            public HistoryList.Memento HistoryList;
            [DataMember]
            public BookmarkList.Memento BookmarkList;
            [DataMember]
            public PagemarkList.Memento PagemarkList;
        }

        //
        public Memento CreateMemento()
        {
            var memento = new Memento();
            memento.HistoryList = this.HistoryList.CreateMemento();
            memento.BookmarkList = this.BookmarkList.CreateMemento();
            memento.PagemarkList = this.PagemarkList.CreateMemento();
            return memento;
        }

        //
        public void Resore(Memento memento)
        {
            if (memento == null) return;
            this.HistoryList.Restore(memento.HistoryList);
            this.BookmarkList.Restore(memento.BookmarkList);
            this.PagemarkList.Restore(memento.PagemarkList);
        }
        #endregion
    }
}
