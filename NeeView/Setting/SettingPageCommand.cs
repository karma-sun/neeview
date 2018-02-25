using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeeView.Setting
{
    class SettingPageCommand : SettingPage
    {
        public SettingPageCommand() : base("コマンド")
        {
            this.Children = new List<SettingPage>
            {
                new SettingPageCommandMain(),
                new SettingPageCommandGeneral(),
            };
        }
    }

    public class SettingPageCommandGeneral : SettingPage
    {
        public SettingPageCommandGeneral() : base("コマンド全般")
        {
            this.Items = new List<SettingItem>
            {
                new SettingItemSection("詳細設定",
                    new SettingItemProperty(PropertyMemberElement.Create(MainWindowModel.Current, nameof(MainWindowModel.IsAccessKeyEnabled))),
                    new SettingItemProperty(PropertyMemberElement.Create(CommandTable.Current, nameof(CommandTable.IsReversePageMove))),
                    new SettingItemProperty(PropertyMemberElement.Create(CommandTable.Current, nameof(CommandTable.IsReversePageMoveWheel)))
                    {
                        IsEnabled = new IsEnabledPropertyValue(CommandTable.Current, nameof(CommandTable.IsReversePageMove)),
                    }),
            };
        }
    }

    class SettingPageCommandMain : SettingPage
    {
        public SettingPageCommandMain() : base("コマンド設定")
        {
            this.IsScrollEnabled = false;

            this.Items = new List<SettingItem>
            {
                new SettingItemCommand(),
            };
        }
    }

}
