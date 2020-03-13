using System;
using System.Windows.Data;


namespace NeeView
{
    public class ToggleIsSupportedWidePageCommand : CommandElement
    {
        public ToggleIsSupportedWidePageCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupPageSetting;
            this.Text = Properties.Resources.CommandToggleIsSupportedWidePage;
            this.Note = Properties.Resources.CommandToggleIsSupportedWidePageNote;
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.BindingBookSetting(nameof(BookSettingPresenter.Current.LatestSetting.IsSupportedWidePage));
        }

        public override string ExecuteMessage(CommandParameter param, object[] args, CommandOption option)
        {
            return BookSettingPresenter.Current.LatestSetting.IsSupportedWidePage ? Properties.Resources.CommandToggleIsSupportedWidePageOff : Properties.Resources.CommandToggleIsSupportedWidePageOn;
        }

        public override bool CanExecute(CommandParameter param, object[] args, CommandOption option)
        {
            return BookSettingPresenter.Current.CanPageModeSubSetting(PageMode.WidePage);
        }

        [MethodArgument("@CommandToggleArgument")]
        public override void Execute(CommandParameter param, object[] args, CommandOption option)
        {
            if (args.Length > 0)
            {
                BookSettingPresenter.Current.SetIsSupportedWidePage(Convert.ToBoolean(args[0]));
            }
            else
            {
                BookSettingPresenter.Current.ToggleIsSupportedWidePage();
            }
        }
    }
}
