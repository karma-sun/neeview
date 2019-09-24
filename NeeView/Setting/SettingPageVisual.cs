using NeeLaboratory.Windows.Input;
using NeeView.Data;
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
                new SettingPageVisualNotify(),
                new SettingPageVisualWindowTitile(),
                new SettingPageVisualFilmstrip(),
                new SettingPageVisualSlider(),
                new SettingPagePanelGeneral(),
                new SettingPagePanelItem(),
                new SettingPagePanelFolderList(),
                new SettingPagePanelFileInfo(),
                new SettingPagePanelEffect(),
                new SettingPageVisualSlideshow(),
                new SettingPageVisualThumbnail(),
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
                    new SettingItemProperty(PropertyMemberElement.Create(ThemeProfile.Current, nameof(ThemeProfile.PanelColor))),
                    new SettingItemProperty(PropertyMemberElement.Create(ThemeProfile.Current, nameof(ThemeProfile.MenuColor)))),

                new SettingItemSection(Properties.Resources.SettingPageVisualGeneralOpacity,
                    new SettingItemProperty(PropertyMemberElement.Create(SidePanelProfile.Current, nameof(SidePanelProfile.Opacity))),
                    new SettingItemProperty(PropertyMemberElement.Create(MainWindowModel.Current, nameof(MainWindowModel.SliderOpacity)))),

                new SettingItemSection(Properties.Resources.SettingPageVisualGeneralBackground,
                    new SettingItemProperty(PropertyMemberElement.Create(ContentCanvasBrush.Current, nameof(ContentCanvasBrush.CustomBackground)),
                        new BackgroundSettingControl(ContentCanvasBrush.Current.CustomBackground))),

                new SettingItemSection(Properties.Resources.SettingPageVisualGeneralAdvance,
                    new SettingItemProperty(PropertyMemberElement.Create(MenuBar.Current, nameof(MenuBar.IsHamburgerMenu))),
                    new SettingItemProperty(PropertyMemberElement.Create(App.Current, nameof(App.AutoHideDelayTime))),
                    new SettingItemProperty(PropertyMemberElement.Create(WindowShape.Current, nameof(WindowShape.IsFullScreenWithTaskBar))),
                    new SettingItemProperty(PropertyMemberElement.Create(WindowShape.Current, nameof(WindowShape.WindowChromeFrame))),
                    new SettingItemProperty(PropertyMemberElement.Create(WindowShape.Current, nameof(WindowShape.MaximizeWindowGapWidth)))),
            };
        }
    }

    public class SettingPageVisualThumbnail : SettingPage
    {
        public SettingPageVisualThumbnail() : base(Properties.Resources.SettingPageVisualThumbnail)
        {
            this.Items = new List<SettingItem>
            {
                new SettingItemSection(Properties.Resources.SettingPageVisualThumbnailCache,
                    new SettingItemProperty(PropertyMemberElement.Create(ThumbnailProfile.Current, nameof(ThumbnailProfile.IsCacheEnabled))),
                    new SettingItemButton(Properties.Resources.SettingPageVisualThumbnailCacheClear, Properties.Resources.SettingPageVisualThumbnailCacheClearTips,  RemoveCache)),

               new SettingItemSection(Properties.Resources.SettingPageVisualThumbnailAdvance,
                    new SettingItemProperty(PropertyMemberElement.Create(ThumbnailProfile.Current, nameof(ThumbnailProfile.Format))),
                    new SettingItemProperty(PropertyMemberElement.Create(ThumbnailProfile.Current, nameof(ThumbnailProfile.Quality)))),
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
            try
            {
                ThumbnailCache.Current.Remove();

                var dialog = new MessageDialog("", Properties.Resources.DialogCacheDeletedTitle);
                if (element != null)
                {
                    dialog.Owner = Window.GetWindow(element);
                }
                dialog.ShowDialog();
            }
            catch (Exception ex)
            {
                var dialog = new MessageDialog(ex.Message, Properties.Resources.DialogCacheDeletedFailedTitle);
                if (element != null)
                {
                    dialog.Owner = Window.GetWindow(element);
                }
                dialog.ShowDialog();
            }
        }

        #endregion
    }


    public class SettingPageVisualFilmstrip : SettingPage
    {
        public SettingPageVisualFilmstrip() : base(Properties.Resources.SettingPageVisualFilmstrip)
        {
            this.Items = new List<SettingItem>
            {
                 new SettingItemSection(Properties.Resources.SettingPageVisualFilmstripFilmstrip,
                    new SettingItemProperty(PropertyMemberElement.Create(ThumbnailList.Current, nameof(ThumbnailList.ThumbnailSize))),
                    new SettingItemProperty(PropertyMemberElement.Create(PageSlider.Current, nameof(PageSlider.IsSliderLinkedThumbnailList))),
                    new SettingItemProperty(PropertyMemberElement.Create(ThumbnailList.Current, nameof(ThumbnailList.IsVisibleThumbnailNumber))),
                    new SettingItemProperty(PropertyMemberElement.Create(ThumbnailList.Current, nameof(ThumbnailList.IsVisibleThumbnailPlate))),
                    new SettingItemProperty(PropertyMemberElement.Create(ThumbnailList.Current, nameof(ThumbnailList.IsSelectedCenter))),
                    new SettingItemProperty(PropertyMemberElement.Create(ThumbnailList.Current, nameof(ThumbnailList.IsManipulationBoundaryFeedbackEnabled)))),
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
            try
            {
                ThumbnailCache.Current.Remove();

                var dialog = new MessageDialog("", Properties.Resources.DialogCacheDeletedTitle);
                if (element != null)
                {
                    dialog.Owner = Window.GetWindow(element);
                }
                dialog.ShowDialog();
            }
            catch (Exception ex)
            {
                var dialog = new MessageDialog(ex.Message, Properties.Resources.DialogCacheDeletedFailedTitle);
                if (element != null)
                {
                    dialog.Owner = Window.GetWindow(element);
                }
                dialog.ShowDialog();
            }
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
                    new SettingItemProperty(PropertyMemberElement.Create(InfoMessage.Current, nameof(InfoMessage.BookNameShowMessageStyle))),
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
                new SettingItemSection(Properties.Resources.SettingPageVisualSliderVisual,
                    new SettingItemProperty(PropertyMemberElement.Create(MainWindowModel.Current, nameof(MainWindowModel.IsHidePageSliderInFullscreen))),
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

                new SettingItemSection(Properties.Resources.SettingPagePanelGeneralAdvance,
                    new SettingItemProperty(PropertyMemberElement.Create(SidePanelProfile.Current, nameof(SidePanelProfile.HitTestMargin)))),
            };
        }
    }

    public class SettingPagePanelItem : SettingPage
    {
        public SettingPagePanelItem() : base(Properties.Resources.SettingPagePanelListItem)
        {
            this.Items = new List<SettingItem>
            {
                new SettingItemSection(Properties.Resources.SettingPageVisualFontPanel,
                    new SettingItemPropertyFont(PropertyMemberElement.Create(SidePanelProfile.Current, nameof(SidePanelProfile.FontName))),
                    new SettingItemProperty(PropertyMemberElement.Create(SidePanelProfile.Current, nameof(SidePanelProfile.FontSize)))),

                new SettingItemSection(Properties.Resources.WordStyleContent,
                    new SettingItemProperty(PropertyMemberElement.Create(SidePanelProfile.Current, nameof(SidePanelProfile.ContentItemImageWidth))),
                    new SettingItemProperty(PropertyMemberElement.Create(SidePanelProfile.Current, nameof(SidePanelProfile.ContentItemImageShape))),
                    new SettingItemProperty(PropertyMemberElement.Create(SidePanelProfile.Current, nameof(SidePanelProfile.ContentItemIsImagePopupEnabled))),
                    new SettingItemProperty(PropertyMemberElement.Create(SidePanelProfile.Current, nameof(SidePanelProfile.ContentItemIsTextWrapped))),
                    new SettingItemProperty(PropertyMemberElement.Create(SidePanelProfile.Current, nameof(SidePanelProfile.ContentItemNoteOpacity))),
                    new SettingItemProperty(PropertyMemberElement.Create(SidePanelProfile.Current, nameof(SidePanelProfile.IsDecoratePlace)))),

                new SettingItemSection(Properties.Resources.WordStyleBanner,
                    new SettingItemProperty(PropertyMemberElement.Create(SidePanelProfile.Current, nameof(SidePanelProfile.BannerItemImageWidth))),
                    new SettingItemProperty(PropertyMemberElement.Create(SidePanelProfile.Current, nameof(SidePanelProfile.BannerItemIsTextWrapped)))),

                new SettingItemSection(Properties.Resources.WordStyleThumbnail,
                    new SettingItemProperty(PropertyMemberElement.Create(SidePanelProfile.Current, nameof(SidePanelProfile.ThumbnailItemImageWidth))),
                    new SettingItemProperty(PropertyMemberElement.Create(SidePanelProfile.Current, nameof(SidePanelProfile.ThumbnailItemImageShape))),
                    new SettingItemProperty(PropertyMemberElement.Create(SidePanelProfile.Current, nameof(SidePanelProfile.ThumbnailItemIsTextVisibled))),
                    new SettingItemProperty(PropertyMemberElement.Create(SidePanelProfile.Current, nameof(SidePanelProfile.ThumbnailItemIsTextWrapped)))),
            };
        }
    }

    public class SettingPagePanelFolderList : SettingPage
    {
        public SettingPagePanelFolderList() : base(Properties.Resources.SettingPagePanelBookshelf)
        {
            this.Items = new List<SettingItem>
            {
                 new SettingItemSection(Properties.Resources.SettingPagePanelBookshelfGeneral,
                    new SettingItemProperty(PropertyMemberElement.Create(BookshelfFolderList.Current, nameof(FolderList.Home))) {IsStretch = true}),

                new SettingItemSection(Properties.Resources.SettingPagePanelBookshelfVisual,
                    new SettingItemProperty(PropertyMemberElement.Create(BookshelfFolderList.Current, nameof(BookshelfFolderList.IsVisibleBookmarkMark))),
                    new SettingItemProperty(PropertyMemberElement.Create(BookshelfFolderList.Current, nameof(BookshelfFolderList.IsVisibleHistoryMark)))),

                new SettingItemSection(Properties.Resources.SettingPagePanelBookshelfTree,
                    new SettingItemProperty(PropertyMemberElement.Create(BookshelfFolderList.Current, nameof(FolderList.FolderTreeLayout))),
                    new SettingItemProperty(PropertyMemberElement.Create(SidePanelProfile.Current, nameof(SidePanelProfile.FolderTreeFontSize))),
                    new SettingItemProperty(PropertyMemberElement.Create(BookshelfFolderList.Current, nameof(FolderList.IsSyncFolderTree)))),

                new SettingItemSection(Properties.Resources.SettingPagePanelBookshelfAdvance,
                    new SettingItemProperty(PropertyMemberElement.Create(PageListPlacementService.Current, nameof(PageListPlacementService.IsPlacedInBookshelf))),
                    new SettingItemProperty(PropertyMemberElement.Create(BookshelfFolderList.Current, nameof(FolderList.IsCloseBookWhenMove))),
                    new SettingItemProperty(PropertyMemberElement.Create(BookshelfFolderList.Current, nameof(FolderList.IsOpenNextBookWhenRemove))),
                    new SettingItemProperty(PropertyMemberElement.Create(BookshelfFolderList.Current, nameof(FolderList.IsInsertItem))),
                    new SettingItemProperty(PropertyMemberElement.Create(FileIOProfile.Current, nameof(FileIOProfile.IsHiddenFileVisibled))),
                    new SettingItemProperty(PropertyMemberElement.Create(BookshelfFolderList.Current, nameof(FolderList.IsMultipleRarFilterEnabled))),
                    new SettingItemProperty(PropertyMemberElement.Create(BookshelfFolderList.Current, nameof(FolderList.ExcludePattern))) { IsStretch = true }),
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

            public override string ValueString => $"{Value}{Properties.Resources.WordSec}";
        }

        #endregion
    }
}
