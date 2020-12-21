namespace NeeView
{
    public class TogglePageModeCommand : CommandElement
    {
        public TogglePageModeCommand()
        {
            this.Group = Properties.Resources.CommandGroup_PageSetting;
            this.IsShowMessage = true;
        }

        public override string ExecuteMessage(object sender, CommandContext e)
        {
            return BookSettingPresenter.Current.LatestSetting.PageMode.GetToggle().ToAliasName();
        }

        public override void Execute(object sender, CommandContext e)
        {
            BookSettingPresenter.Current.TogglePageMode();
        }
    }
}
