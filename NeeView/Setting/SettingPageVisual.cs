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

            this.Items = new List<SettingItem>
            {
                new SettingItemSection(Properties.Resources.SettingPageVisualGeneralTheme,
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Layout.Theme, nameof(ThemeConfig.PanelColor))),
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Layout.Theme, nameof(ThemeConfig.MenuColor)))),

                new SettingItemSection(Properties.Resources.SettingPageVisualGeneralOpacity,
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Layout.Panels, nameof(PanelsConfig.Opacity))),
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Layout.Slider, nameof(SliderConfig.Opacity)))),

                new SettingItemSection(Properties.Resources.SettingPageVisualGeneralBackground,
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Layout.Background, nameof(BackgroundConfig.CustomBackground)),
                        new BackgroundSettingControl(Config.Current.Layout.Background.CustomBackground))),

                new SettingItemSection(Properties.Resources.SettingPageVisualGeneralAutoHide,
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Layout.AutoHide, nameof(AutoHideConfig.AutoHideFocusLockMode))),
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Layout.AutoHide, nameof(AutoHideConfig.IsAutoHideKeyDownDelay))),
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Layout.AutoHide, nameof(AutoHideConfig.AutoHideDelayVisibleTime))),
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Layout.AutoHide, nameof(AutoHideConfig.AutoHideDelayTime))),
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Layout.AutoHide, nameof(AutoHideConfig.AutoHideHitTestMargin)))),

                new SettingItemSection(Properties.Resources.SettingPageVisualGeneralAdvance,
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Layout.MenuBar, nameof(MenuBarConfig.IsHamburgerMenu))),
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Window, nameof(WindowConfig.IsFullScreenWithTaskBar))),
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Window, nameof(WindowConfig.WindowChromeFrame))),
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Window, nameof(WindowConfig.MaximizeWindowGapWidth)))),
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
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Thumbnail, nameof(ThumbnailConfig.IsCacheEnabled))),
                    new SettingItemButton(Properties.Resources.SettingPageVisualThumbnailCacheClear, Properties.Resources.SettingPageVisualThumbnailCacheClearTips,  RemoveCache)),

               new SettingItemSection(Properties.Resources.SettingPageVisualThumbnailAdvance,
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Thumbnail, nameof(ThumbnailConfig.Format))),
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Thumbnail, nameof(ThumbnailConfig.Quality)))),
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
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Layout.FilmStrip, nameof(FilmStripConfig.ThumbnailSize))),
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Layout.Slider, nameof(SliderConfig.IsSliderLinkedFilmStrip))),
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Layout.FilmStrip, nameof(FilmStripConfig.IsVisibleNumber))),
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Layout.FilmStrip, nameof(FilmStripConfig.IsVisiblePlate))),
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Layout.FilmStrip, nameof(FilmStripConfig.IsSelectedCenter))),
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Layout.FilmStrip, nameof(FilmStripConfig.IsManipulationBoundaryFeedbackEnabled)))),
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
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Layout.Notice, nameof(NoticeConfig.NoticeShowMessageStyle))),
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Layout.Notice, nameof(NoticeConfig.BookNameShowMessageStyle))),
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Layout.Notice, nameof(NoticeConfig.CommandShowMessageStyle))),
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Layout.Notice, nameof(NoticeConfig.GestureShowMessageStyle))),
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Layout.Notice, nameof(NoticeConfig.NowLoadingShowMessageStyle))),
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Layout.Notice, nameof(NoticeConfig.ViewTransformShowMessageStyle))),
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Layout.Notice, nameof(NoticeConfig.IsOriginalScaleShowMessage)))),
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
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Layout.WindowTittle, nameof(WindowTitleConfig.WindowTitleFormat1))) {IsStretch = true },
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Layout.WindowTittle, nameof(WindowTitleConfig.WindowTitleFormat2))) {IsStretch = true },
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Layout.WindowTittle, nameof(WindowTitleConfig.WindowTitleFormatMedia))) {IsStretch = true },
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Layout.WindowTittle, nameof(WindowTitleConfig.IsMainViewDisplayEnabled)))),

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
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Layout.Slider, nameof(SliderConfig.IsHidePageSliderInFullscreen))),
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Layout.Slider, nameof(SliderConfig.SliderDirection))),
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Layout.Slider, nameof(SliderConfig.SliderIndexLayout)))),
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
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Layout.Panels, nameof(PanelsConfig.IsHidePanelInFullscreen)))),

                new SettingItemSection(Properties.Resources.SettingPagePanelGeneralOperation,
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Layout.Panels, nameof(PanelsConfig.IsLeftRightKeyEnabled))),
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Layout.Panels, nameof(PanelsConfig.IsManipulationBoundaryFeedbackEnabled)))),
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
                    new SettingItemPropertyFont(PropertyMemberElement.Create(Config.Current.Layout.Panels, nameof(PanelsConfig.FontName))),
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Layout.Panels, nameof(PanelsConfig.FontSize)))),

                new SettingItemSection(Properties.Resources.WordStyleContent,
                    new SettingItemProperty(PropertyMemberElement.Create(SidePanelProfile.Current, nameof(SidePanelProfile.ContentItemImageWidth))),
                    new SettingItemProperty(PropertyMemberElement.Create(SidePanelProfile.Current, nameof(SidePanelProfile.ContentItemImageShape))),
                    new SettingItemProperty(PropertyMemberElement.Create(SidePanelProfile.Current, nameof(SidePanelProfile.ContentItemIsImagePopupEnabled))),
                    new SettingItemProperty(PropertyMemberElement.Create(SidePanelProfile.Current, nameof(SidePanelProfile.ContentItemIsTextWrapped))),
                    new SettingItemProperty(PropertyMemberElement.Create(SidePanelProfile.Current, nameof(SidePanelProfile.ContentItemNoteOpacity))),
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Layout.Panels, nameof(PanelsConfig.IsDecoratePlace)))),

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
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Layout.Bookshelf, nameof(BookshelfPanelConfig.Home))) {IsStretch = true}),

                new SettingItemSection(Properties.Resources.SettingPagePanelBookshelfVisual,
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Layout.Bookshelf, nameof(BookshelfPanelConfig.IsBookmarkMark))),
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Layout.Bookshelf, nameof(BookshelfPanelConfig.IsHistoryMark)))),

                new SettingItemSection(Properties.Resources.SettingPagePanelBookshelfTree,
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Layout.Bookshelf, nameof(BookshelfPanelConfig.FolderTreeLayout))),
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Layout.Panels, nameof(PanelsConfig.FolderTreeFontSize))),
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Layout.Bookshelf, nameof(BookshelfPanelConfig.IsSyncFolderTree)))),

                new SettingItemSection(Properties.Resources.SettingPagePanelBookshelfAdvance,
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.History, nameof(HistoryConfig.IsKeepFolderStatus))),
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.History, nameof(HistoryConfig.IsKeepSearchHistory))),
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Layout.Bookshelf, nameof(BookshelfPanelConfig.IsPageListDocked))),
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Layout.Bookshelf, nameof(BookshelfPanelConfig.IsCloseBookWhenMove))),
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Layout.Bookshelf, nameof(BookshelfPanelConfig.IsOpenNextBookWhenRemove))),
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Layout.Bookshelf, nameof(BookshelfPanelConfig.IsInsertItem))),
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.System, nameof(SystemConfig.IsHiddenFileVisibled))),
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Layout.Bookshelf, nameof(BookshelfPanelConfig.IsMultipleRarFilterEnabled))),
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Layout.Bookshelf, nameof(BookshelfPanelConfig.ExcludePattern))) { IsStretch = true }),
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
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Layout.Information, nameof(InformationPanelConfig.IsVisibleFilePath))),
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Layout.Information, nameof(InformationPanelConfig.IsVisibleBitsPerPixel))),
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Layout.Information, nameof(InformationPanelConfig.IsVisibleLoader)))),
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
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Effect, nameof(EffectConfig.IsHsvMode)))),
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
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.SlideShow, nameof(SlideShowConfig.IsSlideShowByLoop))),
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.SlideShow, nameof(SlideShowConfig.IsCancelSlideByMouseMove))),
                    new SettingItemIndexValue<double>(PropertyMemberElement.Create(Config.Current.SlideShow, nameof(SlideShowConfig.SlideShowInterval)), new SlideShowInterval(), true)),
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
