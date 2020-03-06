namespace NeeView
{
    public class ToggleSortModeCommand : CommandElement
    {
        public ToggleSortModeCommand() : base(CommandType.ToggleSortMode)
        {
            this.Group = Properties.Resources.CommandGroupPageOrder;
            this.Text = Properties.Resources.CommandToggleSortMode;
            this.Note = Properties.Resources.CommandToggleSortModeNote;
            this.IsShowMessage = true;
        }
        public override string ExecuteMessage(CommandParameter param, CommandOption option = CommandOption.None)
        {
            return BookSettingPresenter.Current.LatestSetting.SortMode.GetToggle().ToAliasName();
        }

        public override bool CanExecute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            return !NowLoading.Current.IsDispNowLoading;
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            BookSettingPresenter.Current.ToggleSortMode();
        }
    }
}
