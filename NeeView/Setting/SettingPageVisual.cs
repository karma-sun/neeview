using NeeLaboratory.Windows.Input;
using NeeView.Data;
using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.Linq;
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

            this.Items = new List<SettingItem>();

            var section = new SettingItemSection(Properties.Resources.SettingPageVisualGeneralTheme);
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Theme, nameof(ThemeConfig.PanelColor))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Theme, nameof(ThemeConfig.MenuColor))));
            this.Items.Add(section);

            section = new SettingItemSection(Properties.Resources.SettingPageVisualGeneralOpacity);
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Panels, nameof(PanelsConfig.Opacity))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Slider, nameof(SliderConfig.Opacity))));
            this.Items.Add(section);

            section = new SettingItemSection(Properties.Resources.SettingPageVisualGeneralBackground);
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Background, nameof(BackgroundConfig.CustomBackground)),
                new BackgroundSettingControl(Config.Current.Background.CustomBackground)));
            this.Items.Add(section);

            section = new SettingItemSection(Properties.Resources.SettingPageVisualGeneralAutoHide);
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.AutoHide, nameof(AutoHideConfig.AutoHideFocusLockMode))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.AutoHide, nameof(AutoHideConfig.IsAutoHideKeyDownDelay))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.AutoHide, nameof(AutoHideConfig.AutoHideDelayVisibleTime))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.AutoHide, nameof(AutoHideConfig.AutoHideDelayTime))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.AutoHide, nameof(AutoHideConfig.AutoHideHitTestMargin))));
            this.Items.Add(section);

            section = new SettingItemSection(Properties.Resources.SettingPageVisualGeneralAdvance);
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.MenuBar, nameof(MenuBarConfig.IsHamburgerMenu))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Window, nameof(WindowConfig.IsFullScreenWithTaskBar))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Window, nameof(WindowConfig.WindowChromeFrame))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Window, nameof(WindowConfig.MaximizeWindowGapWidth))));
            this.Items.Add(section);
        }
    }

    public class SettingPageVisualThumbnail : SettingPage
    {
        public SettingPageVisualThumbnail() : base(Properties.Resources.SettingPageVisualThumbnail)
        {
            this.Items = new List<SettingItem>();

            var section = new SettingItemSection(Properties.Resources.SettingPageVisualThumbnailCache);
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Thumbnail, nameof(ThumbnailConfig.IsCacheEnabled))));
            section.Children.Add(new SettingItemIndexValue<TimeSpan>(PropertyMemberElement.Create(Config.Current.Thumbnail, nameof(ThumbnailConfig.CacheLimitSpan)), new CacheLimitSpan(), false));

            section.Children.Add(new SettingItemButton(Properties.Resources.SettingPageVisualThumbnailCacheClear, Properties.Resources.SettingPageVisualThumbnailCacheClearButton, RemoveCache) { Tips = Properties.Resources.SettingPageVisualThumbnailCacheClearTips });
            this.Items.Add(section);

            section = new SettingItemSection(Properties.Resources.SettingPageVisualThumbnailAdvance);
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Thumbnail, nameof(ThumbnailConfig.Resolution))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Thumbnail, nameof(ThumbnailConfig.Format))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Thumbnail, nameof(ThumbnailConfig.Quality))));
            this.Items.Add(section);
        }

        #region Commands

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

        /// <summary>
        /// 履歴期限テーブル
        /// </summary>
        public class CacheLimitSpan : IndexTimeSpanValue
        {
            private static List<TimeSpan> _values = new List<TimeSpan>() {
                TimeSpan.FromDays(2),
                TimeSpan.FromDays(3),
                TimeSpan.FromDays(7),
                TimeSpan.FromDays(15),
                TimeSpan.FromDays(30),
                TimeSpan.FromDays(100),
                TimeSpan.FromDays(365),
                default(TimeSpan),
            };

            public CacheLimitSpan() : base(_values)
            {
            }

            public CacheLimitSpan(TimeSpan value) : base(_values)
            {
                Value = value;
            }

            public override string ValueString => Value == default(TimeSpan) ? Properties.Resources.WordNoLimit : string.Format(Properties.Resources.WordDaysAgo, Value.Days);
        }

    }


    public class SettingPageVisualFilmstrip : SettingPage
    {
        public SettingPageVisualFilmstrip() : base(Properties.Resources.SettingPageVisualFilmstrip)
        {
            var section = new SettingItemSection(Properties.Resources.SettingPageVisualFilmstripFilmstrip);
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.FilmStrip, nameof(FilmStripConfig.ThumbnailSize))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Slider, nameof(SliderConfig.IsSliderLinkedFilmStrip))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.FilmStrip, nameof(FilmStripConfig.IsVisibleNumber))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.FilmStrip, nameof(FilmStripConfig.IsVisiblePlate))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.FilmStrip, nameof(FilmStripConfig.IsSelectedCenter))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.FilmStrip, nameof(FilmStripConfig.IsManipulationBoundaryFeedbackEnabled))));

            this.Items = new List<SettingItem>() { section };
        }

        #region Commands

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
            var section = new SettingItemSection(Properties.Resources.SettingPageVisualNotifyDisplay);
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Notice, nameof(NoticeConfig.NoticeShowMessageStyle))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Notice, nameof(NoticeConfig.BookNameShowMessageStyle))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Notice, nameof(NoticeConfig.CommandShowMessageStyle))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Notice, nameof(NoticeConfig.GestureShowMessageStyle))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Notice, nameof(NoticeConfig.NowLoadingShowMessageStyle))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Notice, nameof(NoticeConfig.ViewTransformShowMessageStyle))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Notice, nameof(NoticeConfig.IsOriginalScaleShowMessage))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Notice, nameof(NoticeConfig.IsEmptyMessageEnabled))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Notice, nameof(NoticeConfig.IsBusyMarkEnabled))));

            this.Items = new List<SettingItem>() { section };
        }
    }

    public class SettingPageVisualWindowTitile : SettingPage
    {
        public SettingPageVisualWindowTitile() : base(Properties.Resources.SettingPageVisualWindowTitile)
        {
            var section = new SettingItemSection(Properties.Resources.SettingPageVisualWindowTitileDisplay);
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.WindowTittle, nameof(WindowTitleConfig.WindowTitleFormat1))) { IsStretch = true });
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.WindowTittle, nameof(WindowTitleConfig.WindowTitleFormat2))) { IsStretch = true });
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.WindowTittle, nameof(WindowTitleConfig.WindowTitleFormatMedia))) { IsStretch = true });
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.WindowTittle, nameof(WindowTitleConfig.IsMainViewDisplayEnabled))));
            section.Children.Add(new SettingItemNote(Properties.Resources.SettingPageVisualWindowTitileNote));

            this.Items = new List<SettingItem>() { section };
        }
    }

    public class SettingPageVisualSlider : SettingPage
    {
        public SettingPageVisualSlider() : base(Properties.Resources.SettingPageVisualSlider)
        {
            var section = new SettingItemSection(Properties.Resources.SettingPageVisualSliderVisual);
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Slider, nameof(SliderConfig.IsHidePageSliderInFullscreen))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Slider, nameof(SliderConfig.SliderDirection))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Slider, nameof(SliderConfig.SliderIndexLayout))));

            this.Items = new List<SettingItem>() { section };
        }
    }

    public class SettingPagePanelGeneral : SettingPage
    {
        public SettingPagePanelGeneral() : base(Properties.Resources.SettingPagePanelGeneral)
        {
            var section = new SettingItemSection(Properties.Resources.SettingPagePanelGeneralVisual);
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Panels, nameof(PanelsConfig.IsHidePanelInFullscreen))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Panels, nameof(PanelsConfig.IsLeftRightKeyEnabled))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Panels, nameof(PanelsConfig.IsManipulationBoundaryFeedbackEnabled))));

            this.Items = new List<SettingItem>() { section };
        }
    }

    public class SettingPagePanelItem : SettingPage
    {
        public SettingPagePanelItem() : base(Properties.Resources.SettingPagePanelListItem)
        {
            this.Items = new List<SettingItem>();

            var section = new SettingItemSection(Properties.Resources.SettingPageVisualFontPanel);
            section.Children.Add(new SettingItemPropertyFont(PropertyMemberElement.Create(Config.Current.Panels, nameof(PanelsConfig.FontName))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Panels, nameof(PanelsConfig.FontSize))));
            this.Items.Add(section);

            section = new SettingItemSection(Properties.Resources.SettingPagePanelStyleContent);
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Panels.ContentItemProfile, nameof(PanelListItemProfile.ImageWidth))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Panels.ContentItemProfile, nameof(PanelListItemProfile.ImageShape))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Panels.ContentItemProfile, nameof(PanelListItemProfile.IsImagePopupEnabled))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Panels.ContentItemProfile, nameof(PanelListItemProfile.IsTextWrapped))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Panels.ContentItemProfile, nameof(PanelListItemProfile.NoteOpacity))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Panels, nameof(PanelsConfig.IsDecoratePlace))));
            this.Items.Add(section);

            section = new SettingItemSection(Properties.Resources.SettingPagePanelStyleBanner);
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Panels.BannerItemProfile, nameof(PanelListItemProfile.ImageWidth))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Panels.BannerItemProfile, nameof(PanelListItemProfile.IsImagePopupEnabled))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Panels.BannerItemProfile, nameof(PanelListItemProfile.IsTextWrapped))));
            this.Items.Add(section);

            section = new SettingItemSection(Properties.Resources.SettingPagePanelStyleThumbnail);
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Panels.ThumbnailItemProfile, nameof(PanelListItemProfile.ImageWidth))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Panels.ThumbnailItemProfile, nameof(PanelListItemProfile.ImageShape))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Panels.ThumbnailItemProfile, nameof(PanelListItemProfile.IsImagePopupEnabled))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Panels.ThumbnailItemProfile, nameof(PanelListItemProfile.IsTextVisible))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Panels.ThumbnailItemProfile, nameof(PanelListItemProfile.IsTextWrapped))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Panels, nameof(PanelsConfig.MouseWheelSpeedRate))));
            this.Items.Add(section);
        }
    }

    public class SettingPagePanelFolderList : SettingPage
    {
        public SettingPagePanelFolderList() : base(Properties.Resources.SettingPagePanelBookshelf)
        {
            this.Items = new List<SettingItem>();

            var section = new SettingItemSection(Properties.Resources.SettingPagePanelBookshelfGeneral);
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Bookshelf, nameof(BookshelfConfig.Home))) { IsStretch = true });
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Bookshelf, nameof(BookshelfConfig.IsBookmarkMark))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Bookshelf, nameof(BookshelfConfig.IsHistoryMark))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.History, nameof(HistoryConfig.IsKeepFolderStatus))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.History, nameof(HistoryConfig.IsKeepSearchHistory))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Bookshelf, nameof(BookshelfConfig.IsPageListDocked))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Bookshelf, nameof(BookshelfConfig.IsCloseBookWhenMove))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Bookshelf, nameof(BookshelfConfig.IsOpenNextBookWhenRemove))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Bookshelf, nameof(BookshelfConfig.IsInsertItem))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.System, nameof(SystemConfig.IsHiddenFileVisibled))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Bookshelf, nameof(BookshelfConfig.IsMultipleRarFilterEnabled))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Bookshelf, nameof(BookshelfConfig.ExcludePattern))) { IsStretch = true });
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Bookshelf, nameof(BookshelfConfig.DefaultFolderOrder), new PropertyMemberElementOptions() { EnumMap = FolderOrderClass.Normal.GetFolderOrderMap().ToDictionary(e => (Enum)e.Key, e => e.Value) })));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Bookshelf, nameof(BookshelfConfig.PlaylistFolderOrder), new PropertyMemberElementOptions() { EnumMap = FolderOrderClass.Full.GetFolderOrderMap().ToDictionary(e => (Enum)e.Key, e => e.Value) })));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Bookmark, nameof(BookmarkConfig.BookmarkFolderOrder), new PropertyMemberElementOptions() { EnumMap = FolderOrderClass.Full.GetFolderOrderMap().ToDictionary(e => (Enum)e.Key, e => e.Value) })));
            this.Items.Add(section);

            section = new SettingItemSection(Properties.Resources.SettingPagePanelBookshelfTree);
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Bookshelf, nameof(BookshelfConfig.FolderTreeLayout))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Panels, nameof(PanelsConfig.FolderTreeFontSize))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Bookshelf, nameof(BookshelfConfig.IsSyncFolderTree))));
            this.Items.Add(section);
        }
    }

    public class SettingPagePanelFileInfo : SettingPage
    {
        public SettingPagePanelFileInfo() : base(Properties.Resources.SettingPagePanelFileInfo)
        {
            var section = new SettingItemSection(Properties.Resources.SettingPagePanelFileInfoVisual);
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Information, nameof(InformationConfig.IsVisibleFilePath))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Information, nameof(InformationConfig.IsVisibleBitsPerPixel))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Information, nameof(InformationConfig.IsVisibleLoader))));

            this.Items = new List<SettingItem>() { section };
        }
    }

    public class SettingPagePanelEffect : SettingPage
    {
        public SettingPagePanelEffect() : base(Properties.Resources.SettingPagePanelEffect)
        {
            var section = new SettingItemSection(Properties.Resources.SettingPagePanelEffectVisual);
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.ImageEffect, nameof(ImageEffectConfig.IsHsvMode))));

            this.Items = new List<SettingItem>() { section };
        }
    }

    public class SettingPageVisualSlideshow : SettingPage
    {
        public SettingPageVisualSlideshow() : base(Properties.Resources.SettingPageVisualSlideshow)
        {
            var section = new SettingItemSection(Properties.Resources.SettingPageVisualSlideshowGeneral);
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.SlideShow, nameof(SlideShowConfig.IsSlideShowByLoop))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.SlideShow, nameof(SlideShowConfig.IsCancelSlideByMouseMove))));
            section.Children.Add(new SettingItemIndexValue<double>(PropertyMemberElement.Create(Config.Current.SlideShow, nameof(SlideShowConfig.SlideShowInterval)), new SlideShowInterval(), true));

            this.Items = new List<SettingItem>() { section };
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
