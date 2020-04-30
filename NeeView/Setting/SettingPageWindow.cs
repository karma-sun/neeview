using NeeView.Windows.Property;
using System.Collections.Generic;

namespace NeeView.Setting
{
    /// <summary>
    /// Setting: Window
    /// </summary>
    public class SettingPageWindow : SettingPage
    {
        public SettingPageWindow() : base(Properties.Resources.SettingPageWindow)
        {
            this.Children = new List<SettingPage>
            {
                new SettingPageWindowTitile(),
            };

            this.Items = new List<SettingItem>();

            var section = new SettingItemSection(Properties.Resources.SettingPageVisualGeneralTheme);
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Theme, nameof(ThemeConfig.PanelColor))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Theme, nameof(ThemeConfig.MenuColor))));
            this.Items.Add(section);

            section = new SettingItemSection(Properties.Resources.SettingPageVisualGeneralBackground);
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Background, nameof(BackgroundConfig.CustomBackground)),
                new BackgroundSettingControl(Config.Current.Background.CustomBackground)));
            this.Items.Add(section);

            section = new SettingItemSection(Properties.Resources.SettingPageVisualGeneralAdvance);
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.MenuBar, nameof(MenuBarConfig.IsHamburgerMenu))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Window, nameof(WindowConfig.IsFullScreenWithTaskBar))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Window, nameof(WindowConfig.IsCaptionEmulateInFullScreen))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Window, nameof(WindowConfig.WindowChromeFrame))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Window, nameof(WindowConfig.MaximizeWindowGapWidth))));
            this.Items.Add(section);
        }
    }


    /// <summary>
    /// Setting: WindowTitle
    /// </summary>
    public class SettingPageWindowTitile : SettingPage
    {
        public SettingPageWindowTitile() : base(Properties.Resources.SettingPageVisualWindowTitile)
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

}
