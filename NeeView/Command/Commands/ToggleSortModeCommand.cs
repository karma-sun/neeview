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
        public override string ExecuteMessage(object sender, CommandContext e)
        {
            return BookSettingPresenter.Current.LatestSetting.SortMode.GetToggle().ToAliasName();
        }

        public override bool CanExecute(object sender, CommandContext e)
        {
            return !NowLoading.Current.IsDispNowLoading;
        }

        public override void Execute(object sender, CommandContext e)
        {
            BookSettingPresenter.Current.ToggleSortMode();
        }
    }
}
