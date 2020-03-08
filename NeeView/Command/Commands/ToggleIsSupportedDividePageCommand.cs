using System.Windows.Data;


namespace NeeView
{
    public class ToggleIsSupportedDividePageCommand : CommandElement
    {
        public ToggleIsSupportedDividePageCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupPageSetting;
            this.Text = Properties.Resources.CommandToggleIsSupportedDividePage;
            this.Note = Properties.Resources.CommandToggleIsSupportedDividePageNote;
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.BindingBookSetting(nameof(BookSettingPresenter.Current.LatestSetting.IsSupportedDividePage));
        }

        public override string ExecuteMessage(CommandParameter param, CommandOption option = CommandOption.None)
        {
            return BookSettingPresenter.Current.LatestSetting.IsSupportedDividePage ? Properties.Resources.CommandToggleIsSupportedDividePageOff : Properties.Resources.CommandToggleIsSupportedDividePageOn;
        }

        public override bool CanExecute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            return BookSettingPresenter.Current.CanPageModeSubSetting(PageMode.SinglePage);
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            BookSettingPresenter.Current.ToggleIsSupportedDividePage();
        }
    }
}
