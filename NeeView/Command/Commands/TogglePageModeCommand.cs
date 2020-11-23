namespace NeeView
{
    public class TogglePageModeCommand : CommandElement
    {
        public TogglePageModeCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupPageSetting;
            this.Text = Properties.Resources.CommandTogglePageMode;
            this.Note = Properties.Resources.CommandTogglePageModeNote;
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
