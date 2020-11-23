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

        public override string ExecuteMessage(object sender, CommandContext e)
        {
            return BookSettingPresenter.Current.LatestSetting.BookReadOrder.GetToggle().ToAliasName();
        }

        public override bool CanExecute(object sender, CommandContext e)
        {
            return !NowLoading.Current.IsDispNowLoading;
        }

        public override void Execute(object sender, CommandContext e)
        {
            BookSettingPresenter.Current.ToggleBookReadOrder();
        }
    }
}
