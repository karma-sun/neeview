using NeeView.Windows.Controls;
using NeeLaboratory.Windows.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media.Effects;
using System.Windows.Shapes;


namespace NeeView
{
    /// <summary>
    /// データ互換用
    /// </summary>
    [Obsolete]
    public class MainWindowVM
    {
        /// <summary>
        /// 旧フォルダーリスト設定。
        /// </summary>
        [Obsolete, DataContract]
        public class FolderListSetting
        {
            [DataMember]
            public bool IsVisibleHistoryMark { get; set; }

            [DataMember]
            public bool IsVisibleBookmarkMark { get; set; }
        }

        #region Memento

        [Obsolete, DataContract]
        public class Memento
        {
            [Obsolete, DataMember(EmitDefaultValue = false)]
            public int _Version { get; set; }

            [Obsolete, DataMember(EmitDefaultValue = false)]
            public bool IsLimitMove { get; set; } // no used (ver.23)

            [Obsolete, DataMember(EmitDefaultValue = false)]
            public bool IsControlCenterImage { get; set; } // no used (ver.23)

            [Obsolete, DataMember(EmitDefaultValue = false)]
            public bool IsAngleSnap { get; set; } // no used (ver.23)

            [Obsolete, DataMember(Order = 19, EmitDefaultValue = false)]
            public double AngleFrequency { get; set; } // no used (ver.23)

            [Obsolete, DataMember(EmitDefaultValue = false)]
            public bool IsViewStartPositionCenter { get; set; } // no used (ver.23)

            [Obsolete, DataMember(EmitDefaultValue = false)]
            public PageStretchMode StretchMode { get; set; } // no used (ver.23)

            [Obsolete, DataMember(EmitDefaultValue = false)]
            public BackgroundStyle Background { get; set; } // no used (ver.23)

            [Obsolete, DataMember(EmitDefaultValue = false)]
            public bool IsSliderDirectionReversed { get; set; } // no used

            [Obsolete, DataMember(Order = 4, EmitDefaultValue = false)]
            public ShowMessageStyle NoticeShowMessageStyle { get; set; } // no used (ver.23)

            [Obsolete, DataMember(EmitDefaultValue = false)]
            public ShowMessageStyle CommandShowMessageStyle { get; set; } // no used (ver.22)

            [Obsolete, DataMember(EmitDefaultValue = false)]
            public ShowMessageStyle GestureShowMessageStyle { get; set; } // no used (ver.23)

            [Obsolete, DataMember(Order = 4, EmitDefaultValue = false)]
            public ShowMessageStyle NowLoadingShowMessageStyle { get; set; } // no used (ver.23)

            [Obsolete, DataMember(Order = 1, EmitDefaultValue = false)]
            public bool IsEnabledNearestNeighbor { get; set; } // no used (ver.22)

            [Obsolete, DataMember(Order = 2, EmitDefaultValue = false)]
            public bool IsKeepScale { get; set; } // no used(ver.23)

            [Obsolete, DataMember(Order = 2, EmitDefaultValue = false)]
            public bool IsKeepAngle { get; set; }  // no used(ver.23)

            [Obsolete, DataMember(Order = 4, EmitDefaultValue = false)]
            public bool IsKeepFlip { get; set; } // no used(ver.23)

            [Obsolete, DataMember(Order = 2, EmitDefaultValue = false)]
            public bool IsLoadLastFolder { get; set; } // no used (ver.22)

            [Obsolete, DataMember(Order = 2, EmitDefaultValue = false)]
            public bool IsDisableMultiBoot { get; set; } // no used (ver.23)

            [Obsolete, DataMember(Order = 4, EmitDefaultValue = false)]
            public bool IsAutoPlaySlideShow { get; set; } // no used (ver.23)

            [Obsolete, DataMember(Order = 7, EmitDefaultValue = false)]
            public bool IsSaveWindowPlacement { get; set; } // no used (ver.23)

            [Obsolete, DataMember(Order = 2, EmitDefaultValue =false)]
            public bool IsHideMenu { get; set; }

