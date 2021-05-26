using NeeLaboratory.Windows.Input;
using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace NeeView.Setting
{
    /// <summary>
    /// SettingPage: Command
    /// </summary>
    class SettingPageCommand : SettingPage
    {
        public SettingPageCommand() : base(Properties.Resources.SettingPage_Command)
        {
            this.Children = new List<SettingPage>
            {
                new SettingPageCommandList(),
                new SettingPageContextMenu(),
                new SettingPageScript(),
            };

            this.Items = new List<SettingItem>();

            var section = new SettingItemSection(Properties.Resources.SettingPage_Command_GeneralAdvance);
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Command, nameof(CommandConfig.IsAccessKeyEnabled))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Command, nameof(CommandConfig.IsReversePageMove))));
            section.Children.Add(new SettingItemSubProperty(PropertyMemberElement.Create(Config.Current.Command, nameof(CommandConfig.IsReversePageMoveWheel)))
            {
                IsEnabled = new IsEnabledPropertyValue(Config.Current.Command, nameof(CommandConfig.IsReversePageMove)),
            });
            this.Items.Add(section);
        }
    }

    /// <summary>
    /// SettingPage: Script
    /// </summary>
    class SettingPageScript : SettingPage
    {
        public SettingPageScript() : base(Properties.Resources.SettingPage_Script)
        {
            this.Items = new List<SettingItem>();

            var section = new SettingItemSection(Properties.Resources.SettingPage_Script);
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Script, nameof(ScriptConfig.IsScriptFolderEnabled))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Script, nameof(ScriptConfig.ScriptFolder), new PropertyMemberElementOptions() { EmptyValue = SaveData.DefaultScriptsFolder }))
            {
                IsStretch = true,
                SubContent = UIElementTools.CreateHyperlink(Properties.Resources.SettingPage_Script_OpenScriptFolder, new RelayCommand(CommandTable.Current.ScriptManager.OpenScriptsFolder)),
            });

            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Script, nameof(ScriptConfig.ErrorLevel))) { SubContent = CreateScriptErrorLevelRemarks() });

            this.Items.Add(section);
        }


        private TextBlock CreateScriptErrorLevelRemarks()
        {
            var binding = new Binding(nameof(ScriptConfig.ErrorLevel))
            {
                Source = Config.Current.Script,
                Converter = new ScriptErrorLevelToRemarksConverter(),
            };
            var textBlock = new TextBlock();
            textBlock.TextWrapping = TextWrapping.Wrap;
            textBlock.SetBinding(TextBlock.TextProperty, binding);
            textBlock.SetResourceReference(TextBlock.ForegroundProperty, "Control.GrayText");
            return textBlock;
        }

        private class ScriptErrorLevelToRemarksConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                if (value is ScriptErrorLevel errorLevel)
                {
                    return errorLevel.GetRemarks();
                }
                return value;
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }

    }

    /// <summary>
    /// SettingPage: CommandList
    /// </summary>
    class SettingPageCommandList : SettingPage
    {
        public SettingPageCommandList() : base(Properties.Resources.SettingPage_Command_Main)
        {
            var linkCommand = new RelayCommand(() => this.IsSelected = true);

            this.IsScrollEnabled = false;

            var section = new SettingItemSection(Properties.Resources.SettingPage_Command_Main);
            section.Children.Add(new SettingItemCommand() { SearchResultItem = new SettingItemLink(Properties.Resources.SettingPage_Command_Main, linkCommand) { IsContentOnly = true } });
            this.Items = new List<SettingItem>() { section };
        }
    }

    /// <summary>
    /// SettingPage: ContextMenu
    /// </summary>
    class SettingPageContextMenu : SettingPage
    {
        public SettingPageContextMenu() : base(Properties.Resources.SettingPage_ContextMenu)
        {
            var linkCommand = new RelayCommand(() => this.IsSelected = true);

            this.IsScrollEnabled = false;

            var section = new SettingItemSection(Properties.Resources.SettingPage_ContextMenu_Edit);
            section.Children.Add(new SettingItemContextMenu() { SearchResultItem = new SettingItemLink(Properties.Resources.SettingPage_ContextMenu_Edit, linkCommand) { IsContentOnly = true } });
            this.Items = new List<SettingItem>() { section };
        }
    }
}
