using NeeLaboratory.Windows.Input;
using NeeView.Effects;
using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NeeView.Setting
{
    public class SettingPageVisual : SettingPage
    {
        public SettingPageVisual() : base(Properties.Resources.SettingPageVisual)
        {
            this.Children = new List<SettingPage>
            {
                new SettingPageVisualGeneral(),
                new SettingPageVisualFont(),
                new SettingPageVisualThumbnail(),
                new SettingPageVisualNotify(),
                new SettingPageVisualWindowTitile(),
                new SettingPageVisualSlider(),
                new SettingPagePanelGeneral(),
                new SettingPagePanelFolderList(),
                new SettingPagePanelFileInfo(),
                new SettingPagePanelEffect(),
                new SettingPageVisualSlideshow(),
            };
        }
    }

    public class SettingPageVisualGeneral : SettingPage
    {
        public SettingPageVisualGeneral() : base(Properties.Resources.SettingPageVisualGeneral)
        {
            this.Items = new List<SettingItem>
            {
                new SettingItemSection(Properties.Resources.SettingPageVisualGeneralTheme,
                    new SettingItemProperty(PropertyMemberElement.Create(MainWindowModel.Current, nameof(MainWindowModel.PanelColor)))),

                new SettingItemSection(Properties.Resources.SettingPageVisualGeneralBackground,
                    new SettingItemProperty(PropertyMemberElement.Create(ContentCanvasBrush.Current, nameof(ContentCanvasBrush.CustomBackground)),
                        new BackgroundSettingControl(ContentCanvasBrush.Current.CustomBackground))),

                new SettingItemSection(Properties.Resources.SettingPageVisualGeneralAdvance,
                    new SettingItemProperty(PropertyMemberElement.Create(App.Current, nameof(App.AutoHideDelayTime))),
                    new SettingItemProperty(PropertyMemberElement.Create(WindowShape.Current, nameof(WindowShape.WindowChromeFrame)))),
            };
        }
    }

    public class SettingPageVisualFont : SettingPage
    {
        public SettingPageVisualFont() : base(Properties.Resources.SettingPageVisualFont)
        {
            this.Items = new List<SettingItem>
            {
                new SettingItemSection(Properties.Resources.SettingPageVisualFontPanel,
                    new SettingItemPropertyFont(PropertyMemberElement.Create(SidePanel.Current, nameof(SidePanel.FontName))),
                    new SettingItemProperty(PropertyMemberElement.Create(SidePanel.Current, nameof(SidePanel.FontSize))),
                    new SettingItemProperty(PropertyMemberElement.Create(SidePanel.Current, nameof(SidePanel.IsTextWrapped))),
                    new SettingItemProperty(PropertyMemberElement.Create(SidePanel.Current, nameof(SidePanel.NoteOpacity)))),
            };
        }
    }

    public class SettingPageVisualThumbnail : SettingPage
    {
        public SettingPageVisualThumbnail() : base(Properties.Resources.SettingPageVisualThumbnail)
        {
            this.Items = new List<SettingItem>
            {

                new SettingItemSection(Properties.Resources.SettingPageVisualThumbnailPanel,
                    new SettingItemProperty(PropertyMemberElement.Create(ThumbnailProfile.Current, nameof(ThumbnailProfile.ThumbnailWidth))),
                    new SettingItemProperty(PropertyMemberElement.Create(ThumbnailProfile.Current, nameof(ThumbnailProfile.IsThumbnailPopup))),
                    new SettingItemProperty(PropertyMemberElement.Create(ThumbnailProfile.Current, nameof(ThumbnailProfile.BannerWidth)))),

                 new SettingItemSection(Properties.Resources.SettingPageVisualThumbnailFilmStrip,
                    new SettingItemProperty(PropertyMemberElement.Create(ThumbnailList.Current, nameof(ThumbnailList.ThumbnailSize))),
                    new SettingItemProperty(PropertyMemberElement.Create(PageSlider.Current, nameof(PageSlider.IsSliderLinkedThumbnailList))),
                    new SettingItemProperty(PropertyMemberElement.Create(ThumbnailList.Current, nameof(ThumbnailList.IsVisibleThumbnailNumber))),
                    new SettingItemProperty(PropertyMemberElement.Create(ThumbnailList.Current, nameof(ThumbnailList.IsVisibleThumbnailPlate))),
                    new SettingItemProperty(PropertyMemberElement.Create(ThumbnailList.Current, nameof(ThumbnailList.IsSelectedCenter))),
                    new SettingItemProperty(PropertyMemberElement.Create(ThumbnailList.Current, nameof(ThumbnailList.IsManipulationBoundaryFeedbackEnabled)))),

                new SettingItemSection(Properties.Resources.SettingPageVisualThumbnailCache,
                    new SettingItemProperty(PropertyMemberElement.Create(ThumbnailProfile.Current, nameof(ThumbnailProfile.IsCacheEnabled))),
                    new SettingItemButton(Properties.Resources.SettingPageVisualThumbnailCacheClear, Properties.Resources.SettingPageVisualThumbnailCacheClearTips,  RemoveCache)),

               new SettingItemSection(Properties.Resources.SettingPageVisualThumbnailAdvance,
                    new SettingItemProperty(PropertyMemberElement.Create(ThumbnailProfile.Current, nameof(ThumbnailProfile.Format))),
                    new SettingItemProperty(PropertyMemberElement.Create(ThumbnailProfile.Current, nameof(ThumbnailProfile.Quality))),
                    new SettingItemProperty(PropertyMemberElement.Create(ThumbnailProfile.Current, nameof(ThumbnailProfile.BookCapacity))),
                    new SettingItemProperty(PropertyMemberElement.Create(ThumbnailProfile.Current, nameof(ThumbnailProfile.PageCapacity)))),
            };
        }

        #region Commands

        /// <summary>
        /// RemoveCache command.
        /// </summary>
        private RelayCommand<UIElement> _RemoveCache;
        public RelayCommand<UIElement> RemoveCache
        {
            get { return _RemoveCache = _RemoveCache ?? new RelayCommand<UIElement>(RemoveCache_Executed); }
        }

        private void RemoveCache_Executed(UIElement element)
        {
            ThumbnailCache.Current.Remove();

            var dialog = new MessageDialog("", Properties.Resources.DialogCacheDeletedTitle);
            if (element != null)
            {
                dialog.Owner = Window.GetWindow(element);
            }
            dialog.ShowDialog();
        }

        #endregion
    }

    public class SettingPageVisualNotify : SettingPage
    {
        public SettingPageVisualNotify() : base(Properties.Resources.SettingPageVisualNotify)
        {
            this.Items = new List<SettingItem>
            {
                new SettingItemSection(Properties.Resources.SettingPageVisualNotifyDisplay,
                    new SettingItemProperty(PropertyMemberElement.Create(InfoMessage.Current, nameof(InfoMessage.NoticeShowMessageStyle))),
                    new SettingItemProperty(PropertyMemberElement.Create(InfoMessage.Current, nameof(InfoMessage.CommandShowMessageStyle))),
                    new SettingItemProperty(PropertyMemberElement.Create(InfoMessage.Current, nameof(InfoMessage.GestureShowMessageStyle))),
                    new SettingItemProperty(PropertyMemberElement.Create(InfoMessage.Current, nameof(InfoMessage.NowLoadingShowMessageStyle))),
                    new SettingItemProperty(PropertyMemberElement.Create(InfoMessage.Current, nameof(InfoMessage.ViewTransformShowMessageStyle))),
                    new SettingItemProperty(PropertyMemberElement.Create(DragTransformControl.Current, nameof(DragTransformControl.IsOriginalScaleShowMessage)))),
            };
        }
    }

    public class SettingPageVisualWindowTitile : SettingPage
    {
        public SettingPageVisualWindowTitile() : base(Properties.Resources.SettingPageVisualWindowTitile)
        {
            this.Items = new List<SettingItem>
            {
                new SettingItemSection(Properties.Resources.SettingPageVisualWindowTitileDisplay,
                    new SettingItemProperty(PropertyMemberElement.Create(WindowTitle.Current, nameof(WindowTitle.WindowTitleFormat1))) {IsStretch = true },
                    new SettingItemProperty(PropertyMemberElement.Create(WindowTitle.Current, nameof(WindowTitle.WindowTitleFormat2))) {IsStretch = true },
                    new SettingItemProperty(PropertyMemberElement.Create(WindowTitle.Current, nameof(WindowTitle.WindowTitleFormatMedia))) {IsStretch = true },
                    new SettingItemProperty(PropertyMemberElement.Create(MainWindowModel.Current, nameof(MainWindowModel.IsVisibleWindowTitle)))),

                new SettingItemNote(Properties.Resources.SettingPageVisualWindowTitileNote),
            };
        }
    }

    public class SettingPageVisualSlider : SettingPage
    {
        public SettingPageVisualSlider() : base(Properties.Resources.SettingPageVisualSlider)
        {
            this.Items = new List<SettingItem>
            {
                new SettingItemSection(Properties.Resources.SettingPageVisualSliderSlider,
                    new SettingItemProperty(PropertyMemberElement.Create(PageSlider.Current, nameof(PageSlider.SliderDirection))),
                    new SettingItemProperty(PropertyMemberElement.Create(PageSlider.Current, nameof(PageSlider.SliderIndexLayout)))),
            };
        }
    }

    public class SettingPagePanelGeneral : SettingPage
    {
        public SettingPagePanelGeneral() : base(Properties.Resources.SettingPagePanelGeneral)
        {
            this.Items = new List<SettingItem>
            {
                 new SettingItemSection(Properties.Resources.SettingPagePanelGeneralVisual,
                    new SettingItemProperty(PropertyMemberElement.Create(MainWindowModel.Current, nameof(MainWindowModel.IsHidePanelInFullscreen)))),

                new SettingItemSection(Properties.Resources.SettingPagePanelGeneralOperation,
                    new SettingItemProperty(PropertyMemberElement.Create(SidePanelProfile.Current, nameof(SidePanelProfile.IsLeftRightKeyEnabled))),
                    new SettingItemProperty(PropertyMemberElement.Create(SidePanel.Current, nameof(SidePanel.IsManipulationBoundaryFeedbackEnabled)))),
            };
        }
    }

    public class SettingPagePanelFolderList : SettingPage
    {
        public SettingPagePanelFolderList() : base(Properties.Resources.SettingPagePanelFolderList)
        {
            this.Items = new List<SettingItem>
            {
                 new SettingItemSection(Properties.Resources.SettingPagePanelFolderListGeneral,
                    new SettingItemProperty(PropertyMemberElement.Create(FolderList.Current, nameof(FolderList.Home))) {IsStretch = true}),

                new SettingItemSection(Properties.Resources.SettingPagePanelFolderListVisual,
                    new SettingItemProperty(PropertyMemberElement.Create(FolderList.Current, nameof(FolderList.IsVisibleBookmarkMark))),
                    new SettingItemProperty(PropertyMemberElement.Create(FolderList.Current, nameof(FolderList.IsVisibleHistoryMark))),
                    new SettingItemProperty(PropertyMemberElement.Create(FolderList.Current, nameof(FolderList.FolderIconLayout)))),

                new SettingItemSection(Properties.Resources.SettingPagePanelFolderListAdvance,
                    new SettingItemProperty(PropertyMemberElement.Create(FolderList.Current, nameof(FolderList.IsInsertItem))),
                    new SettingItemProperty(PropertyMemberElement.Create(FolderList.Current, nameof(FolderList.IsMultipleRarFilterEnabled))),
                    new SettingItemProperty(PropertyMemberElement.Create(FolderList.Current, nameof(FolderList.ExcludePattern))) { IsStretch = true }),
            };
        }
    }

    public class SettingPagePanelFileInfo : SettingPage
    {
        public SettingPagePanelFileInfo() : base(Properties.Resources.SettingPagePanelFileInfo)
        {
            this.Items = new List<SettingItem>
            {
                new SettingItemSection(Properties.Resources.SettingPagePanelFileInfoVisual,
                    new SettingItemProperty(PropertyMemberElement.Create(FileInformation.Current, nameof(FileInformation.IsVisibleFilePath))),
                    new SettingItemProperty(PropertyMemberElement.Create(FileInformation.Current, nameof(FileInformation.IsUseExifDateTime))),
                    new SettingItemProperty(PropertyMemberElement.Create(FileInformation.Current, nameof(FileInformation.IsVisibleBitsPerPixel))),
                    new SettingItemProperty(PropertyMemberElement.Create(FileInformation.Current, nameof(FileInformation.IsVisibleLoader)))),
            };
        }
    }

    public class SettingPagePanelEffect : SettingPage
    {
        public SettingPagePanelEffect() : base(Properties.Resources.SettingPagePanelEffect)
        {
            this.Items = new List<SettingItem>
            {
                new SettingItemSection(Properties.Resources.SettingPagePanelEffectVisual,
                    new SettingItemProperty(PropertyMemberElement.Create(ImageEffect.Current, nameof(ImageEffect.IsHsvMode)))),

                new SettingItemSection(Properties.Resources.SettingPagePanelEffectAdvance,
                    new SettingItemProperty(PropertyMemberElement.Create(PictureProfile.Current, nameof(PictureProfile.IsMagicScaleSimdEnabled)))),
            };
        }
    }

    public class SettingPageVisualSlideshow : SettingPage
    {
        public SettingPageVisualSlideshow() : base(Properties.Resources.SettingPageVisualSlideshow)
        {
            this.Items = new List<SettingItem>
            {
                new SettingItemSection(Properties.Resources.SettingPageVisualSlideshowGeneral,
                    new SettingItemProperty(PropertyMemberElement.Create(SlideShow.Current, nameof(SlideShow.IsSlideShowByLoop))),
                    new SettingItemProperty(PropertyMemberElement.Create(SlideShow.Current, nameof(SlideShow.IsCancelSlideByMouseMove))),
                    new SettingItemIndexValue<double>(PropertyMemberElement.Create(SlideShow.Current, nameof(SlideShow.SlideShowInterval)), new SlideShowInterval(), true)),
            };
        }


        #region IndexValue

        /// <summary>
        /// スライドショー インターバルテーブル
        /// </summary>
        public class SlideShowInterval : IndexDoubleValue
        {
            private static List<double> _values = new List<double>
            {
                1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 15, 20, 30, 45, 60, 90, 120, 180, 240, 300
            };

            public SlideShowInterval() : base(_values)
            {
                IsValueSyncIndex = false;
            }

            public SlideShowInterval(double value) : base(_values)
            {
                IsValueSyncIndex = false;
                Value = value;
            }

            public override string ValueString => $"{Value}秒";
        }

        #endregion
    }
}