            [Obsolete, DataMember(Order = 4, EmitDefaultValue = false)]
            public bool IsHideTitleBar { get; set; } // no used

            [Obsolete, DataMember(Order = 8, EmitDefaultValue = false)]
            public bool IsVisibleTitleBar { get; set; } // no used (ver.22)

            [Obsolete, DataMember(Order = 4, EmitDefaultValue = false)]
            public bool IsSaveFullScreen { get; set; } // no used (ver.23)

            [Obsolete, DataMember(Order = 4, EmitDefaultValue = false)]
            public bool IsTopmost { get; set; } // no used (ver.22)

            [Obsolete, DataMember(Order = 5, EmitDefaultValue = false)]
            public FileInfoSetting FileInfoSetting { get; set; } // no used

            [Obsolete, DataMember(Order = 5, EmitDefaultValue = false)]
            public string UserDownloadPath { get; set; } // no used (ver.23)

            [Obsolete, DataMember(Order = 6, EmitDefaultValue = false)]
            public FolderListSetting FolderListSetting { get; set; } // no used

            [Obsolete, DataMember(Order = 6, EmitDefaultValue = false)]
            public PanelColor PanelColor { get; set; } // no used (ver.23)

            [Obsolete, DataMember(Order = 7, EmitDefaultValue = false)]
            public string WindowTitleFormat1 { get; set; } // no used (ver.23)

            [Obsolete, DataMember(Order = 7, EmitDefaultValue = false)]
            public string WindowTitleFormat2 { get; set; } // no used (ver.23)

            [Obsolete, DataMember(Order = 8, EmitDefaultValue = false)]
            public bool IsVisibleAddressBar { get; set; }

            [Obsolete, DataMember(Order = 8, EmitDefaultValue = false)]
            public bool IsHidePanel { get; set; }

            [Obsolete, DataMember(Order = 8, EmitDefaultValue = false)]
            public bool IsHidePanelInFullscreen { get; set; }

            [Obsolete, DataMember(Order = 8, EmitDefaultValue = false)]
            public ContextMenuSetting ContextMenuSetting { get; set; } // no used (ver.23)

            [Obsolete, DataMember(Order = 8, EmitDefaultValue = false)]
            public bool IsEnableThumbnailList { get; set; } // no used (ver.23)

            [Obsolete, DataMember(Order = 8, EmitDefaultValue = false)]
            public bool IsHideThumbnailList { get; set; } // no used (ver.23)

            [Obsolete, DataMember(Order = 8, EmitDefaultValue = false)]
            public double ThumbnailSize { get; set; } // no used (ver.23)

            [Obsolete, DataMember(Order = 8, EmitDefaultValue = false)]
            public bool IsSliderLinkedThumbnailList { get; set; } // no used (ver.23)

            [Obsolete, DataMember(Order = 8, EmitDefaultValue = false)]
            public bool IsVisibleThumbnailNumber { get; set; } // no used (ver.23)

            [Obsolete, DataMember(Order = 9, EmitDefaultValue = false)]
            public bool IsAutoGC { get; set; } // no used (ver.23)

            [Obsolete, DataMember(Order = 9, EmitDefaultValue = false)]
            public bool IsVisibleThumbnailPlate { get; set; } // no used (ver.23)

            [Obsolete, DataMember(Order = 10, EmitDefaultValue = false)]
            public ShowMessageStyle ViewTransformShowMessageStyle { get; set; } // no used (ver.23)

            [Obsolete, DataMember(Order = 10, EmitDefaultValue = false)]
            public bool IsOriginalScaleShowMessage { get; set; } // no used (ver.23)

            [Obsolete, DataMember(Order = 12, EmitDefaultValue = false)]
            public double ContentsSpace { get; set; } // no used (ver.23)

            [Obsolete, DataMember(Order = 12, EmitDefaultValue = false)]
            public LongButtonDownMode LongLeftButtonDownMode { get; set; } // no used (ver.23)

            [Obsolete, DataMember(Order = 16, EmitDefaultValue = false)]
            public SliderDirection SliderDirection { get; set; } // no used (ver.23)

