using NeeView.ComponentModel;
using NeeView.Effects;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace NeeView
{
    /// <summary>
    /// NeeView全体のモデル。
    /// 各Modelのインスタンスを管理する。
    /// </summary>
    public class Models : BindableBase, IEngine
    {
        // System Object
        public static Models Current { get; private set; }

        //
        public CommandTable CommandTable { get; private set; }
        public RoutedCommandTable RoutedCommandTable { get; private set; }

        //
        public InfoMessage InfoMessage { get; private set; }

        //
        public BookHub BookHub { get; private set; }
        public BookOperation BookOperation { get; private set; }

        //
        public ContentCanvasTransform ContentCanvasTransform { get; private set; }
        public ContentCanvas ContentCanvas { get; private set; }
        public MouseInput MouseInput { get; private set; }
        public SlideShow SlideShow { get; private set; }
        public WindowTitle WindowTitle { get; private set; }

        //
        public PageSlider PageSlider { get; private set; }
        public ThumbnailList ThumbnailList { get; private set; }

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
        public SidePanel SidePanel { get; set; }



        /// <summary>
        /// Construcotr
        /// </summary>
        public Models()
        {
            Current = this;

            this.CommandTable = new CommandTable();
            this.RoutedCommandTable = new RoutedCommandTable(this.CommandTable);

            this.InfoMessage = new InfoMessage();

            this.BookOperation = new BookOperation();
            this.BookHub = new BookHub(this.BookOperation);

            // TODO: MainWindowVMをモデル分離してModelとして参照させる？
            this.CommandTable.SetTarget(this, MainWindowVM.Current);

            this.ContentCanvasTransform = new ContentCanvasTransform();
            this.ContentCanvas = new ContentCanvas(this.ContentCanvasTransform, this.BookHub);
            this.MouseInput = new MouseInput();
            this.SlideShow = new SlideShow(this.BookHub, this.BookOperation, this.MouseInput);
            this.WindowTitle = new WindowTitle(this.ContentCanvas, this.ContentCanvasTransform);

            this.ThumbnailList = new ThumbnailList(this.BookOperation, this.BookHub);
            this.PageSlider = new PageSlider(this.BookOperation, this.BookHub, this.ThumbnailList);

            this.FolderPanelModel = new FolderPanelModel();
            this.FolderList = new FolderList(this.BookHub, this.FolderPanelModel);
            this.PageList = new PageList(this.BookHub, this.BookOperation);
            this.HistoryList = new HistoryList(this.BookHub);
            this.BookmarkList = new BookmarkList(this.BookHub);
            this.PagemarkList = new PagemarkList(this.BookHub, this.BookOperation);
            this.FileInformation = new FileInformation(this.ContentCanvas);
            this.ImageEffecct = new ImageEffect();

            this.SidePanel = new SidePanel(this);
        }

        //
        public void StartEngine()
        {
            this.SlideShow.StartEngine();
        }

        //
        public void StopEngine()
        {
            this.SlideShow.StopEngine();
        }

        #region Memento
        [DataContract]
        public class Memento
        {
            [DataMember]
            public RoutedCommandTable.Memento RoutedCommandTable { get; set; }
            [DataMember]
            public BookOperation.Memento BookOperation { get; set; }
            [DataMember]
            public ContentCanvasTransform.Memento ContentCanvasTransform { get; set; }
            [DataMember]
            public ContentCanvas.Memento ContentCanvas { get; set; }
            [DataMember]
            public SlideShow.Memento SlideShow { get; set; }
            [DataMember]
            public WindowTitle.Memento WindowTitle { get; set; }
            [DataMember]
            public PageSlider.Memento PageSlider { get; set; }
            [DataMember]
            public ThumbnailList.Memento ThumbnailList { get; set; }
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
            [DataMember]
            public SidePanelFrameModel.Memento SidePanel { get; set; }
        }

        //
        public Memento CreateMemento()
        {
            var memento = new Memento();
            memento.RoutedCommandTable = this.RoutedCommandTable.CreateMemento();
            memento.BookOperation = this.BookOperation.CreateMemento();
            memento.ContentCanvasTransform = this.ContentCanvasTransform.CreateMemento();
            memento.ContentCanvas = this.ContentCanvas.CreateMemento();
            memento.SlideShow = this.SlideShow.CreateMemento();
            memento.WindowTitle = this.WindowTitle.CreateMemento();
            memento.PageSlider = this.PageSlider.CreateMemento();
            memento.ThumbnailList = this.ThumbnailList.CreateMemento();
            memento.FolderPanel = this.FolderPanelModel.CreateMemento();
            memento.FolderList = this.FolderList.CreateMemento();
            memento.PageList = this.PageList.CreateMemento();
            memento.HistoryList = this.HistoryList.CreateMemento();
            memento.BookmarkList = this.BookmarkList.CreateMemento();
            memento.PagemarkList = this.PagemarkList.CreateMemento();
            memento.FileInformation = this.FileInformation.CreateMemento();
            memento.ImageEffect = this.ImageEffecct.CreateMemento();
            memento.SidePanel = this.SidePanel.CreateMemento();
            return memento;
        }

        //
        public void Resore(Memento memento, bool fromLoad)
        {
            if (memento == null) return;
            this.RoutedCommandTable.Restore(memento.RoutedCommandTable);
            this.BookOperation.Restore(memento.BookOperation);
            this.ContentCanvasTransform.Restore(memento.ContentCanvasTransform);
            this.ContentCanvas.Restore(memento.ContentCanvas);
            this.SlideShow.Restore(memento.SlideShow);
            this.WindowTitle.Restore(memento.WindowTitle);
            this.PageSlider.Restore(memento.PageSlider);
            this.ThumbnailList.Restore(memento.ThumbnailList);
            this.FolderPanelModel.Restore(memento.FolderPanel);
            this.FolderList.Restore(memento.FolderList);
            this.PageList.Restore(memento.PageList);
            this.HistoryList.Restore(memento.HistoryList);
            this.BookmarkList.Restore(memento.BookmarkList);
            this.PagemarkList.Restore(memento.PagemarkList);
            this.FileInformation.Restore(memento.FileInformation);
            this.ImageEffecct.Restore(memento.ImageEffect, fromLoad); // TODO: formLoadフラグの扱いを検討
            this.SidePanel.Restore(memento.SidePanel);
        }
        #endregion
    }
}
