namespace NeeView
{
    public class SetDefaultPageSettingCommand : CommandElement
    {
        public SetDefaultPageSettingCommand() : base(CommandType.SetDefaultPageSetting)
        {
            this.Group = Properties.Resources.CommandGroupPageSetting;
            this.Text = Properties.Resources.CommandSetDefaultPageSetting;
            this.Note = Properties.Resources.CommandSetDefaultPageSettingNote;
            this.IsShowMessage = true;
        }

        public override bool CanExecute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            return !NowLoading.Current.IsDispNowLoading;
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            BookSettingPresenter.Current.SetDefaultPageSetting();
        }
    }
}
