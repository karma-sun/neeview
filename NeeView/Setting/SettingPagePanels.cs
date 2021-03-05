using NeeLaboratory.Windows.Input;
using NeeView.Data;
using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace NeeView.Setting
{
    public class SettingPagePanels : SettingPage
    {
        /// <summary>
        /// Setting: Panels
        /// </summary>
        public SettingPagePanels() : base(Properties.Resources.SettingPage_Panels)
        {
            this.Children = new List<SettingPage>
            {
                new SettingPagePanelItems(),
                new SettingPageBookshelf(),
                new SettingPageFileInfo(),
                new SettingPageEffect(),
                new SettingPageFilmstrip(),
                new SettingPageSlider(),
                new SettingPagePageTitle(),
            };

            this.Items = new List<SettingItem>();

            var section = new SettingItemSection(Properties.Resources.SettingPage_Panels_AutoHide);
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.AutoHide, nameof(AutoHideConfig.AutoHideFocusLockMode))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.AutoHide, nameof(AutoHideConfig.IsAutoHideKeyDownDelay))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.AutoHide, nameof(AutoHideConfig.AutoHideDelayVisibleTime))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.AutoHide, nameof(AutoHideConfig.AutoHideDelayTime))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.AutoHide, nameof(AutoHideConfig.AutoHideHitTestMargin))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.AutoHide, nameof(AutoHideConfig.AutoHideConfrictMargin))));
            this.Items.Add(section);

            section = new SettingItemSection(Properties.Resources.SettingPage_Panels_AutoHideMode);
            section.Children.Add(new SettingItemHeader(Properties.Resources.SettingPage_Panels_AutoHideMode_WindowState));
            section.Children.Add(new SettingItemSubProperty(PropertyMemberElement.Create(Config.Current.Window, nameof(WindowConfig.IsAutoHideInFullScreen), new PropertyMemberElementOptions() { Name = AliasNameExtensions.GetAliasName(WindowStateEx.FullScreen) })));
            section.Children.Add(new SettingItemSubProperty(PropertyMemberElement.Create(Config.Current.Window, nameof(WindowConfig.IsAutoHidInMaximized), new PropertyMemberElementOptions() { Name = AliasNameExtensions.GetAliasName(WindowStateEx.Maximized) })));
            section.Children.Add(new SettingItemSubProperty(PropertyMemberElement.Create(Config.Current.Window, nameof(WindowConfig.IsAutoHideInNormal), new PropertyMemberElementOptions() { Name = AliasNameExtensions.GetAliasName(WindowStateEx.Normal) })));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.MenuBar, nameof(MenuBarConfig.IsHideMenuInAutoHideMode))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Panels, nameof(PanelsConfig.IsHidePanelInAutoHideMode))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Slider, nameof(SliderConfig.IsHidePageSliderInAutoHideMode))));
            this.Items.Add(section);


            section = new SettingItemSection(Properties.Resources.SettingPage_Panels_SidePanels);
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Panels, nameof(PanelsConfig.Opacity))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Panels, nameof(PanelsConfig.OpenWithDoubleClick))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Panels, nameof(PanelsConfig.IsLeftRightKeyEnabled))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Panels, nameof(PanelsConfig.IsManipulationBoundaryFeedbackEnabled))));
            this.Items.Add(section);

        }
    }


    /// <summary>
    /// Setting: PanelItems
    /// </summary>
    public class SettingPagePanelItems : SettingPage
    {
        public SettingPagePanelItems() : base(Properties.Resources.SettingPage_PanelItems)
        {
            this.Items = new List<SettingItem>();

            var section = new SettingItemSection(Properties.Resources.SettingPage_PanelItems_Font);
            section.Children.Add(new SettingItemPropertyFont(PropertyMemberElement.Create(Config.Current.Panels, nameof(PanelsConfig.FontName))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Panels, nameof(PanelsConfig.FontSize))));
            this.Items.Add(section);

            section = new SettingItemSection(Properties.Resources.SettingPage_PanelItems_StyleContent);
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Panels.ContentItemProfile, nameof(PanelListItemProfile.ImageWidth))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Panels.ContentItemProfile, nameof(PanelListItemProfile.ImageShape))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Panels.ContentItemProfile, nameof(PanelListItemProfile.IsImagePopupEnabled))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Panels.ContentItemProfile, nameof(PanelListItemProfile.IsTextWrapped))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Panels.ContentItemProfile, nameof(PanelListItemProfile.NoteOpacity))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Panels, nameof(PanelsConfig.IsDecoratePlace))));
            this.Items.Add(section);

            section = new SettingItemSection(Properties.Resources.SettingPage_PanelItems_StyleBanner);
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Panels.BannerItemProfile, nameof(PanelListItemProfile.ImageWidth))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Panels.BannerItemProfile, nameof(PanelListItemProfile.IsImagePopupEnabled))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Panels.BannerItemProfile, nameof(PanelListItemProfile.IsTextWrapped))));
            this.Items.Add(section);

            section = new SettingItemSection(Properties.Resources.SettingPage_PanelItems_StyleThumbnail);
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Panels.ThumbnailItemProfile, nameof(PanelListItemProfile.ImageWidth))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Panels.ThumbnailItemProfile, nameof(PanelListItemProfile.ImageShape))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Panels.ThumbnailItemProfile, nameof(PanelListItemProfile.IsImagePopupEnabled))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Panels.ThumbnailItemProfile, nameof(PanelListItemProfile.IsTextVisible))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Panels.ThumbnailItemProfile, nameof(PanelListItemProfile.IsTextWrapped))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Panels, nameof(PanelsConfig.MouseWheelSpeedRate))));
            this.Items.Add(section);
        }
    }


    /// <summary>
    /// Setting: Bookshelf
    /// </summary>
    public class SettingPageBookshelf : SettingPage
    {
        public SettingPageBookshelf() : base(Properties.Resources.SettingPage_Bookshelf)
        {
            this.Items = new List<SettingItem>();

            var section = new SettingItemSection(Properties.Resources.SettingPage_Bookshelf_General);
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Bookshelf, nameof(BookshelfConfig.Home))) { IsStretch = true });
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Bookshelf, nameof(BookshelfConfig.IsVisibleItemsCount))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Bookshelf, nameof(BookshelfConfig.IsVisibleBookmarkMark))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Bookshelf, nameof(BookshelfConfig.IsVisibleHistoryMark))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.History, nameof(HistoryConfig.IsKeepFolderStatus))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.History, nameof(HistoryConfig.IsKeepSearchHistory))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Bookshelf, nameof(BookshelfConfig.IsCloseBookWhenMove))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Bookshelf, nameof(BookshelfConfig.IsOpenNextBookWhenRemove))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Bookshelf, nameof(BookshelfConfig.IsInsertItem))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.System, nameof(SystemConfig.IsHiddenFileVisibled))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Bookshelf, nameof(BookshelfConfig.IsMultipleRarFilterEnabled))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Bookshelf, nameof(BookshelfConfig.IsOrderWithoutFileType))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Bookshelf, nameof(BookshelfConfig.ExcludePattern))) { IsStretch = true });
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Bookshelf, nameof(BookshelfConfig.DefaultFolderOrder), new PropertyMemberElementOptions() { EnumMap = FolderOrderClass.Normal.GetFolderOrderMap().ToDictionary(e => (Enum)e.Key, e => e.Value) })));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Bookshelf, nameof(BookshelfConfig.PlaylistFolderOrder), new PropertyMemberElementOptions() { EnumMap = FolderOrderClass.Full.GetFolderOrderMap().ToDictionary(e => (Enum)e.Key, e => e.Value) })));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Bookmark, nameof(BookmarkConfig.BookmarkFolderOrder), new PropertyMemberElementOptions() { EnumMap = FolderOrderClass.Full.GetFolderOrderMap().ToDictionary(e => (Enum)e.Key, e => e.Value) })));
            this.Items.Add(section);

            section = new SettingItemSection(Properties.Resources.SettingPage_Bookshelf_Tree);
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Bookshelf, nameof(BookshelfConfig.FolderTreeLayout))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Panels, nameof(PanelsConfig.FolderTreeFontSize))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Bookshelf, nameof(BookshelfConfig.IsSyncFolderTree))));
            this.Items.Add(section);
        }
    }


    /// <summary>
    /// Setting: FileInfo
    /// </summary>
    public class SettingPageFileInfo : SettingPage
    {
        public SettingPageFileInfo() : base(Properties.Resources.SettingPage_Information)
        {
            var section = new SettingItemSection(Properties.Resources.SettingPage_Information_Visual);
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Information, nameof(InformationConfig.DateTimeFormat))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Information, nameof(InformationConfig.MapProgramFormat))) { IsStretch = true });

            this.Items = new List<SettingItem>() { section };
        }
    }


    /// <summary>
    /// Setting: Effect
    /// </summary>
    public class SettingPageEffect : SettingPage
    {
        public SettingPageEffect() : base(Properties.Resources.SettingPage_Effect)
        {
            var section = new SettingItemSection(Properties.Resources.SettingPage_Effect_Visual);
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.ImageDotKeep, nameof(ImageDotKeepConfig.Threshold))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.ImageEffect, nameof(ImageEffectConfig.IsHsvMode))));

            this.Items = new List<SettingItem>() { section };
        }
    }


    /// <summary>
    /// Setting: Filmstrip
    /// </summary>
    public class SettingPageFilmstrip : SettingPage
    {
        public SettingPageFilmstrip() : base(Properties.Resources.SettingPage_Filmstrip)
        {
            var section = new SettingItemSection(Properties.Resources.SettingPage_Filmstrip);
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.FilmStrip, nameof(FilmStripConfig.ImageWidth))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Slider, nameof(SliderConfig.IsSliderLinkedFilmStrip))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.FilmStrip, nameof(FilmStripConfig.IsVisibleNumber))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.FilmStrip, nameof(FilmStripConfig.IsVisiblePagemark))));
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

                var dialog = new MessageDialog("", Properties.Resources.CacheDeletedDialog_Title);
                if (element != null)
                {
                    dialog.Owner = Window.GetWindow(element);
                }
                dialog.ShowDialog();
            }
            catch (Exception ex)
            {
                var dialog = new MessageDialog(ex.Message, Properties.Resources.CacheDeletedFailedDialog_Title);
                if (element != null)
                {
                    dialog.Owner = Window.GetWindow(element);
                }
                dialog.ShowDialog();
            }
        }

        #endregion
    }


    /// <summary>
    /// Setting: Slider
    /// </summary>
    public class SettingPageSlider : SettingPage
    {
        public SettingPageSlider() : base(Properties.Resources.SettingPage_Slider)
        {
            var section = new SettingItemSection(Properties.Resources.SettingPage_Slider);
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Slider, nameof(SliderConfig.Opacity))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Slider, nameof(SliderConfig.SliderDirection))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Slider, nameof(SliderConfig.SliderIndexLayout))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Slider, nameof(SliderConfig.IsVisiblePagemark))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Slider, nameof(SliderConfig.IsSyncPageMode))));

            this.Items = new List<SettingItem>() { section };
        }
    }


    /// <summary>
    /// Setting: PageTitle
    /// </summary>
    public class SettingPagePageTitle : SettingPage
    {
        public SettingPagePageTitle() : base(Properties.Resources.SettingPage_PageTitle)
        {
            var section = new SettingItemSection(Properties.Resources.SettingPage_PageTitle);
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.PageTitle, nameof(PageTitleConfig.IsEnabled))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.PageTitle, nameof(PageTitleConfig.FontSize))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.PageTitle, nameof(PageTitleConfig.PageTitleFormat1))) { IsStretch = true });
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.PageTitle, nameof(PageTitleConfig.PageTitleFormat2))) { IsStretch = true });
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.PageTitle, nameof(PageTitleConfig.PageTitleFormatMedia))) { IsStretch = true });
            section.Children.Add(new SettingItemNote(Properties.Resources.SettingPage_WindowTitle_Note));

            this.Items = new List<SettingItem>() { section };
        }
    }
}
