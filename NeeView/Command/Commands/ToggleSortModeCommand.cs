namespace NeeView
{
    public class ToggleSortModeCommand : CommandElement
    {
        public ToggleSortModeCommand()
        {
            this.Group = Properties.Resources.CommandGroup_PageOrder;
            this.IsShowMessage = true;
        }
        public override string ExecuteMessage(object sender, CommandContext e)
        {
            return BookOperation.Current.PageSortModeClass.GetTogglePageSortMode(BookSettingPresenter.Current.LatestSetting.SortMode).ToAliasName();
        }

        public override bool CanExecute(object sender, CommandContext e)
        {
            return !NowLoading.Current.IsDispNowLoading;
        }

        public override void Execute(object sender, CommandContext e)
        {
            BookSettingPresenter.Current.ToggleSortMode(BookOperation.Current.PageSortModeClass);
        }
    }
}
