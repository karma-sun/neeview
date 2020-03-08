namespace NeeView
{
    public class ToggleStretchModeReverseCommand : CommandElement
    {
        public ToggleStretchModeReverseCommand() : base("ToggleStretchModeReverse")
        {
            this.Group = Properties.Resources.CommandGroupImageScale;
            this.Text = Properties.Resources.CommandToggleStretchModeReverse;
            this.Note = Properties.Resources.CommandToggleStretchModeReverseNote;
            this.ShortCutKey = "LeftButton+WheelUp";
            this.IsShowMessage = true;

            // "ToggleStretchMode"
            this.ParameterSource = new CommandParameterSource(new ToggleStretchModeCommandParameter() { IsLoop = true });
        }

        public override string ExecuteMessage(CommandParameter param, CommandOption option = CommandOption.None)
        {
            return ContentCanvas.Current.GetToggleStretchModeReverse((ToggleStretchModeCommandParameter)param).ToAliasName();
        }

        public override bool CanExecute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            return !NowLoading.Current.IsDispNowLoading;
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            ContentCanvas.Current.StretchMode = ContentCanvas.Current.GetToggleStretchModeReverse((ToggleStretchModeCommandParameter)param);
        }
    }
}
