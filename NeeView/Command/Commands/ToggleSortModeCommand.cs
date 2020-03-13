namespace NeeView
{
    public class ToggleSortModeCommand : CommandElement
    {
        public ToggleSortModeCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupPageOrder;
            this.Text = Properties.Resources.CommandToggleSortMode;
            this.Note = Properties.Resources.CommandToggleSortModeNote;
            this.IsShowMessage = true;
        }
        public override string ExecuteMessage(CommandParameter param, object[] args, CommandOption option)
        {
            return BookSettingPresenter.Current.LatestSetting.SortMode.GetToggle().ToAliasName();
        }

        public override bool CanExecute(CommandParameter param, object[] args, CommandOption option)
        {
            return !NowLoading.Current.IsDispNowLoading;
        }

        public override void Execute(CommandParameter param, object[] args, CommandOption option)
        {
            BookSettingPresenter.Current.ToggleSortMode();
        }
    }
}
