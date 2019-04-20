using NeeLaboratory;
using NeeLaboratory.ComponentModel;
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
    /// NeeViewの設定のまとめ
    /// </summary>
    public class Models
    {
        #region Memento
        [DataContract]
        public class Memento
        {
            [DataMember]
            public int _Version { get; set; }

            [DataMember]
            public FileIOProfile.Memento FileIOProfile { get; set; }
            [DataMember]
            public JobEngine.Memento JobEngine { get; set; }
            [DataMember]
            public SoundPlayerService.Memento SoundPlayerService { get; set; }
            [DataMember]
            public PictureProfile.Memento PictureProfile { get; set; }
            [DataMember]
            public ZipArchiverProfile.Memento ZipArchiverProfile { get; set; }
            [DataMember]
            public SevenZipArchiverProfile.Memento SevenZipArchiverProfile { get; set; }
            [DataMember]
            public PdfArchiverProfile.Memento PdfArchiverProfile { get; set; }
            [DataMember]
            public MediaArchiverProfile.Memento MediaArchiverProfile { get; set; }
            [DataMember]
            public ArchiverManager.Memento ArchiverManager { get; set; }
            [DataMember]
            public ThumbnailProfile.Memento ThumbnailProfile { get; set; }
            [DataMember]
            public ExporterProfile.Memento ExporterProfile { get; set; }
            [DataMember]
            public InfoMessage.Memento InfoMessage { get; set; }
            [DataMember]
            public BookProfile.Memento BookProfile { get; set; }
            [DataMember]
            public BookHub.Memento BookHub { get; set; }
            [DataMember]
            public BookOperation.Memento BookOperation { get; set; }
            [DataMember]
            public BookSettingPresenter.Memento BookSettingPresenter { get; set; }
            [DataMember]
            public ThemeProfile.Memento ThemeProfile { get; set; }
            [DataMember]
            public MainWindowModel.Memento MainWindowModel { get; set; }
            [DataMember]
            public ContentCanvas.Memento ContentCanvas { get; set; }
            [DataMember]
            public ContentCanvasBrush.Memento ContentCanvasBrush { get; set; }
            [DataMember]
            public DragTransform.Memento DragTransform { get; set; }
            [DataMember]
            public DragTransformControl.Memento DragTransformControl { get; set; }
            [DataMember]
            public LoupeTransform.Memento LoupeTransform { get; set; }
            [DataMember]
            public MouseInput.Memento MouseInput { get; set; }
            [DataMember]
            public TouchInput.Memento TouchInput { get; set; }
            [DataMember]
            public SlideShow.Memento SlideShow { get; set; }
            [DataMember]
            public WindowTitle.Memento WindowTitle { get; set; }
            [DataMember]
            public PageSlider.Memento PageSlider { get; set; }
            [DataMember]
            public MediaControl.Memento MediaControl { get; set; }
            [DataMember]
            public ThumbnailList.Memento ThumbnailList { get; set; }
            [DataMember]
            public MenuBar.Memento MenuBar { get; set; }
            [DataMember]
            public SidePanelProfile.Memento SidePanelProfile { get; set; }
            [DataMember]
            public PageListPlacementService.Memento PageListPlacementService { get; set; }
            [DataMember]
            public FolderPanelModel.Memento FolderPanel { get; set; }
            [DataMember]
            public BookshelfFolderList.Memento BookshelfFolderList { get; set; }
            [DataMember]
            public BookmarkFolderList.Memento BookmarkFolderList { get; set; }
            [DataMember]
            public PageList.Memento PageList { get; set; }
            [DataMember]
            public HistoryList.Memento HistoryList { get; set; }
            [DataMember]
            public PagemarkList.Memento PagemarkList { get; set; }
            [DataMember]
            public FileInformation.Memento FileInformation { get; set; }
            [DataMember]
            public ImageFilter.Memento ImageFilter { get; set; }
            [DataMember]
            public ImageEffect.Memento ImageEffect { get; set; }
            [DataMember]
            public SidePanelFrameModel.Memento SidePanel { get; set; }

            [Obsolete, DataMember(EmitDefaultValue = false)]
            public RoutedCommandTable.Memento RoutedCommandTable { get; set; }
            [Obsolete, DataMember(EmitDefaultValue = false)]
            public FolderListLegacy.Memento FolderList { get; set; }
            [Obsolete, DataMember(Name = "BookSetting", EmitDefaultValue = false)]
            public BookSettingPresenterLegacy.Memento BookSettingPresenterLegacy { get; set; }

            [OnDeserialized]
            private void Deserialized(StreamingContext c)
            {
#pragma warning disable CS0612
                if (BookshelfFolderList == null && FolderList != null)
                {
                    BookshelfFolderList = FolderListLegacy.ConvertFrom(FolderList);
                }

                if (_Version < Config.GenerateProductVersionNumber(34, 0, 0))
                {
                    if (BookSettingPresenterLegacy != null)
                    {
                        BookSettingPresenter = BookSettingPresenterLegacy.ToBookSettingPresenter();
                    }
                }
#pragma warning restore CS0612
                }
        }

        public Memento CreateMemento()
        {
            var memento = new Memento();

            memento._Version = Config.Current.ProductVersionNumber;

            memento.FileIOProfile = FileIOProfile.Current.CreateMemento();
            memento.JobEngine = JobEngine.Current.CreateMemento();
            memento.SoundPlayerService = SoundPlayerService.Current.CreateMemento();
            memento.PictureProfile = PictureProfile.Current.CreateMemento();
            memento.ImageFilter = ImageFilter.Current.CreateMemento();
            memento.ZipArchiverProfile = ZipArchiverProfile.Current.CreateMemento();
            memento.SevenZipArchiverProfile = SevenZipArchiverProfile.Current.CreateMemento();
            memento.PdfArchiverProfile = PdfArchiverProfile.Current.CreateMemento();
            memento.MediaArchiverProfile = MediaArchiverProfile.Current.CreateMemento();
            memento.ArchiverManager = ArchiverManager.Current.CreateMemento();
            memento.ThumbnailProfile = ThumbnailProfile.Current.CreateMemento();
            memento.ExporterProfile = ExporterProfile.Current.CreateMemento();
            memento.InfoMessage = InfoMessage.Current.CreateMemento();
            memento.BookProfile = BookProfile.Current.CreateMemento();
            memento.BookHub = BookHub.Current.CreateMemento();
            memento.BookOperation = BookOperation.Current.CreateMemento();
            memento.BookSettingPresenter = BookSettingPresenter.Current.CreateMemento();
            memento.ThemeProfile = ThemeProfile.Current.CreateMemento();
            memento.MainWindowModel = MainWindowModel.Current.CreateMemento();
            memento.ContentCanvas = ContentCanvas.Current.CreateMemento();
            memento.ContentCanvasBrush = ContentCanvasBrush.Current.CreateMemento();
            memento.DragTransform = DragTransform.Current.CreateMemento();
            memento.DragTransformControl = DragTransformControl.Current.CreateMemento();
            memento.LoupeTransform = LoupeTransform.Current.CreateMemento();
            memento.MouseInput = MouseInput.Current.CreateMemento();
            memento.TouchInput = TouchInput.Current.CreateMemento();
            memento.SlideShow = SlideShow.Current.CreateMemento();
            memento.WindowTitle = WindowTitle.Current.CreateMemento();
            memento.PageSlider = PageSlider.Current.CreateMemento();
            memento.MediaControl = MediaControl.Current.CreateMemento();
            memento.ThumbnailList = ThumbnailList.Current.CreateMemento();
            memento.MenuBar = MenuBar.Current.CreateMemento();
            memento.SidePanelProfile = SidePanelProfile.Current.CreateMemento();
            memento.PageListPlacementService = PageListPlacementService.Current.CreateMemento();
            memento.FolderPanel = FolderPanelModel.Current.CreateMemento();
            memento.BookshelfFolderList = BookshelfFolderList.Current.CreateMemento();
            memento.BookmarkFolderList = BookmarkFolderList.Current.CreateMemento();
            memento.PageList = PageList.Current.CreateMemento();
            memento.HistoryList = HistoryList.Current.CreateMemento();
            memento.PagemarkList = PagemarkList.Current.CreateMemento();
            memento.FileInformation = FileInformation.Current.CreateMemento();
            memento.ImageEffect = ImageEffect.Current.CreateMemento();
            memento.SidePanel = SidePanel.Current.CreateMemento();
            return memento;
        }

        public void Resore(Memento memento)
        {
            if (memento == null) return;
            FileIOProfile.Current.Restore(memento.FileIOProfile);
            JobEngine.Current.Restore(memento.JobEngine);
            SoundPlayerService.Current.Restore(memento.SoundPlayerService);
            PictureProfile.Current.Restore(memento.PictureProfile);
            ImageFilter.Current.Restore(memento.ImageFilter);
            ZipArchiverProfile.Current.Restore(memento.ZipArchiverProfile);
            SevenZipArchiverProfile.Current.Restore(memento.SevenZipArchiverProfile);
            PdfArchiverProfile.Current.Restore(memento.PdfArchiverProfile);
            MediaArchiverProfile.Current.Restore(memento.MediaArchiverProfile);
            ArchiverManager.Current.Restore(memento.ArchiverManager);
            ThumbnailProfile.Current.Restore(memento.ThumbnailProfile);
            ExporterProfile.Current.Restore(memento.ExporterProfile);
            InfoMessage.Current.Restore(memento.InfoMessage);
            BookProfile.Current.Restore(memento.BookProfile);
            BookHub.Current.Restore(memento.BookHub);
            BookOperation.Current.Restore(memento.BookOperation);
            BookSettingPresenter.Current.Restore(memento.BookSettingPresenter);
            ThemeProfile.Current.Restore(memento.ThemeProfile);
            MainWindowModel.Current.Restore(memento.MainWindowModel);
            ContentCanvas.Current.Restore(memento.ContentCanvas);
            ContentCanvasBrush.Current.Restore(memento.ContentCanvasBrush);
            DragTransform.Current.Restore(memento.DragTransform);
            DragTransformControl.Current.Restore(memento.DragTransformControl);
            LoupeTransform.Current.Restore(memento.LoupeTransform);
            MouseInput.Current.Restore(memento.MouseInput);
            TouchInput.Current.Restore(memento.TouchInput);
            SlideShow.Current.Restore(memento.SlideShow);
            WindowTitle.Current.Restore(memento.WindowTitle);
            PageSlider.Current.Restore(memento.PageSlider);
            MediaControl.Current.Restore(memento.MediaControl);
            ThumbnailList.Current.Restore(memento.ThumbnailList);
            MenuBar.Current.Restore(memento.MenuBar);
            SidePanelProfile.Current.Restore(memento.SidePanelProfile);
            PageListPlacementService.Current.Restore(memento.PageListPlacementService);
            FolderPanelModel.Current.Restore(memento.FolderPanel);
            BookshelfFolderList.Current.Restore(memento.BookshelfFolderList);
            BookmarkFolderList.Current.Restore(memento.BookmarkFolderList);
            PageList.Current.Restore(memento.PageList);
            HistoryList.Current.Restore(memento.HistoryList);
            PagemarkList.Current.Restore(memento.PagemarkList);
            FileInformation.Current.Restore(memento.FileInformation);
            ImageEffect.Current.Restore(memento.ImageEffect);
            SidePanel.Current.Restore(memento.SidePanel);
        }

        public void ResoreCompatible(Memento memento)
        {
            if (memento == null) return;

            BookHub.Current.RestoreCompatible(memento.BookHub);

#pragma warning disable CS0612
            // compatible before ver.23
            if (memento._Version < Config.GenerateProductVersionNumber(1, 23, 0))
            {
                RoutedCommandTable.Current.RestoreCompatible(memento.RoutedCommandTable);
            }
#pragma warning restore CS0612

            ThumbnailProfile.Current.RestoreCompatible(memento.ThumbnailProfile);
            SidePanel.Current.RestoreCompatible(memento.SidePanel);
        }

        #endregion
    }
}
