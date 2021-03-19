using NeeView.Windows.Property;
using System.Collections.Generic;

namespace NeeView.Setting
{
    /// <summary>
    /// Setting: Window
    /// </summary>
    public class SettingPageWindow : SettingPage
    {
        public SettingPageWindow() : base(Properties.Resources.SettingPage_Window)
        {
            this.Children = new List<SettingPage>
            {
                new SettingPageFonts(),
                new SettingPageWindowTitle(),
            };

            this.Items = new List<SettingItem>();

            var section = new SettingItemSection(Properties.Resources.SettingPage_Window_Theme);
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Theme, nameof(ThemeConfig.PanelColor))));
            this.Items.Add(section);

            section = new SettingItemSection(Properties.Resources.SettingPage_Window_Background);
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Background, nameof(BackgroundConfig.CustomBackground)),
                new BackgroundSettingControl(Config.Current.Background.CustomBackground)));
            this.Items.Add(section);

            section = new SettingItemSection(Properties.Resources.SettingPage_Window_Advance);
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.View, nameof(ViewConfig.MainViewMergin))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.MenuBar, nameof(MenuBarConfig.IsHamburgerMenu))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Window, nameof(WindowConfig.IsCaptionEmulateInFullScreen))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Window, nameof(WindowConfig.WindowChromeFrame))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Window, nameof(WindowConfig.MaximizeWindowGapWidth))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Window, nameof(WindowConfig.MouseActivateAndEat))));
            this.Items.Add(section);
        }
    }


    /// <summary>
    /// SettingPage: Fonts
    /// </summary>
    public class SettingPageFonts : SettingPage
    {
        public SettingPageFonts() : base(Properties.Resources.SettingPage_Fonts)
        {
            this.Items = new List<SettingItem>();

            var section = new SettingItemSection(Properties.Resources.SettingPage_Fonts);
            section.Children.Add(new SettingItemPropertyFont(PropertyMemberElement.Create(Config.Current.Fonts, nameof(FontsConfig.FontName))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Fonts, nameof(FontsConfig.FontScale))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Fonts, nameof(FontsConfig.MenuFontScale))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Fonts, nameof(FontsConfig.FolderTreeFontScale))));
            section.Children.Add(new SettingItemPropertyFont(PropertyMemberElement.Create(Config.Current.Fonts, nameof(FontsConfig.PanelFontName))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Fonts, nameof(FontsConfig.PanelFontScale))));
            this.Items.Add(section);
        }
    }

    /// <summary>
    /// Setting: WindowTitle
    /// </summary>
    public class SettingPageWindowTitle : SettingPage
    {
        public SettingPageWindowTitle() : base(Properties.Resources.SettingPage_WindowTitle)
        {
            var section = new SettingItemSection(Properties.Resources.SettingPage_WindowTitle);
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.WindowTitle, nameof(WindowTitleConfig.WindowTitleFormat1))) { IsStretch = true });
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.WindowTitle, nameof(WindowTitleConfig.WindowTitleFormat2))) { IsStretch = true });
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.WindowTitle, nameof(WindowTitleConfig.WindowTitleFormatMedia))) { IsStretch = true });
            section.Children.Add(new SettingItemNote(Properties.Resources.SettingPage_WindowTitle_Note));

            this.Items = new List<SettingItem>() { section };
        }
    }

}
