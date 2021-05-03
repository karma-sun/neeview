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
        public class Memento : IMemento
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
            public BookSettingPresenter.Memento BookSettingPresenter { get; set; }
            [DataMember]
            public ThemeManager.Memento ThemeProfile { get; set; }
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
            public FileInformation.Memento FileInformation { get; set; }
            [DataMember]
            public ImageFilter.Memento ImageFilter { get; set; }
            [DataMember]
            public ImageEffect.Memento ImageEffect { get; set; }
            [DataMember]
            public SidePanelFrame.Memento SidePanel { get; set; }
            [DataMember]
            public PageViewRecorder.Memento PageViewRecorder { get; set; }

            [Obsolete, DataMember(EmitDefaultValue = false)]
            public FolderListLegacy.Memento FolderList { get; set; }
            [Obsolete, DataMember(Name = "BookSetting", EmitDefaultValue = false)]
            public BookSettingPresenterLegacy.Memento BookSettingPresenterLegacy { get; set; }

            [OnDeserialized]
            private void OnDeserialized(StreamingContext c)
            {
#pragma warning disable CS0612

                // before ver.32
                if (_Version < Environment.GenerateProductVersionNumber(32, 0, 0))
                {
                    SidePanelProfile.ContentItemProfile.ImageWidth = ThumbnailProfile.ThumbnailWidth > 0 ? ThumbnailProfile.ThumbnailWidth : 64;
                    SidePanelProfile.BannerItemProfile.ImageWidth = ThumbnailProfile.BannerWidth > 0 ? ThumbnailProfile.BannerWidth : 200;
                    SidePanelProfile.ContentItemProfile.IsImagePopupEnabled = ThumbnailProfile.IsThumbnailPopup;

                    SidePanelProfile.FontName = SidePanel.FontName;
                    SidePanelProfile.FontSize = SidePanel.FontSize > 0.0 ? SidePanel.FontSize : 15.0;
                    SidePanelProfile.FolderTreeFontSize = SidePanel.FolderTreeFontSize > 0.0 ? SidePanel.FolderTreeFontSize : 12.0;
                    SidePanelProfile.ContentItemProfile.IsTextWrapped = SidePanel.IsTextWrapped;
                    SidePanelProfile.BannerItemProfile.IsTextWrapped = SidePanel.IsTextWrapped;
                    SidePanelProfile.ThumbnailItemProfile.IsTextWrapped = SidePanel.IsTextWrapped;
                }

                // before 33.0
                if (BookshelfFolderList == null && FolderList != null)
                {
                    BookshelfFolderList = FolderListLegacy.ConvertFrom(FolderList);
                }

                // before 34.0
                if (_Version < Environment.GenerateProductVersionNumber(34, 0, 0))
                {
                    if (BookSettingPresenterLegacy != null)
                    {
                        BookSettingPresenter = BookSettingPresenterLegacy.ToBookSettingPresenter();
                    }

                    if (MouseInput.Drag != null)
                    {
                        DragTransformControl.IsOriginalScaleShowMessage = MouseInput.Drag.IsOriginalScaleShowMessage;
                        DragTransformControl.DragControlRotateCenter = MouseInput.Drag.IsControlCenterImage ? DragControlCenter.Target : DragControlCenter.View;
                        DragTransformControl.DragControlScaleCenter = MouseInput.Drag.IsControlCenterImage ? DragControlCenter.Target : DragControlCenter.View;
                        DragTransformControl.DragControlFlipCenter = MouseInput.Drag.IsControlCenterImage ? DragControlCenter.Target : DragControlCenter.View;
                        DragTransformControl.IsKeepScale = MouseInput.Drag.IsKeepScale;
                        DragTransformControl.IsKeepAngle = MouseInput.Drag.IsKeepAngle;
                        DragTransformControl.IsKeepFlip = MouseInput.Drag.IsKeepFlip;
                        DragTransformControl.IsViewStartPositionCenter = MouseInput.Drag.IsViewStartPositionCenter;
                    }
                }
#pragma warning restore CS0612
            }

            public void RestoreConfig(Config config)
            {
                FileIOProfile?.RestoreConfig(config);
                JobEngine?.RestoreConfig(config);
                SoundPlayerService?.RestoreConfig(config);
                PictureProfile?.RestoreConfig(config);
                ImageFilter?.RestoreConfig(config);
                ZipArchiverProfile?.RestoreConfig(config);
                SevenZipArchiverProfile?.RestoreConfig(config);
                PdfArchiverProfile?.RestoreConfig(config);
                MediaArchiverProfile?.RestoreConfig(config);
                ThumbnailProfile?.RestoreConfig(config);
                InfoMessage?.RestoreConfig(config);
                BookProfile?.RestoreConfig(config);
                BookHub?.RestoreConfig(config);
                BookOperation?.RestoreConfig(config);
                BookSettingPresenter?.RestoreConfig(config);
                ThemeProfile?.RestoreConfig(config);
                MainWindowModel?.RestoreConfig(config);
                ContentCanvas?.RestoreConfig(config);
                ContentCanvasBrush?.RestoreConfig(config);
                DragTransform?.RestoreConfig(config);
                DragTransformControl?.RestoreConfig(config);
                LoupeTransform?.RestoreConfig(config);
                MouseInput?.RestoreConfig(config);
                TouchInput?.RestoreConfig(config);
                SlideShow?.RestoreConfig(config);
                WindowTitle?.RestoreConfig(config);
                PageSlider?.RestoreConfig(config);
                MediaControl?.RestoreConfig(config);
                ThumbnailList?.RestoreConfig(config);
                MenuBar?.RestoreConfig(config);
                SidePanelProfile?.RestoreConfig(config);
                PageListPlacementService?.RestoreConfig(config);
                FolderPanel?.RestoreConfig(config);
                BookshelfFolderList?.RestoreConfig(config);
                BookmarkFolderList?.RestoreConfig(config);
                PageList?.RestoreConfig(config);
                HistoryList?.RestoreConfig(config);
                FileInformation?.RestoreConfig(config);
                ImageEffect?.RestoreConfig(config);
                SidePanel?.RestoreConfig(config);
                PageViewRecorder?.RestoreConfig(config);
            }
        }

        #endregion
    }
}
