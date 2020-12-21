using System;
using System.Windows.Data;


namespace NeeView
{
    public class ToggleIsSupportedDividePageCommand : CommandElement
    {
        public ToggleIsSupportedDividePageCommand()
        {
            this.Group = Properties.Resources.CommandGroup_PageSetting;
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.BindingBookSetting(nameof(BookSettingPresenter.Current.LatestSetting.IsSupportedDividePage));
        }

        public override string ExecuteMessage(object sender, CommandContext e)
        {
            return BookSettingPresenter.Current.LatestSetting.IsSupportedDividePage ? Properties.Resources.ToggleIsSupportedDividePageCommand_Off : Properties.Resources.ToggleIsSupportedDividePageCommand_On;
        }

        public override bool CanExecute(object sender, CommandContext e)
        {
            return BookSettingPresenter.Current.CanPageModeSubSetting(PageMode.SinglePage);
        }

        [MethodArgument("@CommandToggleArgument")]
        public override void Execute(object sender, CommandContext e)
        {
            if (e.Args.Length > 0)
            {
                BookSettingPresenter.Current.SetIsSupportedDividePage(Convert.ToBoolean(e.Args[0]));
            }
            else
            {
                BookSettingPresenter.Current.ToggleIsSupportedDividePage();
            }
        }
    }
}
