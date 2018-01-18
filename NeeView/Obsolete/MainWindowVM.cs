// Copyright (c) 2016-2018 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using NeeView.Utility;
using NeeView.Windows.Controls;
using NeeView.Windows.Input;
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
                _Version = Config.Current.ProductVersionNumber;
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
                if (_Version < Config.GenerateProductVersionNumber(1, 16, 0))
                {
                    SliderDirection = IsSliderDirectionReversed ? SliderDirection.RightToLeft : SliderDirection.LeftToRight;
                }
                IsSliderDirectionReversed = false;

                if (_Version < Config.GenerateProductVersionNumber(1, 17, 0))
                {
                    IsHidePageSlider = IsHideMenu;
                    IsHideMenu = false;
                }

                if (_Version < Config.GenerateProductVersionNumber(1, 19, 0))
                {
                    AngleFrequency = IsAngleSnap ? 45 : 0;
                }
                IsAngleSnap = false;

                if (_Version < Config.GenerateProductVersionNumber(1, 21, 0))
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

            var models = Models.Current;

            // compatible before ver.22
            if (memento._Version < Config.GenerateProductVersionNumber(1, 22, 0))
            {
                if (memento.FileInfoSetting != null)
                {
                    models.FileInformation.IsUseExifDateTime = memento.FileInfoSetting.IsUseExifDateTime;
                    models.FileInformation.IsVisibleBitsPerPixel = memento.FileInfoSetting.IsVisibleBitsPerPixel;
                    models.FileInformation.IsVisibleLoader = memento.FileInfoSetting.IsVisibleLoader;
                }
                if (memento.FolderListSetting != null)
                {
                    models.FolderList.IsVisibleBookmarkMark = memento.FolderListSetting.IsVisibleBookmarkMark;
                    models.FolderList.IsVisibleHistoryMark = memento.FolderListSetting.IsVisibleHistoryMark;
                }

                models.InfoMessage.CommandShowMessageStyle = memento.CommandShowMessageStyle;

                WindowShape.Current.IsTopmost = memento.IsTopmost;
                WindowShape.Current.IsCaptionVisible = memento.IsVisibleTitleBar;
                App.Current.IsOpenLastBook = memento.IsLoadLastFolder;
            }

            // compatible before ver.23
            if (memento._Version < Config.GenerateProductVersionNumber(1, 23, 0))
            {
                models.MainWindowModel.PanelColor = memento.PanelColor;
                models.MainWindowModel.ContextMenuSetting = memento.ContextMenuSetting;
                models.MainWindowModel.IsHideMenu = memento.IsHideMenu;
                models.MainWindowModel.IsHidePageSlider = memento.IsHidePageSlider;
                models.MainWindowModel.IsHidePanel = memento.IsHidePanel;
                models.MainWindowModel.IsVisibleAddressBar = memento.IsVisibleAddressBar;
                models.MainWindowModel.IsHidePanelInFullscreen = memento.IsHidePanelInFullscreen;
                models.MainWindowModel.IsVisibleWindowTitle = memento.IsVisibleWindowTitle;

                models.MemoryControl.IsAutoGC = memento.IsAutoGC;

                models.InfoMessage.NoticeShowMessageStyle = memento.NoticeShowMessageStyle;
                models.InfoMessage.GestureShowMessageStyle = memento.GestureShowMessageStyle;
                models.InfoMessage.NowLoadingShowMessageStyle = memento.NowLoadingShowMessageStyle;
                models.InfoMessage.ViewTransformShowMessageStyle = memento.ViewTransformShowMessageStyle;

                models.SlideShow.IsAutoPlaySlideShow = memento.IsAutoPlaySlideShow;

                models.DragTransform.IsLimitMove = memento.IsLimitMove;
                models.DragTransform.AngleFrequency = memento.AngleFrequency;

                models.MouseInput.Normal.LongLeftButtonDownMode = memento.LongLeftButtonDownMode;
                models.MouseInput.Loupe.IsLoupeCenter = memento.IsLoupeCenter;
                models.LoupeTransform.IsVisibleLoupeInfo = memento.IsVisibleLoupeInfo;
                models.DragTransformControl.IsOriginalScaleShowMessage = memento.IsOriginalScaleShowMessage;
                models.DragTransformControl.IsControlCenterImage = memento.IsControlCenterImage;
                models.DragTransformControl.IsKeepAngle = memento.IsKeepAngle;
                models.DragTransformControl.IsKeepFlip = memento.IsKeepFlip;
                models.DragTransformControl.IsKeepScale = memento.IsKeepScale;
                models.DragTransformControl.IsViewStartPositionCenter = memento.IsViewStartPositionCenter;

                models.ContentCanvas.StretchMode = memento.StretchMode;
                models.ContentCanvas.IsEnabledNearestNeighbor = memento.IsEnabledNearestNeighbor;
                models.ContentCanvas.ContentsSpace = memento.ContentsSpace;
                models.ContentCanvas.IsAutoRotate = memento.IsAutoRotate;

                models.ContentCanvasBrush.CustomBackground = memento.CustomBackground;
                models.ContentCanvasBrush.Background = memento.Background;

                models.WindowTitle.WindowTitleFormat1 = memento.WindowTitleFormat1;
                models.WindowTitle.WindowTitleFormat2 = memento.WindowTitleFormat2;

                models.PageSlider.SliderIndexLayout = memento.SliderIndexLayout;
                models.PageSlider.SliderDirection = memento.SliderDirection;
                models.PageSlider.IsSliderLinkedThumbnailList = memento.IsSliderLinkedThumbnailList;

                models.ThumbnailList.IsEnableThumbnailList = memento.IsEnableThumbnailList;
                models.ThumbnailList.IsHideThumbnailList = memento.IsHideThumbnailList;
                models.ThumbnailList.ThumbnailSize = memento.ThumbnailSize;
                models.ThumbnailList.IsVisibleThumbnailNumber = memento.IsVisibleThumbnailNumber;
                models.ThumbnailList.IsVisibleThumbnailPlate = memento.IsVisibleThumbnailPlate;
            }
        }

#pragma warning restore CS0612

        #endregion
    }
}
