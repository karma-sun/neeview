using NeeView.Effects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        public FolderPanelModel FolderPanelModel { get; private set; }
        public FolderList FolderList { get; private set; }
        public PageList PageList { get; private set; }
        public HistoryList HistoryList { get; private set; }
        public BookmarkList BookmarkList { get; private set; }
        public PagemarkList PagemarkList { get; private set; }
        public FileInformation FileInformation { get; private set; }
        public ImageEffect ImageEffecct { get; private set; }

        //
        public Models()
        {
            // TODO: VMを渡すのはよろしくない
            //var vm = MainWindowVM.Current;
            //Debug.Assert(vm != null);

            this.BookHub = new BookHub();

            this.FolderPanelModel = new FolderPanelModel();
            this.FolderList = new FolderList(this.BookHub, this.FolderPanelModel);
            this.PageList = new PageList(this.BookHub);
            this.HistoryList = new HistoryList(this.BookHub);
            this.BookmarkList = new BookmarkList(this.BookHub);
            this.PagemarkList = new PagemarkList(this.BookHub);
            this.FileInformation = new FileInformation();
            this.ImageEffecct = new ImageEffect();
        }



        #region Memento
        [DataContract]
        public class Memento
        {
            [DataMember]
            public FolderPanelModel.Memento FolderPanel { get; set; }
            [DataMember]
            public FolderList.Memento FolderList { get; set; }
            [DataMember]
            public PageList.Memento PageList { get; set; }
            [DataMember]
            public HistoryList.Memento HistoryList { get; set; }
            [DataMember]
            public BookmarkList.Memento BookmarkList { get; set; }
            [DataMember]
            public PagemarkList.Memento PagemarkList { get; set; }
            [DataMember]
            public FileInformation.Memento FileInformation { get; set; }
            [DataMember]
            public ImageEffect.Memento ImageEffect { get; set; }
        }

        //
        public Memento CreateMemento()
        {
            var memento = new Memento();
            memento.FolderPanel = this.FolderPanelModel.CreateMemento();
            memento.FolderList = this.FolderList.CreateMemento();
            memento.PageList = this.PageList.CreateMemento();
            memento.HistoryList = this.HistoryList.CreateMemento();
            memento.BookmarkList = this.BookmarkList.CreateMemento();
            memento.PagemarkList = this.PagemarkList.CreateMemento();
            memento.FileInformation = this.FileInformation.CreateMemento();
            memento.ImageEffect = this.ImageEffecct.CreateMemento();
            return memento;
        }

        //
        public void Resore(Memento memento, bool fromLoad)
        {
            if (memento == null) return;
            this.FolderPanelModel.Restore(memento.FolderPanel);
            this.FolderList.Restore(memento.FolderList);
            this.PageList.Restore(memento.PageList);
            this.HistoryList.Restore(memento.HistoryList);
            this.BookmarkList.Restore(memento.BookmarkList);
            this.PagemarkList.Restore(memento.PagemarkList);
            this.FileInformation.Restore(memento.FileInformation);
            this.ImageEffecct.Restore(memento.ImageEffect, fromLoad);
        }
        #endregion
    }
}
