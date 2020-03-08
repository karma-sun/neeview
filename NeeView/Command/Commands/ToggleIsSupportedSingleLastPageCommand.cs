using System.Windows.Data;


namespace NeeView
{
    public class ToggleIsSupportedSingleLastPageCommand : CommandElement
    {
        public ToggleIsSupportedSingleLastPageCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupPageSetting;
            this.Text = Properties.Resources.CommandToggleIsSupportedSingleLastPage;
            this.Note = Properties.Resources.CommandToggleIsSupportedSingleLastPageNote;
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.BindingBookSetting(nameof(BookSettingPresenter.Current.LatestSetting.IsSupportedSingleLastPage));
        }

        public override string ExecuteMessage(CommandParameter param, CommandOption option = CommandOption.None)
        {
            return BookSettingPresenter.Current.LatestSetting.IsSupportedSingleLastPage ? Properties.Resources.CommandToggleIsSupportedSingleLastPageOff : Properties.Resources.CommandToggleIsSupportedSingleLastPageOn;
        }

        public override bool CanExecute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            return BookSettingPresenter.Current.CanPageModeSubSetting(PageMode.WidePage);
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            BookSettingPresenter.Current.ToggleIsSupportedSingleLastPage();
        }
    }
}
