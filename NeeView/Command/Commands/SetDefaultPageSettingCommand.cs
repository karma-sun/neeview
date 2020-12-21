namespace NeeView
{
    public class SetDefaultPageSettingCommand : CommandElement
    {
        public SetDefaultPageSettingCommand()
        {
            this.Group = Properties.Resources.CommandGroup_PageSetting;
            this.IsShowMessage = true;
        }

        public override bool CanExecute(object sender, CommandContext e)
        {
            return !NowLoading.Current.IsDispNowLoading;
        }

        public override void Execute(object sender, CommandContext e)
        {
            BookSettingPresenter.Current.SetDefaultPageSetting();
        }
    }
}
