namespace NeeView
{
    public class SetDefaultPageSettingCommand : CommandElement
    {
        public SetDefaultPageSettingCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupPageSetting;
            this.Text = Properties.Resources.CommandSetDefaultPageSetting;
            this.Note = Properties.Resources.CommandSetDefaultPageSettingNote;
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
