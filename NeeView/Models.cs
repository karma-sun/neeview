using NeeView.ComponentModel;
using NeeView.Effects;
using NeeView.Windows.Property;
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
        public MemoryControl MemoryControl { get; private set; }
        public FileIOProfile FileIOProfile { get; private set; }
        public JobEngine JobEngine { get; private set; }
        public SusieContext SusieContext { get; private set; }
        public BookMementoCollection BookMementoCollection { get; private set; }
        public BookHistory BookHistory { get; private set; }
        public BookmarkCollection BookmarkCollection { get; private set; }
        public PagemarkCollection PagemarkCollection { get; private set; }
        public SevenZipArchiverProfile SevenZipArchiverProfile { get; private set; }
        public ArchiverManager ArchiverManager { get; private set; }
        public BitmapLoaderManager BitmapLoaderManager { get; private set; }
        public DragActionTable DragActionTable { get; private set; }
        public ThumbnailProfile ThumbnailProfile { get; private set; }
        public ThumbnailCache ThumbnailCache { get; private set; }

        //
        public CommandTable CommandTable { get; private set; }
        public RoutedCommandTable RoutedCommandTable { get; private set; }

        public MouseInputContext MouseInputContext { get; private set; }
        public MouseInput MouseInput { get; private set; }
        public ContentDropManager ContentDropManager { get; private set; }

        //
        public InfoMessage InfoMessage { get; private set; }

        //
        public BookProfile BookProfile { get; private set; }
        public BookHub BookHub { get; private set; }
        public BookSetting BookSetting { get; private set; }
        public BookOperation BookOperation { get; private set; }

        //
        public BookHistoryCommand BookHistoryCommand { get; private set; }

        //
        public MainWindowModel MainWindowModel { get; private set; }

        //
        public ContentCanvas ContentCanvas { get; private set; }
        public ContentCanvasBrush ContentCanvasBrush { get; private set; }
        public SlideShow SlideShow { get; private set; }
        public WindowTitle WindowTitle { get; private set; }

        //
        public PageSlider PageSlider { get; private set; }
        public ThumbnailList ThumbnailList { get; private set; }
        public AddressBar AddressBar { get; private set; }
        public MenuBar MenuBar { get; private set; }
        public NowLoading NowLoading { get; private set; }

        //
        public FolderPanelModel FolderPanelModel { get; private set; }
        public FolderList FolderList { get; private set; }
        public PageList PageList { get; private set; }
        public HistoryList HistoryList { get; private set; }
        public BookmarkList BookmarkList { get; private set; }
        public PagemarkList PagemarkList { get; private set; }
        public FileInformation FileInformation { get; private set; }
        public ImageEffect ImageEffect { get; private set; }

        //
        public SidePanel SidePanel { get; set; }

        //
        public Development Development { get; set; }


        /// <summary>
        /// Construcotr
        /// </summary>
        public Models(MainWindow window)
        {
            Current = this;

            MemoryControl = new MemoryControl(App.Current.Dispatcher);
            FileIOProfile = new FileIOProfile();
            JobEngine = new JobEngine();
            BookMementoCollection = new BookMementoCollection();
            BookHistory = new BookHistory();
            BookmarkCollection = new BookmarkCollection();
            PagemarkCollection = new PagemarkCollection();
            SevenZipArchiverProfile = new SevenZipArchiverProfile();
            ArchiverManager = new ArchiverManager();
            BitmapLoaderManager = new BitmapLoaderManager();
            DragActionTable = new DragActionTable();
            SusieContext = new SusieContext();
            ThumbnailProfile = new ThumbnailProfile();
            ThumbnailCache = new ThumbnailCache();


            //
            this.CommandTable = new CommandTable();
            this.RoutedCommandTable = new RoutedCommandTable(window, this.CommandTable);

            this.MouseInputContext = new MouseInputContext();
            this.MouseInputContext.Initialize(window, window.MainView, window.MainContent, window.MainContentShadow);
            this.MouseInput = new MouseInput(this.MouseInputContext);
            this.ContentDropManager = new ContentDropManager(window);

            this.InfoMessage = new InfoMessage();

            this.BookProfile = new BookProfile();
            this.BookSetting = new BookSetting();
            this.BookHub = new BookHub(this.BookSetting);
            this.BookOperation = new BookOperation(this.BookHub);


            // TODO: MainWindowVMをモデル分離してModelとして参照させる？
            this.CommandTable.SetTarget(this);

            this.MainWindowModel = new MainWindowModel();

            this.ContentCanvas = new ContentCanvas(this.MouseInput, this.BookHub);
            this.ContentCanvasBrush = new ContentCanvasBrush(this.ContentCanvas);

            this.SlideShow = new SlideShow(this.BookHub, this.BookOperation, this.MouseInput);
            this.WindowTitle = new WindowTitle(this.ContentCanvas, this.MouseInput.Drag);

            this.ThumbnailList = new ThumbnailList(this.BookOperation, this.BookHub);
            this.PageSlider = new PageSlider(this.BookOperation, this.BookSetting, this.BookHub, this.ThumbnailList);
            this.AddressBar = new AddressBar();
            this.MenuBar = new MenuBar();
            this.NowLoading = new NowLoading();

            this.FolderPanelModel = new FolderPanelModel();
            this.FolderList = new FolderList(this.BookHub, this.FolderPanelModel);
            this.PageList = new PageList(this.BookHub, this.BookOperation);
            this.HistoryList = new HistoryList(this.BookHub);
            this.BookmarkList = new BookmarkList(this.BookHub);
            this.PagemarkList = new PagemarkList(this.BookHub, this.BookOperation);
            this.FileInformation = new FileInformation(this.ContentCanvas);
            this.ImageEffect = new ImageEffect();

            this.BookHistoryCommand = new BookHistoryCommand(this.BookHistory, this.BookHub);

            this.SidePanel = new SidePanel(this);

            this.Development = new Development();
        }

        //
        public void StartEngine()
        {
            this.JobEngine.StartEngine();
            this.SlideShow.StartEngine();
            this.BookHub.StartEngine();
        }

        //
        public void StopEngine()
        {
            this.BookHub.StopEngine();
            this.SlideShow.StopEngine();
            this.JobEngine.StopEngine();
        }

        /// <summary>
        /// Preference反映
        /// TODO: 各モデルで処理
        /// </summary>
        public void ApplyPreference()
        {
            var preference = Preference.Current;

            // banner size
            int bannerWidth = Math.Min(preference.banner_width, 512);
            int bannerHeight = bannerWidth / 4;
            App.Current.Resources["BannerWidth"] = (double)bannerWidth;
            App.Current.Resources["BannerHeight"] = (double)bannerHeight;

            // Jobワーカーサイズ
            ////JobEngine.WorkerSize = preference.loader_thread_size;

            // ワイドページ判定用比率
            ////Page.WideRatio = preference.view_image_wideratio;

            // SevenZip対応拡張子設定
            ////ArchiverManager.UpdateSevenZipSupprtedFileTypes(preference.loader_archiver_7z_supprtfiletypes);

            // 7z.dll の場所
            ////SevenZipArchiver.DllPath = Config.Current.IsX64 ? preference.loader_archiver_7z_dllpath_x64 : preference.loader_archiver_7z_dllpath;

            // SevenZip Lock時間
            ////SevenZipSource.LockTime = preference.loader_archiver_7z_locktime;

#if false
            // マウスジェスチャーの最小移動距離
            NeeView.MouseInput.Current.Gesture.SetGestureMinimumDistance(
                preference.input_gesture_minimumdistance_x,
                preference.input_gesture_minimumdistance_y);
#endif

            // 除外パス更新
            ////BitmapLoaderManager.Excludes = preference.loader_archiver_exclude.Split(';').Select(e => e.Trim()).ToList();
        }

        #region Memento
        [DataContract]
        public class Memento
        {
            [DataMember]
            public int _Version { get; set; }

            [DataMember]
            public MemoryControl.Memento MemoryControl { get; set; }
            [DataMember]
            public FileIOProfile.Memento FileIOProfile { get; set; }
            [DataMember]
            public JobEngine.Memento JobEngine { get; set; }
            [DataMember]
            public SevenZipArchiverProfile.Memento SevenZipArchiverProfile { get; set; }
            [DataMember]
            public ArchiverManager.Memento ArchiverManager { get; set; }
            [DataMember]
            public ThumbnailProfile.Memento ThumbnailProfile { get; set; }
            [DataMember]
            public InfoMessage.Memento InfoMessage { get; set; }
            [DataMember]
            public BookProfile.Memento BookProfile { get; set; }
            [DataMember]
            public BookHub.Memento BookHub { get; set; }
            [DataMember]
            public BookOperation.Memento BookOperation { get; set; }
            [DataMember]
            public BookSetting.Memento BookSetting { get; set; }
            [DataMember]
            public MainWindowModel.Memento MainWindowModel { get; set; }
            [DataMember]
            public ContentCanvas.Memento ContentCanvas { get; set; }
            [DataMember]
            public ContentCanvasBrush.Memento ContentCanvasBrush { get; set; }
            [DataMember]
            public MouseInput.Memento MouseInput { get; set; }
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

            // no used
            [Obsolete, DataMember(EmitDefaultValue = false)]
            public RoutedCommandTable.Memento RoutedCommandTable { get; set; }
        }

        //
        public Memento CreateMemento()
        {
            var memento = new Memento();

            memento._Version = Config.Current.ProductVersionNumber;

            memento.MemoryControl = this.MemoryControl.CreateMemento();
            memento.FileIOProfile = this.FileIOProfile.CreateMemento();
            memento.JobEngine = this.JobEngine.CreateMemento();
            memento.SevenZipArchiverProfile = this.SevenZipArchiverProfile.CreateMemento();
            memento.ArchiverManager = this.ArchiverManager.CreateMemento();
            memento.ThumbnailProfile = this.ThumbnailProfile.CreateMemento();
            memento.InfoMessage = this.InfoMessage.CreateMemento();
            memento.BookProfile = this.BookProfile.CreateMemento();
            memento.BookHub = this.BookHub.CreateMemento();
            memento.BookOperation = this.BookOperation.CreateMemento();
            memento.BookSetting = this.BookSetting.CreateMemento();
            memento.MainWindowModel = this.MainWindowModel.CreateMemento();
            memento.ContentCanvas = this.ContentCanvas.CreateMemento();
            memento.ContentCanvasBrush = this.ContentCanvasBrush.CreateMemento();
            memento.MouseInput = this.MouseInput.CreateMemento();
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
            memento.ImageEffect = this.ImageEffect.CreateMemento();
            memento.SidePanel = this.SidePanel.CreateMemento();
            return memento;
        }

        //
        public void Resore(Memento memento, bool fromLoad)
        {
            if (memento == null) return;
            this.MemoryControl.Restore(memento.MemoryControl);
            this.FileIOProfile.Restore(memento.FileIOProfile);
            this.JobEngine.Restore(memento.JobEngine);
            this.SevenZipArchiverProfile.Restore(memento.SevenZipArchiverProfile);
            this.ArchiverManager.Restore(memento.ArchiverManager);
            this.ThumbnailProfile.Restore(memento.ThumbnailProfile);
            this.InfoMessage.Restore(memento.InfoMessage);
            this.BookProfile.Restore(memento.BookProfile);
            this.BookHub.Restore(memento.BookHub);
            this.BookOperation.Restore(memento.BookOperation);
            this.BookSetting.Restore(memento.BookSetting);
            this.MainWindowModel.Restore(memento.MainWindowModel);
            this.ContentCanvas.Restore(memento.ContentCanvas);
            this.ContentCanvasBrush.Restore(memento.ContentCanvasBrush);
            this.MouseInput.Restore(memento.MouseInput);
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
            this.ImageEffect.Restore(memento.ImageEffect, fromLoad); // TODO: formLoadフラグの扱いを検討
            this.SidePanel.Restore(memento.SidePanel);
        }


#pragma warning disable CS0612

        public void ResoreCompatible(Memento memento)
        {
            if (memento == null) return;

            //
            this.BookHub.RestoreCompatible(memento.BookHub);

            // compatible before ver.23
            if (memento._Version < Config.GenerateProductVersionNumber(1, 23, 0))
            {
                this.RoutedCommandTable.RestoreCompatible(memento.RoutedCommandTable);
            }
        }

#pragma warning restore CS0612


        #endregion
    }
}
