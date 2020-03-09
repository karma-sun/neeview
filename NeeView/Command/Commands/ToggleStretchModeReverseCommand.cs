namespace NeeView
{
    public class ToggleStretchModeReverseCommand : CommandElement
    {
        public ToggleStretchModeReverseCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupImageScale;
            this.Text = Properties.Resources.CommandToggleStretchModeReverse;
            this.Note = Properties.Resources.CommandToggleStretchModeReverseNote;
            this.ShortCutKey = "LeftButton+WheelUp";
            this.IsShowMessage = true;

            // "ToggleStretchMode"
            this.ParameterSource = new CommandParameterSource(new ToggleStretchModeCommandParameter() { IsLoop = true });
        }

        public override string ExecuteMessage(CommandParameter param, object arg, CommandOption option)
        {
            return ContentCanvas.Current.GetToggleStretchModeReverse((ToggleStretchModeCommandParameter)param).ToAliasName();
        }

        public override bool CanExecute(CommandParameter param, object arg, CommandOption option)
        {
            return !NowLoading.Current.IsDispNowLoading;
        }

        public override void Execute(CommandParameter param, object arg, CommandOption option)
        {
            ContentCanvas.Current.StretchMode = ContentCanvas.Current.GetToggleStretchModeReverse((ToggleStretchModeCommandParameter)param);
        }
    }
}
