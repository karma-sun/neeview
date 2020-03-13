using System;
using System.Windows.Data;


namespace NeeView
{
    public class ToggleIsSupportedSingleFirstPageCommand : CommandElement
    {
        public ToggleIsSupportedSingleFirstPageCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupPageSetting;
            this.Text = Properties.Resources.CommandToggleIsSupportedSingleFirstPage;
            this.Note = Properties.Resources.CommandToggleIsSupportedSingleFirstPageNote;
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.BindingBookSetting(nameof(BookSettingPresenter.Current.LatestSetting.IsSupportedSingleFirstPage));
        }

        public override string ExecuteMessage(CommandParameter param, object[] args, CommandOption option)
        {
            return BookSettingPresenter.Current.LatestSetting.IsSupportedSingleFirstPage ? Properties.Resources.CommandToggleIsSupportedSingleFirstPageOff : Properties.Resources.CommandToggleIsSupportedSingleFirstPageOn;
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
                BookSettingPresenter.Current.SetIsSupportedSingleFirstPage(Convert.ToBoolean(args[0]));
            }
            else
            {
                BookSettingPresenter.Current.ToggleIsSupportedSingleFirstPage();
            }
        }
    }
}