            [Obsolete, DataMember(Order = 17, EmitDefaultValue = false)]
            public bool IsHidePageSlider { get; set; }

            [Obsolete, DataMember(Order = 18, EmitDefaultValue = false)]
            public bool IsAutoRotate { get; set; } // no used (ver.23)

            [Obsolete, DataMember(Order = 19, EmitDefaultValue = false)]
            public bool IsVisibleWindowTitle { get; set; }

            [Obsolete, DataMember(Order = 19, EmitDefaultValue = false)]
            public bool IsVisibleLoupeInfo { get; set; } // no used (ver.23)

            [Obsolete, DataMember(Order = 20, EmitDefaultValue = false)]
            public bool IsSliderWithIndex { get; set; } // no used

            [Obsolete, DataMember(Order = 20, EmitDefaultValue = false)]
            public bool IsLoupeCenter { get; set; } // no used (ver.23)

            [Obsolete, DataMember(Order = 21, EmitDefaultValue = false)]
            public SliderIndexLayout SliderIndexLayout { get; set; } // no used (ver.23)

            [Obsolete, DataMember(Order = 21, EmitDefaultValue = false)]
            public BrushSource CustomBackground { get; set; } // no used (ver.23)

            //
            private void Constructor()
            {
                IsHidePanelInFullscreen = true;
                IsVisibleWindowTitle = true;
            }

            public Memento()
            {
                _Version = Environment.ProductVersionNumber;
                Constructor();
            }

            [OnDeserializing]
            private void Deserializing(StreamingContext c)
            {
                Constructor();
            }

            [OnDeserialized]
            private void Deserialized(StreamingContext c)
            {
                if (_Version < Environment.GenerateProductVersionNumber(1, 16, 0))
                {
                    SliderDirection = IsSliderDirectionReversed ? SliderDirection.RightToLeft : SliderDirection.LeftToRight;
                }
                IsSliderDirectionReversed = false;

                if (_Version < Environment.GenerateProductVersionNumber(1, 17, 0))
                {
                    IsHidePageSlider = IsHideMenu;
                    IsHideMenu = false;
                }

                if (_Version < Environment.GenerateProductVersionNumber(1, 19, 0))
                {
                    AngleFrequency = IsAngleSnap ? 45 : 0;
                }
                IsAngleSnap = false;

                if (_Version < Environment.GenerateProductVersionNumber(1, 21, 0))
                {
                    SliderIndexLayout = IsSliderWithIndex ? SliderIndexLayout.Right : SliderIndexLayout.None;
                }
                IsSliderWithIndex = false;
            }
        }

#pragma warning disable CS0612

