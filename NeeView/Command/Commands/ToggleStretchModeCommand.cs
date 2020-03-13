namespace NeeView
{
    public class ToggleStretchModeCommand : CommandElement
    {
        public ToggleStretchModeCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupImageScale;
            this.Text = Properties.Resources.CommandToggleStretchMode;
            this.Note = Properties.Resources.CommandToggleStretchModeNote;
            this.ShortCutKey = "LeftButton+WheelDown";
            this.IsShowMessage = true;

            this.ParameterSource = new CommandParameterSource(new ToggleStretchModeCommandParameter() { IsLoop = true });
        }

        public override string ExecuteMessage(CommandParameter param, object[] args, CommandOption option)
        {
            return ContentCanvas.Current.GetToggleStretchMode((ToggleStretchModeCommandParameter)param).ToAliasName();
        }

        public override bool CanExecute(CommandParameter param, object[] args, CommandOption option)
        {
            return !NowLoading.Current.IsDispNowLoading;
        }

        public override void Execute(CommandParameter param, object[] args, CommandOption option)
        {
            ContentCanvas.Current.StretchMode = ContentCanvas.Current.GetToggleStretchMode((ToggleStretchModeCommandParameter)param);
        }
    }
}
