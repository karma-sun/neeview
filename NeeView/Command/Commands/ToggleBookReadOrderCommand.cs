namespace NeeView
{
    public class ToggleBookReadOrderCommand : CommandElement
    {
        public ToggleBookReadOrderCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupPageSetting;
            this.Text = Properties.Resources.CommandToggleBookReadOrder;
            this.Note = Properties.Resources.CommandToggleBookReadOrderNote;
            this.IsShowMessage = true;
        }

        public override string ExecuteMessage(CommandParameter param, object arg, CommandOption option)
        {
            return BookSettingPresenter.Current.LatestSetting.BookReadOrder.GetToggle().ToAliasName();
        }

        public override bool CanExecute(CommandParameter param, object arg, CommandOption option)
        {
            return !NowLoading.Current.IsDispNowLoading;
        }

        public override void Execute(CommandParameter param, object arg, CommandOption option)
        {
            BookSettingPresenter.Current.ToggleBookReadOrder();
        }
    }
}