        //
        public static void RestoreCompatible(Memento memento)
        {
            if (memento == null) return;

            // compatible before ver.22
            if (memento._Version < Environment.GenerateProductVersionNumber(1, 22, 0))
            {
                if (memento.FileInfoSetting != null)
                {
                    FileInformation.Current.IsVisibleBitsPerPixel = memento.FileInfoSetting.IsVisibleBitsPerPixel;
                    FileInformation.Current.IsVisibleLoader = memento.FileInfoSetting.IsVisibleLoader;
                }
                if (memento.FolderListSetting != null)
                {
                    BookshelfFolderList.Current.IsVisibleBookmarkMark = memento.FolderListSetting.IsVisibleBookmarkMark;
                    BookshelfFolderList.Current.IsVisibleHistoryMark = memento.FolderListSetting.IsVisibleHistoryMark;
                }

                InfoMessage.Current.CommandShowMessageStyle = memento.CommandShowMessageStyle;

                WindowShape.Current.IsTopmost = memento.IsTopmost;
                WindowShape.Current.IsCaptionVisible = memento.IsVisibleTitleBar;
                Config.Current.StartUp.IsOpenLastBook = memento.IsLoadLastFolder;
            }

            // compatible before ver.23
            if (memento._Version < Environment.GenerateProductVersionNumber(1, 23, 0))
            {
                ThemeProfile.Current.PanelColor = memento.PanelColor;
                MainWindowModel.Current.ContextMenuSetting = memento.ContextMenuSetting;
                MainWindowModel.Current.IsHideMenu = memento.IsHideMenu;
                MainWindowModel.Current.IsHidePageSlider = memento.IsHidePageSlider;
                MainWindowModel.Current.IsHidePanel = memento.IsHidePanel;
                MainWindowModel.Current.IsVisibleAddressBar = memento.IsVisibleAddressBar;
                MainWindowModel.Current.IsHidePanelInFullscreen = memento.IsHidePanelInFullscreen;
                MainWindowModel.Current.IsVisibleWindowTitle = memento.IsVisibleWindowTitle;

                MemoryControl.Current.IsAutoGC = memento.IsAutoGC;

                InfoMessage.Current.NoticeShowMessageStyle = memento.NoticeShowMessageStyle;
                InfoMessage.Current.GestureShowMessageStyle = memento.GestureShowMessageStyle;
                InfoMessage.Current.NowLoadingShowMessageStyle = memento.NowLoadingShowMessageStyle;
                InfoMessage.Current.ViewTransformShowMessageStyle = memento.ViewTransformShowMessageStyle;

                SlideShow.Current.IsAutoPlaySlideShow = memento.IsAutoPlaySlideShow;

                DragTransform.Current.IsLimitMove = memento.IsLimitMove;
                DragTransform.Current.AngleFrequency = memento.AngleFrequency;

                MouseInput.Current.Normal.LongButtonDownMode = memento.LongLeftButtonDownMode;
                MouseInput.Current.Loupe.IsLoupeCenter = memento.IsLoupeCenter;
                LoupeTransform.Current.IsVisibleLoupeInfo = memento.IsVisibleLoupeInfo;
                DragTransformControl.Current.IsOriginalScaleShowMessage = memento.IsOriginalScaleShowMessage;
                DragTransformControl.Current.DragControlRotateCenter = memento.IsControlCenterImage ? DragControlCenter.Target : DragControlCenter.View;
                DragTransformControl.Current.DragControlScaleCenter = memento.IsControlCenterImage ? DragControlCenter.Target : DragControlCenter.View;
                DragTransformControl.Current.DragControlFlipCenter = memento.IsControlCenterImage ? DragControlCenter.Target : DragControlCenter.View;
                DragTransformControl.Current.IsKeepAngle = memento.IsKeepAngle;
                DragTransformControl.Current.IsKeepFlip = memento.IsKeepFlip;
                DragTransformControl.Current.IsKeepScale = memento.IsKeepScale;
                DragTransformControl.Current.IsViewStartPositionCenter = memento.IsViewStartPositionCenter;

                ContentCanvas.Current.StretchMode = memento.StretchMode;
                ContentCanvas.Current.IsEnabledNearestNeighbor = memento.IsEnabledNearestNeighbor;
                ContentCanvas.Current.ContentsSpace = memento.ContentsSpace;
                ContentCanvas.Current.IsAutoRotateRight = memento.IsAutoRotate;

                ContentCanvasBrush.Current.CustomBackground = memento.CustomBackground;
                ContentCanvasBrush.Current.Background = memento.Background;

                WindowTitle.Current.WindowTitleFormat1 = memento.WindowTitleFormat1;
                WindowTitle.Current.WindowTitleFormat2 = memento.WindowTitleFormat2;

                PageSlider.Current.SliderIndexLayout = memento.SliderIndexLayout;
                PageSlider.Current.SliderDirection = memento.SliderDirection;
                PageSlider.Current.IsSliderLinkedThumbnailList = memento.IsSliderLinkedThumbnailList;

                ThumbnailList.Current.IsEnableThumbnailList = memento.IsEnableThumbnailList;
                ThumbnailList.Current.IsHideThumbnailList = memento.IsHideThumbnailList;
                ThumbnailList.Current.ThumbnailSize = memento.ThumbnailSize;
                ThumbnailList.Current.IsVisibleThumbnailNumber = memento.IsVisibleThumbnailNumber;
                ThumbnailList.Current.IsVisibleThumbnailPlate = memento.IsVisibleThumbnailPlate;
            }
        }

#pragma warning restore CS0612

        #endregion
    }
}
