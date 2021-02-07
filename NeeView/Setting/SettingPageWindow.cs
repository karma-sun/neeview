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
                new SettingPageWindowTitile(),
            };

            this.Items = new List<SettingItem>();

            var section = new SettingItemSection(Properties.Resources.SettingPage_Window_Theme);
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Theme, nameof(ThemeConfig.PanelColor))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Theme, nameof(ThemeConfig.MenuColor))));
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
    /// Setting: WindowTitle
    /// </summary>
    public class SettingPageWindowTitile : SettingPage
    {
        public SettingPageWindowTitile() : base(Properties.Resources.SettingPage_WindowTitile)
        {
            var section = new SettingItemSection(Properties.Resources.SettingPage_WindowTitile);
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.WindowTittle, nameof(WindowTitleConfig.WindowTitleFormat1))) { IsStretch = true });
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.WindowTittle, nameof(WindowTitleConfig.WindowTitleFormat2))) { IsStretch = true });
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.WindowTittle, nameof(WindowTitleConfig.WindowTitleFormatMedia))) { IsStretch = true });
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.WindowTittle, nameof(WindowTitleConfig.IsMainViewDisplayEnabled))));
            section.Children.Add(new SettingItemNote(Properties.Resources.SettingPage_WindowTitile_Note));

            this.Items = new List<SettingItem>() { section };
        }
    }

}
